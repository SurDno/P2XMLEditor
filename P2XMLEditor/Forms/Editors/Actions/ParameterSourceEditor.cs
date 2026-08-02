using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using Message = P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Message;

namespace P2XMLEditor.Forms.Editors.Actions;

public enum ParameterSourceKind {
	Empty,
	Literal,
	Constant,
	Message,
	InputParam,
	LoopIndex,
	LoopElement,
	ParameterRef,
	DynamicParameter,
	ObjectRef,
	Hierarchy,
	GlobalList,
	Raw
}

/// <summary>
/// Edits one <see cref="ParameterSource"/> — the slot behind a function argument, an event
/// message or a SetParam source.
///
/// Two rules shape it. The slot's declared <see cref="VmTypeInfo"/> decides which kinds are
/// offered at all and filters every reference list by
/// <see cref="VmTypeCompatibility"/>, so a System.Boolean slot never offers an object and a
/// parameter of the wrong type is not in the list to pick. <see cref="ActionScope"/> decides
/// which messages, input parameters and loop variables exist here, so everything offered
/// actually resolves at this action.
///
/// Values are composed back into the wire string and re-parsed through
/// <see cref="ParameterSource.Create"/> rather than assembled field by field, so what the
/// editor produces is by construction what the loader accepts.
/// </summary>
public sealed class ParameterSourceEditor : UserControl {
	public const int PreferredHeight = 30;
	private const int ExtraColumnWidth = 140;
	private const int PickColumnWidth = 86;

	private readonly VirtualMachine _vm;
	private readonly ActionScope _scope;
	private ParamTarget? _target;

	private readonly ComboBox _kind;
	private readonly Panel _valueHost;
	private readonly TextBox _literal;
	private readonly ComboBox _choice;
	private readonly TextBox _reference;
	private readonly ComboBox _extra;
	private readonly ComboBox _named;
	private readonly Button _pick;
	private readonly TableLayoutPanel _layout;

	private VmTypeInfo? _expectedType;
	private SlotConstraint? _constraint;
	private VmElement? _pickedElement;
	private HierarchyGuid? _pickedHierarchy;
	private string _originalText = "";
	private bool _dirty;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	/// <summary>
	/// Whether "const_" values are offered. They are only meaningful in an action line's loop
	/// bounds, never in a function argument or an event message, so this is off by default.
	/// </summary>
	public bool AllowConstant { get; }

	public ParameterSourceEditor(VirtualMachine vm, ActionScope scope, VmTypeInfo? expectedType = null,
		ParamTarget? target = null, bool allowConstant = false) {
		_vm = vm;
		_scope = scope;
		_expectedType = expectedType;
		_target = target;
		AllowConstant = allowConstant;

		Height = PreferredHeight;
		Margin = new Padding(0, 2, 0, 2);

		_kind = NewCombo(ComboBoxStyle.DropDownList);
		_kind.Margin = new Padding(0, 0, 6, 0);
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_literal = new TextBox { Dock = DockStyle.Fill };
		_literal.TextChanged += (_, _) => OnUserEdit(null);

		_choice = NewCombo(ComboBoxStyle.DropDownList);
		_choice.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_reference = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };

