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
using P2XMLEditor.Logging;
using P2XMLEditor.WindowsFormsExtensions;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.MainForm.Holders;

/// <summary>
/// Edits an object: the components it has, the parameters those components bring, its own custom
/// parameters and their values, and its event graph.
///
/// The half that does not exist anywhere else is the bottom panel. A parameter's value is easy to
/// find and almost never the question; the question is what touches it, and until now answering
/// that meant reading every action in the game. Selecting a parameter here lists what writes it
/// and what reads it — see <see cref="ParameterUsageIndex"/> for what counts as either — with the
/// graph path each mention sits in, and opens the action or expression on a double-click.
/// </summary>
public class ParameterHoldersBrowser : SplitContainer {
	private readonly VirtualMachine _vm;
	private readonly ComponentCatalogue _catalogue;
	private ParameterUsageIndex _usages;

	private readonly SearchControl _search;
	private readonly ListView _holders;

	private readonly ListBox _components;
	private readonly Label _graph;
	private readonly ListView _parameters;
	private readonly ListView _usageList;
	private readonly Label _usageCaption;
	private SplitContainer _rows = null!;
	private SplitContainer _columns = null!;

	private readonly Button _addComponent;
	private readonly Button _removeComponent;
	private readonly Button _openGraph;
	private readonly Button _createGraph;
	private readonly Button _deleteGraph;
	private readonly Button _addParam;
	private readonly Button _editParam;
	private readonly Button _removeParam;

	/// <summary>Raised when the user asks to see an object's graph, which lives in another tab.</summary>
	public event EventHandler<Graph>? OpenGraphRequested;

	[PerformanceLogHook]
	public ParameterHoldersBrowser(VirtualMachine vm) {
		_vm = vm;
		_catalogue = ComponentCatalogue.Build(vm);
		_usages = ParameterUsageIndex.Build(vm);

		Dock = DockStyle.Fill;
		Orientation = Orientation.Vertical;

		// ---------------------------------------------------------------- objects
		_search = new SearchControl { Dock = DockStyle.Top };
		_search.SearchChanged += (_, _) => ReloadHolders();

		_holders = new ListView {
			Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false,
			HideSelection = false
		};
		_holders.Columns.Add("Object", 190);
		_holders.Columns.Add("Kind", 90);
		_holders.Columns.Add("Params", 60);
		_holders.SelectedIndexChanged += (_, _) => ShowHolder();

		var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
		left.Controls.Add(_holders);
		left.Controls.Add(_search);
		Panel1.Controls.Add(left);

		// ---------------------------------------------------------------- the object
		_components = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
		_addComponent = NewButton("Add…", AddComponent);
		_removeComponent = NewButton("Remove", RemoveComponent);

		_graph = new Label {
			Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, AutoEllipsis = true
		};
		_openGraph = NewButton("Open", () => {
			if (SelectedHolder()?.EventGraph is { } graph) OpenGraphRequested?.Invoke(this, graph);
		});
		_createGraph = NewButton("Create", CreateGraph);
		_deleteGraph = NewButton("Delete", DeleteGraph);

		_parameters = new ListView {
			Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false,
			HideSelection = false
		};
		_parameters.Columns.Add("Parameter", 200);
		_parameters.Columns.Add("Type", 150);
		_parameters.Columns.Add("Value", 170);
		_parameters.Columns.Add("From", 110);
		_parameters.SelectedIndexChanged += (_, _) => ShowUsages();
		_parameters.DoubleClick += (_, _) => EditParameter();

		_addParam = NewButton("Add custom…", AddCustomParameter);
		_editParam = NewButton("Edit value…", EditParameter);
		_removeParam = NewButton("Remove", RemoveParameter);

		// ---------------------------------------------------------------- usages
		_usageCaption = new Label {
			Dock = DockStyle.Top, Height = 22, TextAlign = ContentAlignment.MiddleLeft,
			Text = "Select a parameter to see what touches it."
		};
		_usageList = new ListView {
			Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, HideSelection = false
		};
		_usageList.Columns.Add("", 90);
		_usageList.Columns.Add("What", 260);
		_usageList.Columns.Add("Where", 320);
		_usageList.Columns.Add("Slot", 160);
		_usageList.DoubleClick += (_, _) => OpenUsage();

		Panel2.Controls.Add(BuildRight());

		ReloadHolders();
		ShowHolder();
	}

	private bool _placed;

