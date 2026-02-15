using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Abstract;

public abstract class GraphViewer : UserControl {
    protected readonly Dictionary<ulong, (float x, float y)> NodePositions = new();
    protected readonly Panel GraphPanel;
    protected float ZoomLevel = 1.0f;
    protected Point ViewOffset;
    protected Point LastMousePosition;
    private bool _isPanning;
    private bool _isDraggingNode;
    private ulong? _draggedNodeId;
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
    protected abstract ulong? GetNodeAtPosition(Point screenPoint);
    protected abstract void HandleNodeClick(ulong nodeId, MouseButtons button, Point screenPoint);
    protected abstract void HandleNodeMoved(ulong nodeId, (float x, float y) newPosition);
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
    
    protected void DrawArrow(
        Graphics g,
        Pen pen,
        (float x, float y) from,
        (float x, float y) to)
    {
        var start = GameToScreen(from.x, from.y);
        var end = GameToScreen(to.x, to.y);

        float startOffset = 40f * ZoomLevel;
        float endOffset = 40f * ZoomLevel;

        var direction = new PointF(end.X - start.X, end.Y - start.Y);
        float length = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
        if (length < 0.01f) return;

        direction.X /= length;
        direction.Y /= length;

        start = new Point(
            (int)(start.X + direction.X * startOffset),
            (int)(start.Y + direction.Y * startOffset));

        end = new Point(
            (int)(end.X - direction.X * endOffset),
            (int)(end.Y - direction.Y * endOffset));

        g.DrawLine(pen, start, end);
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
                    HandleNodeClick(nodeId.Value, e.Button, e.Location);
                }
                _draggedNodeId = nodeId;
                if (_draggedNodeId != null) {
                    _isDraggingNode = true;
                    GraphPanel.Cursor = Cursors.Hand;
                    if (NodePositions.TryGetValue(_draggedNodeId.Value, out var nodePos)) {
                        var mouseGamePos = ScreenToGame(e.Location);
                        _dragOffset = (mouseGamePos.x - nodePos.x, mouseGamePos.y - nodePos.y);
                    }
                }
                break;
            }
            case MouseButtons.Right: {
                var nodeId = GetNodeAtPosition(e.Location);
                if (nodeId != null) {
                    HandleNodeClick(nodeId.Value, e.Button, e.Location);
                }
                break;
            }
        }
    }

    private void OnMouseUp(object? sender, MouseEventArgs e) {
        if (_isDraggingNode && _draggedNodeId != null) {
            HandleNodeMoved(_draggedNodeId.Value, NodePositions[(ulong)_draggedNodeId]);
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

            var currentPos = NodePositions[_draggedNodeId.Value];
            if (Math.Abs(currentPos.x - newX) > float.Epsilon || Math.Abs(currentPos.y - newY) > float.Epsilon) {
                NodePositions[_draggedNodeId.Value] = (newX, newY);
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