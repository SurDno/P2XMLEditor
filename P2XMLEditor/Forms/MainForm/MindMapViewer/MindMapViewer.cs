using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Forms.MainForm.MindMapViewer;

public class MindMapViewer : GraphViewer {
    private readonly VirtualMachine _vm;
    private readonly MindMap _mindMap;
    private MindMapNode? _selectedNode;
    private MindMapNode? _linkSourceNode;
    private readonly NodePropertiesPanel _propertiesPanel;
    private ContextMenuStrip _backgroundMenu;
    
    private const float CIRCLE_SIZE = 100f; 

    public MindMapViewer(VirtualMachine vm, MindMap mindMap) {
        _vm = vm;
        _mindMap = mindMap;
        
        foreach (var node in mindMap.Nodes) 
            NodePositions[node.Id] = (node.GameScreenPosX, node.GameScreenPosY);

        _propertiesPanel = new NodePropertiesPanel(vm) { 
            Dock = DockStyle.Right, 
            Width = 350 
        };
        Controls.Add(_propertiesPanel);

        InitializeContextMenu();
        CenterView();
    }
    
    protected override void OnMouseClick(MouseEventArgs e) {
        base.OnMouseClick(e);
        if (e.Button == MouseButtons.Right && GetNodeAtPosition(e.Location) != null) {
            GraphPanel.ContextMenuStrip.Hide();
        }
    }

    private void InitializeContextMenu() {
        _backgroundMenu = new ContextMenuStrip();
        _backgroundMenu.Items.Add("Add Node", null, (_, _) => 
            AddNewNode(ScreenToGame(GraphPanel.PointToClient(Cursor.Position))));
        _backgroundMenu.Items.Add("Fit View", null, (_, _) => CenterView());

        GraphPanel.MouseClick += (_, e) => {
            if (e.Button == MouseButtons.Right && GetNodeAtPosition(e.Location) == null) {
                _backgroundMenu.Show(GraphPanel, e.Location);
            }
        };
    }

