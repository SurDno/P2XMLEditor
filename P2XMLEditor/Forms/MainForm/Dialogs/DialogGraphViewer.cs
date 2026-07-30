using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Services;

namespace P2XMLEditor.Forms.MainForm.Dialogs;

public class DialogGraphViewer : GraphViewer {
	private readonly VirtualMachine _vm;
	private readonly Talking _talking;
	private VmElement? _selectedNode;
	private readonly DialogPropertiesPanel _propertiesPanel;
	private readonly Panel _headerPanel;
	private ContextMenuStrip _backgroundMenu;
	private readonly Dictionary<ulong, (ulong from, ulong to)> _conditionEdgeMarkers = [], _actionEdgeMarkers = [];
	private readonly HashSet<ulong> _decoratorIds = [];

	private const float NODE_WIDTH = 220f;
	private const float NODE_HEIGHT = 100f;
	private const float H_SPACING = 1.0f;
	private const float V_SPACING = 0.55f;
	private const float DECORATOR_SIZE = 60f;

	private enum NodeType {
		Speech,
		Reply,
		Condition,
		Action
	}

	private class DialogNode {
		public VmElement Element { get; set; }
		public NodeType Type { get; set; }
	}

	private readonly Dictionary<ulong, DialogNode> _dialogNodes = new();

	public DialogGraphViewer(VirtualMachine vm, Talking talking) {
		_vm = vm;
		_talking = talking;
		
		_headerPanel = new Panel { 
			Dock = DockStyle.Top, 
			Height = 80,
			BackColor = SystemColors.Control,
			Padding = new Padding(10)
		};
		
		var nameLabel = new Label {
			Text = "Dialog Name:",
			Location = new Point(10, 15),
			AutoSize = true
		};
		_headerPanel.Controls.Add(nameLabel);
		
		var nameTextBox = new TextBox {
			Text = _talking.Name,
			Location = new Point(100, 12),
			Width = 200
		};
		nameTextBox.TextChanged += (_, _) => _talking.Name = nameTextBox.Text;
		_headerPanel.Controls.Add(nameTextBox);
		
		var ownerLabel = new Label {
			Text = "Owner:",
			Location = new Point(10, 45),
			AutoSize = true
		};
		_headerPanel.Controls.Add(ownerLabel);
		
		var ownerName = _talking.Owner.Element switch {
			Character c => $"Character: {c.Name}",
			Blueprint b => $"Blueprint: {b.Name}",
			_ => "Unknown"
		};
		var ownerText = new Label {
			Text = ownerName,
			Location = new Point(100, 45),
			AutoSize = true,
			ForeColor = SystemColors.GrayText
		};
		_headerPanel.Controls.Add(ownerText);
		
		Controls.Add(_headerPanel);
		
		CalculateLayout();

		_propertiesPanel = new DialogPropertiesPanel(vm) { 
			Dock = DockStyle.Right, 
			Width = 350 
		};
		Controls.Add(_propertiesPanel);

		InitializeContextMenu();
		CenterView();

		PreviewLanguageService.LanguageChanged += OnPreviewLanguageChanged;
	}

	private void OnPreviewLanguageChanged(string _) {
		if (IsDisposed) return;
		if (InvokeRequired) {
			Invoke(() => OnPreviewLanguageChanged(_));
			return;
		}
		GraphPanel.Invalidate();
	}

	protected override void Dispose(bool disposing) {
		if (disposing)
			PreviewLanguageService.LanguageChanged -= OnPreviewLanguageChanged;
		base.Dispose(disposing);
	}

