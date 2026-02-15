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

namespace P2XMLEditor.Forms.MainForm.Dialogs;

public class DialogGraphViewer : GraphViewer {
    private readonly VirtualMachine _vm;
    private readonly Talking _talking;
    private VmElement? _selectedNode;
    private readonly DialogPropertiesPanel _propertiesPanel;
    private readonly Panel _headerPanel;
    private ContextMenuStrip _backgroundMenu;
    
    private const float NODE_WIDTH = 220f;
    private const float NODE_HEIGHT = 100f;
    private const float VERTICAL_SPACING = 0.05f;
    private const float HORIZONTAL_SPACING = 0.2f;
    private const float CONDITION_NODE_SIZE = 60f;
    private const float ACTION_NODE_SIZE = 60f;

    private enum NodeType {
        Speech,
        Reply,
        Condition,
        Action
    }

    private class DialogNode {
        public VmElement Element { get; set; }
        public NodeType Type { get; set; }
        public int Column { get; set; }
        public int Row { get; set; }
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
    }
    
private void CalculateLayout() {
    _dialogNodes.Clear();
    NodePositions.Clear();

    const float H_SPACING = 1f;
    const float V_SPACING = 1f;

    var adjacency = new Dictionary<ulong, List<ulong>>();
    var reverseAdj = new Dictionary<ulong, List<ulong>>();
    var incoming = new Dictionary<ulong, int>();
    var rawLayer = new Dictionary<ulong, int>();

    void AddNode(ulong id, VmElement? element, NodeType type) {
        if (!_dialogNodes.ContainsKey(id) && element != null) {
            _dialogNodes[id] = new DialogNode {
                Element = element,
                Type = type
            };
        }
        if (!adjacency.ContainsKey(id)) adjacency[id] = new List<ulong>();
        if (!reverseAdj.ContainsKey(id)) reverseAdj[id] = new List<ulong>();
        if (!incoming.ContainsKey(id)) incoming[id] = 0;
        if (!rawLayer.ContainsKey(id)) rawLayer[id] = 0;
    }

    void AddEdge(ulong from, ulong to) {
        adjacency[from].Add(to);
        reverseAdj[to].Add(from);
        incoming[to]++;
    }

    foreach (var state in _talking.States) {
        if (state.Element is not Speech speech) continue;

        AddNode(speech.Id, speech, NodeType.Speech);

        var replies = speech.Replies.OrderBy(r => r.OrderIndex).ToList();

        for (int i = 0; i < replies.Count; i++) {
            var reply = replies[i];

            AddNode(reply.Id, reply, NodeType.Reply);
            AddEdge(speech.Id, reply.Id);

            if (reply.EnableCondition != null) {
                AddNode(reply.EnableCondition.Id, reply.EnableCondition, NodeType.Condition);
                AddEdge(reply.EnableCondition.Id, reply.Id);
            }

            ulong transition = reply.Id;

            if (reply.ActionLine != null) {
                AddNode(reply.ActionLine.Id, reply.ActionLine, NodeType.Action);
                AddEdge(reply.Id, reply.ActionLine.Id);
                transition = reply.ActionLine.Id;
            }

            if (speech.OutputLinks != null && i < speech.OutputLinks.Count) {
                var link = speech.OutputLinks[i];
                if (link.Destination?.Element is Speech nextSpeech) {
                    AddNode(nextSpeech.Id, nextSpeech, NodeType.Speech);
                    AddEdge(transition, nextSpeech.Id);
                }
            }
        }
    }

    

    var queue = new Queue<ulong>(
        incoming.Where(kv => kv.Value == 0).Select(kv => kv.Key));

    var topo = new List<ulong>();

    while (queue.Count > 0) {
        var node = queue.Dequeue();
        topo.Add(node);
        foreach (var child in adjacency[node]) {
            incoming[child]--;
            if (incoming[child] == 0)
                queue.Enqueue(child);
        }
    }

    

    foreach (var node in topo)
        foreach (var child in adjacency[node])
            rawLayer[child] = Math.Max(rawLayer[child], rawLayer[node] + 1);

    

    var visualLayer = new Dictionary<ulong, int>();
    int visualIndex = 0;

    foreach (var group in rawLayer
        .Where(kv => _dialogNodes.ContainsKey(kv.Key))
        .GroupBy(kv => kv.Value)
        .OrderBy(g => g.Key)) {

        foreach (var type in new[] {
            NodeType.Speech,
            NodeType.Reply,
            NodeType.Action
        }) {
            var nodes = group
                .Where(n => _dialogNodes[n.Key].Type == type)
                .Select(n => n.Key)
                .ToList();

            if (!nodes.Any()) continue;

            foreach (var n in nodes)
                visualLayer[n] = visualIndex;

            visualIndex++;
        }
    }

    var column = new Dictionary<ulong, float>();

    

    int rootIndex = 0;
    foreach (var node in _dialogNodes) {
        if (node.Value.Type == NodeType.Speech &&
            reverseAdj[node.Key].Count == 0) {
            column[node.Key] = rootIndex * 4f;
            rootIndex++;
        }
    }

    
    foreach (var node in topo) {

        if (column.ContainsKey(node)) continue;

        var parents = reverseAdj[node]
            .Where(p => column.ContainsKey(p))
            .ToList();

        if (parents.Count == 1) {

            var parent = parents[0];

            var siblings = adjacency[parent]
                .Where(c => _dialogNodes.ContainsKey(c))
                .ToList();

            if (siblings.Count == 1) {
                column[node] = column[parent];
            }
        }
    }


    

    foreach (var speech in _dialogNodes
        .Where(n => n.Value.Type == NodeType.Speech)
        .Select(n => n.Key)) {

        if (!column.ContainsKey(speech)) continue;

        var replies = adjacency[speech]
            .Where(r => _dialogNodes.ContainsKey(r) &&
                        _dialogNodes[r].Type == NodeType.Reply)
            .ToList();

        if (!replies.Any()) continue;

        bool anySkip = false;

        foreach (var r in replies) {
            var targets = adjacency[r]
                .Where(c => _dialogNodes.ContainsKey(c) &&
                            _dialogNodes[c].Type == NodeType.Speech)
                .ToList();

            if (!targets.Any()) continue;

            int skip = visualLayer[targets[0]] - visualLayer[r];
            if (skip > 1) {
                anySkip = true;
                break;
            }
        }

        float spacing = anySkip ? 2f : 1f;
        float center = column[speech];
        int count = replies.Count;

        for (int i = 0; i < count; i++) {
            float offset = i - (count - 1) / 2f;
            column[replies[i]] = center + offset * spacing;
        }
    }

    

    foreach (var node in topo) {
        var parents = reverseAdj[node]
            .Where(p => column.ContainsKey(p))
            .ToList();

        if (parents.Count > 1) {
            float min = parents.Min(p => column[p]);
            float max = parents.Max(p => column[p]);
            column[node] = (min + max) / 2f;
        }
    }

    

    void ShiftSubtree(ulong node, float dx) {
        column[node] += dx;
        foreach (var child in adjacency[node])
            if (column.ContainsKey(child))
                ShiftSubtree(child, dx);
    }

    foreach (var layer in visualLayer.GroupBy(kv => kv.Value)) {

        var nodes = layer
            .Select(n => n.Key)
            .Where(n => column.ContainsKey(n))
            .OrderBy(n => column[n])
            .ToList();

        for (int i = 1; i < nodes.Count; i++) {
            float delta = column[nodes[i]] - column[nodes[i - 1]];
            if (delta < 1f) {
                float shift = 1f - delta;
                ShiftSubtree(nodes[i], shift);
            }
        }
    }

    

    foreach (var id in visualLayer.Keys) {
        float x = column.ContainsKey(id) ? column[id] * H_SPACING : 0f;
        float y = -visualLayer[id] * V_SPACING;
        NodePositions[id] = (x, y);
    }

    

    foreach (var node in _dialogNodes) {
        if (node.Value.Type != NodeType.Condition) continue;

        var parent = reverseAdj[node.Key].FirstOrDefault();
        var child = adjacency[node.Key].FirstOrDefault();

        if (column.ContainsKey(parent) && column.ContainsKey(child)) {
            float x = (column[parent] + column[child]) / 2f * H_SPACING;
            float y = (NodePositions[parent].y + NodePositions[child].y) / 2f;
            NodePositions[node.Key] = (x, y);
        }
    }
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
        using var font = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 9f * ZoomLevel));
        using var smallFont = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 7f * ZoomLevel));
        using var format = new StringFormat { 
            Alignment = StringAlignment.Center, 
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter
        };

        foreach (var (id, node) in _dialogNodes) {
            if (!NodePositions.ContainsKey(id)) continue;
            
            var pos = NodePositions[id];
            var screenPos = GameToScreen(pos.x, pos.y);
            
            
            if (screenPos.X < -500 || screenPos.X > GraphPanel.Width + 500 ||
                screenPos.Y < -500 || screenPos.Y > GraphPanel.Height + 500)
                continue;
            
            switch (node.Type) {
                case NodeType.Speech:
                    DrawSpeechNode(g, node.Element as Speech, screenPos, font, format);
                    break;
                case NodeType.Reply:
                    DrawReplyNode(g, node.Element as Reply, screenPos, font, format);
                    break;
                case NodeType.Condition:
                    DrawConditionNode(g, node.Element as Condition, screenPos, smallFont, format);
                    break;
                case NodeType.Action:
                    DrawActionNode(g, node.Element as ActionLine, screenPos, smallFont, format);
                    break;
            }
        }
    }

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

        
        var authorName = speech.AuthorGuid.Element switch {
            Character c => c.Name,
            Blueprint b => b.Name,
            _ => "Unknown"
        };
        
        var authorBounds = new RectangleF(bounds.X, bounds.Y + 3, bounds.Width, bounds.Height * 0.2f);
        using var authorBrush = new SolidBrush(Color.DarkGoldenrod);
        g.DrawString(authorName, font, authorBrush, authorBounds, format);

        
        var text = speech.Text.GetText("english");
        if (text.Length > 150) text = text.Substring(0, 147) + "...";
        
        var textFormat = new StringFormat { 
            Alignment = StringAlignment.Near, 
            LineAlignment = StringAlignment.Near,
            Trimming = StringTrimming.EllipsisWord,
            FormatFlags = StringFormatFlags.LineLimit
        };
        
        var textBounds = new RectangleF(
            bounds.X + 8, 
            bounds.Y + bounds.Height * 0.25f, 
            bounds.Width - 16, 
            bounds.Height * 0.7f
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

        
        var text = reply.Text.GetText("english");
        if (text.Length > 80) text = text.Substring(0, 77) + "...";
        
        var textBounds = new RectangleF(
            bounds.X + 10, 
            bounds.Y + 5, 
            bounds.Width - 20, 
            bounds.Height - 10
        );
        g.DrawString(text, font, Brushes.Black, textBounds, format);

        
        if (reply.OnlyOnce ?? false) {
            using var flagBrush = new SolidBrush(Color.Orange);
            g.FillEllipse(flagBrush, bounds.Right - 15, bounds.Y + 5, 10, 10);
        }
    }

    private void DrawConditionNode(Graphics g, Condition condition, Point screenPos, Font font, StringFormat format) {
        var size = (int)(CONDITION_NODE_SIZE * ZoomLevel);
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
        var size = (int)(ACTION_NODE_SIZE * ZoomLevel);
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
                        if (!NodePositions.ContainsKey(reply.Id)) continue;
                        
                        var replyPos = NodePositions[reply.Id];
                        
                        
                        if (reply.EnableCondition != null && 
                            NodePositions.ContainsKey(reply.EnableCondition.Id)) {
                            var condPos = NodePositions[reply.EnableCondition.Id];
                            DrawArrow(g, pen, pos, condPos);
                            DrawArrow(g, pen, condPos, replyPos);
                        } else {
                            DrawArrow(g, pen, pos, replyPos);
                        }
                    }
                    break;

                case Reply reply:
                    
                    if (reply.ActionLine != null && 
                        NodePositions.ContainsKey(reply.ActionLine.Id)) {
                        var actionPos = NodePositions[reply.ActionLine.Id];
                        DrawArrow(g, pen, pos, actionPos);
                    }
                    
                    
                    if (reply.Parent is { OutputLinks: not null } parentSpeech &&
                        reply.OrderIndex < parentSpeech.OutputLinks.Count) {
                        var link = parentSpeech.OutputLinks[reply.OrderIndex];

                        if (link.Destination?.Element is Speech nextSpeech && NodePositions.TryGetValue(nextSpeech.Id, out var nextPos)) {
                            DrawArrow(g, pen, pos, nextPos);
                        }
                    }

                    break;
            }
        }
    }

    protected override float GetNodeRadius() => NODE_WIDTH / 2;

    protected override ulong? GetNodeAtPosition(Point screenPoint) {
        foreach (var (nodeId, node) in _dialogNodes) {
            if (!NodePositions.ContainsKey(nodeId)) continue;
            
            var pos = NodePositions[nodeId];
            var nodePos = GameToScreen(pos.x, pos.y);
            
            var (width, height) = node.Type switch {
                NodeType.Speech or NodeType.Reply => (NODE_WIDTH, NODE_HEIGHT),
                NodeType.Condition => (CONDITION_NODE_SIZE, CONDITION_NODE_SIZE),
                NodeType.Action => (ACTION_NODE_SIZE, ACTION_NODE_SIZE),
                _ => (0f, 0f)
            };
            
            var w = (int)(width * ZoomLevel);
            var h = (int)(height * ZoomLevel);
            var bounds = new Rectangle(nodePos.X - w/2, nodePos.Y - h/2, w, h);
            
            if (bounds.Contains(screenPoint)) return nodeId;
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

    protected override void HandleNodeMoved(ulong nodeId, (float x, float y) newPosition) {
        
        NodePositions[nodeId] = newPosition;
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
        MessageBox.Show($"Speech Editor not yet implemented.\n\nText: {speech.Text.GetText("english")}", 
            "Not Implemented", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void EditReply(Reply reply) {
        MessageBox.Show($"Reply Editor not yet implemented.\n\nText: {reply.Text.GetText("english")}", 
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