    protected override void DrawNodes(Graphics g) {
        using var font = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 10f * ZoomLevel));
        var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

        foreach (var node in _mindMap.Nodes) {
            var pos = NodePositions[node.Id];
            var screenPos = GameToScreen(pos.x, pos.y);
            var size = (int)(CIRCLE_SIZE * ZoomLevel);
            var nodeBounds = new Rectangle(
                screenPos.X - size/2,
                screenPos.Y - size/2,
                size, size
            );

            using (var path = new GraphicsPath()) {
                path.AddEllipse(nodeBounds);
                using var brush = new LinearGradientBrush(
                    nodeBounds,
                    node == _selectedNode ? Color.LightBlue : Color.White,
                    node == _selectedNode ? Color.LightSkyBlue : Color.WhiteSmoke,
                    45f
                );
                if (path.GetBounds() is not { Width: > 0 and < 10000, Height: > 0 and < 10000 }) return;
                g.FillPath(brush, path);
                using var pen = new Pen(Color.Black, Math.Max(1.0f, 1.5f * ZoomLevel));
                g.DrawPath(pen, path);
            }

            g.DrawString(node.Name, font, Brushes.Black, nodeBounds, format);
        }
    }

    protected override void DrawEdges(Graphics g) {
        using var pen = new Pen(Color.Gray, Math.Max(1.0f, 1.5f * ZoomLevel));
        pen.CustomEndCap = new AdjustableArrowCap(5 * ZoomLevel, 5 * ZoomLevel);
        
        foreach (var link in _mindMap.Links) {
            var sourcePos = NodePositions[link.Source.Id];
            var destPos = NodePositions[link.Destination.Id];
            DrawArrow(g, pen, sourcePos, destPos);
        }

        if (_linkSourceNode != null) {
            using var tempPen = new Pen(Color.Gray, Math.Max(1.0f, 1.5f * ZoomLevel)) { DashStyle = DashStyle.Dash };
            var start = NodePositions[_linkSourceNode.Id];
            var startPoint = GameToScreen(start.x, start.y);
            var mousePos = GraphPanel.PointToClient(Cursor.Position);
            g.DrawLine(tempPen, startPoint, mousePos);
        }
    }

    protected override float GetNodeRadius() => CIRCLE_SIZE / 2;

    protected override ulong? GetNodeAtPosition(Point screenPoint) {
        foreach (var (nodeId, pos) in NodePositions) {
            var nodePos = GameToScreen(pos.x, pos.y);
            var size = (int)(CIRCLE_SIZE * ZoomLevel);
            var bounds = new Rectangle(nodePos.X - size/2, nodePos.Y - size/2, size, size);
            if (bounds.Contains(screenPoint)) return nodeId;
        }
        return null;
    }

    [SuppressMessage("ReSharper", "SwitchStatementMissingSomeEnumCasesNoDefault")]
    protected override void HandleNodeClick(ulong nodeId, MouseButtons button, Point screenPoint) {
        var node = _mindMap.Nodes.First(n => n.Id == nodeId);
        
        switch (button) {
            case MouseButtons.Left when _linkSourceNode != null: {
                if (_linkSourceNode != node) AddLink(_linkSourceNode, node);
                _linkSourceNode = null;
                break;
            }
            case MouseButtons.Left:
                _selectedNode = node;
                _propertiesPanel.SetNode(_selectedNode);
                break;
            case MouseButtons.Right:
                ShowNodeContextMenu(node, screenPoint);
                break;
        }
        
        GraphPanel.Invalidate();
    }

    protected override void HandleNodeMoved(ulong nodeId, (float x, float y) newPosition) {
        var node = _mindMap.Nodes.First(n => n.Id == nodeId);
        node.GameScreenPosX = newPosition.x;
        node.GameScreenPosY = newPosition.y;
    }

    private void ShowNodeContextMenu(MindMapNode node, Point location) {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Remove Node", null, (_, _) => RemoveNode(node));
        menu.Items.Add("Start Link", null, (_, _) => {
            _linkSourceNode = node;
            GraphPanel.Cursor = Cursors.Cross;
        });
        menu.Items.Add(new ToolStripSeparator());

        var linksMenu = new ToolStripMenuItem("Links");
        menu.Items.Add(linksMenu);
        var addLinkMenu = new ToolStripMenuItem("Add Link to...");
        foreach (var target in _mindMap.Nodes.Where(n => n != node && node.OutputLinks.All(l => l.Destination != n)))
            addLinkMenu.DropDownItems.Add(target.Name, null, (_, _) => AddLink(node, target));
        linksMenu.DropDownItems.Add(addLinkMenu);
        var removeLinkMenu = new ToolStripMenuItem("Remove Link to...");
        foreach (var link in node.OutputLinks)
            removeLinkMenu.DropDownItems.Add(link.Destination.Name, null, (_, _) => RemoveLink(link));
        linksMenu.DropDownItems.Add(removeLinkMenu);
        menu.Show(GraphPanel.PointToScreen(location));
    }

    private void AddNewNode((float x, float y) position) {
        var node = VmElement.CreateDefault<MindMapNode>(_vm, _mindMap);
        node.GameScreenPosX = position.x;
        node.GameScreenPosY = position.y;
        NodePositions[node.Id] = (node.GameScreenPosX, node.GameScreenPosY);
        GraphPanel.Invalidate();
    }

    private void RemoveNode(MindMapNode node) {
        NodePositions.Remove(node.Id);
        if (_selectedNode == node) {
            _selectedNode = null;
            _propertiesPanel.SetNode(null);
        }
        GraphPanel.Invalidate();
    }

    private void AddLink(MindMapNode source, MindMapNode destination) {
        var link = VmElement.CreateDefault<MindMapLink>(_vm, _mindMap);
        link.Source = source;
        link.Destination = destination;
        source.OutputLinks.Add(link);
        destination.InputLinks.Add(link);
        GraphPanel.Invalidate();
    }

    private void RemoveLink(MindMapLink link) {
        _vm.RemoveElement(link);
        GraphPanel.Invalidate();
    }
    
    public void RefreshView() => GraphPanel.Invalidate();
}