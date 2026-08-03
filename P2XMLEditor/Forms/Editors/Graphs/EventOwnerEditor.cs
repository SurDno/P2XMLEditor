using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors.Graphs;

/// <summary>
/// Edits a <see cref="GraphLink"/>'s EventObject — the object whose event fires the link.
///
/// Only the four static forms are offered, because only those subscribe. The slot is read at
/// subscribe time through GetEventOwnerDynamicContext, which binds against the graph's owning
/// blueprint before any event has fired and outside any action; a loop variable, a message or
/// an input parameter parses there and then yields no context, and the link silently never
/// subscribes. See <see cref="EventOwner"/> for the full account. The shipped data agrees —
/// all 42 408 values are empty, a holder, a placement or a parameter.
///
/// Empty is the common case and means something: the link fires on the owning FSM's own event,
/// which is 35 392 of them.
/// </summary>
public sealed class EventOwnerEditor : UserControl {
	public const int PreferredHeight = 30;
	private const int KindColumnWidth = 210;
	private const int PickColumnWidth = 86;

	private readonly VirtualMachine _vm;

	private readonly ComboBox _kind;
	private readonly TextBox _reference;
	private readonly Button _pick;
	private readonly TableLayoutPanel _layout;

	private ParameterHolder? _holder;
	private HierarchyGuid? _hierarchy;
	private Parameter? _parameter;
	private bool _isSelf;
	private bool _hasLeadingPercent;

	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	public EventOwnerEditor(VirtualMachine vm) {
		_vm = vm;

		Height = PreferredHeight;
		Margin = new Padding(0, 2, 0, 2);

		_kind = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false,
			Margin = new Padding(0, 0, 6, 0)
		};
		foreach (var kind in Kinds) _kind.Items.Add(new KindItem(kind));
		_kind.SelectedIndex = 0;
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_reference = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 0, 6, 0) };

		_pick = new Button { Dock = DockStyle.Fill, Text = "Select…", Margin = Padding.Empty };
		_pick.Click += (_, _) => Pick();

		_layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, KindColumnWidth));
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		_layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, PickColumnWidth));
		_layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		_layout.Controls.Add(_kind, 0, 0);
		_layout.Controls.Add(_reference, 1, 0);
		_layout.Controls.Add(_pick, 2, 0);

		Controls.Add(_layout);
		UpdateVisibleControls();
	}

	/// <summary>Null for "the FSM's own event", which is what an empty EventObject means.</summary>
	public EventOwner? Value {
		get => SelectedKind switch {
			EventOwnerKind.Holder when _holder != null => new EventOwner {
				Kind = EventOwnerKind.Holder, Holder = _holder,
				IsSelf = _isSelf, HasLeadingPercent = _hasLeadingPercent
			},
			EventOwnerKind.Hierarchy when _hierarchy != null => new EventOwner {
				Kind = EventOwnerKind.Hierarchy, Hierarchy = _hierarchy,
				IsSelf = _isSelf, HasLeadingPercent = _hasLeadingPercent
			},
			EventOwnerKind.ParameterRef when _parameter != null => new EventOwner {
				Kind = EventOwnerKind.ParameterRef, ParameterRef = _parameter,
				IsSelf = _isSelf, HasLeadingPercent = _hasLeadingPercent
			},
			_ => null
		};
	}

	/// <summary>The object the events on offer belong to, or null when it is not pinned here.</summary>
	public ParameterHolder? ResolvedHolder => SelectedKind switch {
		EventOwnerKind.Holder => _holder,
		EventOwnerKind.Hierarchy => _hierarchy?.Elements[^1].Element as ParameterHolder,
		EventOwnerKind.ParameterRef => PinnedByType(_parameter),
		_ => null
	};

	public void Load(EventOwner? owner) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_holder = owner?.Holder;
			_hierarchy = owner?.Hierarchy;
			_parameter = owner?.ParameterRef;
			_isSelf = owner?.IsSelf ?? false;
			_hasLeadingPercent = owner?.HasLeadingPercent ?? false;

			SelectKind(owner?.Kind ?? OwnEventKind);
			UpdateVisibleControls();
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	// ---------------------------------------------------------------- kinds

	// The "own event" case has no EventOwnerKind of its own — it is the absence of a value —
	// so it borrows a sentinel that is not a real member.
	private const EventOwnerKind OwnEventKind = (EventOwnerKind)(-1);

	private static readonly EventOwnerKind[] Kinds =
		[OwnEventKind, EventOwnerKind.Holder, EventOwnerKind.Hierarchy, EventOwnerKind.ParameterRef];

	private EventOwnerKind SelectedKind =>
		_kind.SelectedItem is KindItem item ? item.Kind : OwnEventKind;

	private void SelectKind(EventOwnerKind kind) {
		for (var i = 0; i < _kind.Items.Count; i++) {
			if (_kind.Items[i] is KindItem item && item.Kind == kind) {
				_kind.SelectedIndex = i;
				return;
			}
		}
	}

	private void UpdateVisibleControls() {
		var kind = SelectedKind;
		var needsReference = kind != OwnEventKind;

		_reference.Visible = needsReference;
		_pick.Visible = needsReference;
		_layout.ColumnStyles[2].Width = needsReference ? PickColumnWidth : 0;

		_reference.Text = kind switch {
			EventOwnerKind.Holder => VmElementPicker.DescribeDetailed(_holder, _vm),
			EventOwnerKind.Hierarchy => DescribeHierarchy(_hierarchy),
			EventOwnerKind.ParameterRef => VmElementPicker.DescribeDetailed(_parameter, _vm),
			_ => ""
		};
	}

	private string DescribeHierarchy(HierarchyGuid? hierarchy) {
		if (hierarchy == null) return "";
		var path = string.Join(" → ", hierarchy.Elements.Select(e => VmElementPicker.Describe(e.Element, _vm)));
		return $"{path}   ({hierarchy.Write()})";
	}

	// ---------------------------------------------------------------- picking

	private void Pick() {
		switch (SelectedKind) {
			case EventOwnerKind.Holder:
				if (VmElementPicker.TryPick(FindForm(), "Select the object whose event fires this link",
						_vm.AllParameterHolders(), e => VmElementPicker.Describe(e, _vm), _holder, out var holder)) {
					_holder = holder as ParameterHolder;
					OnUserEdit(UpdateVisibleControls);
				}
				break;

			case EventOwnerKind.Hierarchy:
				if (HierarchyPicker.TryPick(FindForm(), _vm, "Select a place in the world", _hierarchy, out var path)) {
					_hierarchy = path;
					OnUserEdit(UpdateVisibleControls);
				}
				break;

			case EventOwnerKind.ParameterRef:
				// Bind is called with needType = IObjRef, so only an object-valued parameter can
				// name an owner. A constant is the expression's own storage and holds no object.
				if (VmElementPicker.TryPick(FindForm(), "Select the parameter naming the owner",
						_vm.GetElementsByType<Parameter>()
							.Where(p => !p.IsConstant && VmTypeCompatibility.IsObjectValued(p.Type, _vm)),
						e => VmElementPicker.Describe(e, _vm), _parameter, out var parameter)) {
					_parameter = parameter as Parameter;
					OnUserEdit(UpdateVisibleControls);
				}
				break;
		}
	}

	/// <summary>
	/// The one object a parameter typed "IObjRef%cf_&lt;blueprintId&gt;" can hold, so the event
	/// list can still be narrowed when the owner is named indirectly.
	/// </summary>
	private ParameterHolder? PinnedByType(Parameter? parameter) {
		if (parameter == null || string.IsNullOrEmpty(parameter.Type)) return null;
		try {
			return VmTypeHelper.GetVmTypeInfo(parameter.Type, _vm).ObjBlueprint;
		} catch {
			return null;
		}
	}

	private void OnUserEdit(System.Action? before) {
		if (_suppressEvents) return;
		before?.Invoke();
		ValueChanged?.Invoke(this, EventArgs.Empty);
	}

	private sealed class KindItem(EventOwnerKind kind) {
		public EventOwnerKind Kind { get; } = kind;
		public override string ToString() => Kind switch {
			EventOwnerKind.Holder => "An object",
			EventOwnerKind.Hierarchy => "A place in the world",
			EventOwnerKind.ParameterRef => "Whatever a parameter points at",
			_ => "This FSM itself"
		};
	}
}
