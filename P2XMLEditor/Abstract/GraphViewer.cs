using System.Drawing.Drawing2D;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Abstract;

public abstract class GraphViewer : UserControl {
    protected readonly Dictionary<string, (float x, float y)> NodePositions = new();
    protected readonly Panel GraphPanel;
    protected float ZoomLevel = 1.0f;
    protected Point ViewOffset;
    protected Point LastMousePosition;
    private bool _isPanning;
    private bool _isDraggingNode;
    private string? _draggedNodeId;
    private (float x, float y) _dragOffset;

    private const float GRID_STEP = 0.0025f;
    private const float GRAPH_SCALE = 400f;

    protected GraphViewer() {
        GraphPanel = new DoubleBufferedPanel {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        Controls.Add(GraphPanel);

        GraphPanel.Paint += (_, e) => {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.Clear(BackColor);
            DrawEdges(e.Graphics);
            DrawNodes(e.Graphics);
        };
        GraphPanel.MouseDown += OnMouseDown;
        GraphPanel.MouseUp += OnMouseUp;
        GraphPanel.MouseMove += OnMouseMove;
        GraphPanel.MouseWheel += OnMouseWheel;
        GraphPanel.Resize += (_, _) => CenterView();
    }

    protected abstract void DrawNodes(Graphics g);
    protected abstract void DrawEdges(Graphics g);
    protected abstract string? GetNodeAtPosition(Point screenPoint);
    protected abstract void HandleNodeClick(string nodeId, MouseButtons button, Point screenPoint);
    protected abstract void HandleNodeMoved(string nodeId, (float x, float y) newPosition);
    protected abstract float GetNodeRadius(); 

    protected virtual void LimitPanOffset() {
        if (!NodePositions.Any()) return;

        var minX = NodePositions.Min(p => p.Value.x) * GRAPH_SCALE;
        var maxX = NodePositions.Max(p => p.Value.x) * GRAPH_SCALE;
        var minY = NodePositions.Min(p => p.Value.y) * GRAPH_SCALE;
        var maxY = NodePositions.Max(p => p.Value.y) * GRAPH_SCALE;

        var padding = 1000f;
        var viewWidth = GraphPanel.Width;
        var viewHeight = GraphPanel.Height;

        ViewOffset.X = (int)Math.Max(minX - padding - viewWidth, Math.Min(maxX + padding, ViewOffset.X));
        ViewOffset.Y = (int)Math.Max(minY - padding - viewHeight, Math.Min(maxY + padding, ViewOffset.Y));
    }
    
    protected void DrawArrow(Graphics g, Pen pen, (float x, float y) start, (float x, float y) end) {
        const int maxCoord = 30000;
    
        var startPoint = GameToScreen(start.x, start.y);
        var endPoint = GameToScreen(end.x, end.y);
    
        if (Math.Abs(startPoint.X) > maxCoord || Math.Abs(startPoint.Y) > maxCoord || 
            Math.Abs(endPoint.X) > maxCoord || Math.Abs(endPoint.Y) > maxCoord) 
            return;
    
        float dx = endPoint.X - startPoint.X;
        float dy = endPoint.Y - startPoint.Y;
        var length = (float)Math.Sqrt(dx * dx + dy * dy);

        if (length <= float.Epsilon) 
            return;
    
        dx /= length;
        dy /= length;
    
        var radius = GetNodeRadius() * ZoomLevel;
    
        var startX = Math.Max(-maxCoord, Math.Min(maxCoord, startPoint.X + dx * radius));
        var startY = Math.Max(-maxCoord, Math.Min(maxCoord, startPoint.Y + dy * radius));
        var endX = Math.Max(-maxCoord, Math.Min(maxCoord, endPoint.X - dx * radius));
        var endY = Math.Max(-maxCoord, Math.Min(maxCoord, endPoint.Y - dy * radius));
    
        var adjustedStart = new PointF(startX, startY);
        var adjustedEnd = new PointF(endX, endY);
    
        if (Math.Abs(adjustedEnd.X - adjustedStart.X) > maxCoord || Math.Abs(adjustedEnd.Y - adjustedStart.Y) > maxCoord) 
            return;
        
        g.DrawLine(pen, adjustedStart, adjustedEnd);
    }

    protected Point GameToScreen(float x, float y) => 
        new((int)(x * GRAPH_SCALE * ZoomLevel + ViewOffset.X), (int)(-y * GRAPH_SCALE * ZoomLevel + ViewOffset.Y));

    protected (float x, float y) ScreenToGame(Point screenPoint) {
        var x = (screenPoint.X - ViewOffset.X) / (GRAPH_SCALE * ZoomLevel);
        var y = -(screenPoint.Y - ViewOffset.Y) / (GRAPH_SCALE * ZoomLevel);
        return (x, y);
    }

    private static float SnapToGrid(float value) => (float)(Math.Round(value / GRID_STEP) * GRID_STEP);
    
    private void OnMouseDown(object? sender, MouseEventArgs e) {
        LastMousePosition = e.Location;
        switch (e.Button) {
            case MouseButtons.Middle:
                _isPanning = true;
                GraphPanel.Cursor = Cursors.SizeAll;
                break;
            case MouseButtons.Left: {
                var nodeId = GetNodeAtPosition(e.Location);
                if (nodeId != null) {
                    HandleNodeClick(nodeId, e.Button, e.Location);
                }
                _draggedNodeId = nodeId;
                if (_draggedNodeId != null) {
                    _isDraggingNode = true;
                    GraphPanel.Cursor = Cursors.Hand;
                    if (NodePositions.TryGetValue(_draggedNodeId, out var nodePos)) {
                        var mouseGamePos = ScreenToGame(e.Location);
                        _dragOffset = (mouseGamePos.x - nodePos.x, mouseGamePos.y - nodePos.y);
                    }
                }
                break;
            }
            case MouseButtons.Right: {
                var nodeId = GetNodeAtPosition(e.Location);
                if (nodeId != null) {
                    HandleNodeClick(nodeId, e.Button, e.Location);
                }
                break;
            }
        }
    }

    private void OnMouseUp(object? sender, MouseEventArgs e) {
        if (_isDraggingNode && _draggedNodeId != null) {
            HandleNodeMoved(_draggedNodeId, NodePositions[_draggedNodeId]);
        }
   
        _isPanning = false;
        _isDraggingNode = false;
        _draggedNodeId = null;
        GraphPanel.Cursor = Cursors.Default;
    }

    private void OnMouseMove(object? sender, MouseEventArgs e) {
        if (_isPanning) {
            var deltaX = e.Location.X - LastMousePosition.X;
            var deltaY = e.Location.Y - LastMousePosition.Y;
    
            ViewOffset = new Point(
                ViewOffset.X + deltaX,
                ViewOffset.Y + deltaY
            );
    
            LimitPanOffset();
            GraphPanel.Invalidate();
        } else if (_isDraggingNode && _draggedNodeId != null) {
            var gameCoords = ScreenToGame(e.Location);
            var newX = SnapToGrid(gameCoords.x - _dragOffset.x);
            var newY = SnapToGrid(gameCoords.y - _dragOffset.y);

            var currentPos = NodePositions[_draggedNodeId];
            if (Math.Abs(currentPos.x - newX) > float.Epsilon || Math.Abs(currentPos.y - newY) > float.Epsilon) {
                NodePositions[_draggedNodeId] = (newX, newY);
                GraphPanel.Invalidate();
            }
        }

        LastMousePosition = e.Location;
    }

    private void OnMouseWheel(object? sender, MouseEventArgs e) {
        var oldZoom = ZoomLevel;
        ZoomLevel *= e.Delta > 0 ? 1.1f : 0.9f;
        ZoomLevel = Math.Max(0.1f, Math.Min(3.0f, ZoomLevel));

        if (!(Math.Abs(oldZoom - ZoomLevel) > float.Epsilon)) return;
        var mousePos = GraphPanel.PointToClient(MousePosition);
        var zoomDelta = ZoomLevel / oldZoom;

        ViewOffset = new Point(
            mousePos.X - (int)((mousePos.X - ViewOffset.X) * zoomDelta),
            mousePos.Y - (int)((mousePos.Y - ViewOffset.Y) * zoomDelta)
        );

        GraphPanel.Invalidate();
    }

    protected void CenterView() {
        if (NodePositions.Count == 0) return;

        float sumX = 0, sumY = 0;
        foreach (var pos in NodePositions.Values) {
            sumX += pos.x;
            sumY += pos.y;
        }
        var centerX = sumX / NodePositions.Count;
        var centerY = sumY / NodePositions.Count;

        var screenCenter = GameToScreen(centerX, centerY);

        ViewOffset = new Point(
            Width / 2 - screenCenter.X + (int)(ViewOffset.X / ZoomLevel),
            Height / 2 - screenCenter.Y + (int)(ViewOffset.Y / ZoomLevel)
        );

        GraphPanel.Invalidate();
    }
}