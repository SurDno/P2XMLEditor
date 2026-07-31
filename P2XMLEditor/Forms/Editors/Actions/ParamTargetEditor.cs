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
/// Both are dropdowns: the target object is already known, so its parameters are a short list
/// rather than something to go hunting for in the 59500 the data holds.
///
/// <see cref="ResolvedType"/> is the reason this control matters beyond its own value: it is
/// the declared type of the destination, and the source editor next to it takes that as its
/// expected type, so choosing the target reshapes what the source will accept.
/// </summary>
public sealed class ParamTargetEditor : UserControl {
	public const int PreferredHeight = 30;

	private readonly VirtualMachine _vm;
	private readonly Func<ParameterHolder?> _targetHolder;

	private readonly ComboBox _kind;
	private readonly Panel _valueHost;
	private readonly ComboBox _componentParam;
	private readonly ComboBox _parameter;
	private readonly Label _hint;

	private ParameterHolder? _context;
	private Parameter? _storedParameter;
	private string? _storedParameterId;
	private string _originalText = "%";
	private bool _dirty;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	/// <param name="targetHolder">
	/// Supplies the action's current target object, so both lists can be what that object
	/// actually declares rather than everything in the game.
	/// </param>
	public ParamTargetEditor(VirtualMachine vm, Func<ParameterHolder?> targetHolder) {
		_vm = vm;
		_targetHolder = targetHolder;

		Height = PreferredHeight;
		Margin = new Padding(0, 2, 0, 2);

		_kind = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false,
			Margin = new Padding(0, 0, 6, 0)
		};
		_kind.Items.Add(new KindItem(ParamTargetKind.Empty));
		_kind.Items.Add(new KindItem(ParamTargetKind.Parameter));
		_kind.Items.Add(new KindItem(ParamTargetKind.ComponentParam));
		_kind.SelectedIndex = 0;
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_parameter = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false
		};
		_parameter.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_componentParam = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, IntegralHeight = false,
			AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems
		};
		_componentParam.SelectedIndexChanged += (_, _) => OnUserEdit(null);
		_componentParam.TextChanged += (_, _) => OnUserEdit(null);

		_hint = new Label {
			Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
			ForeColor = System.Drawing.SystemColors.GrayText,
			Text = "Select a target object first."
		};

		_valueHost = new Panel { Dock = DockStyle.Fill };
		_valueHost.Controls.AddRange([_parameter, _componentParam, _hint]);

		var layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.Controls.Add(_kind, 0, 0);
		layout.Controls.Add(_valueHost, 1, 0);

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
					var type = SelectedParameter?.Type;
					return string.IsNullOrEmpty(type) ? null : SafeTypeInfo(type);
				case ParamTargetKind.ComponentParam:
					var name = _componentParam.Text;
					if (string.IsNullOrEmpty(name)) return null;
					// The holder's own declaration is authoritative; the game-wide index is a
					// fallback for targets that are only known at runtime.
					var holder = _targetHolder();
					if (holder?.StandartParams != null && holder.StandartParams.TryGetValue(name, out var declared))
						return SafeTypeInfo(declared.Type);
					return _vm.TryResolveStandartParamType(name, out var resolved) ? resolved : null;
				default:
					return null;
			}
		}
	}

	private Parameter? SelectedParameter => (_parameter.SelectedItem as ParameterItem)?.Parameter;

	private VmTypeInfo? SafeTypeInfo(string xmlType) {
		try {
			return VmTypeHelper.GetVmTypeInfo(xmlType, _vm);
		} catch {
			return null;
		}
	}

	public void Load(ParamTarget target, string? rawText = null) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = rawText ?? SafeWrite(target);
			_dirty = false;
			_context = target.ContextHolder;

			// The stored parameter may belong to an object the target no longer names, or be a
			// placeholder for one missing from the data. Either way it is remembered and kept
			// in the list, so opening and saving never silently moves the target.
			_storedParameter = target.Parameter?.Element as Parameter;
			_storedParameterId = target.Parameter?.Id.ToString();

			SelectKind(target.Kind);
			UpdateVisibleControls();

			switch (target.Kind) {
				case ParamTargetKind.Parameter:
					if (_storedParameterId != null) SelectParameterId(_storedParameterId);
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

	/// <summary>Rebuilds both lists after the action's target object changed.</summary>
	public void RefreshForTarget() => UpdateVisibleControls();

	private string Compose() {
		switch (SelectedKind) {
			case ParamTargetKind.Empty:
				// The data writes an unset target as a bare "%", not as an empty element.
				return "%";
			case ParamTargetKind.Parameter:
				var id = (_parameter.SelectedItem as ParameterItem)?.Id;
				if (string.IsNullOrEmpty(id)) return "%";
				return _context != null ? $"{_context.Id}%{id}" : $"%{id}";
			case ParamTargetKind.ComponentParam:
				var name = _componentParam.Text;
				return name.Length == 0 ? "%" : _context != null ? $"{_context.Id}%{name}" : $"%{name}";
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
		var holder = _targetHolder();

		var wantsParameter = kind == ParamTargetKind.Parameter;
		var wantsComponent = kind == ParamTargetKind.ComponentParam;

		if (wantsParameter) PopulateParameters(holder);
		if (wantsComponent) PopulateComponentParams(holder);

		// Derived from local state, never read back off Control.Visible, which reports false
		// for anything whose parent chain is not shown yet — reading it during construction
		// would leave the dependent controls hidden until the user changed a dropdown.
		var hasParameters = wantsParameter && _parameter.Items.Count > 0;
		_parameter.Visible = hasParameters;
		_componentParam.Visible = wantsComponent;
		_hint.Visible = wantsParameter && !hasParameters;
	}

	private void PopulateParameters(ParameterHolder? holder) {
		var selected = (_parameter.SelectedItem as ParameterItem)?.Id ?? _storedParameterId;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_parameter.Items.Clear();
			var listed = false;
			foreach (var parameter in ParametersOf(holder)) {
				_parameter.Items.Add(new ParameterItem(parameter, $"{parameter.Name}   [{parameter.Type}]"));
				listed |= _storedParameterId == parameter.Id.ToString();
			}

			if (_storedParameterId != null && !listed)
				_parameter.Items.Insert(0, new ParameterItem(_storedParameter,
					_storedParameter != null
						? $"{VmElementPicker.Describe(_storedParameter)}   (not on target object)"
						: $"(missing parameter {_storedParameterId})",
					_storedParameterId));

			if (selected != null) SelectParameterId(selected);
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>Standard and custom parameters of the object, in a stable order.</summary>
	private static IEnumerable<Parameter> ParametersOf(ParameterHolder? holder) {
		if (holder == null) return [];
		var standart = holder.StandartParams ?? new Dictionary<string, Parameter>();
		var custom = holder.CustomParams ?? new Dictionary<string, Parameter>();
		return standart.Concat(custom)
			.Where(kvp => kvp.Value != null)
			.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
			.Select(kvp => kvp.Value);
	}

	private void SelectParameterId(string id) {
		for (var i = 0; i < _parameter.Items.Count; i++) {
			if (_parameter.Items[i] is ParameterItem item && item.Id == id) {
				_parameter.SelectedIndex = i;
				return;
			}
		}
	}

	private void PopulateComponentParams(ParameterHolder? holder) {
		var current = _componentParam.Text;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_componentParam.Items.Clear();
			foreach (var name in ComponentParamNames(holder))
				_componentParam.Items.Add(name);
			_componentParam.Text = current;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// The target's own standard slots when it is known, otherwise every standard slot name in
	/// the data — the fallback is broad, but a "Component.Param" target is resolved by name at
	/// runtime and narrowing it further would hide legitimate choices.
	/// </summary>
	private IEnumerable<string> ComponentParamNames(ParameterHolder? holder) {
		if (holder?.StandartParams is { Count: > 0 })
			return holder.StandartParams.Keys.OrderBy(k => k, StringComparer.Ordinal);

		return _vm.GetElementsByType<ParameterHolder>()
			.SelectMany(h => h.StandartParams?.Keys ?? Enumerable.Empty<string>())
			.Distinct(StringComparer.Ordinal)
			.OrderBy(k => k, StringComparer.Ordinal)
			.ToList();
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

	private sealed class ParameterItem {
		public Parameter? Parameter { get; }
		public string Id { get; }
		private readonly string _label;

		public ParameterItem(Parameter? parameter, string label, string? id = null) {
			Parameter = parameter;
			Id = id ?? parameter?.Id.ToString() ?? "";
			_label = label;
		}

		public override string ToString() => _label;
	}
}
