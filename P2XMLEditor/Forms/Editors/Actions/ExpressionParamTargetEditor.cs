using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using Message = P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Message;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// What an EXPRESSION_SRC_PARAM expression reads.
///
/// The whole address lives here, not in the expression's TargetObject.
/// <c>ExpressionUtility.CalculateExpressionResult</c> handles that kind with a single call —
/// <c>GetDynamicVariableValue(expression.TargetParam, expression.ResultType, dynamicObjContext)</c>
/// — and never touches TargetObject, which is only read for a function call. The object, when
/// one is named, is the context part of this value: <c>GetDynamicParam</c> resolves
/// <c>variable.Context</c> and falls back to the running FSM when it is empty, which is what
/// the bare "%&lt;paramId&gt;" form in 6174 of the 8411 param expressions means.
///
/// So this control has to cover every shape <see cref="ExpressionParamTarget"/> parses, not
/// just the parameter one. The other four are 1950 expressions across the two corpora — 1247
/// event messages, 350 objects written by id, 106 written as a placement, 20 graph input
/// params — and an editor that silently loaded none of them wrote an empty value over each on
/// save.
/// </summary>
public sealed class ExpressionParamTargetEditor : UserControl {
	public const int PreferredHeight = 30;
	private const int KindColumnWidth = 190;
	private const int PickColumnWidth = 86;

	private readonly VirtualMachine _vm;
	private readonly ActionScope _scope;

	private readonly ComboBox _kind;
	private readonly Panel _valueHost;
	private readonly ParamTargetEditor _parameter;
	private readonly ComboBox _choice;
	private readonly TextBox _reference;
	private readonly TextBox _raw;
	private readonly Button _pick;
	private readonly TableLayoutPanel _layout;

	private VmTypeInfo? _expectedType;
	private VmElement? _picked;
	private HierarchyGuid? _pickedHierarchy;
	private bool _byEngineGuid;
	private bool _hasLeadingPercent;

	private string _originalText = "";
	private bool _dirty;
	private bool _suppressEvents;

	public event EventHandler? ValueChanged;

