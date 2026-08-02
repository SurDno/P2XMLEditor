using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using Message = P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Message;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Picks the object an action runs against.
///
/// Same shape as <see cref="ParameterSourceEditor"/> — kinds on the left, a value control
/// that follows the kind, values composed into the wire string and handed back through
/// <see cref="TargetObject.Read"/> — but the kind set is fixed by
/// <see cref="TargetObjectKind"/> rather than derived from a type, and the message and
/// input-param entries again come from <see cref="ActionScope"/> so they resolve.
/// </summary>
public sealed class TargetObjectEditor : UserControl {
	public const int PreferredHeight = 30;

	private readonly VirtualMachine _vm;
	private readonly ActionScope _scope;

	private readonly ComboBox _kind;
	private readonly Panel _valueHost;
	private readonly ComboBox _choice;
	private readonly TextBox _reference;
	private readonly Button _pick;

	private VmElement? _pickedElement;
	private HierarchyGuid? _pickedHierarchy;
	private string? _storedChoiceId;
	private string _originalText = "";
	private bool _dirty;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	public TargetObjectEditor(VirtualMachine vm, ActionScope scope) {
		_vm = vm;
		_scope = scope;

		Height = PreferredHeight;
		Margin = new Padding(0, 2, 0, 2);

		_kind = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false,
			Margin = new Padding(0, 0, 6, 0)
		};
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_choice = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false
		};
		_choice.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_reference = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };

		_valueHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
		_valueHost.Controls.AddRange([_choice, _reference]);

		_pick = new Button { Dock = DockStyle.Fill, Text = "Select…", Margin = Padding.Empty };
		_pick.Click += (_, _) => Pick();

		var layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.Controls.Add(_kind, 0, 0);
		layout.Controls.Add(_valueHost, 1, 0);
		layout.Controls.Add(_pick, 2, 0);

		Controls.Add(layout);

		PopulateKinds();
		UpdateVisibleControls();
	}

	public string SerializedValue => _dirty ? Compose() : _originalText;

	/// <summary>The holder the target resolves to, or null when it is only known at runtime.</summary>
	public ParameterHolder? ResolvedHolder {
		get {
			try {
				return Value.ResolvedHolder;
			} catch {
				return null;
			}
		}
	}

	/// <summary>
	/// The object whose parameters can be listed for this target: the concrete one where there
	/// is one, otherwise the single blueprint an indirect target is pinned to by its declared
	/// type. <see cref="IsConcreteTarget"/> tells the two apart.
	/// </summary>
	public ParameterHolder? EffectiveHolder {
		get {
			try {
				var value = Value;
				return value.ResolvedHolder ?? ActionScope.PinnedBlueprint(value, _vm);
			} catch {
				return null;
			}
		}
	}

	/// <summary>True when the target names an object outright rather than resolving to one.</summary>
	public bool IsConcreteTarget => ResolvedHolder != null;

	/// <summary>
	/// Components callable on the current target, or null when nothing constrains it.
	/// </summary>
	public IReadOnlySet<string>? ResolvedComponents {
		get {
			try {
				return ActionScope.ComponentsOfTarget(Value, _vm);
			} catch {
				return null;
			}
		}
	}

	/// <summary>
	/// TryRead, not Read: the form asks for this on every keystroke and about a target the user
	/// has not chosen yet, and an unfinished edit is not something to report as bad data.
	/// </summary>
	public TargetObject Value {
		get {
			try {
				return TargetObject.TryRead(SerializedValue, _vm, out var target, _scope.LocalContext)
					? target
					: default;
			} catch {
				return default;
			}
		}
	}

	public void Load(TargetObject target, string? rawText = null) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = rawText ?? SafeWrite(target);
			_dirty = false;

			// Read before the lists are built: a stored choice the filters would leave out is
			// still listed, so opening an action and saving it cannot move its target.
			_storedChoiceId = target.Kind switch {
				TargetObjectKind.Message => target.Message?.Name,
				TargetObjectKind.InputParam => target.InputParam?.Name,
				TargetObjectKind.Loop => target.Loop?.ParamId,
				_ => null
			};

			SelectKind(target.Kind);
			UpdateVisibleControls();

			switch (target.Kind) {
				case TargetObjectKind.Holder:
					_pickedElement = target.Holder;
					_reference.Text = DescribeHolder(target.Holder);
					break;
				case TargetObjectKind.ParameterRef:
					_pickedElement = target.ParameterRef;
					_reference.Text = VmElementPicker.DescribeDetailed(target.ParameterRef, _vm);
					break;
				case TargetObjectKind.Hierarchy:
					_pickedHierarchy = target.Hierarchy;
					_reference.Text = DescribeHierarchy(target.Hierarchy);
					break;
				case TargetObjectKind.Message:
					SelectById(_choice, target.Message?.Name ?? "");
					break;
				case TargetObjectKind.InputParam:
					SelectById(_choice, target.InputParam?.Name ?? "");
					break;
				case TargetObjectKind.Loop:
					SelectById(_choice, target.Loop?.ParamId ?? "");
					break;
			}

			UpdateVisibleControls();
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	public TargetObjectKind SelectedKind =>
		_kind.SelectedItem is KindItem item ? item.Kind : TargetObjectKind.Holder;

	private string Compose() {
		var value = SelectedKind switch {
			TargetObjectKind.Holder => ComposeHolder(),
			TargetObjectKind.ParameterRef => _pickedElement is Parameter p ? p.Id.ToString() : "",
			TargetObjectKind.Hierarchy => _pickedHierarchy?.Write() ?? "",
			TargetObjectKind.Message => (_choice.SelectedItem as ChoiceItem)?.Id ?? "",
			TargetObjectKind.InputParam => (_choice.SelectedItem as ChoiceItem)?.Id ?? "",
			TargetObjectKind.Loop => (_choice.SelectedItem as ChoiceItem)?.Id ?? "",
			_ => ""
		};

		// The data sometimes spells a target "<id>%<id>". TargetObject.Read understands it and
		// an untouched value still round-trips through _originalText, but it makes no
		// difference to the engine, so nothing the user edits is written that way again.
		return value;
	}

	/// <summary>
	/// Always by id. An engine GUID names the same object and the data uses it in places, so
	/// an untouched value still round-trips through _originalText, but there is no reason to
	/// author a new one that way.
	/// </summary>
	private string ComposeHolder() => _pickedElement is ParameterHolder holder ? holder.Id.ToString() : "";

	private static string SafeWrite(TargetObject target) {
		try {
			return target.Write();
		} catch {
			return "";
		}
	}

	private string DescribeHierarchy(HierarchyGuid? hierarchy) =>
		hierarchy == null
			? ""
			: $"{string.Join(" → ", hierarchy.Elements.Select(e => VmElementPicker.DescribeDetailed(e.Element, _vm)))}   ({hierarchy.Write()})";

	private void PopulateKinds() {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_kind.Items.Clear();
			_kind.Items.Add(new KindItem(TargetObjectKind.Holder));
			_kind.Items.Add(new KindItem(TargetObjectKind.Hierarchy));
			_kind.Items.Add(new KindItem(TargetObjectKind.ParameterRef));
			if (ObjectValuedMessages().Any()) _kind.Items.Add(new KindItem(TargetObjectKind.Message));
			if (ObjectValuedInputParams().Any()) _kind.Items.Add(new KindItem(TargetObjectKind.InputParam));
			if (LoopElements().Any()) _kind.Items.Add(new KindItem(TargetObjectKind.Loop));
			_kind.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private void SelectKind(TargetObjectKind kind) {
		for (var i = 0; i < _kind.Items.Count; i++) {
			if (_kind.Items[i] is KindItem item && item.Kind == kind) {
				_kind.SelectedIndex = i;
				return;
			}
		}
		// A stored kind whose scope list came back empty still has to be selectable.
		_kind.Items.Insert(0, new KindItem(kind));
		_kind.SelectedIndex = 0;
	}

	private void UpdateVisibleControls() {
		var kind = SelectedKind;

		// Derived from a local boolean, never read back off Control.Visible, which reports
		// false for anything whose parent chain is not shown yet — reading it during
		// construction would leave the picker hidden until the user changed the dropdown.
		var showChoice = kind is TargetObjectKind.Message or TargetObjectKind.InputParam or TargetObjectKind.Loop;

		_choice.Visible = showChoice;
		_reference.Visible = !showChoice;
		_pick.Visible = !showChoice;

		if (showChoice) PopulateChoices(kind);
	}

	private void PopulateChoices(TargetObjectKind kind) {
		var selected = (_choice.SelectedItem as ChoiceItem)?.Id ?? _storedChoiceId;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_choice.Items.Clear();
			var listed = false;
			switch (kind) {
				case TargetObjectKind.Message:
					foreach (var message in ObjectValuedMessages()) {
						_choice.Items.Add(new ChoiceItem(message.Name,
							$"{message.ParamName}   [{message.Type}]   ← {message.Event.Name}"));
						listed |= message.Name == _storedChoiceId;
					}
					break;
				case TargetObjectKind.InputParam:
					foreach (var inputParam in ObjectValuedInputParams()) {
						_choice.Items.Add(new ChoiceItem(inputParam.Name,
							$"{inputParam.ParamName}   [{inputParam.Type}]   ← {inputParam.Graph.Name}"));
						listed |= inputParam.Name == _storedChoiceId;
					}
					break;
				case TargetObjectKind.Loop:
					foreach (var loop in LoopElements()) {
						_choice.Items.Add(new ChoiceItem(loop.ParamId,
							$"element of {loop.ListName} in {loop.ActionLine.Name}"));
						listed |= loop.ParamId == _storedChoiceId;
					}
					break;
			}

			// What the action already says stays selectable even where the filter would have
			// left it out, so merely opening and saving cannot move the target.
			if (_storedChoiceId != null && !listed && KindOfStoredChoice() == kind)
				_choice.Items.Insert(0, new ChoiceItem(_storedChoiceId, $"{_storedChoiceId}   (does not hold an object)"));

			if (selected != null) SelectById(_choice, selected);
			if (_choice.SelectedIndex < 0 && _choice.Items.Count > 0) _choice.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private TargetObjectKind KindOfStoredChoice() =>
		_storedChoiceId == null ? TargetObjectKind.Holder
		: _storedChoiceId.Contains("_message_") ? TargetObjectKind.Message
		: _storedChoiceId.Contains("_inputparam_") ? TargetObjectKind.InputParam
		: TargetObjectKind.Loop;

	/// <summary>
	/// The action runs against an object, so only a variable that holds one can name it.
	///
	/// Every local variable named as a target across both corpora agrees: all 41 input-param
	/// references and all 21 message references that resolve are declared IObjRef, and all 93
	/// loop references are the element rather than the index — which is what the types say
	/// anyway, an index being an Int32 with no object behind it.
	/// </summary>
	private IEnumerable<Message> ObjectValuedMessages() =>
		_scope.Messages.Where(m => VmTypeCompatibility.IsObjectValued(m.Type, _vm));

	private IEnumerable<InputParameter> ObjectValuedInputParams() =>
		_scope.InputParams.Where(p => VmTypeCompatibility.IsObjectValued(p.Type, _vm));

	private IEnumerable<LoopParameter> LoopElements() => _scope.LoopVariables.Where(l => !l.IsIndex);

	private void Pick() {
		switch (SelectedKind) {
			case TargetObjectKind.Holder:
				PickElement("Select object", _vm.AllParameterHolders(), BareIdNote);
				break;
			case TargetObjectKind.ParameterRef:
				// The action runs against whatever the parameter points at, so only a
				// parameter that actually holds an object reference can stand here — and an
				// expression's constant holds a literal, not an object.
				PickElement("Select object parameter",
					_vm.GetElementsByType<Parameter>()
						.Where(p => !p.IsConstant && VmTypeCompatibility.IsObjectValued(p.Type, _vm)));
				break;
			case TargetObjectKind.Hierarchy:
				PickHierarchy();
				break;
		}
	}

	/// <summary>
	/// Why an id would not reach this object when the action runs — see
	/// <see cref="BareIdReach"/>. Nothing is filtered out on the strength of it: the answer
	/// depends on where the action lives, which is exactly why it is shown rather than enforced.
	/// </summary>
	private string? BareIdNote(VmElement element) =>
		BareIdReach.Problem(element as ParameterHolder, _scope.Owner, _vm);

	/// <summary>
	/// A chosen object, carrying the warning where an id cannot reach it at runtime. Shown on
	/// the target itself and not only inside the picker, so an action that already names an
	/// unreachable object says so on sight rather than only while it is being re-chosen.
	/// </summary>
	private string DescribeHolder(VmElement? element) {
		var text = VmElementPicker.DescribeDetailed(element, _vm);
		var problem = element == null ? null : BareIdNote(element);
		return problem == null ? text : $"{text}   ⚠ {problem}";
	}

	private void PickElement(string title, IEnumerable<VmElement> candidates,
		Func<VmElement, string?>? note = null) {
		if (!VmElementPicker.TryPick(FindForm(), title, candidates, e => VmElementPicker.Describe(e, _vm), _pickedElement,
				out var picked, note))
			return;
		_pickedElement = picked;
		_reference.Text = SelectedKind == TargetObjectKind.Holder
			? DescribeHolder(picked)
			: VmElementPicker.DescribeDetailed(picked, _vm);
		OnUserEdit(null);
	}

	/// <summary>
	/// Picked as a placement, not as an object: see <see cref="HierarchyPicker"/>. The old path
	/// was read off the object's own parent chain, which is the VM ownership tree and matches
	/// nothing the engine builds — of the 2673 hierarchy guids in the Sandbox, none equals its
	/// leaf's parent chain.
	/// </summary>
	private void PickHierarchy() {
		if (!HierarchyPicker.TryPick(FindForm(), _vm, "Select a place in the world", _pickedHierarchy, out var picked))
			return;

		_pickedHierarchy = picked;
		_reference.Text = DescribeHierarchy(picked);
		OnUserEdit(null);
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
		_dirty = true;
		before?.Invoke();
		ValueChanged?.Invoke(this, EventArgs.Empty);
	}

	private sealed class KindItem(TargetObjectKind kind) {
		public TargetObjectKind Kind { get; } = kind;
		public override string ToString() => Kind switch {
			TargetObjectKind.Holder => "Object",
			TargetObjectKind.ParameterRef => "Object held by parameter",
			TargetObjectKind.Hierarchy => "Scene hierarchy",
			TargetObjectKind.Loop => "Loop variable",
			TargetObjectKind.InputParam => "Graph input param",
			TargetObjectKind.Message => "Event message",
			_ => Kind.ToString()
		};
	}

	private sealed class ChoiceItem(string id, string label) {
		public string Id { get; } = id;
		public override string ToString() => label;
	}
}
