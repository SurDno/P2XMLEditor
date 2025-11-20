using System.Drawing.Drawing2D;
using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Forms.MainForm.FiniteStateMachines;

public class FSMGraphViewer : GraphViewer {
    private readonly VirtualMachine _vm;
    private readonly Graph _graph;
    private VmElement? _selectedNode;
    private VmElement? _linkSourceNode;
    private readonly FSMPropertiesPanel _propertiesPanel;
    private ContextMenuStrip _backgroundMenu;
    
    private const float NODE_SIZE = 100f;
    private const float HORIZONTAL_SPACING = 1.5f;
    private const float VERTICAL_SPACING = 1.5f;

    public FSMGraphViewer(VirtualMachine vm, Graph graph) {
        _vm = vm;
        _graph = graph;
        
        InitializeNodePositions();

        _propertiesPanel = new FSMPropertiesPanel(vm) { 
            Dock = DockStyle.Right, 
            Width = 350 
        };
        _propertiesPanel.NavigateToGraph += OnNavigateToGraph;
        Controls.Add(_propertiesPanel);

        InitializeContextMenu();
        CenterView();
    }

    private void InitializeNodePositions() {
        var processed = new HashSet<string>();

        
        var validStates = _graph.States.Where(s => s?.Element != null).ToList();
        var invalidStates = _graph.States.Where(s => s?.Element == null).ToList();

        if (invalidStates.Any()) {
            Logger.Log(LogLevel.Warning, $"Graph {_graph.Id} contains {invalidStates.Count} invalid states");
            foreach (var state in invalidStates) {
                Logger.Log(LogLevel.Warning, $"Invalid state ID: {state?.Id ?? "null"}");
            }
        }

        
        var initialStates = validStates.Where(s => {
            if (s.Element is IGraphElement graphElement) return graphElement.Initial ?? false;
            if (s.Element is Talking talking) return talking.Initial ?? false;
            return false;
        }).ToList();

        float currentX = 0;

        if (initialStates.Any()) {
            foreach (var state in initialStates) {
                NodePositions[state.Id] = (currentX, 0);
                processed.Add(state.Id);
                currentX += HORIZONTAL_SPACING;
            }
        } else {
            
            if (validStates.Any()) {
                var firstState = validStates.First();
                NodePositions[firstState.Id] = (currentX, 0);
                processed.Add(firstState.Id);
                currentX += HORIZONTAL_SPACING;
            }
        }

        
        var currentRow = 0;
        bool addedAny;
        do {
            addedAny = false;
            currentX = 0;
            var statesToProcess = new List<VmEither<State, Graph, Branch, Talking>>();

            foreach (var link in _graph.EventLinks.Where(l => l?.Source != null && l?.Destination != null)) {
                if (processed.Contains(link.Source.Id) && !processed.Contains(link.Destination.Id)) {
                    var destState = validStates.FirstOrDefault(s => s.Id == link.Destination.Id);
                    if (destState != null && !statesToProcess.Contains(destState)) {
                        statesToProcess.Add(destState);
                    }
                }
            }

            foreach (var state in statesToProcess) {
                NodePositions[state.Id] = (currentX, currentRow * -VERTICAL_SPACING);
                processed.Add(state.Id);
                currentX += HORIZONTAL_SPACING;
                addedAny = true;
            }

            if (!addedAny && processed.Count < validStates.Count) {
                currentRow++;
                foreach (var state in validStates.Where(s => !processed.Contains(s.Id))) {
                    NodePositions[state.Id] = (currentX, currentRow * -VERTICAL_SPACING);
                    processed.Add(state.Id);
                    currentX += HORIZONTAL_SPACING;
                }
            }

            currentRow++;
        } while (addedAny);
    }