	/// <summary>
	/// Splitter positions are set on the first real layout rather than in the constructor:
	/// SplitterDistance is validated against the control's current size, and a control that has
	/// not been laid out yet is 150px wide, so every one of these would have been refused.
	/// </summary>
	protected override void OnSizeChanged(EventArgs e) {
		base.OnSizeChanged(e);
		if (_placed || Width < 500 || Height < 400) return;
		_placed = true;

		Place(this, 340);
		Place(_columns, 300);
		Place(_rows, (int)(_rows.Height * 0.55f));
	}

	private static void Place(SplitContainer split, int distance) {
		var span = split.Orientation == Orientation.Vertical ? split.Width : split.Height;
		var room = span - split.Panel2MinSize;
		if (room <= split.Panel1MinSize) return;
		split.SplitterDistance = Math.Clamp(distance, split.Panel1MinSize, room);
	}

	private Control BuildRight() {
		var vertical = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal };
		_rows = vertical;

		var top = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
		_columns = top;
		top.Panel1.Controls.Add(BuildComponentsAndGraph());
		top.Panel2.Controls.Add(Framed("Parameters", _parameters,
			[_addParam, _editParam, _removeParam]));
		vertical.Panel1.Controls.Add(top);

		var usages = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
		usages.Controls.Add(_usageList);
		usages.Controls.Add(_usageCaption);
		var usageBox = new GroupBox { Text = "Edited by / Read by", Dock = DockStyle.Fill, Padding = new Padding(6) };
		usageBox.Controls.Add(usages);
		vertical.Panel2.Controls.Add(usageBox);

