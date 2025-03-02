using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Forms.MainForm.FSMViewer;

public class FSMBrowser : SplitContainer {
    private readonly VirtualMachine _vm;
    private readonly ComboBox _categoryComboBox;
    private readonly ListView _graphList;
    private FSMGraphViewer? _currentViewer;
    
    private enum OwnerCategory {
        Character,
        Blueprint,
        Quest,
        Geom,
        Item,
        Other,
        GameRoot
    }

    public FSMBrowser(VirtualMachine vm) {
        _vm = vm;
        Dock = DockStyle.Fill;
        Orientation = Orientation.Vertical;
        SplitterDistance = 300;
        
        var leftPanel = new Panel { 
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };
        Panel1.Controls.Add(leftPanel);
        
        _categoryComboBox = new ComboBox {
            Dock = DockStyle.Top,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _categoryComboBox.Items.AddRange(Enum.GetNames<OwnerCategory>());
        _categoryComboBox.SelectedIndexChanged += OnCategoryChanged;
        leftPanel.Controls.Add(_categoryComboBox);
        
        _graphList = new ListView {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            Top = _categoryComboBox.Bottom + 5
        };
        _graphList.Columns.Add("Name", -2);
        _graphList.SelectedIndexChanged += OnGraphSelected;
        leftPanel.Controls.Add(_graphList);
        
        if (_categoryComboBox.Items.Count > 0) {
            _categoryComboBox.SelectedIndex = 0;
        }
    }

    private void OnCategoryChanged(object? sender, EventArgs e) {
        _graphList.Items.Clear();
        
        if (_categoryComboBox.SelectedItem is not string selectedCategory || 
            !Enum.TryParse<OwnerCategory>(selectedCategory, out var category)) return;

        var graphs = _vm.GetElementsByType<Graph>()
            .Where(g => !g.IsOrphaned() && IsInCategory(g, category))
            .OrderBy(g => g.Name);

        foreach (var graph in graphs) {
            var item = new ListViewItem(graph.Name) { Tag = graph };
            _graphList.Items.Add(item);
        }
    }

    private bool IsInCategory(Graph graph, OwnerCategory category) {
        if (graph.Parent.Element is not ParameterHolder owner) return false;
        
        return category switch {
            OwnerCategory.Character => owner is Character,
            OwnerCategory.Blueprint => owner is Blueprint,
            OwnerCategory.Quest => owner is Quest,
            OwnerCategory.Geom => owner is Geom,
            OwnerCategory.Item => owner is Item,
            OwnerCategory.GameRoot => owner is GameRoot,
            OwnerCategory.Other => !(owner is Character or Blueprint or Quest or Geom or Item or GameRoot),
            _ => false
        };
    }

    private void OnGraphSelected(object? sender, EventArgs e) {
        if (_graphList.SelectedItems.Count == 0) return;
        
        var graph = (Graph)_graphList.SelectedItems[0].Tag;
        LoadGraph(graph);
    }

    public void LoadGraph(Graph graph) {
        _currentViewer?.Dispose();
        
        _currentViewer = new FSMGraphViewer(_vm, graph) {
            Dock = DockStyle.Fill
        };
        Panel2.Controls.Clear();
        Panel2.Controls.Add(_currentViewer);
    }
}