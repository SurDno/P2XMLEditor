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
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

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
/// Two rules shape it. Which kinds are offered comes from the slot's declared
/// <see cref="VmTypeInfo"/>: a string slot takes a literal or anything that evaluates to one,
/// an IObjRef slot takes an element, a hierarchy or an engine GUID, a list slot takes a list.
/// Which *references* are offered comes from <see cref="ActionScope"/>, so the messages and
/// input parameters in the dropdown are only the ones that actually resolve at this action.
///
/// Values are composed back into the wire string and re-parsed through
/// <see cref="ParameterSource.Create"/> rather than assembled field by field, so what the
/// editor produces is by construction what the loader accepts. A slot the user never touches
/// re-emits its original text verbatim, which keeps quirks like a doubled hierarchy or a
/// comma decimal separator from churning on save.
/// </summary>
public sealed class ParameterSourceEditor : UserControl {
	private readonly VirtualMachine _vm;
	private readonly ActionScope _scope;
	private ParamTarget? _target;

	private readonly ComboBox _kind;
	private readonly Panel _valueHost;
	private readonly TextBox _literal;
	private readonly ComboBox _enum;
	private readonly ComboBox _choice;
	private readonly TextBox _reference;
	private readonly Panel _extraHost;
	private readonly TextBox _extra;
	private readonly CheckBox _byEngineGuid;
	private readonly Button _pick;