	public ExpressionParamTargetEditor(VirtualMachine vm, ActionScope scope,
		Func<TargetObjectBinding> target, VmTypeInfo? expectedType = null) {
		_vm = vm;
		_scope = scope;
		_expectedType = expectedType;

		Height = PreferredHeight;
		Margin = new Padding(0, 2, 0, 2);

		_kind = NewCombo(ComboBoxStyle.DropDownList);
		_kind.Margin = new Padding(0, 0, 6, 0);
		_kind.SelectedIndexChanged += (_, _) => OnUserEdit(UpdateVisibleControls);

		// The parameter case keeps the control that already knows it, context part and all.
		_parameter = new ParamTargetEditor(vm, target) { Dock = DockStyle.Fill, ExpectedType = expectedType };
		_parameter.ValueChanged += (_, _) => OnUserEdit(null);

		_choice = NewCombo(ComboBoxStyle.DropDownList);
		_choice.SelectedIndexChanged += (_, _) => OnUserEdit(null);

		_reference = new TextBox { Dock = DockStyle.Fill, ReadOnly = true };
		_raw = new TextBox { Dock = DockStyle.Fill };
		_raw.TextChanged += (_, _) => OnUserEdit(null);

		_valueHost = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 6, 0) };
		_valueHost.Controls.AddRange([_parameter, _choice, _reference, _raw]);

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
		_layout.Controls.Add(_valueHost, 1, 0);
		_layout.Controls.Add(_pick, 2, 0);

		Controls.Add(_layout);

		PopulateKinds();
		UpdateVisibleControls();
	}

	private static ComboBox NewCombo(ComboBoxStyle style) =>
		new() { Dock = DockStyle.Fill, DropDownStyle = style, IntegralHeight = false };

	public VmTypeInfo? ExpectedType {
		get => _expectedType;
		set {
			_expectedType = value;
			_parameter.ExpectedType = value;
			var current = SelectedKind;
			PopulateKinds();
			SelectKind(current);
			UpdateVisibleControls();
		}
	}

	/// <summary>The wire string, ready for <see cref="ExpressionParamTarget.Read"/>.</summary>
	public string SerializedValue => _dirty ? Compose() : _originalText;

	public void Load(ExpressionParamTarget? target, string? rawText = null) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = rawText ?? SafeWrite(target);
			_dirty = false;
			_hasLeadingPercent = target?.HasLeadingPercent ?? false;
			_byEngineGuid = target?.ByEngineGuid ?? false;
			_picked = target?.ObjectLiteral;
			_pickedHierarchy = target?.LiteralHierarchy;

			var kind = target?.Kind ?? ExpressionParamKind.Param;
			EnsureKindOffered(kind);
			SelectKind(kind);
			UpdateVisibleControls();

			switch (kind) {
				case ExpressionParamKind.Param:
					if (target?.Param is { } param) _parameter.Load(param, _originalText);
					break;
				case ExpressionParamKind.Message:
					SelectById(_choice, target?.Message?.Name);
					break;
				case ExpressionParamKind.InputParam:
					SelectById(_choice, target?.InputParam?.Name);
					break;
				case ExpressionParamKind.ObjectLiteral:
					_reference.Text = DescribeObject();
					break;
				case ExpressionParamKind.Unresolved:
					_raw.Text = target?.UnresolvedRawString ?? _originalText;
					break;
			}

			UpdateVisibleControls();
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>Shows a value that would not parse, verbatim and editable.</summary>
	public void LoadRaw(string text) {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_originalText = text;
			_dirty = false;
			EnsureKindOffered(ExpressionParamKind.Unresolved);
			SelectKind(ExpressionParamKind.Unresolved);
			UpdateVisibleControls();
			_raw.Text = text;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	public ExpressionParamKind SelectedKind =>
		_kind.SelectedItem is KindItem item ? item.Kind : ExpressionParamKind.Param;

	// ---------------------------------------------------------------- composition

	private string Compose() {
		switch (SelectedKind) {
			case ExpressionParamKind.Param:
				return _parameter.SerializedValue;
			case ExpressionParamKind.Message:
			case ExpressionParamKind.InputParam:
				return WithPercent((_choice.SelectedItem as ChoiceItem)?.Id ?? "");
			case ExpressionParamKind.ObjectLiteral:
				if (_pickedHierarchy != null) return WithPercent(_pickedHierarchy.Write());
				if (_byEngineGuid && _picked is GameObject byGuid && !string.IsNullOrEmpty(byGuid.EngineTemplateId))
					return WithPercent(byGuid.EngineTemplateId);
				return _picked == null ? "" : WithPercent(_picked.Id.ToString());
			default:
				return _raw.Text;
		}
	}

	/// <summary>
	/// The leading '%' is kept as it was found. It forces an empty context in
	/// <c>CommonVariable.Read</c>, and a value that carries one and loses it would be read with
	/// its first segment taken for a context.
	/// </summary>
	private string WithPercent(string value) =>
		value.Length > 0 && _hasLeadingPercent ? "%" + value : value;

	private static string SafeWrite(ExpressionParamTarget? target) {
		try {
			return target?.Write() ?? "";
		} catch {
			return "";
		}
	}

	// ---------------------------------------------------------------- kinds

	private void PopulateKinds() {
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_kind.Items.Clear();
			foreach (var kind in OfferedKinds()) _kind.Items.Add(new KindItem(kind));
			if (_kind.Items.Count > 0) _kind.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	/// <summary>
	/// A parameter always; the scope-backed kinds only where something of a fitting type is
	/// actually in scope; an object only where the slot can take one. Unresolved is never
	/// offered — <see cref="EnsureKindOffered"/> adds it back for a value that needs it.
	/// </summary>
	private IEnumerable<ExpressionParamKind> OfferedKinds() {
		yield return ExpressionParamKind.Param;
		if (CompatibleMessages().Any()) yield return ExpressionParamKind.Message;
		if (CompatibleInputParams().Any()) yield return ExpressionParamKind.InputParam;
		if (_expectedType == null || _expectedType.BaseType == VmType.Unknown ||
			VmTypeCompatibility.IsElementLike(_expectedType))
			yield return ExpressionParamKind.ObjectLiteral;
	}

	private void EnsureKindOffered(ExpressionParamKind kind) {
		if (_kind.Items.Cast<KindItem>().Any(i => i.Kind == kind)) return;
		_kind.Items.Insert(0, new KindItem(kind));
	}

	private void SelectKind(ExpressionParamKind kind) {
		for (var i = 0; i < _kind.Items.Count; i++) {
			if (_kind.Items[i] is KindItem item && item.Kind == kind) {
				_kind.SelectedIndex = i;
				return;
			}
		}
	}

	private IEnumerable<Message> CompatibleMessages() =>
		_scope.Messages.Where(m => VmTypeCompatibility.Matches(_expectedType, m.Type, _vm));

	private IEnumerable<InputParameter> CompatibleInputParams() =>
		_scope.InputParams.Where(p => VmTypeCompatibility.Matches(_expectedType, p.Type, _vm));

	// ---------------------------------------------------------------- visibility

	private void UpdateVisibleControls() {
		var kind = SelectedKind;
		var isChoice = kind is ExpressionParamKind.Message or ExpressionParamKind.InputParam;
		var isObject = kind == ExpressionParamKind.ObjectLiteral;

		_parameter.Visible = kind == ExpressionParamKind.Param;
		_choice.Visible = isChoice;
		_reference.Visible = isObject;
		_raw.Visible = kind == ExpressionParamKind.Unresolved;
		_pick.Visible = isObject;
		_layout.ColumnStyles[2].Width = isObject ? PickColumnWidth : 0;

		if (isChoice) PopulateChoices(kind);
		if (isObject) _reference.Text = DescribeObject();
	}

	private void PopulateChoices(ExpressionParamKind kind) {
		var selected = (_choice.SelectedItem as ChoiceItem)?.Id;
		var previouslySuppressed = _suppressEvents;
		_suppressEvents = true;
		try {
			_choice.Items.Clear();
			if (kind == ExpressionParamKind.Message)
				foreach (var message in CompatibleMessages())
					_choice.Items.Add(new ChoiceItem(message.Name,
						$"{message.ParamName}   [{message.Type}]   ← {message.Event.Name}"));
			else
				foreach (var inputParam in CompatibleInputParams())
					_choice.Items.Add(new ChoiceItem(inputParam.Name,
						$"{inputParam.ParamName}   [{inputParam.Type}]   ← {inputParam.Graph.Name}"));

			if (selected != null) SelectById(_choice, selected);
			if (_choice.SelectedIndex < 0 && _choice.Items.Count > 0) _choice.SelectedIndex = 0;
		} finally {
			_suppressEvents = previouslySuppressed;
		}
	}

	private string DescribeObject() {
		if (_pickedHierarchy != null) {
			var path = string.Join(" → ",
				_pickedHierarchy.Elements.Select(e => VmElementPicker.Describe(e.Element, _vm)));
			return $"{path}   ({_pickedHierarchy.Write()})";
		}
		return VmElementPicker.DescribeDetailed(_picked, _vm);
	}

	// ---------------------------------------------------------------- picking

	/// <summary>
	/// An object, chosen either as itself or as a place in the world. Both forms occur — 350
	/// expressions name one by id and 106 by placement — and they are the same choice made two
	/// ways, so they share one button rather than splitting the kind in two.
	/// </summary>
	private void Pick() {
		var menu = new ContextMenuStrip();

		var byId = new ToolStripMenuItem("Object…");
		byId.Click += (_, _) => {
			if (!VmElementPicker.TryPick(FindForm(), "Select object", _vm.AllParameterHolders(),
					e => VmElementPicker.Describe(e, _vm), _picked, out var picked, BareIdNote))
				return;
			_picked = picked;
			_pickedHierarchy = null;
			// An engine guid names the same object; a fresh pick is written by id.
			_byEngineGuid = false;
			OnUserEdit(UpdateVisibleControls);
		};

		var byPlacement = new ToolStripMenuItem("Place in the world…");
		byPlacement.Click += (_, _) => {
			if (!HierarchyPicker.TryPick(FindForm(), _vm, "Select a place in the world", _pickedHierarchy, out var path))
				return;
			_pickedHierarchy = path;
			_picked = path?.Elements[^1].Element;
			_byEngineGuid = false;
			OnUserEdit(UpdateVisibleControls);
		};

		menu.Items.AddRange([byId, byPlacement]);
		menu.Show(_pick, new Point(0, _pick.Height));
	}

	private string? BareIdNote(VmElement element) =>
		BareIdReach.Problem(element as ParameterHolder, _scope.Owner, _vm);

	// ---------------------------------------------------------------- helpers

	private static void SelectById(ComboBox box, string? id) {
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

	private sealed class KindItem(ExpressionParamKind kind) {
		public ExpressionParamKind Kind { get; } = kind;
		public override string ToString() => Kind switch {
			ExpressionParamKind.Param => "A parameter",
			ExpressionParamKind.Message => "An event message",
			ExpressionParamKind.InputParam => "A graph input param",
			ExpressionParamKind.ObjectLiteral => "An object, written in",
			_ => "Raw text"
		};
	}

	private sealed class ChoiceItem(string id, string label) {
		public string Id { get; } = id;
		public override string ToString() => label;
	}
}