		// Some slots take an object's name as a plain string. Editable, because the data has a
		// few names that are real objects without the component the slot usually wants.
		_named = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, IntegralHeight = false,
			AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems
		};
		_named.SelectedIndexChanged += (_, _) => OnUserEdit(null);
		_named.TextChanged += (_, _) => OnUserEdit(null);

		_valueHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
		_valueHost.Controls.AddRange([_literal, _choice, _named, _reference]);

		_extra = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, IntegralHeight = false,
			AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems,
			Margin = new Padding(0, 0, 6, 0)
		};
		_extra.SelectedIndexChanged += (_, _) => OnUserEdit(null);
		_extra.TextChanged += (_, _) => OnUserEdit(null);

		_pick = new Button { Dock = DockStyle.Fill, Text = "Select…", Margin = Padding.Empty };
		_pick.Click += (_, _) => Pick();

		_layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ExtraColumnWidth));
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, PickColumnWidth));
		_layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		_layout.Controls.Add(_kind, 0, 0);
		_layout.Controls.Add(_valueHost, 1, 0);
		_layout.Controls.Add(_extra, 2, 0);
		_layout.Controls.Add(_pick, 3, 0);

		Controls.Add(_layout);

		PopulateKinds();
		UpdateVisibleControls();
	}

	private static ComboBox NewCombo(ComboBoxStyle style) =>
		new() { Dock = DockStyle.Fill, DropDownStyle = style, IntegralHeight = false };

	/// <summary>
	/// The slot's declared type. Setting it re-derives which kinds are on offer and rebuilds
	/// every filtered list, which is how a DoFunction slot reshapes itself when the selected
	/// function changes.
	/// </summary>
	public VmTypeInfo? ExpectedType {
		get => _expectedType;
		set {
			var before = SerializedValue;
			_expectedType = value;
			var current = SelectedKind;
			PopulateKinds();
			SelectKind(current);
			UpdateVisibleControls();

			// Retyping can take the value away — the kind may no longer be offered, or the
			// message that was selected may no longer be type-compatible. When that happens
			// the control no longer holds what it was loaded with, so it stops claiming to:
			// otherwise an untouched slot would keep emitting a value the new type rejects.
			if (Compose() != before) _dirty = true;
		}
	}

	/// <summary>
	/// What this slot really accepts, where the function narrows it beyond the declared type.
	/// </summary>
	public SlotConstraint? Constraint {
		get => _constraint;
		set {
			_constraint = value;
			PopulateKinds();
			UpdateVisibleControls();
		}
	}

	/// <summary>Target of the enclosing SetParam, used by the parser to infer an absent type.</summary>
	public ParamTarget? Target {
		get => _target;
		set => _target = value;
	}

	/// <summary>The wire string this editor currently represents.</summary>
	public string SerializedValue => _dirty ? Compose() : _originalText;

	public ParameterSource Value {
		get {
			try {
				// ParameterSource.Create resolves "<graphId>_inputparam_<name>" through
				// InputParameter.TryParse, which walks up from VirtualMachine.FillScope to find
				// the declaring graph. That is only set while the XML is being read, so outside
				// a load an input-param reference silently degrades to a literal string. The
				// action's own local context is exactly the scope it should resolve against.
				using var fillScope = VirtualMachine.EnterFillScope(_scope.LocalContext);
				return ParameterSource.Create(SerializedValue, _vm, _target, _expectedType);
			} catch (Exception ex) {
				Logger.Log(LogLevel.Warning, $"Could not build a parameter source from '{SerializedValue}': {ex.Message}");
				return default;
			}
		}
		set => Load(value);
	}

	public void Load(ParameterSource source, string? rawText = null) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = rawText ?? SafeWrite(source);
			_dirty = false;
			_pickedElement = source.ElementReference;
			_pickedHierarchy = source.HierarchyReference;

			var kind = KindOf(source);
			EnsureKindOffered(kind);
			SelectKind(kind);
			// Before filling anything in: the choice lists are populated here, and a selection
			// cannot be restored into a list that does not exist yet.
			UpdateVisibleControls();

			switch (kind) {
				case ParameterSourceKind.Literal:
				case ParameterSourceKind.Constant:
					var literal = source.LiteralValue?.Serialize() ?? "";
					if (source.IsCommaSeparator) literal = literal.Replace('.', ',');
					_literal.Text = literal;
					_named.Text = literal;
					SelectById(_choice, literal);
					break;
				case ParameterSourceKind.Message:
					SelectById(_choice, source.MessageReference?.Name ?? "");
					break;
				case ParameterSourceKind.InputParam:
					SelectById(_choice, source.InputParamReference?.Name ?? "");
					break;
				case ParameterSourceKind.LoopIndex:
				case ParameterSourceKind.LoopElement:
					SelectLoop(source);
					break;
				case ParameterSourceKind.ParameterRef:
					_pickedElement = source.ParameterReference;
					_reference.Text = VmElementPicker.DescribeDetailed(source.ParameterReference, _vm);
					break;
				case ParameterSourceKind.DynamicParameter:
					_pickedElement = source.DynamicObjectReference;
					_reference.Text = VmElementPicker.DescribeDetailed(source.DynamicObjectReference, _vm);
					_extra.Text = source.DynamicParameterName ?? "";
					break;
				case ParameterSourceKind.ObjectRef:
					_pickedElement = ReferencedElement(source);
					_reference.Text = VmElementPicker.DescribeDetailed(_pickedElement, _vm);
					break;
				case ParameterSourceKind.Hierarchy:
					_reference.Text = DescribeHierarchy(source.HierarchyReference);
					break;
				case ParameterSourceKind.GlobalList:
					_literal.Text = source.GlobalListName ?? "";
					break;
				case ParameterSourceKind.Raw:
					_literal.Text = _originalText;
					break;
			}

			UpdateVisibleControls();
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// Shows a value the parser could not make sense of, verbatim and editable. Switching to
	/// any other kind replaces it outright.
	/// </summary>
	public void LoadRaw(string text) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = text;
			_dirty = false;
			EnsureKindOffered(ParameterSourceKind.Raw);
			SelectKind(ParameterSourceKind.Raw);
			UpdateVisibleControls();
			_literal.Text = text;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	public ParameterSourceKind SelectedKind =>
		_kind.SelectedItem is KindItem item ? item.Kind : ParameterSourceKind.Empty;

	// ---------------------------------------------------------------- composition

	private string Compose() {
		switch (SelectedKind) {
			case ParameterSourceKind.Empty:
				return "";
			case ParameterSourceKind.Literal:
				return CurrentLiteral();
			case ParameterSourceKind.Constant:
				return "const_" + CurrentLiteral();
			case ParameterSourceKind.Message:
			case ParameterSourceKind.InputParam:
			case ParameterSourceKind.LoopIndex:
			case ParameterSourceKind.LoopElement:
				return (_choice.SelectedItem as ChoiceItem)?.Id ?? "";
			case ParameterSourceKind.ParameterRef:
				// A parameter reference carries its holder as the prefix; the holder is the
				// parameter's own parent, so there is nothing separate for the user to pick.
				return _pickedElement is Parameter p ? $"{p.Parent.Id}%{p.Id}" : "";
			case ParameterSourceKind.DynamicParameter:
				return _pickedElement != null && _extra.Text.Length > 0
					? $"{_pickedElement.Id}%{_extra.Text}"
					: "";
			case ParameterSourceKind.ObjectRef:
				// Always by id. An engine GUID names the same object and the data uses it in
				// places, so an untouched value still round-trips, but nothing is authored
				// that way.
				return _pickedElement?.Id.ToString() ?? "";
			case ParameterSourceKind.Hierarchy:
				return _pickedHierarchy?.Write() ?? "";
			case ParameterSourceKind.GlobalList:
			case ParameterSourceKind.Raw:
				return _literal.Text;
			default:
				return "";
		}
	}

	/// <summary>Enum and boolean literals are chosen, not typed; everything else is free text.</summary>
	private string CurrentLiteral() {
		if (LiteralIsNamedObject) return _named.Text;
		return LiteralIsChosen ? (_choice.SelectedItem as ChoiceItem)?.Id ?? "" : _literal.Text;
	}

	private bool LiteralIsChosen =>
		VmTypeCompatibility.EnumTypeOf(_expectedType) != null || _expectedType?.BaseType == VmType.Boolean;

	private bool LiteralIsNamedObject => _constraint?.Form == SlotValueForm.Name;

	private static string SafeWrite(ParameterSource source) {
		try {
			return source.Write();
		} catch {
			return "";
		}
	}

	private static VmElement? ReferencedElement(ParameterSource source) =>
		source.ElementReference
		?? source.BlueprintReference?.Element.Element
		?? source.EntityReference?.Element;

	private string DescribeHierarchy(HierarchyGuid? hierarchy) {
		if (hierarchy == null) return "";
		var path = string.Join(" → ", hierarchy.Elements.Select(e => VmElementPicker.DescribeDetailed(e.Element, _vm)));
		return $"{path}   ({hierarchy.Write()})";
	}

	// ---------------------------------------------------------------- kinds

	private void PopulateKinds() {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_kind.Items.Clear();
			foreach (var kind in OfferedKinds())
				_kind.Items.Add(new KindItem(kind));
			if (_kind.Items.Count > 0) _kind.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// The kinds that can legally fill this slot, most apt first. A kind is offered only when
	/// the declared type admits it and — for the scope-backed kinds — only when something of a
	/// compatible type is actually in scope, so an empty dropdown is never presented.
	/// </summary>
	private IEnumerable<ParameterSourceKind> OfferedKinds() {
		var type = _expectedType;
		var isRef = VmTypeCompatibility.IsElementLike(type);
		var isList = type?.BaseType == VmType.List;
		var isLiteral = VmTypeCompatibility.IsLiteralLike(type);
		var isUntyped = type == null || type.BaseType == VmType.Unknown;

		if (isRef) {
			yield return ParameterSourceKind.ObjectRef;
			yield return ParameterSourceKind.Hierarchy;
		}
		if (isLiteral) yield return ParameterSourceKind.Literal;
		if (isList) yield return ParameterSourceKind.GlobalList;

		if (CompatibleMessages().Any()) yield return ParameterSourceKind.Message;
		if (CompatibleInputParams().Any()) yield return ParameterSourceKind.InputParam;
		if (CompatibleLoops(true).Any()) yield return ParameterSourceKind.LoopIndex;
		if (CompatibleLoops(false).Any()) yield return ParameterSourceKind.LoopElement;

		// A parameter can back any slot as long as its own declared type fits.
		if (CompatibleParameters().Any()) yield return ParameterSourceKind.ParameterRef;
		// Which object it reads from is only known at runtime, but the name is not: it has to
		// be a standard parameter name whose declared type can fill this slot, so the kind is
		// pointless where no such name exists.
		if (DynamicParameterNames().Any()) yield return ParameterSourceKind.DynamicParameter;

		// const_ is an Int32 literal the engine reads only for loop bounds.
		if (AllowConstant && (isUntyped || type!.BaseType == VmType.Int32))
			yield return ParameterSourceKind.Constant;

		if (isUntyped && !isRef) {
			yield return ParameterSourceKind.ObjectRef;
			yield return ParameterSourceKind.Hierarchy;
		}

		// Empty and Raw are both deliberately absent. Empty is what an unset slot already is,
		// and Raw exists only so a value the parser rejects stays visible and editable;
		// EnsureKindOffered adds either back when a loaded value needs it.: it exists only so a value the parser rejects stays
		// visible and editable, and EnsureKindOffered adds it for exactly those. Authoring a
		// new unresolvable value is not something to offer.
	}

	/// <summary>
	/// A stored value always stays selectable, even where the declared type would not have
	/// suggested it — existing data outranks the editor's opinion of what belongs.
	/// </summary>
	private void EnsureKindOffered(ParameterSourceKind kind) {
		if (_kind.Items.Cast<KindItem>().Any(i => i.Kind == kind)) return;
		_kind.Items.Insert(0, new KindItem(kind));
	}

	private void SelectKind(ParameterSourceKind kind) {
		for (var i = 0; i < _kind.Items.Count; i++) {
			if (_kind.Items[i] is KindItem item && item.Kind == kind) {
				_kind.SelectedIndex = i;
				return;
			}
		}
	}

	private static ParameterSourceKind KindOf(ParameterSource source) {
		if (source.IsConstant) return ParameterSourceKind.Constant;
		if (source.MessageReference != null) return ParameterSourceKind.Message;
		if (source.InputParamReference != null) return ParameterSourceKind.InputParam;
		if (source.IsLoopIndex) return ParameterSourceKind.LoopIndex;
		if (source.IsLoopElement) return ParameterSourceKind.LoopElement;
		if (source.DynamicObjectReference != null) return ParameterSourceKind.DynamicParameter;
		if (source.ParameterReference != null) return ParameterSourceKind.ParameterRef;
		if (source.HierarchyReference != null) return ParameterSourceKind.Hierarchy;
		if (source.GlobalListName != null) return ParameterSourceKind.GlobalList;
		if (ReferencedElement(source) != null) return ParameterSourceKind.ObjectRef;
		if (source.LiteralValue != null) return ParameterSourceKind.Literal;
		return ParameterSourceKind.Empty;
	}

	// ---------------------------------------------------------------- visibility

	private void UpdateVisibleControls() {
		var kind = SelectedKind;
		var literalIsNamed = LiteralIsNamedObject && kind == ParameterSourceKind.Literal;
		var literalIsChosen = !literalIsNamed && LiteralIsChosen && kind == ParameterSourceKind.Literal;

		// Visibility is derived from local booleans rather than read back off the controls:
		// Control.Visible reports false for anything whose parent chain is not yet shown, so
		// reading it during construction would leave the dependent controls hidden until the
		// user happened to change a dropdown.
		var showLiteral = !literalIsChosen && !literalIsNamed && kind is ParameterSourceKind.Literal
			or ParameterSourceKind.Constant or ParameterSourceKind.GlobalList or ParameterSourceKind.Raw;
		var showChoice = literalIsChosen || kind is ParameterSourceKind.Message or ParameterSourceKind.InputParam
			or ParameterSourceKind.LoopIndex or ParameterSourceKind.LoopElement;
		var showReference = kind is ParameterSourceKind.ParameterRef or ParameterSourceKind.DynamicParameter
			or ParameterSourceKind.ObjectRef or ParameterSourceKind.Hierarchy;
		var showExtra = kind == ParameterSourceKind.DynamicParameter;

		_literal.Visible = showLiteral;
		_choice.Visible = showChoice;
		_named.Visible = literalIsNamed;
		_reference.Visible = showReference;
		_extra.Visible = showExtra;
		_pick.Visible = showReference;

		// A column that holds nothing takes no width, so a literal gets the whole row and an
		// object reference has its Select button flush against the value.
		_layout.ColumnStyles[2].Width = showExtra ? ExtraColumnWidth : 0;
		_layout.ColumnStyles[3].Width = showReference ? PickColumnWidth : 0;

		if (showChoice) PopulateChoices(kind, literalIsChosen);
		if (showExtra) PopulateDynamicNames();
		if (literalIsNamed) PopulateObjectNames();
		if (showReference) _reference.Text = CurrentReferenceText(kind);

		_literal.PlaceholderText = kind switch {
			ParameterSourceKind.GlobalList => "global list name",
			ParameterSourceKind.Raw => "verbatim value",
			ParameterSourceKind.Constant => "integer",
			_ => Describe(_expectedType)
		};
	}

	private static string Describe(VmTypeInfo? type) {
		try {
			return type?.Serialize() ?? "";
		} catch {
			return "";
		}
	}

	private void PopulateChoices(ParameterSourceKind kind, bool literalIsChosen) {
		var selected = (_choice.SelectedItem as ChoiceItem)?.Id;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_choice.Items.Clear();

			if (literalIsChosen) {
				foreach (var (id, label) in LiteralChoices())
					_choice.Items.Add(new ChoiceItem(id, label));
			} else {
				switch (kind) {
					case ParameterSourceKind.Message:
						foreach (var message in CompatibleMessages())
							_choice.Items.Add(new ChoiceItem(message.Name,
								$"{message.ParamName}   [{message.Type}]   ← {message.Event.Name}"));
						break;
					case ParameterSourceKind.InputParam:
						foreach (var inputParam in CompatibleInputParams())
							_choice.Items.Add(new ChoiceItem(inputParam.Name,
								$"{inputParam.ParamName}   [{inputParam.Type}]   ← {inputParam.Graph.Name}"));
						break;
					case ParameterSourceKind.LoopIndex:
						foreach (var loop in CompatibleLoops(true))
							_choice.Items.Add(new ChoiceItem(loop.ParamId, $"index of {loop.ActionLine.Name}"));
						break;
					case ParameterSourceKind.LoopElement:
						foreach (var loop in CompatibleLoops(false))
							_choice.Items.Add(new ChoiceItem(loop.ParamId,
								$"element of {loop.ListName} in {loop.ActionLine.Name}"));
						break;
				}
			}

			if (selected != null) SelectById(_choice, selected);
			if (_choice.SelectedIndex < 0 && _choice.Items.Count > 0) _choice.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// Names a "parameter by name on object" may resolve to. The object is only known at
	/// runtime, but the name is not: standard parameter names are declared game-wide with a
	/// type, so the ones whose type cannot fill this slot are left out. The box stays editable
	/// for a name the loaded data uses and the index does not know.
	/// </summary>
	private void PopulateDynamicNames() {
		var current = _extra.Text;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_extra.Items.Clear();
			foreach (var name in DynamicParameterNames())
				_extra.Items.Add(name);
			_extra.Text = current;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private IEnumerable<string> DynamicParameterNames() =>
		_vm.StandartParamTypes
			.Where(entry => VmTypeCompatibility.Matches(_expectedType, entry.Value))
			.Select(entry => entry.Key)
			.OrderBy(name => name, StringComparer.Ordinal);

	/// <summary>
	/// What the reference box should read for the kind now selected. Object reference and scene
	/// hierarchy describe the same object two ways, so switching between them carries the value
	/// across instead of blanking it: a hierarchy keeps its leaf, and an object rebuilds the
	/// path from its own parent chain.
	/// </summary>
	private string CurrentReferenceText(ParameterSourceKind kind) {
		switch (kind) {
			case ParameterSourceKind.ObjectRef:
				_pickedElement ??= _pickedHierarchy?.Elements[^1].Element;
				return VmElementPicker.DescribeDetailed(_pickedElement, _vm);
			case ParameterSourceKind.Hierarchy:
				_pickedHierarchy ??= HierarchyOf(_pickedElement);
				return DescribeHierarchy(_pickedHierarchy);
			case ParameterSourceKind.ParameterRef:
			case ParameterSourceKind.DynamicParameter:
				return VmElementPicker.DescribeDetailed(_pickedElement, _vm);
			default:
				return "";
		}
	}

	/// <summary>
	/// The path naming an object, when there is exactly one.
	///
	/// Only used to carry a value across a kind switch, so it answers nothing it cannot answer
	/// with certainty: an object placed nowhere has no path at all, and one placed in several
	/// spots has no single path to promote it to — the user picks which, in
	/// <see cref="HierarchyPicker"/>.
	/// </summary>
	private HierarchyGuid? HierarchyOf(VmElement? leaf) {
		if (leaf == null) return null;
		var path = WorldHierarchy.For(_vm).SolePlacement(leaf.Id);
		if (path is not { Length: > 1 }) return null;
		return HierarchyGuid.TryParse(string.Join("H", path), _vm, out var hierarchy) ? hierarchy : null;
	}

	/// <summary>Names of the objects a name-form slot accepts.</summary>
	private void PopulateObjectNames() {
		var current = _named.Text;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_named.Items.Clear();
			foreach (var name in ConstrainedObjects().Select(o => o.Name).Distinct(StringComparer.Ordinal)
						 .OrderBy(n => n, StringComparer.Ordinal))
				_named.Items.Add(name);
			_named.Text = current;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>Objects satisfying the slot's component constraint, or all of them when free.</summary>
	private IEnumerable<ParameterHolder> ConstrainedObjects() {
		var required = _constraint?.RequiredComponent ?? VmComponent.None;
		var all = _vm.AllParameterHolders();
		return required == VmComponent.None
			? all
			: all.Where(h => ActionScope.HasComponent(h, required, _vm));
	}

	private IEnumerable<(string Id, string Label)> LiteralChoices() {
		var enumType = VmTypeCompatibility.EnumTypeOf(_expectedType);
		if (enumType != null)
			return Enum.GetValues(enumType).Cast<Enum>().Select(v => (SerializeEnum(v), v.ToString()));
		return [("True", "True"), ("False", "False")];
	}

	private static string SerializeEnum(Enum value) {
		try {
			return value.Serialize();
		} catch {
			return value.ToString();
		}
	}

	// ---------------------------------------------------------------- filtered candidates

	private IEnumerable<Message> CompatibleMessages() =>
		_scope.Messages.Where(m => VmTypeCompatibility.Matches(_expectedType, m.Type, _vm));

	private IEnumerable<InputParameter> CompatibleInputParams() =>
		_scope.InputParams.Where(p => VmTypeCompatibility.Matches(_expectedType, p.Type, _vm));

	/// <summary>
	/// A loop index is an Int32 and a loop element is an object; both are fixed by the loop
	/// construct rather than declared anywhere, so they are typed here.
	/// </summary>
	private IEnumerable<LoopParameter> CompatibleLoops(bool index) =>
		_scope.LoopVariables.Where(l => l.IsIndex == index &&
			VmTypeCompatibility.Matches(_expectedType, index ? VmTypeInfo.Int32 : VmTypeInfo.GameObject));

	/// <summary>
	/// An expression's constant is excluded here as everywhere else: it is the expression's own
	/// literal storage, reachable only through the expression, and no action in either corpus
	/// reads one.
	/// </summary>
	private IEnumerable<Parameter> CompatibleParameters() =>
		_vm.GetElementsByType<Parameter>()
			.Where(p => !p.IsConstant && VmTypeCompatibility.Matches(_expectedType, p.Type, _vm));

	// ---------------------------------------------------------------- picking

	private void Pick() {
		switch (SelectedKind) {
			case ParameterSourceKind.ParameterRef:
				PickElement("Select parameter", CompatibleParameters());
				break;
			case ParameterSourceKind.DynamicParameter:
				// The prefix names an object-valued parameter; the text box names the
				// parameter to read off whatever object it points at.
				PickElement("Select object parameter",
					_vm.GetElementsByType<Parameter>()
						.Where(p => !p.IsConstant && VmTypeCompatibility.IsObjectValued(p.Type, _vm)));
				break;
			case ParameterSourceKind.ObjectRef:
				PickElement("Select object", ObjectCandidates(), BareIdNote);
				break;
			case ParameterSourceKind.Hierarchy:
				PickHierarchy();
				break;
		}
	}

	private void PickElement(string title, IEnumerable<VmElement> candidates,
		Func<VmElement, string?>? note = null) {
		if (!VmElementPicker.TryPick(FindForm(), title, candidates, e => VmElementPicker.Describe(e, _vm), _pickedElement,
				out var picked, note))
			return;
		_pickedElement = picked;
		// The two object kinds describe the same thing, so a fresh pick invalidates the path
		// built from the previous one rather than leaving it to reappear on a kind switch.
		_pickedHierarchy = null;
		_reference.Text = VmElementPicker.DescribeDetailed(picked, _vm);
		OnUserEdit(null);
	}

	/// <summary>
	/// A hierarchy names a spot in the built world, so it is picked as a placement rather than
	/// as an object — see <see cref="HierarchyPicker"/>.
	/// </summary>
	private void PickHierarchy() {
		if (!HierarchyPicker.TryPick(FindForm(), _vm, "Select a place in the world", _pickedHierarchy, out var picked))
			return;

		_pickedHierarchy = picked;
		// The object kind describes the same thing a different way; carrying a stale element
		// across would let a kind switch resurrect the object the path replaced.
		_pickedElement = picked?.Elements[^1].Element;
		_reference.Text = DescribeHierarchy(picked);
		OnUserEdit(null);
	}

	private IEnumerable<VmElement> ObjectCandidates() {
		// A function that only accepts, say, a storable says so regardless of the wider type its
		// declaration carries.
		if (_constraint is { RequiredComponent: not VmComponent.None }) return ConstrainedObjects();

		var type = _expectedType;
		if (type?.BaseType is VmType.BlueprintRef or VmType.BlueprintRefStorable)
			// BlueprintRef.Element is a VmEither<Item, Other, Character>; nothing else fits.
			return _vm.AllParameterHolders().Where(o => o is Item or Other or Character);

		var systemType = type == null ? null : VmTypeHelper.GetSystemType(type.BaseType);
		if (systemType != null && typeof(VmElement).IsAssignableFrom(systemType) && systemType != typeof(GameObject))
			return _vm.AllElements()
				.Where(element => systemType.IsInstanceOfType(element) && element is not IPlaceholder
					&& element is not Parameter { IsConstant: true });

		return _vm.AllParameterHolders();
	}

	/// <summary>
	/// Why an id would not reach this object when the action runs — see
	/// <see cref="BareIdReach"/>. Shown, not enforced: the answer depends on where the action
	/// lives, and the shipped content relies on both outcomes.
	/// </summary>
	private string? BareIdNote(VmElement element) =>
		BareIdReach.Problem(element as ParameterHolder, _scope.Owner, _vm);

	// ---------------------------------------------------------------- helpers

	private static void SelectById(ComboBox box, string id) {
		if (string.IsNullOrEmpty(id)) return;
		for (var i = 0; i < box.Items.Count; i++) {
			if (box.Items[i] is ChoiceItem item && item.Id == id) {
				box.SelectedIndex = i;
				return;
			}
		}
	}

	private void SelectLoop(ParameterSource source) {
		var line = source.LoopActionLine;
		if (line == null) return;
		var id = source.IsLoopIndex
			? $"local_{line.Id}_Loop_Index"
			: $"local_{line.Id}_Loop_List_{line.LoopInfo?.Name.GetVariableName() ?? source.LoopListName}_Element";
		SelectById(_choice, id);
	}

	private void OnUserEdit(System.Action? before) {
		if (_suppressEvents) return;
		_dirty = true;
		before?.Invoke();
		ValueChanged?.Invoke(this, EventArgs.Empty);
	}

	private sealed class KindItem(ParameterSourceKind kind) {
		public ParameterSourceKind Kind { get; } = kind;
		public override string ToString() => Kind switch {
			ParameterSourceKind.Empty => "(empty)",
			ParameterSourceKind.Literal => "Literal value",
			ParameterSourceKind.Constant => "Constant (const_)",
			ParameterSourceKind.Message => "Event message",
			ParameterSourceKind.InputParam => "Graph input param",
			ParameterSourceKind.LoopIndex => "Loop index",
			ParameterSourceKind.LoopElement => "Loop element",
			ParameterSourceKind.ParameterRef => "Parameter",
			ParameterSourceKind.DynamicParameter => "Dynamic parameter",
			ParameterSourceKind.ObjectRef => "Object reference",
			ParameterSourceKind.Hierarchy => "Scene hierarchy",
			ParameterSourceKind.GlobalList => "Global list",
			ParameterSourceKind.Raw => "Raw text",
			_ => Kind.ToString()
		};
	}

	private sealed class ChoiceItem(string id, string label) {
		public string Id { get; } = id;
		public override string ToString() => label;
	}
}
