using P2XMLEditor.Core;
using P2XMLEditor.GameData.Templates;

namespace P2XMLEditor.Forms.MainForm.Templates;

public class TemplatesViewer : SplitContainer {
    private readonly TreeView _templatesTree;
    private readonly PropertyGrid _propertyGrid;
    private readonly TemplateManager _templateManager;

    public TemplatesViewer(TemplateManager templateManager) {
        _templateManager = templateManager;
        Dock = DockStyle.Fill;
        Orientation = Orientation.Vertical;
        SplitterDistance = 300;

        _templatesTree = new TreeView {
            Dock = DockStyle.Fill,
            ShowLines = true,
            HideSelection = false
        };
        _templatesTree.AfterSelect += OnTemplateSelected;
        Panel1.Controls.Add(_templatesTree);

        _propertyGrid = new PropertyGrid {
            Dock = DockStyle.Fill,
            PropertySort = PropertySort.Categorized,
            ToolbarVisible = true
        };
        Panel2.Controls.Add(_propertyGrid);

        LoadTemplatesTree();
    }

    private void LoadTemplatesTree() {
        _templatesTree.Nodes.Clear();

        var templatesByType = _templateManager.Templates
            .GroupBy(t => t.Value.Type)
            .ToDictionary(g => g.Key, g => g.Select(t => t.Value).ToList());

        foreach (var typeGroup in templatesByType) {
            var typeNode = _templatesTree.Nodes.Add(typeGroup.Key);

            foreach (var template in typeGroup.Value.OrderBy(t => t.Name)) {
                var templateNode = typeNode.Nodes.Add(template.Name);
                templateNode.Tag = template;

                if (template is not EntityObject entity) continue;
                foreach (var component in entity.Components) {
                    var componentNode = templateNode.Nodes.Add(component.Type);
                    componentNode.Tag = component;
                }
            }
        }
    }

    private void OnTemplateSelected(object? sender, TreeViewEventArgs e) {
        _propertyGrid.SelectedObject = e.Node?.Tag;
    }

    public void RefreshView() {
        LoadTemplatesTree();
    }
}