    private void InitializeContextMenu() {
        _backgroundMenu = new ContextMenuStrip();
        _backgroundMenu.Items.Add("Add State", null, (_, _) => 
            AddNewState(ScreenToGame(GraphPanel.PointToClient(Cursor.Position))));
        _backgroundMenu.Items.Add("Add Branch", null, (_, _) => 
            AddNewBranch(ScreenToGame(GraphPanel.PointToClient(Cursor.Position))));
        _backgroundMenu.Items.Add("Fit View", null, (_, _) => CenterView());

        GraphPanel.MouseClick += (_, e) => {
            if (e.Button == MouseButtons.Right && GetNodeAtPosition(e.Location) == null) {
                _backgroundMenu.Show(GraphPanel, e.Location);
            }
        };
    }

private bool IsInitialState(VmElement element) {
    if (element is IGraphElement graphElement) return graphElement.Initial ?? false;
    if (element is Talking talking) return talking.Initial ?? false;
    return false;
}

private string GetNodeName(VmElement element) {
    if (element is IGraphElement graphElement) return graphElement.Name;
    if (element is Talking talking) return talking.Name;
    return element.Id;
}

private List<EntryPoint> GetEntryPoints(VmElement element) {
    if (element is IGraphElement graphElement) return graphElement.EntryPoints;
    if (element is Talking talking) return talking.EntryPoints;
    return [];
}

protected override void DrawNodes(Graphics g) {
    using var font = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 10f * ZoomLevel));
    var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

    foreach (var state in _graph.States) {
        if (state?.Element == null) continue;
        if (!NodePositions.TryGetValue(state.Id, out var pos)) continue;

        var screenPos = GameToScreen(pos.x, pos.y);
        var size = (int)(NODE_SIZE * ZoomLevel);
        var nodeBounds = new Rectangle(
            screenPos.X - size / 2,
            screenPos.Y - size / 2,
            size, size
        );

        try {
            using var path = new GraphicsPath();
            if (state.Element is Branch) {
                
                var points = new[] {
                    new Point(nodeBounds.X + nodeBounds.Width / 2, nodeBounds.Y),
                    new Point(nodeBounds.Right, nodeBounds.Y + nodeBounds.Height / 2),
                    new Point(nodeBounds.X + nodeBounds.Width / 2, nodeBounds.Bottom),
                    new Point(nodeBounds.X, nodeBounds.Y + nodeBounds.Height / 2)
                };
                path.AddPolygon(points);
            } else if (state.Element is Talking) {
                
                var points = new[] {
                    new Point(nodeBounds.X + nodeBounds.Width / 4, nodeBounds.Y),
                    new Point(nodeBounds.Right - nodeBounds.Width / 4, nodeBounds.Y),
                    new Point(nodeBounds.Right, nodeBounds.Y + nodeBounds.Height / 2),
                    new Point(nodeBounds.Right - nodeBounds.Width / 4, nodeBounds.Bottom),
                    new Point(nodeBounds.X + nodeBounds.Width / 4, nodeBounds.Bottom),
                    new Point(nodeBounds.X, nodeBounds.Y + nodeBounds.Height / 2)
                };
                path.AddPolygon(points);
            } else {
                
                path.AddEllipse(nodeBounds);
            }

            using var brush = new LinearGradientBrush(
                nodeBounds,
                state.Element == _selectedNode ? Color.LightBlue : Color.White,
                state.Element == _selectedNode ? Color.LightSkyBlue : Color.WhiteSmoke,
                45f
            );
            g.FillPath(brush, path);

            using var pen = new Pen(
                IsInitialState(state.Element) ? Color.Green : Color.Black,
                Math.Max(1.0f, 1.5f * ZoomLevel)
            );
            g.DrawPath(pen, path);

            
            var entryPoints = GetEntryPoints(state.Element);
            if (entryPoints.Any()) {
                var entryPointSize = size / 4;
                var spacing = entryPointSize * 1.2f;
                var startX = nodeBounds.X - entryPointSize / 2;
                var y = nodeBounds.Y - entryPointSize;

                foreach (var entryPoint in entryPoints.Where(ep => ep != null)) {
                    var entryPointBounds = new RectangleF(startX, y, entryPointSize, entryPointSize);
                    g.FillEllipse(Brushes.LightGreen, entryPointBounds);
                    g.DrawEllipse(Pens.DarkGreen, entryPointBounds);
                    startX += (int)spacing;
                }
            }

            
            var displayName = GetNodeName(state.Element);
            if (string.IsNullOrEmpty(displayName)) displayName = state.Element.Id;
            g.DrawString(displayName, font, Brushes.Black, nodeBounds, format);

            
            if (state.Element is Graph) {
                var indicatorBounds = new RectangleF(
                    nodeBounds.Right - size / 4,
                    nodeBounds.Bottom - size / 4,
                    size / 4,
                    size / 4
                );
                g.FillRectangle(Brushes.LightBlue, indicatorBounds);
                g.DrawRectangle(Pens.Blue, indicatorBounds.X, indicatorBounds.Y,
                    indicatorBounds.Width, indicatorBounds.Height);
            }
        } catch (Exception ex) {
            Logger.Log(LogLevel.Error, $"Error drawing node {state.Id}: {ex.Message}");
            g.FillRectangle(Brushes.Red, nodeBounds);
            g.DrawRectangle(Pens.DarkRed, nodeBounds.X, nodeBounds.Y, nodeBounds.Width, nodeBounds.Height);
            g.DrawString("Error", font, Brushes.White, nodeBounds, format);
        }
    }
}

