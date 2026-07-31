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
using P2XMLEditor.Helper;

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
/// The parameter list is filtered to the function's declared return type, and is rebuilt from
/// scratch whenever the destination object changes — a parameter left selected from the
/// previous object would resolve to nothing at runtime.
/// </summary>
public sealed class ResultTargetEditor : UserControl {
	public const int PreferredHeight = 30;

	private readonly VirtualMachine _vm;
	private readonly ActionScope _scope;

	private readonly CheckBox _store;
	private readonly TextBox _objectDisplay;
	private readonly Button _pickObject;
	private readonly ComboBox _parameter;
	private readonly Label _hint;
	private readonly Panel _parameterHost;

	private ParameterHolder? _object;
	private VmTypeInfo? _expectedType;
	private string? _pendingParameterId;
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

		_store = new CheckBox {
			Dock = DockStyle.Fill, Text = "Store result", AutoSize = false, Margin = new Padding(0, 0, 6, 0)
		};
		_store.CheckedChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

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

		_parameterHost = new Panel { Dock = DockStyle.Fill, Margin = Padding.Empty };
		_parameterHost.Controls.AddRange([_parameter, _hint]);

		var layout = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 1,
			Margin = Padding.Empty, Padding = Padding.Empty
		};
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 86));
		layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
		layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		layout.Controls.Add(_store, 0, 0);
		layout.Controls.Add(_objectDisplay, 1, 0);
		layout.Controls.Add(_pickObject, 2, 0);
		layout.Controls.Add(_parameterHost, 3, 0);

		Controls.Add(layout);
		UpdateVisibleControls();
	}

	/// <summary>Raised when the store checkbox is toggled, so the row label can grey out with it.</summary>
	public bool StoresResult => _store.Checked;

	/// <summary>The function's return type. Setting it refilters the parameter list.</summary>
	public VmTypeInfo? ExpectedType {
		get => _expectedType;
		set {
			if (Equals(Describe(_expectedType), Describe(value))) return;
			_expectedType = value;
			// A destination chosen for the previous return type is not a destination for this
			// one, so the list is rebuilt and only a still-valid selection survives.
			UpdateVisibleControls();
		}
	}

	public string SerializedValue => _dirty ? Compose() : _originalText;

	public ParamTarget Value =>
		ParamTarget.TryRead(SerializedValue, _vm, out var target) ? target : ParamTarget.Empty();

	/// <summary>
	/// Why the current destination cannot be saved, or null when it is fine. Storing nowhere
	/// is always fine; storing into nothing is not.
	/// </summary>
	public string? ValidationError {
		get {
			if (!_store.Checked) return null;
			if (_object == null) return "There is no object to store the function result on.";
			if (SelectedParameterId == null)
				return $"Choose a parameter on {_object.Name} to store the result in, or clear \"Store result\".";
			return null;
		}
	}

	private string? SelectedParameterId => (_parameter.SelectedItem as ParameterItem)?.Id;

	public void Load(ParamTarget target, string? rawText = null) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = rawText ?? SafeWrite(target);
			_dirty = false;

			// An absent context means the owner, which is what the engine assumes.
			_object = target.ContextHolder ?? _scope.Owner;
			_pendingParameterId = target.Parameter?.Id.ToString();
			_store.Checked = target.Kind == ParamTargetKind.Parameter;

			UpdateVisibleControls();
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private string Compose() {
		if (!_store.Checked) return "%";
		var id = SelectedParameterId;
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
		var storing = _store.Checked;
		if (storing) PopulateParameters();

		var hasParameters = storing && _parameter.Items.Count > 0;

		// Everything but the checkbox greys out rather than disappearing, so the row keeps its
		// shape and it stays obvious what turning the checkbox on would let you set.
		_objectDisplay.Enabled = storing;
		_pickObject.Enabled = storing;
		_parameter.Enabled = storing;

		_parameter.Visible = !storing || hasParameters;
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
		var wanted = SelectedParameterId ?? _pendingParameterId;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_parameter.Items.Clear();
			foreach (var parameter in CompatibleParameters())
				_parameter.Items.Add(new ParameterItem(parameter));

			// Only a destination that is genuinely in the new list is restored. Anything else
			// — a parameter of the old object, or one the return type does not fit — is left
			// unselected so it has to be chosen again rather than saved as a runtime error.
			if (wanted != null) SelectParameterId(wanted);
			_pendingParameterId = null;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private bool HasCompatibleParameter(ParameterHolder holder) => ParametersOf(holder).Any();

	private IEnumerable<Parameter> CompatibleParameters() => _object == null ? [] : ParametersOf(_object);

	private IEnumerable<Parameter> ParametersOf(ParameterHolder holder) {
		var standart = holder.StandartParams ?? new Dictionary<string, Parameter>();
		var custom = holder.CustomParams ?? new Dictionary<string, Parameter>();
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
		// Offering an object with no parameter of the return type would only let the user pick
		// a destination that immediately reports it has nowhere to store anything.
		var candidates = _vm.AllParameterHolders().Where(HasCompatibleParameter);
		if (!VmElementPicker.TryPick(FindForm(), "Select the object holding the result parameter",
				candidates, VmElementPicker.Describe, _object, out var picked))
			return;

		var chosen = picked as ParameterHolder ?? _scope.Owner;
		if (ReferenceEquals(chosen, _object)) return;

		// The parameter belonged to the previous object; keeping it selected would write a
		// reference that cannot resolve.
		_object = chosen;
		_pendingParameterId = null;
		_parameter.SelectedIndex = -1;
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

	private sealed class ParameterItem(Parameter parameter) {
		public string Id { get; } = parameter.Id.ToString();
		public override string ToString() => $"{parameter.Name}   [{parameter.Type}]";
	}
}
