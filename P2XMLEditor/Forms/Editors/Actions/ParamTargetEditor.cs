using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Picks the parameter an action writes to — either a concrete <see cref="Parameter"/> or a
/// "Component.Param" standard slot resolved by name on whatever the target object turns out
/// to be.
///
/// <see cref="ResolvedType"/> is the reason this control matters beyond its own value: it is
/// the declared type of the destination, and the source editor next to it takes that as its
/// expected type, so choosing the target reshapes what the source will accept.
/// </summary>
public sealed class ParamTargetEditor : UserControl {
	private readonly VirtualMachine _vm;
	private readonly Func<ParameterHolder?> _targetHolder;

	private readonly ComboBox _kind;
	private readonly Panel _valueHost;
	private readonly ComboBox _componentParam;
	private readonly TextBox _reference;
	private readonly Button _pick;

	private Parameter? _pickedParameter;
	private ParameterHolder? _context;
	private string _originalText = "%";
	private bool _dirty;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	/// <param name="targetHolder">
	/// Supplies the action's current target object, so the Component.Param list can be the
	/// slots that object actually has rather than every standard name in the game.
	/// </param>
	public ParamTargetEditor(VirtualMachine vm, Func<ParameterHolder?> targetHolder) {
		_vm = vm;
		_targetHolder = targetHolder;

		Height = 26;
		Margin = new Padding(0, 1, 0, 1);

		_kind = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(0, 0, 4, 0) };
		_kind.Items.Add(new KindItem(ParamTargetKind.Empty));
		_kind.Items.Add(new KindItem(ParamTargetKind.Parameter));
		_kind.Items.Add(new KindItem(ParamTargetKind.ComponentParam));
		_kind.SelectedIndex = 0;
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_componentParam = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems };
		_componentParam.SelectedIndexChanged += (_, _) => OnUserEdit(null);
		_componentParam.TextChanged += (_, _) => OnUserEdit(null);

		_reference = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };

		_valueHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 4, 0) };
		_valueHost.Controls.AddRange([_componentParam, _reference]);

		_pick = new Button { Dock = DockStyle.Fill, Text = "…" };
		_pick.Click += (_, _) => Pick();

		var layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.Controls.Add(_kind, 0, 0);
		layout.Controls.Add(_valueHost, 1, 0);
		layout.Controls.Add(_pick, 2, 0);

		Controls.Add(layout);
		UpdateVisibleControls();
	}

	public string SerializedValue => _dirty ? Compose() : _originalText;

	public ParamTarget Value =>
		ParamTarget.TryRead(SerializedValue, _vm, out var target) ? target : ParamTarget.Empty();

	/// <summary>
	/// Declared type of the destination, or null when it cannot be determined — an unset
	/// target, or a component slot no loaded object declares.
	/// </summary>
	public VmTypeInfo? ResolvedType {
		get {
			switch (SelectedKind) {
				case ParamTargetKind.Parameter:
					var type = _pickedParameter?.Type;
					return string.IsNullOrEmpty(type) ? null : VmTypeHelper.GetVmTypeInfo(type, _vm);
				case ParamTargetKind.ComponentParam:
					var name = _componentParam.Text;
					if (string.IsNullOrEmpty(name)) return null;
					// The holder's own declaration is authoritative; the game-wide index is a
					// fallback for targets that are only known at runtime.
					var holder = _targetHolder();
					if (holder?.StandartParams != null && holder.StandartParams.TryGetValue(name, out var declared))
						return VmTypeHelper.GetVmTypeInfo(declared.Type, _vm);
					return _vm.TryResolveStandartParamType(name, out var resolved) ? resolved : null;
				default:
					return null;
			}
		}
	}

	public void Load(ParamTarget target, string? rawText = null) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = rawText ?? SafeWrite(target);
			_dirty = false;
			_context = target.ContextHolder;
			_pickedParameter = target.Parameter?.Element as Parameter;

			SelectKind(target.Kind);
			UpdateVisibleControls();

			switch (target.Kind) {
				case ParamTargetKind.Parameter:
					_reference.Text = _pickedParameter != null
						? VmElementPicker.Describe(_pickedParameter)
						// A placeholder id: the parameter is referenced but absent from the data.
						: $"(missing parameter {target.Parameter?.Id})";
					break;
				case ParamTargetKind.ComponentParam:
					_componentParam.Text = target.ComponentParamName ?? "";
					break;
			}
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	public ParamTargetKind SelectedKind =>
		_kind.SelectedItem is KindItem item ? item.Kind : ParamTargetKind.Empty;

	/// <summary>Refreshes the component-slot list after the action's target object changed.</summary>
	public void RefreshComponentParams() {
		if (SelectedKind == ParamTargetKind.ComponentParam) PopulateComponentParams();
	}

	private string Compose() {
		switch (SelectedKind) {
			case ParamTargetKind.Empty:
				// The data writes an unset target as a bare "%", not as an empty element.
				return "%";
			case ParamTargetKind.Parameter:
				if (_pickedParameter == null) return "%";
				return _context != null ? $"{_context.Id}%{_pickedParameter.Id}" : $"%{_pickedParameter.Id}";
			case ParamTargetKind.ComponentParam:
				var name = _componentParam.Text;
				return name.Length == 0 ? "%" : (_context != null ? $"{_context.Id}%{name}" : $"%{name}");
			default:
				return "%";
		}
	}

	private static string SafeWrite(ParamTarget target) {
		try {
			return target.Write();
		} catch {
			return "%";
		}
	}

	private void SelectKind(ParamTargetKind kind) {
		for (var i = 0; i < _kind.Items.Count; i++) {
			if (_kind.Items[i] is KindItem item && item.Kind == kind) {
				_kind.SelectedIndex = i;
				return;
			}
		}
	}

	private void UpdateVisibleControls() {
		var kind = SelectedKind;
		_componentParam.Visible = kind == ParamTargetKind.ComponentParam;
		_reference.Visible = kind == ParamTargetKind.Parameter;
		_pick.Visible = _reference.Visible;
		if (_componentParam.Visible) PopulateComponentParams();
	}

	private void PopulateComponentParams() {
		var current = _componentParam.Text;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_componentParam.Items.Clear();
			foreach (var name in ComponentParamNames())
				_componentParam.Items.Add(name);
			_componentParam.Text = current;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// The target's own standard slots when it is known, otherwise every standard slot name
	/// in the data — the fallback is broad, but a "Component.Param" target is resolved by
	/// name at runtime and narrowing it further would hide legitimate choices.
	/// </summary>
	private IEnumerable<string> ComponentParamNames() {
		var holder = _targetHolder();
		if (holder?.StandartParams is { Count: > 0 })
			return holder.StandartParams.Keys.OrderBy(k => k, StringComparer.Ordinal);

		return _vm.GetElementsByType<ParameterHolder>()
			.SelectMany(h => h.StandartParams?.Keys ?? Enumerable.Empty<string>())
			.Distinct(StringComparer.Ordinal)
			.OrderBy(k => k, StringComparer.Ordinal)
			.ToList();
	}

	private void Pick() {
		if (SelectedKind != ParamTargetKind.Parameter) return;

		// Offer the target object's parameters when there is one; that is nearly always what
		// is wanted, and the full list stays reachable by clearing the target first.
		var holder = _targetHolder();
		IEnumerable<VmElement> candidates = holder != null
			? (holder.StandartParams?.Values ?? Enumerable.Empty<Parameter>())
				.Concat(holder.CustomParams?.Values ?? Enumerable.Empty<Parameter>())
			: _vm.GetElementsByType<Parameter>();

		if (!VmElementPicker.TryPick(FindForm(), "Select parameter", candidates, VmElementPicker.Describe,
				_pickedParameter, out var picked))
			return;

		_pickedParameter = picked as Parameter;
		_reference.Text = VmElementPicker.Describe(_pickedParameter);
		OnUserEdit(null);
	}

	private void OnUserEdit(System.Action? before) {
		if (_suppressEvents) return;
		_dirty = true;
		before?.Invoke();
		ValueChanged?.Invoke(this, EventArgs.Empty);
	}

	private sealed class KindItem(ParamTargetKind kind) {
		public ParamTargetKind Kind { get; } = kind;
		public override string ToString() => Kind switch {
			ParamTargetKind.Empty => "(none)",
			ParamTargetKind.Parameter => "Parameter",
			ParamTargetKind.ComponentParam => "Component.Param",
			_ => Kind.ToString()
		};
	}
}
