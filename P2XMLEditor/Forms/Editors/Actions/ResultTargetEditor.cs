using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Where a non-void DoFunction stores its result. It writes the same TargetParam field as
/// <see cref="ParamTargetEditor"/>, but it is a separate control because it means something
/// different and binds differently.
///
/// The engine does not bind the result against the object the function was called on: it
/// ignores TargetObject here and resolves the parameter on the local context's owner. So
/// storing into the owner's own parameter is written bare as "%&lt;paramId&gt;", and storing
/// anywhere else has to name the object explicitly as "&lt;objectId&gt;%&lt;paramId&gt;".
///
/// The parameter list is filtered to the function's declared return type — an IObjRef result
/// cannot be put in a System.Int32 slot.
/// </summary>
public sealed class ResultTargetEditor : UserControl {
	public const int PreferredHeight = 30;

	private readonly VirtualMachine _vm;
	private readonly ActionScope _scope;

	private readonly ComboBox _kind;
	private readonly TextBox _objectDisplay;
	private readonly Button _pickObject;
	private readonly ComboBox _parameter;
	private readonly Label _hint;
	private readonly Panel _parameterHost;

	private ParameterHolder? _object;
	private Parameter? _storedParameter;
	private string? _storedParameterId;
	private VmTypeInfo? _expectedType;
	private string _originalText = "%";
	private bool _dirty;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	public ResultTargetEditor(VirtualMachine vm, ActionScope scope) {
		_vm = vm;
		_scope = scope;
		_object = scope.Owner;

		Height = PreferredHeight;
		Margin = new Padding(0, 2, 0, 2);

		_kind = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false,
			Margin = new Padding(0, 0, 6, 0)
		};
		_kind.Items.Add("Discard result");
		_kind.Items.Add("Store in parameter");
		_kind.SelectedIndex = 0;
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		_objectDisplay = new TextBox { Dock = DockStyle.Fill, ReadOnly = true, Margin = new Padding(0, 0, 6, 0) };

		_pickObject = new Button { Dock = DockStyle.Fill, Text = "Object…", Margin = new Padding(0, 0, 6, 0) };
		_pickObject.Click += (_, _) => PickObject();

		_parameter = new ComboBox {
			Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false
		};
		_parameter.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_hint = new Label {
			Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
			ForeColor = System.Drawing.SystemColors.GrayText
		};

		_parameterHost = new Panel { Dock = DockStyle.Fill };
		_parameterHost.Controls.AddRange([_parameter, _hint]);

		var layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.Controls.Add(_kind, 0, 0);
		layout.Controls.Add(_objectDisplay, 1, 0);
		layout.Controls.Add(_pickObject, 2, 0);
		layout.Controls.Add(_parameterHost, 3, 0);

