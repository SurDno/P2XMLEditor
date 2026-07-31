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

	private readonly VirtualMachine _vm;
	private readonly ActionScope _scope;
	private ParamTarget? _target;

	private readonly ComboBox _kind;
	private readonly Panel _valueHost;
	private readonly TextBox _literal;
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

		_valueHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
		_valueHost.Controls.AddRange([_literal, _choice, _reference]);

		_extra = new TextBox { Dock = DockStyle.Fill, PlaceholderText = "parameter name" };
		_extra.TextChanged += (_, _) => OnUserEdit(null);

		_byEngineGuid = new CheckBox { Dock = DockStyle.Fill, Text = "engine GUID", AutoSize = false };
		_byEngineGuid.CheckedChanged += (_, _) => OnUserEdit(null);

		_extraHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
		_extraHost.Controls.AddRange([_extra, _byEngineGuid]);

		_pick = new Button { Dock = DockStyle.Fill, Text = "Select…" };
		_pick.Click += (_, _) => Pick();

		var layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.Controls.Add(_kind, 0, 0);
		layout.Controls.Add(_valueHost, 1, 0);
		layout.Controls.Add(_extraHost, 2, 0);
		layout.Controls.Add(_pick, 3, 0);

		Controls.Add(layout);

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
				if (_pickedElement == null) return "";
				if (_byEngineGuid.Checked && _pickedElement is GameObject gameObject &&
					!string.IsNullOrEmpty(gameObject.EngineTemplateId))
					return gameObject.EngineTemplateId;
				return _pickedElement.Id.ToString();
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
	private string CurrentLiteral() =>
		LiteralIsChosen ? (_choice.SelectedItem as ChoiceItem)?.Id ?? "" : _literal.Text;

	private bool LiteralIsChosen =>
		VmTypeCompatibility.EnumTypeOf(_expectedType) != null || _expectedType?.BaseType == VmType.Boolean;

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
		// Resolved by name at runtime, so nothing can be checked here and it always applies.
		yield return ParameterSourceKind.DynamicParameter;

		// const_ is an Int32 literal the engine reads only for loop bounds.
		if (AllowConstant && (isUntyped || type!.BaseType == VmType.Int32))
			yield return ParameterSourceKind.Constant;

		if (isUntyped && !isRef) {
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
		var literalIsChosen = LiteralIsChosen && kind == ParameterSourceKind.Literal;

		// Visibility is derived from local booleans rather than read back off the controls:
		// Control.Visible reports false for anything whose parent chain is not yet shown, so
		// reading it during construction would leave the dependent controls hidden until the
		// user happened to change a dropdown.
		var showLiteral = !literalIsChosen && kind is ParameterSourceKind.Literal
			or ParameterSourceKind.Constant or ParameterSourceKind.GlobalList or ParameterSourceKind.Raw;
		var showChoice = literalIsChosen || kind is ParameterSourceKind.Message or ParameterSourceKind.InputParam
			or ParameterSourceKind.LoopIndex or ParameterSourceKind.LoopElement;
		var showReference = kind is ParameterSourceKind.ParameterRef or ParameterSourceKind.DynamicParameter
			or ParameterSourceKind.ObjectRef or ParameterSourceKind.Hierarchy;
		var showExtra = kind == ParameterSourceKind.DynamicParameter;
		var showGuid = kind == ParameterSourceKind.ObjectRef && SupportsEngineGuid(_expectedType);

		_literal.Visible = showLiteral;
		_choice.Visible = showChoice;
		_reference.Visible = showReference;
		_extra.Visible = showExtra;
		_byEngineGuid.Visible = showGuid;
		_extraHost.Visible = showExtra || showGuid;
		_pick.Visible = showReference;

		if (showChoice) PopulateChoices(kind, literalIsChosen);

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
		_scope.Messages.Where(m => VmTypeCompatibility.Accepts(_expectedType, m.Type, _vm));

	private IEnumerable<InputParameter> CompatibleInputParams() =>
		_scope.InputParams.Where(p => VmTypeCompatibility.Accepts(_expectedType, p.Type, _vm));

	/// <summary>
	/// A loop index is an Int32 and a loop element is an object; both are fixed by the loop
	/// construct rather than declared anywhere, so they are typed here.
	/// </summary>
	private IEnumerable<LoopParameter> CompatibleLoops(bool index) =>
		_scope.LoopVariables.Where(l => l.IsIndex == index &&
			VmTypeCompatibility.Accepts(_expectedType, index ? VmTypeInfo.Int32 : VmTypeInfo.GameObject));

	private IEnumerable<Parameter> CompatibleParameters() =>
		_vm.GetElementsByType<Parameter>().Where(p => VmTypeCompatibility.Accepts(_expectedType, p.Type, _vm));

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
						.Where(p => VmTypeCompatibility.Accepts(VmTypeInfo.GameObject, p.Type, _vm)));
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
		// so it is doubled to keep the spelling parseable.
		var text = path.Count == 1 ? $"{path[0]}H{path[0]}" : string.Join("H", path);

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
			ParameterSourceKind.DynamicParameter => "Parameter by name on object",
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
