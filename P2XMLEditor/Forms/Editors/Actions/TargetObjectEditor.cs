using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

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
	private readonly CheckBox _byEngineGuid;
	private readonly Button _pick;

	private VmElement? _pickedElement;
	private HierarchyGuid? _pickedHierarchy;
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

		_byEngineGuid = new CheckBox { Dock = DockStyle.Left, Text = "GUID", AutoSize = true };
		_byEngineGuid.CheckedChanged += (_, _) => OnUserEdit(null);

		var flags = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
		flags.Controls.Add(_byEngineGuid);

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
		layout.Controls.Add(flags, 2, 0);
		layout.Controls.Add(_pick, 3, 0);

		Controls.Add(layout);

		PopulateKinds();
		UpdateVisibleControls();
	}

	public string SerializedValue => _dirty ? Compose() : _originalText;

	/// <summary>The holder the target resolves to, or null when it is only known at runtime.</summary>
	public ParameterHolder? ResolvedHolder {
		get {
			try {
				return TargetObject.Read(SerializedValue, _vm, _scope.LocalContext).ResolvedHolder;
			} catch {
				return null;
			}
		}
	}

	public TargetObject Value {
		get {
			try {
				return TargetObject.Read(SerializedValue, _vm, _scope.LocalContext);
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
			_byEngineGuid.Checked = target.ByEngineGuid;

			SelectKind(target.Kind);
			UpdateVisibleControls();

			switch (target.Kind) {
				case TargetObjectKind.Holder:
					_pickedElement = target.Holder;
					_reference.Text = VmElementPicker.Describe(target.Holder);
					break;
				case TargetObjectKind.ParameterRef:
					_pickedElement = target.ParameterRef;
					_reference.Text = VmElementPicker.Describe(target.ParameterRef);
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

	private string ComposeHolder() {
		if (_pickedElement is not ParameterHolder holder) return "";
		if (_byEngineGuid.Checked && holder is GameObject gameObject &&
			!string.IsNullOrEmpty(gameObject.EngineTemplateId))
			return gameObject.EngineTemplateId;
		return holder.Id.ToString();
	}

	private static string SafeWrite(TargetObject target) {
		try {
			return target.Write();
		} catch {
			return "";
		}
	}

	private static string DescribeHierarchy(HierarchyGuid? hierarchy) =>
		hierarchy == null
			? ""
			: $"{string.Join(" → ", hierarchy.Elements.Select(e => VmElementPicker.Describe(e.Element)))}   ({hierarchy.Write()})";

	private void PopulateKinds() {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_kind.Items.Clear();
			_kind.Items.Add(new KindItem(TargetObjectKind.Holder));
			_kind.Items.Add(new KindItem(TargetObjectKind.Hierarchy));
			_kind.Items.Add(new KindItem(TargetObjectKind.ParameterRef));
			if (_scope.Messages.Count > 0) _kind.Items.Add(new KindItem(TargetObjectKind.Message));
			if (_scope.InputParams.Count > 0) _kind.Items.Add(new KindItem(TargetObjectKind.InputParam));
			if (_scope.LoopVariables.Count > 0) _kind.Items.Add(new KindItem(TargetObjectKind.Loop));
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
		_byEngineGuid.Visible = kind == TargetObjectKind.Holder;

		if (showChoice) PopulateChoices(kind);
	}

	private void PopulateChoices(TargetObjectKind kind) {
		var selected = (_choice.SelectedItem as ChoiceItem)?.Id;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_choice.Items.Clear();
			switch (kind) {
				case TargetObjectKind.Message:
					foreach (var message in _scope.Messages)
						_choice.Items.Add(new ChoiceItem(message.Name,
							$"{message.ParamName}   [{message.Type}]   ← {message.Event.Name}"));
					break;
				case TargetObjectKind.InputParam:
					foreach (var inputParam in _scope.InputParams)
						_choice.Items.Add(new ChoiceItem(inputParam.Name,
							$"{inputParam.ParamName}   [{inputParam.Type}]   ← {inputParam.Graph.Name}"));
					break;
				case TargetObjectKind.Loop:
					foreach (var loop in _scope.LoopVariables)
						_choice.Items.Add(new ChoiceItem(loop.ParamId,
							loop.IsIndex
								? $"index of {loop.ActionLine.Name}"
								: $"element of {loop.ListName} in {loop.ActionLine.Name}"));
					break;
			}

			if (selected != null) SelectById(_choice, selected);
			if (_choice.SelectedIndex < 0 && _choice.Items.Count > 0) _choice.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private void Pick() {
		switch (SelectedKind) {
			case TargetObjectKind.Holder:
				PickElement("Select object", _vm.GetElementsByType<ParameterHolder>());
				break;
			case TargetObjectKind.ParameterRef:
				// An IObjRef-valued parameter: the action runs against whatever it points at.
				PickElement("Select object parameter", _vm.GetElementsByType<Parameter>());
				break;
			case TargetObjectKind.Hierarchy:
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

	private void PickHierarchy() {
		var candidates = _vm.GetElementsByType<ParameterHolder>().Where(h => h is Scene or Geom or Other or Item);
		if (!VmElementPicker.TryPick(FindForm(), "Select hierarchy leaf", candidates, VmElementPicker.Describe,
				_pickedHierarchy?.Elements[^1].Element, out var leaf))
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

		var text = path.Count == 1 ? $"{path[0]}H{path[0]}" : string.Join("H", path);
		HierarchyGuid.TryParse(text, _vm, out _pickedHierarchy);
		_reference.Text = DescribeHierarchy(_pickedHierarchy);
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