		return vertical;
	}

	private Control BuildComponentsAndGraph() {
		var host = new Panel { Dock = DockStyle.Fill };

		var graphButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
		graphButtons.Controls.AddRange([_openGraph, _createGraph, _deleteGraph]);
		var graphBox = new GroupBox { Text = "Event graph", Dock = DockStyle.Bottom, Height = 96, Padding = new Padding(6) };
		graphBox.Controls.Add(_graph);
		graphBox.Controls.Add(graphButtons);

		host.Controls.Add(Framed("Components", _components, [_addComponent, _removeComponent]));
		host.Controls.Add(graphBox);
		return host;
	}

	private static Control Framed(string title, Control content, IReadOnlyList<Button> buttons) {
		var box = new GroupBox { Text = title, Dock = DockStyle.Fill, Padding = new Padding(6) };
		var strip = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true };
		strip.Controls.AddRange(buttons.Cast<Control>().ToArray());
		box.Controls.Add(content);
		box.Controls.Add(strip);
		return box;
	}

	private static Button NewButton(string text, System.Action onClick) {
		var button = new Button { Text = text, AutoSize = true, Margin = new Padding(0, 2, 6, 2) };
		button.Click += (_, _) => onClick();
		return button;
	}

	// ---------------------------------------------------------------- the object list

	private void ReloadHolders() {
		_holders.BeginUpdate();
		_holders.Items.Clear();

		var holders = _vm.GetElementsByType<ParameterHolder>()
			.OrderBy(h => h.Name, StringComparer.Ordinal)
			.ToList();

		var shown = 0;
		foreach (var holder in holders) {
			if (!_search.IsMatchAny(holder.Name ?? "", holder.GetType().Name, holder.Id.ToString())) continue;
			var item = new ListViewItem(holder.Name ?? holder.Id.ToString()) { Tag = holder };
			item.SubItems.Add(holder.GetType().Name);
			item.SubItems.Add(((holder.StandartParams?.Count ?? 0) + (holder.CustomParams?.Count ?? 0)).ToString());
			_holders.Items.Add(item);
			shown++;
		}

		_holders.EndUpdate();
		_search.StatusText = $"Displaying {shown}/{holders.Count} objects.";
	}

	private ParameterHolder? SelectedHolder() =>
		_holders.SelectedItems.Count > 0 ? _holders.SelectedItems[0].Tag as ParameterHolder : null;

	private Parameter? SelectedParameter() =>
		_parameters.SelectedItems.Count > 0 ? _parameters.SelectedItems[0].Tag as Parameter : null;

	// ---------------------------------------------------------------- the object

	private void ShowHolder() {
		var holder = SelectedHolder();

		_components.BeginUpdate();
		_components.Items.Clear();
		foreach (var component in (holder?.FunctionalComponents ?? []).OrderBy(c => c.Name, StringComparer.Ordinal))
			_components.Items.Add(new ComponentItem(component));
		_components.EndUpdate();

		var graph = holder?.EventGraph;
		_graph.Text = holder == null ? ""
			: graph == null ? "No event graph — this object runs nothing of its own."
			: $"{graph.Name}   ({graph.States?.Count ?? 0} node(s), {graph.EventLinks?.Count ?? 0} link(s))";
		_openGraph.Enabled = _deleteGraph.Enabled = graph != null;
		_createGraph.Enabled = holder != null && graph == null;

		ReloadParameters();
	}

	private void ReloadParameters() {
		var holder = SelectedHolder();
		var selected = SelectedParameter()?.Id;

		_parameters.BeginUpdate();
		_parameters.Items.Clear();

		foreach (var (key, parameter) in Ordered(holder)) {
			if (parameter == null) continue;
			var item = new ListViewItem(key) { Tag = parameter };
			item.SubItems.Add(parameter.Type);
			item.SubItems.Add(Truncate(SafePreview(parameter), 60));
			item.SubItems.Add(parameter.Implicit ? "implicit"
				: parameter.Custom ? "custom"
				: parameter.OwnerComponent?.Name ?? "standard");
			if (parameter.Implicit) item.ForeColor = SystemColors.GrayText;
			else if (parameter.Custom) item.ForeColor = Color.DarkGreen;
			_parameters.Items.Add(item);
			if (parameter.Id == selected) item.Selected = true;
		}

		_parameters.EndUpdate();
		ShowUsages();
	}

	/// <summary>Standard parameters by key, then the object's own — the order they are thought of in.</summary>
	private static IEnumerable<KeyValuePair<string, Parameter>> Ordered(ParameterHolder? holder) {
		if (holder == null) return [];
		var empty = new Dictionary<string, Parameter>();
		var standart = (holder.StandartParams ?? empty).OrderBy(p => p.Key, StringComparer.Ordinal);
		var custom = (holder.CustomParams ?? empty).OrderBy(p => p.Key, StringComparer.Ordinal);
		return standart.Concat(custom);
	}

	private string SafePreview(Parameter parameter) {
		try {
			return PreviewHelper.Preview(parameter);
		} catch {
			return parameter.SerializedValue;
		}
	}

	private static string Truncate(string text, int limit) =>
		text.Length <= limit ? text : text[..(limit - 1)] + "…";

	// ---------------------------------------------------------------- components

	private void AddComponent() {
		if (SelectedHolder() is not { } holder) return;

		var already = (holder.FunctionalComponents ?? []).Select(c => c.Name).ToHashSet(StringComparer.Ordinal);
		var candidates = _catalogue.Names.Where(n => !already.Contains(n)).ToList();
		if (candidates.Count == 0) {
			MessageBox.Show(this, "This object already has every component the data knows about.",
				"Add component", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		if (!ChoiceDialog.TryPick(FindForm(), "Add component", candidates,
				n => $"{n}   ({_catalogue.ParamsOf(n).Count} parameter(s))", out var chosen) || chosen == null)
			return;

		var added = _catalogue.AddTo(holder, chosen!, _vm);
		_vm.InvalidateStandartParamTypes();
		Logger.Log(LogLevel.Info,
			$"Added component {added.Name} with {_catalogue.ParamsOf(chosen!).Count} parameter(s) to {holder.Name}");

		ShowHolder();
		SelectComponent(chosen!);
	}

	/// <summary>
	/// Takes the component off, and its parameters with it. Those parameters are routinely read
	/// by actions on other objects, so what breaks is counted before anything happens rather than
	/// discovered later by a graph that stops working.
	/// </summary>
	private void RemoveComponent() {
		if (SelectedHolder() is not { } holder) return;
		if (_components.SelectedItem is not ComponentItem { Component: var component }) return;

		var keys = ComponentCatalogue.ParamKeysOf(holder, component).ToList();
		var touched = keys
			.Select(k => holder.StandartParams[k])
			.Sum(p => _usages.Of(p).Count);

		var message = $"Remove component '{component.Name}' from {holder.Name}?";
		if (keys.Count > 0) message += $"\n\n{keys.Count} standard parameter(s) go with it.";
		if (touched > 0) message += $"\n\n{touched} action(s) or expression(s) read or write those parameters "
									+ "and will be left pointing at nothing.";
		if (component.Events is { Count: > 0 })
			message += $"\n\n{component.Events.Count} event(s) declared on it go too.";

		if (MessageBox.Show(this, message, "Remove component", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
			!= DialogResult.Yes)
			return;

		ComponentCatalogue.RemoveFrom(holder, component, _vm);
		_vm.InvalidateStandartParamTypes();
		Rebuild();
	}

	private void SelectComponent(string name) {
		for (var i = 0; i < _components.Items.Count; i++)
			if (_components.Items[i] is ComponentItem item && item.Component.Name == name) {
				_components.SelectedIndex = i;
				return;
			}
	}

	// ---------------------------------------------------------------- graph

	private void CreateGraph() {
		if (SelectedHolder() is not { } holder || holder.EventGraph != null) return;

		var graph = VmElement.CreateDefault<Graph>(_vm, holder);
		graph.Name = $"{holder.Name}_EventGraph";
		holder.EventGraph = graph;

		ShowHolder();
		OpenGraphRequested?.Invoke(this, graph);
	}

	private void DeleteGraph() {
		if (SelectedHolder() is not { } holder || holder.EventGraph is not { } graph) return;

		var nodes = graph.States?.Count ?? 0;
		if (MessageBox.Show(this,
				$"Delete '{graph.Name}' and its {nodes} node(s)?\n\nEverything inside it — states, branches, "
				+ "links and their actions — goes with it.", "Delete event graph",
				MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
			return;

		holder.EventGraph = null;
		_vm.RemoveElement(graph);
		Rebuild();
	}

	// ---------------------------------------------------------------- parameters

	private void AddCustomParameter() {
		if (SelectedHolder() is not { } holder) return;

		using var dialog = new ParameterValueForm(_vm, "New custom parameter", null);
		if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

		var parameter = VmElement.CreateDefault<Parameter>(_vm, holder);
		parameter.Name = dialog.ParameterName;
		parameter.Custom = true;
		parameter.Implicit = false;
		parameter.OwnerComponent = null;
		if (dialog.Value != null) parameter.Value = dialog.Value;

		(holder.CustomParams ??= new Dictionary<string, Parameter>())[parameter.Name] = parameter;
		ReloadParameters();
	}

	private void EditParameter() {
		if (SelectedParameter() is not { } parameter) return;

		// An implicit parameter's stored value is never read: DynamicParameter computes it from
		// the FSM. Editing it would look like it did something.
		if (parameter.Implicit) {
			MessageBox.Show(this,
				$"'{parameter.Name}' is implicit — the engine works its value out at runtime and ignores "
				+ "what is stored here.", "Implicit parameter", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}

		using var dialog = new ParameterValueForm(_vm, $"Value of {parameter.Name}", parameter);
		if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

		if (dialog.Value != null) parameter.Value = dialog.Value;
		if (parameter.Custom && dialog.ParameterName != parameter.Name && SelectedHolder() is { } holder) {
			holder.CustomParams?.Remove(parameter.Name);
			parameter.Name = dialog.ParameterName;
			(holder.CustomParams ??= new Dictionary<string, Parameter>())[parameter.Name] = parameter;
		}

		ReloadParameters();
	}

	/// <summary>
	/// Only a custom parameter can go on its own. A standard one belongs to a component, and
	/// removing it without the component leaves the object declaring a component whose parameters
	/// it does not have — which is a shape the data never has.
	/// </summary>
	private void RemoveParameter() {
		if (SelectedHolder() is not { } holder || SelectedParameter() is not { } parameter) return;

		if (!parameter.Custom) {
			MessageBox.Show(this,
				$"'{parameter.Name}' comes from the {parameter.OwnerComponent?.Name ?? "standard"} component. "
				+ "Remove the component to remove it.", "Remove parameter", MessageBoxButtons.OK,
				MessageBoxIcon.Information);
			return;
		}

		var touched = _usages.Of(parameter).Count;
		var message = $"Remove custom parameter '{parameter.Name}'?";
		if (touched > 0) message += $"\n\n{touched} action(s) or expression(s) read or write it and will be "
									+ "left pointing at nothing.";
		if (MessageBox.Show(this, message, "Remove parameter", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
			!= DialogResult.Yes)
			return;

		holder.CustomParams?.Remove(parameter.Name);
		_vm.RemoveElement(parameter);
		Rebuild();
	}

	// ---------------------------------------------------------------- usages

	private void ShowUsages() {
		_usageList.BeginUpdate();
		_usageList.Items.Clear();

		var parameter = SelectedParameter();
		if (parameter == null) {
			_usageCaption.Text = "Select a parameter to see what touches it.";
			_usageList.EndUpdate();
			return;
		}

		var usages = _usages.Of(parameter);
		var written = usages.Count(u => u.Use == ParameterUse.Written);
		var watched = usages.Count(u => u.Use == ParameterUse.Watched);

		foreach (var usage in usages
					 .OrderBy(u => (int)u.Use)
					 .ThenBy(u => ParameterUsageIndex.ContextOf(u.Owner), StringComparer.Ordinal)) {
			var item = new ListViewItem(Label(usage.Use)) {
				Tag = usage,
				ForeColor = usage.Use switch {
					ParameterUse.Written => Color.Firebrick,
					ParameterUse.Watched => Color.DarkMagenta,
					_ => Color.DarkBlue
				}
			};
			item.SubItems.Add(Describe(usage.Owner));
			item.SubItems.Add(ParameterUsageIndex.ContextOf(usage.Owner));
			item.SubItems.Add(usage.Slot);
			_usageList.Items.Add(item);
		}

		_usageCaption.Text = usages.Count == 0
			? Untouched(parameter)
			: $"{parameter.Name}: edited by {written}, read by {usages.Count - written - watched}"
			  + (watched > 0 ? $", raises {watched} event(s)" : "")
			  + ".   Double-click to open where it happens.";

		_usageList.EndUpdate();
	}

	private static string Label(ParameterUse use) => use switch {
		ParameterUse.Written => "Edited by",
		ParameterUse.Watched => "Raises",
		_ => "Read by"
	};

	/// <summary>
	/// What "no usages" means, which is not the same thing for every parameter. An implicit
	/// parameter is computed by the engine — DynamicParameter.GetValue hands back the FSM's
	/// current state for a _state parameter and ignores whatever is stored — so nothing naming it
	/// is expected rather than suspicious. A standard parameter is read by the engine component
	/// that declares it, which is not in this data at all. Only a custom one that nothing names
	/// is genuinely unused, and 596 of the Sandbox's 3461 are.
	/// </summary>
	private static string Untouched(Parameter parameter) {
		if (parameter.Implicit)
			return $"{parameter.Name} is implicit — the engine maintains its value and the stored one is ignored.";
		if (!parameter.Custom)
			return $"Nothing in the graphs names {parameter.Name}; a standard parameter is read by its "
				   + "component in the engine.";
		return $"Nothing reads or writes {parameter.Name}.";
	}

	private string Describe(VmElement owner) {
		try {
			return owner switch {
				VmAction action => PreviewHelper.Preview(action),
				Expression expression => PreviewHelper.Preview(expression),
				GraphLink link => $"link '{link.Name}'",
				ActionLine line => $"loop '{line.Name}'",
				Event raised => $"event '{raised.Name}'",
				_ => owner.GetType().Name
			};
		} catch {
			return $"{owner.GetType().Name} {owner.Id}";
		}
	}

	/// <summary>
	/// Opens the mention where it lives. An action and an expression each have their own editor,
	/// which is the whole point of the list — the row says a parameter is written somewhere, and
	/// the next question is always what else that action does.
	/// </summary>
	private void OpenUsage() {
		if (_usageList.SelectedItems.Count == 0) return;
		if (_usageList.SelectedItems[0].Tag is not ParameterUsage usage) return;

		switch (usage.Owner) {
			case VmAction action: {
				using var editor = new ActionEditorForm(_vm, action);
				if (editor.ShowDialog(FindForm()) == DialogResult.OK) Rebuild();
				break;
			}
			case Expression expression: {
				using var editor = new ExpressionEditorForm(_vm, expression);
				if (editor.ShowDialog(FindForm()) == DialogResult.OK) Rebuild();
				break;
			}
			case GraphLink link when link.Parent.Element is Graph graph:
				OpenGraphRequested?.Invoke(this, graph);
				break;
			case ActionLine line when line.LocalContext.Element is { } context &&
									  GraphTopology.ContainerOf(context) is Graph graph:
				OpenGraphRequested?.Invoke(this, graph);
				break;
		}
	}

	/// <summary>
	/// Rebuilds the usage index. Editing an action can add or remove a mention of any parameter,
	/// so the index is only true until the next edit — and a stale "nothing reads this" is the
	/// one answer here that must never be wrong.
	/// </summary>
	private void Rebuild() {
		_usages = ParameterUsageIndex.Build(_vm);
		ReloadHolders();
		ShowHolder();
	}

	private sealed class ComponentItem(FunctionalComponent component) {
		public FunctionalComponent Component { get; } = component;

		public override string ToString() {
			var owned = component.Parent?.StandartParams?.Count(p => ReferenceEquals(p.Value?.OwnerComponent, component)) ?? 0;
			return $"{component.Name}   ({owned} param(s){(component.Main ? ", main" : "")})";
		}
	}
}