	private void CalculateLayout() {

		_dialogNodes.Clear();
		NodePositions.Clear();
		_conditionEdgeMarkers.Clear();
		_actionEdgeMarkers.Clear();
		_decoratorIds.Clear();

		var adjacency = new Dictionary<ulong, List<ulong>>();
		var reverseAdj = new Dictionary<ulong, List<ulong>>();
		var incoming = new Dictionary<ulong, int>();
		var layer = new Dictionary<ulong, int>();
		var layoutNodes = new HashSet<ulong>();

		void AddDialogNode(ulong id, VmElement element, NodeType type) {
			if (!_dialogNodes.ContainsKey(id)) {
				_dialogNodes[id] = new DialogNode {
					Element = element,
					Type = type
				};
			}
		}

		void EnsureLayoutNode(ulong id) {
			if (!layoutNodes.Add(id)) {
				return;
			}

			if (!adjacency.ContainsKey(id)) adjacency[id] = [];
			if (!reverseAdj.ContainsKey(id)) reverseAdj[id] = [];
			if (!incoming.ContainsKey(id)) incoming[id] = 0;
			if (!layer.ContainsKey(id)) layer[id] = 0;
		}

		void AddLayoutNode(ulong id, VmElement element, NodeType type) {
			AddDialogNode(id, element, type);
			EnsureLayoutNode(id);
		}

		void AddDecoratorNode(ulong id, VmElement element, NodeType type, ulong from, ulong to) {
			AddDialogNode(id, element, type);
			_decoratorIds.Add(id);

			if (type == NodeType.Condition) {
				_conditionEdgeMarkers[id] = (from, to);
			} else if (type == NodeType.Action) {
				_actionEdgeMarkers[id] = (from, to);
			}
		}

		void AddLayoutEdge(ulong from, ulong to) {
			EnsureLayoutNode(from);
			EnsureLayoutNode(to);

			adjacency[from].Add(to);
			reverseAdj[to].Add(from);
			incoming[to]++;
		}

		foreach (var state in _talking.States) {
			if (state.Element is not Speech speech) continue;

			AddLayoutNode(speech.Id, speech, NodeType.Speech);

			var replies = speech.Replies.OrderBy(r => r.OrderIndex).ToList();

			for (var i = 0; i < replies.Count; i++) {
				var reply = replies[i];
				AddLayoutNode(reply.Id, reply, NodeType.Reply);

				if (reply.EnableCondition != null) {
					var cond = reply.EnableCondition;
					AddDecoratorNode(cond.Id, cond, NodeType.Condition, speech.Id, reply.Id);
				}

				AddLayoutEdge(speech.Id, reply.Id);

				Speech? nextSpeech = null;

				if (speech.OutputLinks != null) {
					var link = speech.OutputLinks.FirstOrDefault(l => l.SourceExitPointIndex == i);
					if (link?.Destination?.Element is Speech ns) {
						nextSpeech = ns;
						AddLayoutNode(ns.Id, ns, NodeType.Speech);
					}
				}

				if (reply.ActionLine != null) {
					var action = reply.ActionLine;

					if (nextSpeech != null) {
						AddDecoratorNode(action.Id, action, NodeType.Action, reply.Id, nextSpeech.Id);
						AddLayoutEdge(reply.Id, nextSpeech.Id);
					} else {
						AddLayoutNode(action.Id, action, NodeType.Action);
						AddLayoutEdge(reply.Id, action.Id);
					}
				} else {
					if (nextSpeech != null) {
						AddLayoutEdge(reply.Id, nextSpeech.Id);
					}
				}
			}
		}

		if (layoutNodes.Count == 0) {
			return;
		}

		var queue = new Queue<ulong>(incoming.Where(x => x.Value == 0).Select(x => x.Key));
		var topo = new List<ulong>();

		while (queue.Count > 0) {
			var n = queue.Dequeue();
			topo.Add(n);

			foreach (var c in adjacency[n]) {
				incoming[c]--;
				if (incoming[c] == 0) {
					queue.Enqueue(c);
				}
			}
		}
		
		if (topo.Count != layoutNodes.Count)
		{
			foreach (var id in layoutNodes)
			{
				if (!topo.Contains(id))
				{
					topo.Add(id);
				}
			}
		}

		foreach (var n in topo) {
			foreach (var c in adjacency[n]) {
				layer[c] = Math.Max(layer[c], layer[n] + 1);
			}
		}

		var layers = layer
			.Where(kv => layoutNodes.Contains(kv.Key))
			.GroupBy(kv => kv.Value)
			.OrderBy(g => g.Key)
			.Select(g => g.Select(x => x.Key).ToList())
			.ToList();

		var halfWidth = new Dictionary<ulong, float>();

		foreach (var n in layoutNodes) {
			var childCount = adjacency.TryGetValue(n, out var list) ? list.Count : 0;
			halfWidth[n] = childCount > 1 ? (childCount - 1) * H_SPACING * 0.5f : 0f;
		}

		var xPos = new Dictionary<ulong, float>();
		
		var roots = layers[0];
		var cursor = 0f;

		for (var i = 0; i < roots.Count; i++) {
			var node = roots[i];

			if (i == 0) {
				xPos[node] = 0f;
				cursor = 0f;
			} else {
				var prev = roots[i - 1];

				var minDist =
					halfWidth[prev] +
					halfWidth[node] +
					H_SPACING;

				cursor += minDist;
				xPos[node] = cursor;
			}
		}

		{
			var minR = roots.Min(r => xPos[r]);
			var maxR = roots.Max(r => xPos[r]);
			var midR = (minR + maxR) * 0.5f;

			for (var i = 0; i < roots.Count; i++) {
				xPos[roots[i]] -= midR;
			}
		}

		for (var li = 1; li < layers.Count; li++) {
			var current = layers[li];

			var blocks = new List<(ulong parent, List<ulong> nodes)>();
			var grouped = new HashSet<ulong>();

			var groups = new Dictionary<ulong, List<ulong>>();

			for (var i = 0; i < current.Count; i++) {
				var node = current[i];

				if (!reverseAdj.TryGetValue(node, out var parents) || parents.Count != 1) {
					continue;
				}

				var p = parents[0];
				if (!xPos.ContainsKey(p)) {
					continue;
				}

				if (!groups.TryGetValue(p, out var list)) {
					list = [];
					groups[p] = list;
				}

				list.Add(node);
			}

			foreach (var kv in groups) {
				var nodes = kv.Value;
				nodes.Sort((a, b) => current.IndexOf(a).CompareTo(current.IndexOf(b)));

				blocks.Add((kv.Key, nodes));

				for (var i = 0; i < nodes.Count; i++) {
					grouped.Add(nodes[i]);
				}
			}

			for (var i = 0; i < current.Count; i++) {
				var node = current[i];
				if (grouped.Contains(node)) {
					continue;
				}

				blocks.Add((0UL, [node]));
			}

			for (var b = 0; b < blocks.Count; b++) {
				var (parent, nodes) = blocks[b];

				if (parent != 0UL) {
					var parentX = xPos[parent];
					var center = (nodes.Count - 1) * 0.5f;

					for (var i = 0; i < nodes.Count; i++) {
						xPos[nodes[i]] = parentX + (i - center) * H_SPACING;
					}
				} else {
					var node = nodes[0];

					if (!reverseAdj.TryGetValue(node, out var parents) || parents.Count == 0) {
						xPos[node] = 0f;
					} else {
						var positionedParents = parents.Where(p => xPos.ContainsKey(p)).ToList();
						xPos[node] = positionedParents.Count == 0 ? 0f : positionedParents.Average(p => xPos[p]);
					}
				}
			}

			float BlockCenterX((ulong parent, List<ulong> nodes) block) {
				var min = float.PositiveInfinity;
				var max = float.NegativeInfinity;

				for (var i = 0; i < block.nodes.Count; i++) {
					var x = xPos[block.nodes[i]];
					if (x < min) min = x;
					if (x > max) max = x;
				}

				return (min + max) * 0.5f;
			}

			blocks = blocks.OrderBy(b => BlockCenterX(b)).ToList();

			float BlockMin((ulong parent, List<ulong> nodes) block) {
				var min = float.PositiveInfinity;

				for (var i = 0; i < block.nodes.Count; i++) {
					var n = block.nodes[i];
					var x = xPos[n] - halfWidth[n];
					if (x < min) min = x;
				}

				return min;
			}

			float BlockMax((ulong parent, List<ulong> nodes) block) {
				var max = float.NegativeInfinity;

				for (var i = 0; i < block.nodes.Count; i++) {
					var n = block.nodes[i];
					var x = xPos[n] + halfWidth[n];
					if (x > max) max = x;
				}

				return max;
			}

			for (var i = 1; i < blocks.Count; i++) {
				var leftBlock = blocks[i - 1];
				var rightBlock = blocks[i];

				var gap = BlockMin(rightBlock) - BlockMax(leftBlock);
				if (gap >= H_SPACING) {
					continue;
				}

				var dx = H_SPACING - gap;

				for (var k = 0; k < rightBlock.nodes.Count; k++) {
					xPos[rightBlock.nodes[k]] += dx;
				}
			}
		}

		var minX = float.PositiveInfinity;
		var maxX = float.NegativeInfinity;

		foreach (var id in layoutNodes) {
			if (!xPos.ContainsKey(id)) continue;

			var x = xPos[id];
			if (x < minX) minX = x;
			if (x > maxX) maxX = x;
		}

		var mid = (minX + maxX) * 0.5f;

		foreach (var id in layoutNodes) {
			if (!xPos.ContainsKey(id)) continue;
			xPos[id] -= mid;
		}

		foreach (var id in layoutNodes) 
			NodePositions[id] = (xPos.GetValueOrDefault(id, 0f), -layer.GetValueOrDefault(id, 0) * V_SPACING);
	}

