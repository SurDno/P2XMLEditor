using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Forms.Editors.Graphs;
using P2XMLEditor.Helper;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.MainForm.Graphs;

/// <summary>
/// Draws one graph and lets it be rearranged and rewired.
///
/// The old viewer drew every node as a circle of one size and every link as a straight line
/// between two centres, which loses the two things a link actually says: which way out of the
/// source it is taken, and which way into the destination it arrives. Both are positions in a
/// list — see <see cref="GraphTopology"/> — so here every node draws one row per exit and one
/// per entry, and a link is a curve between the two rows it names. A branch with four
/// conditions is then five distinct places a link can start from, and which one it starts from
/// is visible without opening anything.
///
/// Dragging from an exit row to a node draws a new link. Nothing else creates one, because a
/// link with no exit is not a thing the data has.
/// </summary>
public sealed class GraphCanvas : UserControl {
	private const float MinZoom = 0.2f;
	private const float MaxZoom = 3f;
	private const int NodeWidth = 210;
	private const int HeaderHeight = 26;
	private const int PortHeight = 18;
	private const int PortPadding = 4;

	private readonly VirtualMachine _vm;
	private readonly DoubleBufferedPanel _surface;

	private readonly Dictionary<ulong, PointF> _positions = [];
	private readonly Dictionary<ulong, RectangleF> _bounds = [];

	private VmElement _container = null!;
	private float _zoom = 1f;
	private PointF _origin = new(60, 60);

	private VmElement? _selectedNode;
	private GraphLink? _selectedLink;

	private bool _panning;
	private Point _lastMouse;

	private VmElement? _dragNode;
	private PointF _dragOffset;

	// A link being drawn: the node it leaves and the exit it leaves by.
	private VmElement? _linkFrom;
	private int _linkFromExit;
	private Point _linkTo;

	public event EventHandler? SelectionChanged;
	public event EventHandler<VmElement>? NodeActivated;
	public event EventHandler<GraphLink>? LinkActivated;
	public event EventHandler? GraphChanged;

	public GraphCanvas(VirtualMachine vm) {
		_vm = vm;

		_surface = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.White };
		_surface.Paint += (_, e) => Draw(e.Graphics);
		_surface.MouseDown += OnMouseDown;
		_surface.MouseMove += OnMouseMove;
		_surface.MouseUp += OnMouseUp;
		_surface.MouseWheel += OnMouseWheel;
		_surface.MouseDoubleClick += OnDoubleClick;
		_surface.MouseEnter += (_, _) => _surface.Focus();