protected override void DrawEdges(Graphics g) {
        using var pen = new Pen(Color.Gray, Math.Max(1.0f, 1.5f * ZoomLevel));
        pen.CustomEndCap = new AdjustableArrowCap(5 * ZoomLevel, 5 * ZoomLevel);
        
        foreach (var link in _graph.EventLinks) {
            if (link.Source == null || link.Destination == null) continue;
            if (!NodePositions.ContainsKey(link.Source.Id) || !NodePositions.ContainsKey(link.Destination.Id)) continue;
            
            var sourcePos = NodePositions[link.Source.Id];
            var destPos = NodePositions[link.Destination.Id];
            DrawArrow(g, pen, sourcePos, destPos);

            
            var centerX = (sourcePos.x + destPos.x) / 2;
            var centerY = (sourcePos.y + destPos.y) / 2;
            var screenPos = GameToScreen(centerX, centerY);
            
            using var font = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 8f * ZoomLevel));
            var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            
            var labelText = link.Event?.Name ?? link.Name;
            var labelBounds = new RectangleF(screenPos.X - 50, screenPos.Y - 10, 100, 20);
            
            g.FillRectangle(Brushes.White, labelBounds);
            g.DrawString(labelText, font, Brushes.Black, labelBounds, format);

            
            if (link.SourceExitPointIndex >= 0 || link.DestEntryPointIndex >= 0) {
                var smallFont = new Font(FontFamily.GenericSansSerif, Math.Max(1.0f, 6f * ZoomLevel));
                if (link.SourceExitPointIndex >= 0) {
                    var sourcePt = GameToScreen(sourcePos.x, sourcePos.y);
                    g.DrawString(link.SourceExitPointIndex.ToString(), smallFont, Brushes.Blue, 
                        sourcePt.X - 10, sourcePt.Y - 10);
                }
                if (link.DestEntryPointIndex >= 0) {
                    var destPt = GameToScreen(destPos.x, destPos.y);
                    g.DrawString(link.DestEntryPointIndex.ToString(), smallFont, Brushes.Blue, 
                        destPt.X + 10, destPt.Y - 10);
                }
            }
        }

        if (_linkSourceNode != null) {
            using var tempPen = new Pen(Color.Gray, Math.Max(1.0f, 1.5f * ZoomLevel)) { DashStyle = DashStyle.Dash };
            var start = NodePositions[_linkSourceNode.Id];
            var startPoint = GameToScreen(start.x, start.y);
            var mousePos = GraphPanel.PointToClient(Cursor.Position);
            g.DrawLine(tempPen, startPoint, mousePos);
        }
    }

    protected override float GetNodeRadius() => NODE_SIZE / 2;

    protected override string? GetNodeAtPosition(Point screenPoint) {
        foreach (var (nodeId, pos) in NodePositions) {
            var nodePos = GameToScreen(pos.x, pos.y);
            var size = (int)(NODE_SIZE * ZoomLevel);
            var bounds = new Rectangle(nodePos.X - size/2, nodePos.Y - size/2, size, size);
            if (bounds.Contains(screenPoint)) return nodeId;
        }
        return null;
    }

    protected override void HandleNodeClick(string nodeId, MouseButtons button, Point screenPoint) {
        var node = _graph.States.First(n => n.Id == nodeId);
        
        switch (button) {
            case MouseButtons.Left when _linkSourceNode != null: {
                if (_linkSourceNode != node.Element) AddLink(_linkSourceNode, node.Element);
                _linkSourceNode = null;
                GraphPanel.Cursor = Cursors.Default;
                break;
            }
            case MouseButtons.Left:
                _selectedNode = node.Element;
                _propertiesPanel.SetElement(_selectedNode);
                break;
            case MouseButtons.Right:
                ShowNodeContextMenu(node.Element, screenPoint);
                break;
        }
        
        GraphPanel.Invalidate();
    }

    protected override void HandleNodeMoved(string nodeId, (float x, float y) newPosition) {
        NodePositions[nodeId] = newPosition;
    }

    private void ShowNodeContextMenu(VmElement node, Point location) {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Remove Node", null, (_, _) => RemoveNode(node));
        menu.Items.Add("Start Link", null, (_, _) => {
            _linkSourceNode = node;
            GraphPanel.Cursor = Cursors.Cross;
        });

        if (node is Graph graph) {
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Open Graph", null, (_, _) => OnNavigateToGraph(graph));
        }

        if (node is IGraphElement graphElement) {
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Add Entry Point", null, (_, _) => {
                var entryPoint = VmElement.CreateDefault<EntryPoint>(_vm, node);
                entryPoint.Name = $"EntryPoint_{graphElement.EntryPoints.Count + 1}";
                graphElement.EntryPoints.Add(entryPoint);
                GraphPanel.Invalidate();
            });
        }

        menu.Show(GraphPanel.PointToScreen(location));
    }

    private void AddNewState((float x, float y) position) {
        var state = VmElement.CreateDefault<State>(_vm, _graph);
        state.Name = $"State_{_graph.States.Count}";
        state.Parent = _graph;
        
        NodePositions[state.Id] = position;
        _graph.States.Add(new(state));
        GraphPanel.Invalidate();
    }

    private void AddNewBranch((float x, float y) position) {
        var branch = VmElement.CreateDefault<Branch>(_vm, _graph);
        branch.Name = $"Branch_{_graph.States.Count}";
        branch.Parent = _graph;
        
        NodePositions[branch.Id] = position;
        _graph.States.Add(new(branch));
        GraphPanel.Invalidate();
    }

    private void RemoveNode(VmElement node) {
        NodePositions.Remove(node.Id);
        _graph.States.RemoveAll(s => s.Element == node);
        
        
        _graph.EventLinks.RemoveAll(link => 
            link.Source?.Element == node || link.Destination?.Element == node);
        
        if (_selectedNode == node) {
            _selectedNode = null;
            _propertiesPanel.SetElement(null);
        }
        GraphPanel.Invalidate();
    }
    
    private void AddLink(VmElement source, VmElement destination) {
        var link = VmElement.CreateDefault<GraphLink>(_vm, _graph);
        link.Name = $"Link_{_graph.EventLinks.Count}";
        link.Source = new(source);
        link.Destination = new(destination);
        
        
        if (source is IGraphElement sourceElement && sourceElement.EntryPoints.Any()) {
            link.SourceExitPointIndex = 0;
        }
        
        
        if (destination is IGraphElement destElement && destElement.EntryPoints.Any()) {
            link.DestEntryPointIndex = 0;
        }
        
        _graph.EventLinks.Add(link);
        ShowLinkPropertiesDialog(link);
        GraphPanel.Invalidate();
    }

    private void ShowLinkPropertiesDialog(GraphLink link) {
        var dialog = new Form {
            Text = "Link Properties",
            Size = new Size(400, 300),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 5,
            Padding = new Padding(10)
        };
        dialog.Controls.Add(layout);

        
        layout.Controls.Add(new Label { Text = "Name:" }, 0, 0);
        var nameBox = new TextBox { Text = link.Name, Width = 200 };
        nameBox.TextChanged += (_, _) => link.Name = nameBox.Text;
        layout.Controls.Add(nameBox, 1, 0);

        
        layout.Controls.Add(new Label { Text = "Event:" }, 0, 1);
        var eventCombo = new ComboBox { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
        eventCombo.Items.Add("(None)");
        var events = _vm.GetElementsByType<Event>().OrderBy(e => e.Name).ToList();
        eventCombo.Items.AddRange(events.Cast<object>().ToArray());
        eventCombo.SelectedItem = link.Event;
        eventCombo.SelectedIndexChanged += (_, _) => {
            link.Event = eventCombo.SelectedItem as Event;
        };
        layout.Controls.Add(eventCombo, 1, 1);

        
        layout.Controls.Add(new Label { Text = "Source Exit Point:" }, 0, 2);
        var sourcePointBox = new NumericUpDown { 
            Minimum = -1,
            Maximum = 100,
            Value = link.SourceExitPointIndex 
        };
        sourcePointBox.ValueChanged += (_, _) => link.SourceExitPointIndex = (int)sourcePointBox.Value;
        layout.Controls.Add(sourcePointBox, 1, 2);

        
        layout.Controls.Add(new Label { Text = "Destination Entry Point:" }, 0, 3);
        var destPointBox = new NumericUpDown { 
            Minimum = -1,
            Maximum = 100,
            Value = link.DestEntryPointIndex 
        };
        destPointBox.ValueChanged += (_, _) => link.DestEntryPointIndex = (int)destPointBox.Value;
        layout.Controls.Add(destPointBox, 1, 3);

        
        var okButton = new Button {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right
        };
        layout.Controls.Add(okButton, 1, 4);

        dialog.ShowDialog(this);
    }

    private void OnNavigateToGraph(Graph targetGraph) {
        var parent = Parent;
        while (parent != null && parent is not FSMBrowser browser) {
            parent = parent.Parent;
        }
        
        if (parent is FSMBrowser fsm) {
            fsm.LoadGraph(targetGraph);
        }
    }
    
    public void RefreshView() => GraphPanel.Invalidate();
}