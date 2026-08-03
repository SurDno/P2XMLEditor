using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Edits an expression's constant.
///
/// A constant is not a slot value with a dozen possible shapes — it is one
/// <see cref="Parameter"/> that the expression owns outright, holding a declared type and a
/// literal of that type. The data is unanimous: all 511 constants in MarbleNest and all 6212 in
/// PathologicSandbox are a Parameter whose Parent is the expression itself and whose Custom flag
/// is set, and none of them is referenced from anywhere else. So there is nothing here to point
/// at a message, a loop variable or another parameter — offering those, as a
/// <see cref="ParameterSourceEditor"/> does, describes a slot this is not.
///
/// What it does need, and a source slot does not, is the type: a slot's type comes from the
/// function or parameter that declares it, while a constant carries its own. Hence two controls
/// — the type, and a value control chosen to suit it.
/// </summary>
public sealed class ConstantEditor : UserControl {
	public const int PreferredHeight = 30;
	private const int TypeColumnWidth = 190;
	private const int PickColumnWidth = 86;

	/// <summary>
	/// Types a constant can be written as. Void and Unknown are not types to author; a list has
	/// no literal form; VMType and the combination struct are values built elsewhere. Everything
	/// else <see cref="ParameterValue.Create"/> can build from a string, which is exactly what a
	/// constant is stored as.
	/// </summary>
	private static readonly VmType[] Authorable = Enum.GetValues<VmType>()
		.Where(t => t is not (VmType.Void or VmType.Unknown or VmType.List or VmType.TypeValue
			or VmType.ObjectCombinationDataStruct))
		.ToArray();

	/// <summary>Offered first, because they are what a constant almost always is.</summary>
	private static readonly VmType[] Common = [
		VmType.Boolean, VmType.Int32, VmType.Single, VmType.String, VmType.UInt64, VmType.GameTime
	];

	private readonly VirtualMachine _vm;

	private readonly ComboBox _type;
	private readonly Panel _valueHost;
	private readonly TextBox _text;
	private readonly ComboBox _choice;
	private readonly TextBox _reference;
	private readonly Button _pick;
	private readonly TableLayoutPanel _layout;

	private VmTypeInfo? _expectedType;
	private VmElement? _picked;

	// The literal behind the reference box, kept as text rather than rebuilt from the element.
	// A reference constant may hold a hierarchy path instead of an id, and one it cannot resolve
	// to anything is still a value — neither survives a round trip through the picked element.
	private string _referenceLiteral = "";

	// The type the constant arrived with, kept offered even where the expected type would not
	// have suggested it: what the data already says outranks the editor's opinion of it.
	private string? _loadedType;

	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	public ConstantEditor(VirtualMachine vm, VmTypeInfo? expectedType = null) {
		_vm = vm;
		_expectedType = expectedType;

		Height = PreferredHeight;
		Margin = new Padding(0, 2, 0, 2);

		_type = NewCombo();
		_type.Margin = new Padding(0, 0, 6, 0);
		_type.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_text = new TextBox { Dock = DockStyle.Fill };
		_text.TextChanged += (_, _) => OnUserEdit(null);

		_choice = NewCombo();
		_choice.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_reference = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };

		_valueHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
		_valueHost.Controls.AddRange([_text, _choice, _reference]);

		_pick = new Button { Dock = DockStyle.Fill, Text = "Select…", Margin = Padding.Empty };
		_pick.Click += (_, _) => Pick();