	private bool TryGetNodeGamePosition(ulong nodeId, out (float x, float y) pos) {
		if (_conditionEdgeMarkers.TryGetValue(nodeId, out var e1)) {
			if (NodePositions.TryGetValue(e1.from, out var a) && NodePositions.TryGetValue(e1.to, out var b)) {
				pos = ((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f);
				return true;
			}
		}

		if (_actionEdgeMarkers.TryGetValue(nodeId, out var e2)) {
			if (NodePositions.TryGetValue(e2.from, out var a) && NodePositions.TryGetValue(e2.to, out var b)) {
				pos = ((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f);
				return true;
			}
		}

		return NodePositions.TryGetValue(nodeId, out pos);
	}

	private void InitializeContextMenu() {
		_backgroundMenu = new ContextMenuStrip();
		_backgroundMenu.Items.Add("Fit View", null, (_, _) => CenterView());
		GraphPanel.MouseClick += (_, e) => {
			if (e.Button == MouseButtons.Right && GetNodeAtPosition(e.Location) == null) {
				_backgroundMenu.Show(GraphPanel, e.Location);
			}
		};
	}

	protected override void DrawNodes(Graphics g) {
		using var font = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 8f * ZoomLevel));
		using var smallFont = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 7f * ZoomLevel));
		using var format = new StringFormat { 
			Alignment = StringAlignment.Center, 
			LineAlignment = StringAlignment.Center,
			Trimming = StringTrimming.EllipsisCharacter
		};

		foreach (var (id, node) in _dialogNodes) {
			if (!TryGetNodeGamePosition(id, out var pos)) continue;
			
			var screenPos = GameToScreen(pos.x, pos.y);
			
			
			if (screenPos.X < -500 || screenPos.X > GraphPanel.Width + 500 ||
				screenPos.Y < -500 || screenPos.Y > GraphPanel.Height + 500)
				continue;
			
			switch (node.Type) {
				case NodeType.Speech:
					DrawSpeechNode(g, (Speech)node.Element, screenPos, font, format);
					break;
				case NodeType.Reply:
					DrawReplyNode(g, (Reply)node.Element, screenPos, font, format);
					break;
				case NodeType.Condition:
					DrawConditionNode(g, (Condition)node.Element, screenPos, smallFont, format);
					break;
				case NodeType.Action:
					DrawActionNode(g, (ActionLine)node.Element, screenPos, smallFont, format);
					break;
			}
		}
	}
	
	private void DrawSmartArrow(Graphics g, Pen pen, (float x, float y) from, (float x, float y) to) {
		if (to.y < from.y) {
			DrawArrow(g, pen, from, to);
			return;
		}

		var p1 = GameToScreen(from.x, from.y);
		var p2 = GameToScreen(to.x, to.y);

		float dx = p2.X - p1.X;
		float dy = p2.Y - p1.Y;

		var controlOffset = Math.Max(80f * ZoomLevel, Math.Abs(dx) * 0.5f);

		var c1 = new PointF(p1.X, p1.Y - controlOffset);
		var c2 = new PointF(p2.X, p2.Y - controlOffset);

		using var path = new GraphicsPath();
		path.AddBezier(p1, c1, c2, p2);

		g.DrawPath(pen, path);
	}

	protected override bool CanMoveNode(ulong nodeId) => !_decoratorIds.Contains(nodeId);
	
	
	private void DrawSpeechNode(Graphics g, Speech speech, Point screenPos, Font font, StringFormat format) {
		var width = (int)(NODE_WIDTH * ZoomLevel);
		var height = (int)(NODE_HEIGHT * ZoomLevel);
		var bounds = new Rectangle(
			screenPos.X - width / 2,
			screenPos.Y - height / 2,
			width, height
		);

		
		var fillColor = speech == _selectedNode ? Color.LightBlue : Color.LightYellow;
		using var brush = new SolidBrush(fillColor);
		g.FillRectangle(brush, bounds);

		
		using var pen = new Pen(Color.DarkGoldenrod, Math.Max(1.0f, 2f * ZoomLevel));
		g.DrawRectangle(pen, bounds);
		
		// Use the currently selected preview language
		var text = speech.Text.GetText(PreviewLanguageService.CurrentLanguage);
		
		var textFormat = new StringFormat { 
			Alignment = StringAlignment.Near, 
			LineAlignment = StringAlignment.Near,
			Trimming = StringTrimming.EllipsisWord,
			FormatFlags = StringFormatFlags.LineLimit
		};
		
		var textBounds = new RectangleF(
			bounds.X + 8,
			bounds.Y + 8,
			bounds.Width - 16,
			bounds.Height - 16
		);
		g.DrawString(text, font, Brushes.Black, textBounds, textFormat);
	}

	private void DrawReplyNode(Graphics g, Reply reply, Point screenPos, Font font, StringFormat format) {
		var width = (int)(NODE_WIDTH * ZoomLevel);
		var height = (int)(NODE_HEIGHT * ZoomLevel);
		var bounds = new Rectangle(
			screenPos.X - width / 2,
			screenPos.Y - height / 2,
			width, height
		);

		
		var points = new[] {
			new Point(bounds.X + bounds.Width / 4, bounds.Y),
			new Point(bounds.Right - bounds.Width / 4, bounds.Y),
			new Point(bounds.Right, bounds.Y + bounds.Height / 2),
			new Point(bounds.Right - bounds.Width / 4, bounds.Bottom),
			new Point(bounds.X + bounds.Width / 4, bounds.Bottom),
			new Point(bounds.X, bounds.Y + bounds.Height / 2)
		};

		var fillColor = reply == _selectedNode ? Color.LightBlue : Color.LightGreen;
		using var brush = new SolidBrush(fillColor);
		g.FillPolygon(brush, points);

		using var pen = new Pen(Color.DarkGreen, Math.Max(1.0f, 2f * ZoomLevel));
		g.DrawPolygon(pen, points);

		
		var text = reply.Text.GetText(PreviewLanguageService.CurrentLanguage);
		if (text.Length > 80) text = text[..77] + "...";
		
		var textBounds = new RectangleF(
			bounds.X + 10, 
			bounds.Y + 5, 
			bounds.Width - 20, 
			bounds.Height - 10
		);
		g.DrawString(text, font, Brushes.Black, textBounds, format);

		
		if (reply.OnlyOnce) {
			using var flagBrush = new SolidBrush(Color.Orange);
			g.FillEllipse(flagBrush, bounds.Right - 15, bounds.Y + 5, 10, 10);
		}
	}

	private void DrawConditionNode(Graphics g, Condition condition, Point screenPos, Font font, StringFormat format) {
		var size = (int)(DECORATOR_SIZE * ZoomLevel);
		var bounds = new Rectangle(
			screenPos.X - size / 2,
			screenPos.Y - size / 2,
			size, size
		);

		
		var points = new[] {
			new Point(bounds.X + bounds.Width / 2, bounds.Y),
			new Point(bounds.Right, bounds.Y + bounds.Height / 2),
			new Point(bounds.X + bounds.Width / 2, bounds.Bottom),
			new Point(bounds.X, bounds.Y + bounds.Height / 2)
		};

		var fillColor = condition == _selectedNode ? Color.LightBlue : Color.LightCoral;
		using var brush = new SolidBrush(fillColor);
		g.FillPolygon(brush, points);

		using var pen = new Pen(Color.DarkRed, Math.Max(1.0f, 1.5f * ZoomLevel));
		g.DrawPolygon(pen, points);

		g.DrawString("?", font, Brushes.DarkRed, bounds, format);
	}

	private void DrawActionNode(Graphics g, ActionLine actionLine, Point screenPos, Font font, StringFormat format) {
		var size = (int)(DECORATOR_SIZE * ZoomLevel);
		var bounds = new Rectangle(
			screenPos.X - size / 2,
			screenPos.Y - size / 2,
			size, size
		);

		var fillColor = actionLine == _selectedNode ? Color.LightBlue : Color.Lavender;
		using var brush = new SolidBrush(fillColor);
		g.FillEllipse(brush, bounds);

		using var pen = new Pen(Color.Purple, Math.Max(1.0f, 1.5f * ZoomLevel));
		g.DrawEllipse(pen, bounds);

	    g.DrawString("A", font, Brushes.Purple, bounds, format);
    }

    protected override void DrawEdges(Graphics g) {
        using var pen = new Pen(Color.Gray, Math.Max(1.0f, 1.5f * ZoomLevel));
        pen.CustomEndCap = new AdjustableArrowCap(5 * ZoomLevel, 5 * ZoomLevel);

        foreach (var (id, node) in _dialogNodes) {
            if (!NodePositions.ContainsKey(id)) continue;
            
            var pos = NodePositions[id];
            
            switch (node.Element) {
                case Speech speech:
                    
                    foreach (var reply in speech.Replies.OrderBy(r => r.OrderIndex)) {
                        if (!NodePositions.TryGetValue(reply.Id, out var replyPos)) continue;


                        if (reply.EnableCondition != null && 
                            NodePositions.ContainsKey(reply.EnableCondition.Id)) {
                            var condPos = NodePositions[reply.EnableCondition.Id];
                            DrawSmartArrow(g, pen, pos, condPos);
                            DrawSmartArrow(g, pen, condPos, replyPos);
                        } else {
                            DrawSmartArrow(g, pen, pos, replyPos);
                        }
                    }
                    break;

                case Reply reply:
                    if (reply.ActionLine != null && NodePositions.ContainsKey(reply.ActionLine.Id)) {
                        var actionPos = NodePositions[reply.ActionLine.Id];
                        DrawSmartArrow(g, pen, pos, actionPos);
                    }
    
                    if (reply.Parent is { OutputLinks: not null } parentSpeech) {
        
                        var replyIndex = parentSpeech.Replies.OrderBy(r => r.OrderIndex).ToList().IndexOf(reply);
        
                        if (replyIndex >= 0) {
                            var link = parentSpeech.OutputLinks.FirstOrDefault(l => l.SourceExitPointIndex == replyIndex);
            
                            if (link?.Destination?.Element is Speech nextSpeech && 
                                NodePositions.TryGetValue(nextSpeech.Id, out var nextPos)) {
                                DrawSmartArrow(g, pen, pos, nextPos);
                            }
                        }
                    }
                    break;
            }
        }
    }

    protected override float GetNodeRadius() => NODE_WIDTH / 2;

    protected override ulong? GetNodeAtPosition(Point screenPoint) {
        foreach (var (nodeId, node) in _dialogNodes) {
            if (!TryGetNodeGamePosition(nodeId, out var pos)) continue;
            
            var nodePos = GameToScreen(pos.x, pos.y);

            var (width, height) = node.Type switch {
                NodeType.Speech or NodeType.Reply => (NODE_WIDTH, NODE_HEIGHT),
                NodeType.Condition => (DECORATOR_SIZE, DECORATOR_SIZE),
                NodeType.Action => (DECORATOR_SIZE, DECORATOR_SIZE),
                _ => (0f, 0f)
            };

            var w = (int)(width * ZoomLevel);
            var h = (int)(height * ZoomLevel);
            var bounds = new Rectangle(nodePos.X - w / 2, nodePos.Y - h / 2, w, h);

            if (bounds.Contains(screenPoint)) {
                return nodeId;
            }
        }

        return null;
    }
    

    protected override void HandleNodeClick(ulong nodeId, MouseButtons button, Point screenPoint) {
        if (!_dialogNodes.TryGetValue(nodeId, out var node))
            return;

        if (button == MouseButtons.Left) {
            _selectedNode = node.Element;
            _propertiesPanel.SetElement(_selectedNode);
            GraphPanel.Invalidate();
        } else if (button == MouseButtons.Right) {
            ShowNodeContextMenu(node.Element, screenPoint);
        }
    }

    protected override void HandleNodeMoved(ulong nodeId, (float x, float y) newPosition)
    {
        if (_decoratorIds.Contains(nodeId)) return;
        NodePositions[nodeId] = newPosition;
        GraphPanel.Invalidate();
    }


    private void ShowNodeContextMenu(VmElement element, Point location) {
        var menu = new ContextMenuStrip();
        
        menu.Items.Add($"ID: {element.Id}", null, null).Enabled = false;
        menu.Items.Add(new ToolStripSeparator());
        
        switch (element) {
            case Speech speech:
                menu.Items.Add("Edit Speech", null, (_, _) => EditSpeech(speech));
                break;
            case Reply reply:
                menu.Items.Add("Edit Reply", null, (_, _) => EditReply(reply));
                break;
            case Condition condition:
                menu.Items.Add("Edit Condition", null, (_, _) => EditCondition(condition));
                break;
            case ActionLine actionLine:
                menu.Items.Add("View Actions", null, (_, _) => ViewActionLine(actionLine));
                break;
        }

        menu.Show(GraphPanel.PointToScreen(location));
    }

    private void EditSpeech(Speech speech) {
        MessageBox.Show($"Speech Editor not yet implemented.\n\nText: {speech.Text.GetText(PreviewLanguageService.CurrentLanguage)}", 
            "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EditReply(Reply reply) {
        MessageBox.Show($"Reply Editor not yet implemented.\n\nText: {reply.Text.GetText(PreviewLanguageService.CurrentLanguage)}", 
            "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EditCondition(Condition condition) {
        MessageBox.Show($"Condition: {PreviewHelper.Preview(condition)}", 
            "Condition Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ViewActionLine(ActionLine actionLine) {
        MessageBox.Show($"ActionLine: {actionLine.Name}\nType: {actionLine.ActionLineType.Serialize()}", 
            "ActionLine Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public void RefreshView() {
        CalculateLayout();
        GraphPanel.Invalidate();
    }
}