		Controls.Add(_surface);
	}

	public VmElement? SelectedNode => _selectedNode;
	public GraphLink? SelectedLink => _selectedLink;
	public VmElement Container => _container;

	/// <summary>Shows a graph or a talking, replacing whatever was on screen.</summary>
	public void Display(VmElement container) {
		_container = container;
		_selectedNode = null;
		_selectedLink = null;
		Relayout();
		SelectionChanged?.Invoke(this, EventArgs.Empty);
	}

	public void Redraw(bool relayout = false) {
		if (relayout) Relayout();
		else _surface.Invalidate();
	}

	// ---------------------------------------------------------------- layout

	/// <summary>
	/// Places nodes by layer, so an unread graph reads top to bottom in the order it runs.
	/// Positions already chosen by hand are kept — the layout is a starting point, not a rule.
	/// </summary>
	private void Relayout() {
		var nodes = GraphTopology.NodesOf(_container);
		var layout = new SugiyamaLayout<ulong>();
		foreach (var node in nodes) layout.AddNode(node.Id);

		foreach (var link in GraphTopology.LinksOf(_container)) {
			var from = link.Source?.Element;
			var to = link.Destination?.Element;
			if (from != null && to != null && _positions.ContainsKey(from.Id) == _positions.ContainsKey(to.Id))
				layout.AddEdge(from.Id, to.Id);
		}

		foreach (var (id, position) in layout.Layout(1.6f, 1.6f))
			_positions[id] = new PointF(position.x * 320f, -position.y * 260f);

		// A node the layout could not place — one with no links at all — still needs somewhere.
		var free = 0;
		foreach (var node in nodes)
			if (!_positions.ContainsKey(node.Id))
				_positions[node.Id] = new PointF(free++ * 260f, 0);

		_surface.Invalidate();
	}

	public void FitView() {
		var nodes = GraphTopology.NodesOf(_container);
		if (nodes.Count == 0) return;

		var minX = nodes.Min(n => Position(n).X);
		var maxX = nodes.Max(n => Position(n).X + NodeWidth);
		var minY = nodes.Min(n => Position(n).Y);
		var maxY = nodes.Max(n => Position(n).Y + NodeHeight(n));

		var scaleX = (_surface.Width - 80f) / Math.Max(1f, maxX - minX);
		var scaleY = (_surface.Height - 80f) / Math.Max(1f, maxY - minY);
		_zoom = Math.Clamp(Math.Min(scaleX, scaleY), MinZoom, MaxZoom);
		_origin = new PointF(40 - minX * _zoom, 40 - minY * _zoom);
		_surface.Invalidate();
	}

	private PointF Position(VmElement node) =>
		_positions.TryGetValue(node.Id, out var position) ? position : PointF.Empty;

	private static int NodeHeight(VmElement node) =>
		HeaderHeight + PortPadding * 2 +
		PortHeight * Math.Max(1, GraphTopology.EntriesOf(node).Count + GraphTopology.ExitsOf(node).Count);

	private PointF ToScreen(PointF world) =>
		new(world.X * _zoom + _origin.X, world.Y * _zoom + _origin.Y);

	private PointF ToWorld(Point screen) =>
		new((screen.X - _origin.X) / _zoom, (screen.Y - _origin.Y) / _zoom);

	// ---------------------------------------------------------------- drawing

	private void Draw(Graphics g) {
		g.SmoothingMode = SmoothingMode.AntiAlias;
		g.Clear(Color.White);

		_bounds.Clear();
		foreach (var node in GraphTopology.NodesOf(_container)) {
			var position = ToScreen(Position(node));
			_bounds[node.Id] = new RectangleF(position.X, position.Y, NodeWidth * _zoom, NodeHeight(node) * _zoom);
		}

		foreach (var link in GraphTopology.LinksOf(_container)) DrawLink(g, link);
		foreach (var node in GraphTopology.NodesOf(_container)) DrawNode(g, node);

		if (_linkFrom != null) {
			using var pen = new Pen(Color.SteelBlue, 2f) { DashStyle = DashStyle.Dash };
			var from = ExitPoint(_linkFrom, _linkFromExit);
			g.DrawLine(pen, from, _linkTo);
		}
	}

	private void DrawNode(Graphics g, VmElement node) {
		if (!_bounds.TryGetValue(node.Id, out var bounds)) return;

		using var header = new Font(FontFamily.GenericSansSerif, Math.Max(5f, 9f * _zoom), FontStyle.Bold);
		using var body = new Font(FontFamily.GenericSansSerif, Math.Max(4f, 7.5f * _zoom));

		var selected = ReferenceEquals(node, _selectedNode);
		using var fill = new SolidBrush(selected ? Color.FromArgb(225, 240, 255) : Color.FromArgb(250, 250, 250));
		using var border = new Pen(selected ? Color.SteelBlue : Color.FromArgb(120, 120, 120),
			Math.Max(1f, (selected ? 2f : 1f) * _zoom));

		g.FillRectangle(fill, bounds);
		g.DrawRectangle(border, bounds.X, bounds.Y, bounds.Width, bounds.Height);

		var headerRect = new RectangleF(bounds.X, bounds.Y, bounds.Width, HeaderHeight * _zoom);
		using var headerFill = new SolidBrush(HeaderColour(node));
		g.FillRectangle(headerFill, headerRect);
		g.DrawString(Truncate(GraphTopology.NameOf(node), 26), header, Brushes.Black,
			headerRect.X + 4 * _zoom, headerRect.Y + 4 * _zoom);

		if (GraphTopology.IsInitial(node)) {
			using var initial = new Pen(Color.SeaGreen, Math.Max(1f, 3f * _zoom));
			g.DrawLine(initial, bounds.X, bounds.Y, bounds.X, bounds.Bottom);
		}

		var y = bounds.Y + (HeaderHeight + PortPadding) * _zoom;
		foreach (var entry in GraphTopology.EntriesOf(node)) {
			g.FillEllipse(Brushes.SeaGreen, bounds.X - 4 * _zoom, y + 5 * _zoom, 8 * _zoom, 8 * _zoom);
			g.DrawString(Truncate(entry.Label, 30), body, Brushes.DimGray, bounds.X + 8 * _zoom, y);
			y += PortHeight * _zoom;
		}

		foreach (var exit in GraphTopology.ExitsOf(node)) {
			var text = Truncate(exit.Label, 30);
			var size = g.MeasureString(text, body);
			g.DrawString(text, body, Brushes.SaddleBrown, bounds.Right - 10 * _zoom - size.Width, y);
			g.FillEllipse(Brushes.SaddleBrown, bounds.Right - 4 * _zoom, y + 5 * _zoom, 8 * _zoom, 8 * _zoom);
			y += PortHeight * _zoom;
		}
	}

	private static Color HeaderColour(VmElement node) => node switch {
		Branch => Color.FromArgb(255, 233, 200),
		Graph => Color.FromArgb(214, 232, 255),
		Talking => Color.FromArgb(226, 214, 255),
		Speech => Color.FromArgb(226, 245, 226),
		_ => Color.FromArgb(235, 235, 235)
	};

	private void DrawLink(Graphics g, GraphLink link) {
		var to = link.Destination?.Element;
		if (to != null && !_bounds.ContainsKey(to.Id)) return;

		var selected = ReferenceEquals(link, _selectedLink);
		var colour = !link.Enabled ? Color.LightGray : selected ? Color.SteelBlue : Color.FromArgb(120, 120, 130);
		using var pen = new Pen(colour, Math.Max(1f, (selected ? 2.4f : 1.4f) * _zoom)) {
			CustomEndCap = new AdjustableArrowCap(4f, 4f)
		};
		if (!link.Enabled) pen.DashStyle = DashStyle.Dash;

		var hasSource = link.Source?.Element is { } source && _bounds.ContainsKey(source.Id);
		if (!hasSource && to == null) return;

		var start = hasSource
			? ExitPoint(link.Source!.Value.Element, link.SourceExitPointIndex)
			// A link with no source is subscribed to an event and enters from outside the graph.
			: new PointF(_bounds[to!.Id].X - 70 * _zoom, EntryPoint(to, link.DestEntryPointIndex).Y);

		// A link with no destination returns rather than moving on — a fifth of them do — and
		// how it returns is DestEntryPointIndex read as a LinkExit. Drawn as a stub carrying that
		// word, because an exit that returns to the previous state and one that leaves the
		// subgraph go to entirely different places.
		if (to == null) {
			var stop = new PointF(start.X + 46 * _zoom, start.Y);
			g.DrawLine(pen, start, stop);
			g.DrawEllipse(pen, stop.X, stop.Y - 5 * _zoom, 10 * _zoom, 10 * _zoom);

			if (_zoom >= 0.45f) {
				using var font = new Font(FontFamily.GenericSansSerif, Math.Max(4f, 7.5f * _zoom));
				using var brush = new SolidBrush(colour);
				g.DrawString(ReturnLabel(link), font, brush, stop.X + 14 * _zoom, stop.Y - 4 * _zoom);
			}
			return;
		}

		var end = EntryPoint(to, link.DestEntryPointIndex);

		var bend = Math.Max(30f, Math.Abs(end.X - start.X) * 0.4f) * _zoom;
		g.DrawBezier(pen, start, new PointF(start.X + bend, start.Y), new PointF(end.X - bend, end.Y), end);

		var label = LabelOf(link);
		if (label.Length == 0 || _zoom < 0.45f) return;

		using var font = new Font(FontFamily.GenericSansSerif, Math.Max(4f, 7.5f * _zoom));
		var middle = new PointF((start.X + end.X) / 2, (start.Y + end.Y) / 2 - 8 * _zoom);
		var size = g.MeasureString(label, font);
		g.FillRectangle(Brushes.White, middle.X - size.Width / 2, middle.Y, size.Width, size.Height);
		g.DrawString(label, font, new SolidBrush(colour), middle.X - size.Width / 2, middle.Y);
	}

	private static string ReturnLabel(GraphLink link) => GraphTopology.ExitTypeOf(link) switch {
		GraphTopology.LinkExit.OuterGraph => "↰ out of subgraph",
		GraphTopology.LinkExit.OuterEventExecution => "↰ out of event",
		GraphTopology.LinkExit.PreviousState => "↩ previous state",
		_ => $"↩ ?{link.DestEntryPointIndex}"
	};

	private static string LabelOf(GraphLink link) {
		if (link.Event != null) return Truncate(link.Event.Name, 28);
		return string.IsNullOrWhiteSpace(link.Name) ? "" : Truncate(link.Name, 28);
	}

	private static string Truncate(string text, int limit) =>
		text.Length <= limit ? text : text[..(limit - 1)] + "…";

	// ---------------------------------------------------------------- ports

	private PointF EntryPoint(VmElement node, int index) {
		var bounds = _bounds[node.Id];
		var entries = GraphTopology.EntriesOf(node);
		var row = entries.ToList().FindIndex(e => e.Index == index);
		if (row < 0) row = 0;
		return new PointF(bounds.X, bounds.Y + (HeaderHeight + PortPadding + PortHeight * row + PortHeight / 2f) * _zoom);
	}

	private PointF ExitPoint(VmElement node, int index) {
		var bounds = _bounds[node.Id];
		var entries = GraphTopology.EntriesOf(node).Count;
		var exits = GraphTopology.ExitsOf(node);
		var row = exits.ToList().FindIndex(e => e.Index == index);
		if (row < 0) row = 0;
		return new PointF(bounds.Right,
			bounds.Y + (HeaderHeight + PortPadding + PortHeight * (entries + row) + PortHeight / 2f) * _zoom);
	}

	/// <summary>The exit whose row is under the point, when the point is on a node's right edge.</summary>
	private int? ExitAt(VmElement node, Point screen) {
		var bounds = _bounds[node.Id];
		if (screen.X < bounds.Right - 14 * _zoom) return null;

		var entries = GraphTopology.EntriesOf(node).Count;
		var offset = (screen.Y - bounds.Y) / _zoom - HeaderHeight - PortPadding;
		var row = (int)(offset / PortHeight) - entries;
		var exits = GraphTopology.ExitsOf(node);
		return row >= 0 && row < exits.Count ? exits[row].Index : null;
	}

	private VmElement? NodeAt(Point screen) =>
		GraphTopology.NodesOf(_container)
			.LastOrDefault(n => _bounds.TryGetValue(n.Id, out var b) && b.Contains(screen));

	/// <summary>The link whose curve passes near the point. Sampled, because a bezier has no cheap hit test.</summary>
	private GraphLink? LinkAt(Point screen) {
		foreach (var link in GraphTopology.LinksOf(_container)) {
			var to = link.Destination?.Element;
			if (to == null || !_bounds.ContainsKey(to.Id)) continue;
			if (link.Source?.Element is { } src && !_bounds.ContainsKey(src.Id)) continue;

			var start = link.Source?.Element is { } from && _bounds.ContainsKey(from.Id)
				? ExitPoint(from, link.SourceExitPointIndex)
				: new PointF(_bounds[to.Id].X - 70 * _zoom, EntryPoint(to, link.DestEntryPointIndex).Y);
			var end = EntryPoint(to, link.DestEntryPointIndex);
			var bend = Math.Max(30f, Math.Abs(end.X - start.X) * 0.4f) * _zoom;

			for (var t = 0f; t <= 1f; t += 0.05f) {
				var point = Bezier(start, new PointF(start.X + bend, start.Y), new PointF(end.X - bend, end.Y), end, t);
				if (Math.Abs(point.X - screen.X) < 6 && Math.Abs(point.Y - screen.Y) < 6) return link;
			}
		}
		return null;
	}

	private static PointF Bezier(PointF a, PointF b, PointF c, PointF d, float t) {
		var u = 1 - t;
		return new PointF(
			u * u * u * a.X + 3 * u * u * t * b.X + 3 * u * t * t * c.X + t * t * t * d.X,
			u * u * u * a.Y + 3 * u * u * t * b.Y + 3 * u * t * t * c.Y + t * t * t * d.Y);
	}

	// ---------------------------------------------------------------- input

	private void OnMouseDown(object? sender, MouseEventArgs e) {
		_lastMouse = e.Location;
		_surface.Focus();

		if (e.Button == MouseButtons.Middle) {
			_panning = true;
			_surface.Cursor = Cursors.SizeAll;
			return;
		}

		var node = NodeAt(e.Location);

		if (e.Button == MouseButtons.Left && node != null) {
			// The right edge of a node is where links come from, so a press there starts one
			// instead of moving the node.
			if (ExitAt(node, e.Location) is { } exit) {
				_linkFrom = node;
				_linkFromExit = exit;
				_linkTo = e.Location;
				return;
			}

			Select(node, null);
			_dragNode = node;
			var world = ToWorld(e.Location);
			var position = Position(node);
			_dragOffset = new PointF(world.X - position.X, world.Y - position.Y);
			return;
		}

		if (e.Button == MouseButtons.Left) {
			Select(null, LinkAt(e.Location));
			return;
		}

		if (e.Button == MouseButtons.Right) {
			if (node != null) Select(node, null);
			else Select(null, LinkAt(e.Location));
			ShowMenu(e.Location);
		}
	}

	private void OnMouseMove(object? sender, MouseEventArgs e) {
		if (_panning) {
			_origin = new PointF(_origin.X + (e.X - _lastMouse.X), _origin.Y + (e.Y - _lastMouse.Y));
			_lastMouse = e.Location;
			_surface.Invalidate();
			return;
		}

		if (_linkFrom != null) {
			_linkTo = e.Location;
			_surface.Invalidate();
			return;
		}

		if (_dragNode != null) {
			var world = ToWorld(e.Location);
			_positions[_dragNode.Id] = new PointF(world.X - _dragOffset.X, world.Y - _dragOffset.Y);
			_surface.Invalidate();
			return;
		}

		var node = NodeAt(e.Location);
		_surface.Cursor = node != null && ExitAt(node, e.Location) != null ? Cursors.Cross : Cursors.Default;
	}

	private void OnMouseUp(object? sender, MouseEventArgs e) {
		if (_panning) {
			_panning = false;
			_surface.Cursor = Cursors.Default;
		}

		if (_linkFrom != null) {
			var target = NodeAt(e.Location);
			if (target != null && !ReferenceEquals(target, _linkFrom)) Connect(_linkFrom, _linkFromExit, target);
			_linkFrom = null;
			_surface.Invalidate();
		}

		_dragNode = null;
	}

	private void OnMouseWheel(object? sender, MouseEventArgs e) {
		var before = ToWorld(e.Location);
		_zoom = Math.Clamp(_zoom * (e.Delta > 0 ? 1.1f : 1 / 1.1f), MinZoom, MaxZoom);
		var after = ToWorld(e.Location);
		_origin = new PointF(_origin.X + (after.X - before.X) * _zoom, _origin.Y + (after.Y - before.Y) * _zoom);
		_surface.Invalidate();
	}

	private void OnDoubleClick(object? sender, MouseEventArgs e) {
		if (NodeAt(e.Location) is { } node) {
			NodeActivated?.Invoke(this, node);
			return;
		}
		if (LinkAt(e.Location) is { } link) LinkActivated?.Invoke(this, link);
	}

	private void Select(VmElement? node, GraphLink? link) {
		_selectedNode = node;
		_selectedLink = link;
		SelectionChanged?.Invoke(this, EventArgs.Empty);
		_surface.Invalidate();
	}

	// ---------------------------------------------------------------- commands

	/// <summary>
	/// Draws a link from one node's exit to another. The endpoints are set here because the
	/// gesture said them; everything else is left to the editor, which opens straight away so a
	/// half-made link is never left lying in the graph.
	/// </summary>
	private void Connect(VmElement from, int exit, VmElement to) {
		var link = VmElement.CreateDefault<GraphLink>(_vm, _container);
		link.Source = new(from);
		link.Destination = new(to);
		link.SourceExitPointIndex = exit;
		link.DestEntryPointIndex = GraphTopology.EntriesOf(to).FirstOrDefault().Index;

		Attach(link, from, to);

		using var editor = new GraphLinkEditorForm(_vm, link);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) {
			Detach(link, from, to);
			_vm.RemoveElement(link);
		}

		Select(null, link);
		GraphChanged?.Invoke(this, EventArgs.Empty);
	}

	private void Attach(GraphLink link, VmElement from, VmElement to) {
		LinksOfContainer()?.Add(link);
		OutputLinksOf(from)?.Add(link);
		InputLinksOf(to)?.Add(link);
	}

	private void Detach(GraphLink link, VmElement? from, VmElement? to) {
		LinksOfContainer()?.Remove(link);
		if (from != null) OutputLinksOf(from)?.Remove(link);
		if (to != null) InputLinksOf(to)?.Remove(link);
	}

	private List<GraphLink>? LinksOfContainer() => _container switch {
		Graph graph => graph.EventLinks,
		Talking talking => talking.EventLinks,
		_ => null
	};

	private static List<GraphLink>? InputLinksOf(VmElement node) => node switch {
		IGraphElement element => element.InputLinks,
		Talking talking => talking.InputLinks,
		Speech speech => speech.InputLinks,
		_ => null
	};

	private static List<GraphLink>? OutputLinksOf(VmElement node) => node switch {
		IGraphElement element => element.OutputLinks,
		Speech speech => speech.OutputLinks,
		_ => null
	};

	private void ShowMenu(Point location) {
		var menu = new ContextMenuStrip();

		if (_selectedNode is { } node) {
			menu.Items.Add("Edit…", null, (_, _) => NodeActivated?.Invoke(this, node));
			menu.Items.Add("Delete node", null, (_, _) => DeleteNode(node));
			menu.Items.Add(new ToolStripSeparator());
		} else if (_selectedLink is { } link) {
			menu.Items.Add("Edit link…", null, (_, _) => LinkActivated?.Invoke(this, link));
			menu.Items.Add(link.Enabled ? "Disable link" : "Enable link", null, (_, _) => {
				link.Enabled = !link.Enabled;
				GraphChanged?.Invoke(this, EventArgs.Empty);
				_surface.Invalidate();
			});
			menu.Items.Add("Delete link", null, (_, _) => DeleteLink(link));
			menu.Items.Add(new ToolStripSeparator());
		}

		if (_container is Graph container) {
			menu.Items.Add("Add state", null, (_, _) => AddNode(VmElement.CreateDefault<State>(_vm, container), location));
			menu.Items.Add("Add branch", null, (_, _) => AddNode(VmElement.CreateDefault<Branch>(_vm, container), location));
		}
		menu.Items.Add("Fit view", null, (_, _) => FitView());
		menu.Items.Add("Re-layout", null, (_, _) => {
			_positions.Clear();
			Relayout();
		});

		menu.Show(_surface, location);
	}

	private void AddNode(VmElement node, Point location) {
		_positions[node.Id] = ToWorld(location);
		switch (_container) {
			case Graph graph: graph.States.Add(new(node)); break;
			case Talking talking: talking.States.Add(new(node)); break;
		}
		Select(node, null);
		GraphChanged?.Invoke(this, EventArgs.Empty);
		_surface.Invalidate();
	}

	private void DeleteNode(VmElement node) {
		var attached = GraphTopology.LinksOf(_container)
			.Where(l => l.Source?.Element == node || l.Destination?.Element == node)
			.ToList();

		var message = $"Delete '{GraphTopology.NameOf(node)}'?";
		if (attached.Count > 0) message += $"\n\n{attached.Count} link(s) touch it and will go too.";
		if (MessageBox.Show(this, message, "Delete node", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
			!= DialogResult.Yes)
			return;

		foreach (var link in attached) DeleteLink(link, confirm: false);

		switch (_container) {
			case Graph graph: graph.States.RemoveAll(s => s.Element == node); break;
			case Talking talking: talking.States.RemoveAll(s => s.Element == node); break;
		}
		_positions.Remove(node.Id);
		_vm.RemoveElement(node);

		Select(null, null);
		GraphChanged?.Invoke(this, EventArgs.Empty);
		_surface.Invalidate();
	}

	private void DeleteLink(GraphLink link, bool confirm = true) {
		if (confirm && MessageBox.Show(this, $"Delete link '{link.Name}'?", "Delete link",
				MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
			return;

		Detach(link, link.Source?.Element, link.Destination?.Element);
		_vm.RemoveElement(link);

		if (ReferenceEquals(link, _selectedLink)) Select(null, null);
		GraphChanged?.Invoke(this, EventArgs.Empty);
		_surface.Invalidate();
	}
}