		_layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, TypeColumnWidth));
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, PickColumnWidth));
		_layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		_layout.Controls.Add(_type, 0, 0);
		_layout.Controls.Add(_valueHost, 1, 0);
		_layout.Controls.Add(_pick, 2, 0);

		Controls.Add(_layout);

		PopulateTypes();
		UpdateVisibleControls();
	}

	private static ComboBox NewCombo() =>
		new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false };

	/// <summary>
	/// What the expression is being read as. A constant is free to declare any type the slot
	/// accepts — an Int32 constant against a Single parameter is fine — so this narrows the type
	/// list rather than fixing it.
	/// </summary>
	public VmTypeInfo? ExpectedType {
		get => _expectedType;
		set {
			_expectedType = value;
			var current = SelectedTypeName;
			PopulateTypes();
			SelectType(current);
			UpdateVisibleControls();
		}
	}

	/// <summary>The declared type as it is written in the xml, or "" when none is selected.</summary>
	public string SelectedTypeName => (_type.SelectedItem as TypeItem)?.XmlType ?? "";

	/// <summary>The literal, in the form <see cref="ParameterValue.Create"/> reads.</summary>
	public string SerializedValue {
		get {
			if (ValueIsChosen) return (_choice.SelectedItem as ChoiceItem)?.Id ?? "";
			if (ValueIsReference) return _referenceLiteral;
			return _text.Text;
		}
	}

	/// <summary>
	/// Whether anything can be stored. A reference constant is exempt: an empty one is a null
	/// reference, which is what 47 of the 63 non-enum reference constants in the two corpora are.
	/// </summary>
	public bool IsComplete => SelectedTypeName.Length > 0 && (ValueIsReference || SerializedValue.Length > 0);

	/// <summary>Builds the value, or null when the text is not readable as the chosen type.</summary>
	public ParameterValue? Build() {
		try {
			return ParameterValue.Create(_vm, SelectedTypeName, SerializedValue);
		} catch {
			return null;
		}
	}

	public void Load(Parameter constant) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_loadedType = constant.Type;
			PopulateTypes();
			SelectType(constant.Type);
			UpdateVisibleControls();

			var serialized = SafeSerialize(constant.Value);
			_referenceLiteral = serialized;
			_picked = ReferencedElement(constant.Value);
			_text.Text = serialized;
			SelectById(_choice, serialized);
			_reference.Text = DescribeReference();

			UpdateVisibleControls();
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	// ---------------------------------------------------------------- types

	private void PopulateTypes() {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_type.Items.Clear();
			foreach (var (xmlType, label) in OfferedTypes())
				_type.Items.Add(new TypeItem(xmlType, label));
			if (_type.Items.Count > 0) _type.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// The types this constant may declare, aptest first.
	///
	/// When the slot expects something, its own spelling comes first — an "IObjRef%cf_Region"
	/// expectation is more than "IObjRef" and a constant may as well say so — followed by every
	/// simple type a value of it would fit. When nothing is expected, everything is on offer.
	/// </summary>
	private IEnumerable<(string XmlType, string Label)> OfferedTypes() {
		var listed = new HashSet<string>(StringComparer.Ordinal);

		foreach (var candidate in Preferred()) {
			if (string.IsNullOrEmpty(candidate) || !listed.Add(candidate)) continue;
			yield return (candidate, candidate);
		}

		foreach (var type in Common.Concat(Authorable.Except(Common))) {
			var info = new VmTypeInfo(type);
			if (!VmTypeCompatibility.Matches(_expectedType, info)) continue;
			var xmlType = type.Serialize();
			if (!listed.Add(xmlType)) continue;
			yield return (xmlType, xmlType);
		}
	}

	private IEnumerable<string> Preferred() {
		var expected = _expectedType;
		if (expected != null && expected.BaseType != VmType.Unknown) {
			var name = SafeSerialize(expected);
			if (name.Length > 0) yield return name;
		}
		if (!string.IsNullOrEmpty(_loadedType)) yield return _loadedType!;
	}

	private void SelectType(string xmlType) {
		if (string.IsNullOrEmpty(xmlType)) return;
		for (var i = 0; i < _type.Items.Count; i++) {
			if (_type.Items[i] is TypeItem item && item.XmlType == xmlType) {
				_type.SelectedIndex = i;
				return;
			}
		}
	}

	private VmTypeInfo? SelectedType() {
		var xmlType = SelectedTypeName;
		if (xmlType.Length == 0) return null;
		try {
			return VmTypeHelper.GetVmTypeInfo(xmlType, _vm);
		} catch {
			return null;
		}
	}

	// ---------------------------------------------------------------- value control

	/// <summary>A boolean or an enum has a fixed set of literals, so it is chosen rather than typed.</summary>
	private bool ValueIsChosen {
		get {
			var type = SelectedType();
			return type != null && (type.BaseType == VmType.Boolean || VmTypeCompatibility.EnumTypeOf(type) != null);
		}
	}

	/// <summary>
	/// True where the literal is an element id. Blueprint and entity references are excluded on
	/// purpose: they are stored as a raw GUID rather than as an id, so they are typed in.
	/// </summary>
	private bool ValueIsReference {
		get {
			var type = SelectedType();
			if (type == null) return false;
			if (type.BaseType is VmType.BlueprintRef or VmType.BlueprintRefStorable or VmType.EntityRef)
				return false;
			var systemType = VmTypeHelper.GetSystemType(type.BaseType);
			return systemType != null && typeof(VmElement).IsAssignableFrom(systemType);
		}
	}

	private void UpdateVisibleControls() {
		var chosen = ValueIsChosen;
		var reference = ValueIsReference;

		_choice.Visible = chosen;
		_reference.Visible = reference;
		_text.Visible = !chosen && !reference;
		_pick.Visible = reference;
		_layout.ColumnStyles[2].Width = reference ? PickColumnWidth : 0;

		if (chosen) PopulateChoices();

		_text.PlaceholderText = SelectedType()?.BaseType switch {
			VmType.GameTime => "days:hours:minutes:seconds",
			VmType.BlueprintRef or VmType.BlueprintRefStorable or VmType.EntityRef => "guid",
			_ => SelectedTypeName
		};
	}

	private void PopulateChoices() {
		var selected = (_choice.SelectedItem as ChoiceItem)?.Id;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_choice.Items.Clear();
			var enumType = VmTypeCompatibility.EnumTypeOf(SelectedType());
			if (enumType != null)
				foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
					_choice.Items.Add(new ChoiceItem(SerializeEnum(value), value.ToString()));
			else
				_choice.Items.AddRange([new ChoiceItem("True", "True"), new ChoiceItem("False", "False")]);

			if (selected != null) SelectById(_choice, selected);
			if (_choice.SelectedIndex < 0 && _choice.Items.Count > 0) _choice.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	// ---------------------------------------------------------------- picking

	private void Pick() {
		var systemType = VmTypeHelper.GetSystemType(SelectedType()?.BaseType ?? VmType.Unknown);
		if (systemType == null) return;

		// A reference constant is legitimately empty — 47 of the 63 non-enum reference constants
		// in the two corpora hold nothing at all — so clearing is offered alongside choosing.
		var candidates = _vm.AllElements()
			.Where(e => systemType.IsInstanceOfType(e) && e is not IPlaceholder
				&& e is not Parameter { IsConstant: true });

		if (!VmElementPicker.TryPick(FindForm(), "Select value", candidates,
				e => VmElementPicker.Describe(e, _vm), _picked, out var picked))
			return;

		_picked = picked;
		_referenceLiteral = picked?.Id.ToString() ?? "";
		_reference.Text = DescribeReference();
		OnUserEdit(null);
	}

	// ---------------------------------------------------------------- helpers

	/// <summary>
	/// The element a value points at, whatever flavour of reference it is. Asked through the
	/// value's own Is/As rather than by matching every RefValue&lt;T&gt; there is, which also
	/// covers HierarchyRefValue — a separate class rather than a subclass of RefValue.
	/// </summary>
	private static VmElement? ReferencedElement(ParameterValue? value) {
		try {
			return value != null && value.Is<VmElement>() ? value.As<VmElement>() : null;
		} catch {
			return null;
		}
	}

	/// <summary>
	/// The reference box's text. A literal that resolves to nothing is shown verbatim instead of
	/// as a blank, so a path or an id the editor cannot place is still visible and still saved.
	/// </summary>
	private string DescribeReference() =>
		_picked != null ? VmElementPicker.DescribeDetailed(_picked, _vm)
		: _referenceLiteral.Length > 0 ? _referenceLiteral
		: "";

	private static string SerializeEnum(Enum value) {
		try {
			return value.Serialize();
		} catch {
			return value.ToString();
		}
	}

	private static string SafeSerialize(ParameterValue? value) {
		try {
			return value?.Serialize() ?? "";
		} catch {
			return "";
		}
	}

	private static string SafeSerialize(VmTypeInfo type) {
		try {
			return type.Serialize();
		} catch {
			return "";
		}
	}

	private static void SelectById(ComboBox box, string id) {
		if (string.IsNullOrEmpty(id)) return;
		for (var i = 0; i < box.Items.Count; i++) {
			if (box.Items[i] is ChoiceItem item && item.Id == id) {
				box.SelectedIndex = i;
				return;
			}
		}
	}

	private void OnUserEdit(System.Action? before) {
		if (_suppressEvents) return;
		before?.Invoke();
		ValueChanged?.Invoke(this, EventArgs.Empty);
	}

	private sealed class TypeItem(string xmlType, string label) {
		public string XmlType { get; } = xmlType;
		public override string ToString() => label;
	}

	private sealed class ChoiceItem(string id, string label) {
		public string Id { get; } = id;
		public override string ToString() => label;
	}
}