		Controls.Add(layout);
		UpdateVisibleControls();
	}

	/// <summary>The function's return type. Setting it refilters the parameter list.</summary>
	public VmTypeInfo? ExpectedType {
		get => _expectedType;
		set {
			_expectedType = value;
			UpdateVisibleControls();
		}
	}

	public string SerializedValue => _dirty ? Compose() : _originalText;

	public ParamTarget Value =>
		ParamTarget.TryRead(SerializedValue, _vm, out var target) ? target : ParamTarget.Empty();

	public void Load(ParamTarget target, string? rawText = null) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = rawText ?? SafeWrite(target);
			_dirty = false;

			// An absent context means the owner, which is what the engine assumes.
			_object = target.ContextHolder ?? _scope.Owner;
			_storedParameter = target.Parameter?.Element as Parameter;
			_storedParameterId = target.Parameter?.Id.ToString();

			_kind.SelectedIndex = target.Kind == ParamTargetKind.Parameter ? 1 : 0;
			UpdateVisibleControls();

			if (_storedParameterId != null) SelectParameterId(_storedParameterId);
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private bool StoresResult => _kind.SelectedIndex == 1;

	private string Compose() {
		if (!StoresResult) return "%";
		var id = (_parameter.SelectedItem as ParameterItem)?.Id;
		if (string.IsNullOrEmpty(id)) return "%";

		// Bare "%<id>" already means "on the local context's owner"; naming the object is only
		// needed — and only correct — when the destination is somewhere else.
		return _object == null || ReferenceEquals(_object, _scope.Owner) ? $"%{id}" : $"{_object.Id}%{id}";
	}

	private static string SafeWrite(ParamTarget target) {
		try {
			return target.Write();
		} catch {
			return "%";
		}
	}

	private void UpdateVisibleControls() {
		var storing = StoresResult;
		if (storing) PopulateParameters();

		var hasParameters = storing && _parameter.Items.Count > 0;
		_objectDisplay.Visible = storing;
		_pickObject.Visible = storing;
		_parameter.Visible = hasParameters;
		_hint.Visible = storing && !hasParameters;
		_hint.Text = _object == null
			? "No local context owner to store on."
			: $"{_object.Name} has no parameter of type {Describe(_expectedType)}.";

		_objectDisplay.Text = _object == null
			? ""
			: ReferenceEquals(_object, _scope.Owner)
				? $"{_object.Name}   (this object)"
				: _object.Name;
	}

	private void PopulateParameters() {
		var selected = (_parameter.SelectedItem as ParameterItem)?.Id ?? _storedParameterId;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_parameter.Items.Clear();
			var listed = false;
			foreach (var parameter in CompatibleParameters()) {
				_parameter.Items.Add(new ParameterItem(parameter, $"{parameter.Name}   [{parameter.Type}]"));
				listed |= _storedParameterId == parameter.Id.ToString();
			}

			// A stored destination stays selectable even when it no longer matches the return
			// type, so simply opening the form cannot silently drop it.
			if (_storedParameterId != null && !listed)
				_parameter.Items.Insert(0, new ParameterItem(_storedParameter,
					_storedParameter != null
						? $"{_storedParameter.Name}   [{_storedParameter.Type}]   (type mismatch)"
						: $"(missing parameter {_storedParameterId})",
					_storedParameterId));

			if (selected != null) SelectParameterId(selected);
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private IEnumerable<Parameter> CompatibleParameters() {
		if (_object == null) return [];
		var standart = _object.StandartParams ?? new Dictionary<string, Parameter>();
		var custom = _object.CustomParams ?? new Dictionary<string, Parameter>();
		return standart.Concat(custom)
			.Where(kvp => kvp.Value != null)
			.OrderBy(kvp => kvp.Key, StringComparer.Ordinal)
			.Select(kvp => kvp.Value)
			.Where(p => VmTypeCompatibility.Accepts(_expectedType, p.Type, _vm));
	}

	private void SelectParameterId(string id) {
		for (var i = 0; i < _parameter.Items.Count; i++) {
			if (_parameter.Items[i] is ParameterItem item && item.Id == id) {
				_parameter.SelectedIndex = i;
				return;
			}
		}
	}

	private void PickObject() {
		if (!VmElementPicker.TryPick(FindForm(), "Select the object holding the result parameter",
				_vm.GetElementsByType<ParameterHolder>(), VmElementPicker.Describe, _object, out var picked))
			return;
		_object = picked as ParameterHolder ?? _scope.Owner;
		OnUserEdit(UpdateVisibleControls);
	}

	private static string Describe(VmTypeInfo? type) {
		try {
			return type?.Serialize() ?? "?";
		} catch {
			return "?";
		}
	}

	private void OnUserEdit(System.Action? before) {
		if (_suppressEvents) return;
		_dirty = true;
		before?.Invoke();
		ValueChanged?.Invoke(this, EventArgs.Empty);
	}

	private sealed class ParameterItem {
		public string Id { get; }
		private readonly string _label;

		public ParameterItem(Parameter? parameter, string label, string? id = null) {
			Id = id ?? parameter?.Id.ToString() ?? "";
			_label = label;
		}

		public override string ToString() => _label;
	}
}
