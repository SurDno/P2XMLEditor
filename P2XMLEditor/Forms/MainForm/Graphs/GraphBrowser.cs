using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Logging;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.MainForm.Graphs;

/// <summary>
/// Picks a graph and edits it: a searchable list on the left, and on the right the graph itself
/// with an inspector for whatever is selected.
///
/// Descending into a subgraph pushes onto a trail rather than replacing the view, because a
/// graph is routinely four deep and the way back out is otherwise a search. The trail is also
/// how a Talking is reached — it is a node inside a graph, drawn by the same canvas as the
/// graph that holds it.
/// </summary>
public class GraphsBrowser : SplitContainer {
	private readonly VirtualMachine _vm;

	private readonly SearchControl _search;
	private readonly CheckBox _showSubgraphs;
	private readonly ListView _list;
	private readonly ToolStrip _trailBar;
	private readonly GraphCanvas _canvas;
	private readonly GraphInspector _inspector;
	private readonly Label _problem;

	private readonly List<VmElement> _trail = [];

	[PerformanceLogHook]
	public GraphsBrowser(VirtualMachine vm) {
		_vm = vm;
		Dock = DockStyle.Fill;
		Orientation = Orientation.Vertical;

		_search = new SearchControl { Dock = DockStyle.Top };
		_search.SearchChanged += (_, _) => ReloadList();

		_list = new ListView {
			Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false,
			HideSelection = false
		};
		_list.Columns.Add("Graph", 200);
		_list.Columns.Add("Owner", 150);
		_list.SelectedIndexChanged += (_, _) => {
			if (_list.SelectedItems.Count > 0 && _list.SelectedItems[0].Tag is Graph graph) Open(graph, reset: true);
		};

		var left = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
		_showSubgraphs = new CheckBox { Dock = DockStyle.Top, Text = "Show subgraphs", Checked = false, Padding = new Padding(2, 4, 0, 4) };
		_showSubgraphs.CheckedChanged += (_, _) => ReloadList();
		left.Controls.Add(_list);
		left.Controls.Add(_showSubgraphs);
		left.Controls.Add(_search);
		Panel1.Controls.Add(left);

		_trailBar = new ToolStrip { Dock = DockStyle.Top, GripStyle = ToolStripGripStyle.Hidden };

		_problem = new Label {
			Dock = DockStyle.Bottom, Height = 26, TextAlign = ContentAlignment.MiddleLeft,
			ForeColor = Color.Firebrick, Visible = false
		};

		_canvas = new GraphCanvas(vm) { Dock = DockStyle.Fill };
		_canvas.SelectionChanged += (_, _) => OnSelectionChanged();
		_canvas.NodeActivated += (_, node) => Activate(node);
		// Double-clicking a link no longer opens anything: it is already selected, and the
		// inspector beside the canvas is where it is edited.
		_canvas.LinkActivated += (_, _) => _inspector.Focus();
		_canvas.GraphChanged += (_, _) => {
			ReloadList();
			RefreshProblems();
		};

		_inspector = new GraphInspector(vm);
		_inspector.Changed += (_, _) => {
			_canvas.Redraw();
			RefreshProblems();
		};
		_inspector.OpenRequested += (_, node) => Activate(node);

		var right = new Panel { Dock = DockStyle.Fill };
		right.Controls.Add(_canvas);
		right.Controls.Add(_inspector);
		right.Controls.Add(_problem);
		right.Controls.Add(_trailBar);
		Panel2.Controls.Add(right);

		ReloadList();
	}

	// ---------------------------------------------------------------- the list