	private VmTypeInfo? _expectedType;
	private VmElement? _pickedElement;
	private HierarchyGuid? _pickedHierarchy;
	private string _originalText = "";
	private bool _dirty;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	public ParameterSourceEditor(VirtualMachine vm, ActionScope scope, VmTypeInfo? expectedType = null,
		ParamTarget? target = null) {
		_vm = vm;
		_scope = scope;
		_expectedType = expectedType;
		_target = target;

		Height = 26;
		Margin = new Padding(0, 1, 0, 1);

		_kind = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 4, 0) };
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_literal = new TextBox { Dock = DockStyle.Fill };
		_literal.TextChanged += (_, _) => OnUserEdit(null);

		_enum = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
		_enum.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_choice = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
		_choice.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_reference = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };

		_valueHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 4, 0) };
		_valueHost.Controls.AddRange([_literal, _enum, _choice, _reference]);

		_extra = new TextBox { Dock = DockStyle.Fill };
		_extra.TextChanged += (_, _) => OnUserEdit(null);

		_byEngineGuid = new CheckBox { Dock = DockStyle.Fill, Text = "engine GUID", AutoSize = false };
		_byEngineGuid.CheckedChanged += (_, _) => OnUserEdit(null);

		_extraHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 4, 0) };
		_extraHost.Controls.AddRange([_extra, _byEngineGuid]);

		_pick = new Button { Dock = DockStyle.Fill, Text = "…" };
		_pick.Click += (_, _) => Pick();

		var layout = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			ColumnCount = 4,
			RowCount = 1,
			Margin = Padding.Empty,
			Padding = Padding.Empty
		};
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.Controls.Add(_kind, 0, 0);
		layout.Controls.Add(_valueHost, 1, 0);
		layout.Controls.Add(_extraHost, 2, 0);
		layout.Controls.Add(_pick, 3, 0);

		Controls.Add(layout);

		PopulateKinds();
		UpdateVisibleControls();
	}

	/// <summary>
	/// The slot's declared type. Setting it re-derives which kinds are on offer, which is how
	/// a DoFunction slot list reshapes itself when the selected function changes.
	/// </summary>
	public VmTypeInfo? ExpectedType {
		get => _expectedType;
		set {
			_expectedType = value;
			var current = SelectedKind;
			// The enum list is rebuilt lazily and keyed off the type, so it has to go now or
			// a re-typed slot would keep offering the previous enum's members.
			_enum.Items.Clear();
			PopulateKinds();
			SelectKind(current);
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
				return ParameterSource.Create(SerializedValue, _vm, _target, _expectedType);
			} catch (Exception ex) {
				Logger.Log(LogLevel.Warning, $"Could not build a parameter source from '{SerializedValue}': {ex.Message}");
				return default;
			}
		}
		set => Load(value);
	}

	/// <summary>
	/// Loads a source. <paramref name="rawText"/> preserves the exact original spelling when
	/// the caller has it — <see cref="ParameterSource.Write"/> is faithful, so passing it is
	/// only an optimisation, but it also covers sources that failed to parse at load.
	/// </summary>
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
			// Before filling anything in: the enum and scope dropdowns are populated here,
			// and a selection cannot be restored into a list that does not exist yet.
			UpdateVisibleControls();

			switch (kind) {
				case ParameterSourceKind.Literal:
				case ParameterSourceKind.Constant:
					var literal = source.LiteralValue?.Serialize() ?? "";
					if (source.IsCommaSeparator) literal = literal.Replace('.', ',');
					_literal.Text = literal;
					SelectById(_enum, literal);
					break;
				case ParameterSourceKind.Message:
					SelectByTag(_choice, source.MessageReference);
					break;
				case ParameterSourceKind.InputParam:
					SelectByTag(_choice, source.InputParamReference);
					break;
				case ParameterSourceKind.LoopIndex:
				case ParameterSourceKind.LoopElement:
					SelectLoop(source);
					break;
				case ParameterSourceKind.ParameterRef:
					_pickedElement = source.ParameterReference;
					_reference.Text = VmElementPicker.Describe(source.ParameterReference);
					break;
				case ParameterSourceKind.DynamicParameter:
					_pickedElement = source.DynamicObjectReference;
					_reference.Text = VmElementPicker.Describe(source.DynamicObjectReference);
					_extra.Text = source.DynamicParameterName ?? "";
					break;
				case ParameterSourceKind.ObjectRef:
					_pickedElement = ReferencedElement(source);
					_reference.Text = VmElementPicker.Describe(_pickedElement);
					_byEngineGuid.Checked = source.BlueprintReference?.SerializeAsGuid == true ||
											source.EntityReference?.SerializeAsGuid == true;
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
	/// Shows a value the parser could not make sense of, verbatim and editable. The slot still
	/// round-trips unchanged, and switching to any other kind replaces it outright.
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
				return (_choice.SelectedItem as ChoiceItem)?.Id ?? "";
			case ParameterSourceKind.InputParam:
				return (_choice.SelectedItem as ChoiceItem)?.Id ?? "";
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
				if (_pickedElement == null) return "";
				if (_byEngineGuid.Checked && _pickedElement is GameObject gameObject &&
					!string.IsNullOrEmpty(gameObject.EngineTemplateId))
					return gameObject.EngineTemplateId;
				return _pickedElement.Id.ToString();
			case ParameterSourceKind.Hierarchy:
				return _pickedHierarchy?.Write() ?? "";
			case ParameterSourceKind.GlobalList:
				return _literal.Text;
			case ParameterSourceKind.Raw:
				return _literal.Text;
			default:
				return "";
		}
	}

	private string CurrentLiteral() =>
		_enum.Visible ? (_enum.SelectedItem as ChoiceItem)?.Id ?? "" : _literal.Text;

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

	private static string DescribeHierarchy(HierarchyGuid? hierarchy) {
		if (hierarchy == null) return "";
		var path = string.Join(" → ", hierarchy.Elements.Select(e => VmElementPicker.Describe(e.Element)));
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
	/// The kinds worth offering for the slot's declared type, most apt first. Local variables
	/// come before literals because a slot that can be filled from scope usually should be,
	/// and Raw stays last as the escape hatch for values the editor cannot model.
	/// </summary>
	private IEnumerable<ParameterSourceKind> OfferedKinds() {
		var type = _expectedType;
		var isRef = IsElementLike(type);
		var isNumeric = type?.BaseType is VmType.Int32 or VmType.Single or VmType.UInt64;
		var isUntyped = type == null || type.BaseType == VmType.Unknown;

		// The type-appropriate way to fill the slot leads.
		if (isRef) {
			yield return ParameterSourceKind.ObjectRef;
			yield return ParameterSourceKind.Hierarchy;
		} else {
			yield return ParameterSourceKind.Literal;
		}

		// Then whatever the action can actually reach, which is usually the right answer and
		// is offered only where the scope walk found something.
		if (_scope.Messages.Count > 0) yield return ParameterSourceKind.Message;
		if (_scope.InputParams.Count > 0) yield return ParameterSourceKind.InputParam;
		if (_scope.LoopVariables.Any(l => l.IsIndex)) yield return ParameterSourceKind.LoopIndex;
		if (_scope.LoopVariables.Any(l => !l.IsIndex)) yield return ParameterSourceKind.LoopElement;

		// Parameters can stand in for any slot, so they are always available.
		yield return ParameterSourceKind.ParameterRef;
		yield return ParameterSourceKind.DynamicParameter;

		if (isRef) yield return ParameterSourceKind.Literal;
		if (type?.BaseType == VmType.List) yield return ParameterSourceKind.GlobalList;
		if (isNumeric || isUntyped) yield return ParameterSourceKind.Constant;
		if (!isRef) {
			yield return ParameterSourceKind.ObjectRef;
			yield return ParameterSourceKind.Hierarchy;
		}

		yield return ParameterSourceKind.Empty;
		yield return ParameterSourceKind.Raw;
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
		var enumType = EnumTypeOf(_expectedType);
		var literalIsEnum = enumType != null && kind == ParameterSourceKind.Literal;

		_literal.Visible = !literalIsEnum && kind is ParameterSourceKind.Literal or ParameterSourceKind.Constant
			or ParameterSourceKind.GlobalList or ParameterSourceKind.Raw;
		_enum.Visible = literalIsEnum;
		_choice.Visible = kind is ParameterSourceKind.Message or ParameterSourceKind.InputParam
			or ParameterSourceKind.LoopIndex or ParameterSourceKind.LoopElement;
		_reference.Visible = kind is ParameterSourceKind.ParameterRef or ParameterSourceKind.DynamicParameter
			or ParameterSourceKind.ObjectRef or ParameterSourceKind.Hierarchy;

		_extra.Visible = kind == ParameterSourceKind.DynamicParameter;
		_byEngineGuid.Visible = kind == ParameterSourceKind.ObjectRef && SupportsEngineGuid(_expectedType);
		_extraHost.Visible = _extra.Visible || _byEngineGuid.Visible;

		_pick.Visible = _reference.Visible;

		if (_enum.Visible && _enum.Items.Count == 0) PopulateEnum(enumType!);
		if (_choice.Visible) PopulateChoices(kind);

		_literal.PlaceholderText = kind switch {
			ParameterSourceKind.GlobalList => "global list name",
			ParameterSourceKind.Raw => "verbatim value",
			ParameterSourceKind.Constant => "integer",
			_ => _expectedType?.Serialize() ?? ""
		};
	}

	private void PopulateEnum(Type enumType) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_enum.Items.Clear();
			foreach (var value in Enum.GetValues(enumType).Cast<Enum>())
				_enum.Items.Add(new ChoiceItem(SerializeEnum(value), value.ToString()));
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private static string SerializeEnum(Enum value) {
		try {
			return value.Serialize();
		} catch {
			return value.ToString();
		}
	}

	private void PopulateChoices(ParameterSourceKind kind) {
		var selected = (_choice.SelectedItem as ChoiceItem)?.Id;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_choice.Items.Clear();
			switch (kind) {
				case ParameterSourceKind.Message:
					foreach (var message in OrderByTypeFit(_scope.Messages, m => m.Type))
						_choice.Items.Add(new ChoiceItem(message.Name,
							$"{message.ParamName}   [{message.Type}]   ← {message.Event.Name}", message));
					break;
				case ParameterSourceKind.InputParam:
					foreach (var inputParam in OrderByTypeFit(_scope.InputParams, p => p.Type))
						_choice.Items.Add(new ChoiceItem(inputParam.Name,
							$"{inputParam.ParamName}   [{inputParam.Type}]   ← {inputParam.Graph.Name}", inputParam));
					break;
				case ParameterSourceKind.LoopIndex:
					foreach (var loop in _scope.LoopVariables.Where(l => l.IsIndex))
						_choice.Items.Add(new ChoiceItem(loop.ParamId, $"index of {loop.ActionLine.Name}", loop));
					break;
				case ParameterSourceKind.LoopElement:
					foreach (var loop in _scope.LoopVariables.Where(l => !l.IsIndex))
						_choice.Items.Add(new ChoiceItem(loop.ParamId,
							$"element of {loop.ListName} in {loop.ActionLine.Name}", loop));
					break;
			}

			if (selected != null) SelectById(_choice, selected);
			if (_choice.SelectedIndex < 0 && _choice.Items.Count > 0) _choice.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// Everything in scope stays selectable; the ones whose declared type matches the slot
	/// simply come first. Type agreement here is a hint — the engine coerces freely — so it
	/// orders the list rather than censoring it.
	/// </summary>
	private IEnumerable<T> OrderByTypeFit<T>(IEnumerable<T> items, Func<T, string> typeOf) {
		var expected = _expectedType;
		if (expected == null || expected.BaseType == VmType.Unknown) return items;
		return items.OrderByDescending(i => Fits(typeOf(i), expected)).ToList();
	}

	private bool Fits(string xmlType, VmTypeInfo expected) {
		try {
			return VmTypeHelper.GetVmTypeInfo(xmlType, _vm).BaseType == expected.BaseType;
		} catch {
			return false;
		}
	}

	// ---------------------------------------------------------------- picking

	private void Pick() {
		switch (SelectedKind) {
			case ParameterSourceKind.ParameterRef:
				PickElement("Select parameter", _vm.GetElementsByType<Parameter>());
				break;
			case ParameterSourceKind.DynamicParameter:
				// The prefix names an object-valued parameter; the text box names the
				// parameter to read off whatever object it points at.
				PickElement("Select object parameter", _vm.GetElementsByType<Parameter>());
				break;
			case ParameterSourceKind.ObjectRef:
				PickElement("Select object", ObjectCandidates());
				break;
			case ParameterSourceKind.Hierarchy:
				PickHierarchy();
				break;
		}
	}

	private void PickElement(string title, IEnumerable<VmElement> candidates) {
		if (!VmElementPicker.TryPick(FindForm(), title, candidates, VmElementPicker.Describe, _pickedElement,
				out var picked))
			return;
		_pickedElement = picked;
		_reference.Text = VmElementPicker.Describe(picked);
		OnUserEdit(null);
	}

	/// <summary>
	/// A hierarchy is a path of nested scene objects. It is picked leaf-first and the path is
	/// then read off the element's own parent chain, which is the only spelling the loader
	/// accepts anyway.
	/// </summary>
	private void PickHierarchy() {
		if (!VmElementPicker.TryPick(FindForm(), "Select hierarchy leaf", HierarchyCandidates(),
				VmElementPicker.Describe, _pickedHierarchy?.Elements[^1].Element, out var leaf))
			return;

		if (leaf == null) {
			_pickedHierarchy = null;
			_reference.Text = "";
			OnUserEdit(null);
			return;
		}

		var path = new List<ulong>();
		var current = leaf as ParameterHolder;
		for (var guard = 0; guard < 32 && current != null; guard++) {
			path.Insert(0, current.Id);
			current = current.Parent;
		}
		if (path.Count == 0) path.Add(leaf.Id);

		// A single-element path is not a hierarchy — LooksLikeHierarchy needs a separator —
		// so it is emitted as "<id>H<id>" only when there really is a chain.
		var text = string.Join("H", path);
		if (path.Count == 1) text = $"{path[0]}H{path[0]}";

		HierarchyGuid.TryParse(text, _vm, out _pickedHierarchy);
		_reference.Text = DescribeHierarchy(_pickedHierarchy);
		OnUserEdit(null);
	}

	private IEnumerable<VmElement> ObjectCandidates() {
		var type = _expectedType;
		if (type?.BaseType is VmType.BlueprintRef or VmType.BlueprintRefStorable)
			return _vm.GetElementsByType<GameObject>().Where(o => o is Item or Other or Character);

		var systemType = type == null ? null : VmTypeHelper.GetSystemType(type.BaseType);
		if (systemType != null && typeof(VmElement).IsAssignableFrom(systemType) && systemType != typeof(GameObject))
			return _vm.ElementsById.Values.Where(element => systemType.IsInstanceOfType(element));

		return _vm.GetElementsByType<ParameterHolder>();
	}

	private IEnumerable<VmElement> HierarchyCandidates() =>
		_vm.GetElementsByType<ParameterHolder>().Where(h => h is Scene or Geom or Other or Item);

	// ---------------------------------------------------------------- helpers

	private static bool SupportsEngineGuid(VmTypeInfo? type) =>
		type?.BaseType is VmType.BlueprintRef or VmType.BlueprintRefStorable or VmType.EntityRef;

	private static bool IsElementLike(VmTypeInfo? type) {
		if (type == null) return false;
		if (type.BaseType is VmType.GameObject or VmType.EntityRef or VmType.BlueprintRef
			or VmType.BlueprintRefStorable)
			return true;
		var systemType = VmTypeHelper.GetSystemType(type.BaseType);
		return systemType != null && typeof(VmElement).IsAssignableFrom(systemType);
	}

	private static Type? EnumTypeOf(VmTypeInfo? type) {
		if (type == null) return null;
		var systemType = VmTypeHelper.GetSystemType(type.BaseType);
		return systemType is { IsEnum: true } ? systemType : null;
	}

	private static void SelectById(ComboBox box, string id) {
		for (var i = 0; i < box.Items.Count; i++) {
			if (box.Items[i] is ChoiceItem item && item.Id == id) {
				box.SelectedIndex = i;
				return;
			}
		}
	}

	private static void SelectByTag(ComboBox box, object? tag) {
		if (tag == null) return;
		for (var i = 0; i < box.Items.Count; i++) {
			if (box.Items[i] is ChoiceItem item && Equals(item.Tag, tag)) {
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
			ParameterSourceKind.DynamicParameter => "Parameter by name on object",
			ParameterSourceKind.ObjectRef => "Object reference",
			ParameterSourceKind.Hierarchy => "Scene hierarchy",
			ParameterSourceKind.GlobalList => "Global list",
			ParameterSourceKind.Raw => "Raw text",
			_ => Kind.ToString()
		};
	}

	private sealed class ChoiceItem(string id, string label, object? tag = null) {
		public string Id { get; } = id;
		public object? Tag { get; } = tag;
		public override string ToString() => label;
	}
}