	private void ReloadList() {
		_list.BeginUpdate();
		_list.Items.Clear();

		var graphs = _vm.GetElementsByType<Graph>()
			.Where(g => !g.IsOrphaned())
			.Where(g => _showSubgraphs.Checked || g.Parent.Element is not Graph)
			.OrderBy(g => OwnerName(g), StringComparer.Ordinal)
			.ThenBy(g => g.Name, StringComparer.Ordinal)
			.ToList();

		var shown = 0;
		foreach (var graph in graphs) {
			if (!_search.IsMatchAny(graph.Name ?? "", OwnerName(graph), graph.Id.ToString())) continue;

			var item = new ListViewItem(graph.Name ?? graph.Id.ToString()) { Tag = graph };
			item.SubItems.Add(OwnerName(graph));
			_list.Items.Add(item);
			shown++;
		}

		_list.EndUpdate();
		_search.StatusText = $"Displaying {shown}/{graphs.Count} graphs.";
	}

	private static string OwnerName(Graph graph) =>
		graph.Owner is INamedElement named && !string.IsNullOrWhiteSpace(named.Name)
			? named.Name
			: graph.Owner?.Id.ToString() ?? "";

	// ---------------------------------------------------------------- navigation

	public void Open(Graph graph, bool reset = false) {
		if (reset) _trail.Clear();
		Descend(graph);
	}

	private void Activate(VmElement node) {
		if (GraphTopology.IsContainer(node)) {
			Descend(node);
			return;
		}
		// A node that cannot be descended into is edited in the inspector, which is already
		// showing it — so the double-click puts the focus there rather than doing nothing.
		_inspector.Focus();
	}

	private void Descend(VmElement container) {
		var existing = _trail.FindIndex(e => ReferenceEquals(e, container));
		if (existing >= 0) _trail.RemoveRange(existing, _trail.Count - existing);
		_trail.Add(container);

		_canvas.Display(container);
		_canvas.FitView();
		RebuildTrail();
		RefreshProblems();
	}

	private void RebuildTrail() {
		_trailBar.Items.Clear();
		for (var i = 0; i < _trail.Count; i++) {
			var target = _trail[i];
			var button = new ToolStripButton($"{GraphTopology.NameOf(target)}") {
				DisplayStyle = ToolStripItemDisplayStyle.Text,
				Checked = i == _trail.Count - 1
			};
			button.Click += (_, _) => Descend(target);
			_trailBar.Items.Add(button);
			if (i < _trail.Count - 1) _trailBar.Items.Add(new ToolStripLabel("›"));
		}

		_trailBar.Items.Add(new ToolStripSeparator());
		var fit = new ToolStripButton("Fit view") { DisplayStyle = ToolStripItemDisplayStyle.Text };
		fit.Click += (_, _) => _canvas.FitView();
		_trailBar.Items.Add(fit);
	}

	// ---------------------------------------------------------------- selection

	private void OnSelectionChanged() {
		_inspector.SetSelection(_canvas.SelectedNode, _canvas.SelectedLink);
		RefreshProblems();
	}

	/// <summary>
	/// The first thing wrong with the graph on screen, if anything is. Every rule behind this
	/// holds across both shipped corpora without exception, so a message here is a real defect
	/// rather than a style note — see <see cref="GraphTopology.Problem"/>.
	/// </summary>
	private void RefreshProblems() {
		if (_trail.Count == 0) {
			_problem.Visible = false;
			return;
		}

		var complaints = new List<string>();

		// The graph as a whole first: a missing or duplicated initial node is about the graph
		// rather than any one link, and it is the one that stops it running at all.
		if (GraphTopology.InitialProblem(_trail[^1]) is { } initial) complaints.Add(initial);

		complaints.AddRange(GraphTopology.LinksOf(_trail[^1])
			.Select(link => (link, problem: GraphTopology.Problem(link)))
			.Where(pair => pair.problem != null)
			.Select(pair => $"{pair.link.Name}: {pair.problem}"));

		_problem.Visible = complaints.Count > 0;
		if (complaints.Count == 0) return;

		_problem.Text = complaints.Count == 1
			? $"⚠ {complaints[0]}"
			: $"⚠ {complaints[0]}   (and {complaints.Count - 1} more)";
	}
}
