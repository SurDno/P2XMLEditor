using P2XMLEditor.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.Forms;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm.FSMViewer;

public class FSMPropertiesPanel : Panel {
    private readonly VirtualMachine _vm;
    private VmElement? _selectedElement;
    private readonly TableLayoutPanel _propertiesTable;
    private readonly Button _parentGraphButton;
    private readonly Button _childGraphButton;
    private readonly ListView _conditionsList;
    private readonly Button _addConditionButton;
    
    public event Action<Graph>? NavigateToGraph;

    public FSMPropertiesPanel(VirtualMachine vm) {
        _vm = vm;
        AutoScroll = true;
        Dock = DockStyle.Right;
        Width = 350;
        Padding = new Padding(5);

        _parentGraphButton = new Button {
            Text = "To Parent Graph",
            Dock = DockStyle.Top,
            Visible = false
        };
        _parentGraphButton.Click += OnParentGraphClick;
        Controls.Add(_parentGraphButton);

        _childGraphButton = new Button {
            Text = "Open Graph",
            Dock = DockStyle.Top,
            Visible = false,
            Top = _parentGraphButton.Bottom + 5
        };
        _childGraphButton.Click += OnChildGraphClick;
        Controls.Add(_childGraphButton);

        _conditionsList = new ListView {
            View = View.Details,
            FullRowSelect = true,
            Visible = false,
            Height = 200,
            Dock = DockStyle.Top,
            Top = _childGraphButton.Bottom + 5
        };
        _conditionsList.Columns.Add("Conditions", -2);
        _conditionsList.DoubleClick += OnConditionDoubleClick;
        Controls.Add(_conditionsList);

        _addConditionButton = new Button {
            Text = "Add Condition",
            Visible = false,
            Dock = DockStyle.Top,
            Top = _conditionsList.Bottom + 5
        };
        _addConditionButton.Click += OnAddCondition;
        Controls.Add(_addConditionButton);

        _propertiesTable = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            ColumnStyles = {
                new ColumnStyle(SizeType.Percent, 40),
                new ColumnStyle(SizeType.Percent, 60)
            },
            AutoSize = true,
            Top = _addConditionButton.Bottom + 10
        };
        Controls.Add(_propertiesTable);
    }

    public void SetElement(VmElement? element) {
        _selectedElement = element;
        UpdateControls();
    }

    private void UpdateControls() {
        _propertiesTable.Controls.Clear();
        _propertiesTable.RowStyles.Clear();
        _parentGraphButton.Visible = false;
        _childGraphButton.Visible = false;
        _conditionsList.Visible = false;
        _addConditionButton.Visible = false;

        if (_selectedElement == null) {
            Enabled = false;
            return;
        }

        Enabled = true;
        
        
        if (_selectedElement is IGraphElement graphElement) {
            AddProperty("Name", graphElement.Name, value => graphElement.Name = value);
        } else if (_selectedElement is Talking talking) {
            AddProperty("Name", talking.Name, value => talking.Name = value);
        }
        AddProperty("ID", _selectedElement.Id, null); 
        
        switch (_selectedElement) {
            case Graph graph:
                SetupGraphProperties(graph);
                break;
            case Branch branch:
                SetupBranchProperties(branch);
                break;
            case State state:
                SetupStateProperties(state);
                break;
            case Talking talking:
                SetupTalkingProperties(talking);
                break;
        }
    }

    private void SetupGraphProperties(Graph graph) {
        AddProperty("Graph Type", graph.GraphType.ToString(), null);
        AddProperty("Initial", graph.Initial.ToString(), value => graph.Initial = bool.Parse(value));
        AddProperty("Ignore Block", graph.IgnoreBlock.ToString(), value => graph.IgnoreBlock = bool.Parse(value));
        
        if (graph.Parent.Element is Graph parentGraph) {
            _parentGraphButton.Visible = true;
            _parentGraphButton.Tag = parentGraph;
        }

        AddEntryPoints(graph.EntryPoints);
    }

    private void SetupBranchProperties(Branch branch) {
        AddProperty("Branch Type", branch.BranchType.ToString(), null);
        AddProperty("Initial", branch.Initial.ToString(), value => branch.Initial = bool.Parse(value));
        AddProperty("Ignore Block", branch.IgnoreBlock.ToString(), value => branch.IgnoreBlock = bool.Parse(value));
        
        if (branch.BranchVariantInfo != null) {
            foreach (var info in branch.BranchVariantInfo) {
                AddProperty($"Variant {info.Name}", info.Type, null);
            }
        }

        _conditionsList.Items.Clear();
        _conditionsList.Visible = true;
        _addConditionButton.Visible = true;

        foreach (var condition in branch.BranchConditions) {
            var item = new ListViewItem(PreviewHelper.Preview(condition.Element));
            item.Tag = condition.Element;
            _conditionsList.Items.Add(item);
        }

        AddEntryPoints(branch.EntryPoints);
    }

    private void SetupStateProperties(State state) {
        AddProperty("Initial", state.Initial.ToString(), value => state.Initial = bool.Parse(value));
        AddProperty("Ignore Block", state.IgnoreBlock.ToString(), value => state.IgnoreBlock = bool.Parse(value));
        AddEntryPoints(state.EntryPoints);
    }

    private void SetupTalkingProperties(Talking talking) {
        AddProperty("Initial", talking.Initial.ToString(), value => talking.Initial = bool.Parse(value));
        AddProperty("Ignore Block", talking.IgnoreBlock.ToString(), value => talking.IgnoreBlock = bool.Parse(value));
        AddEntryPoints(talking.EntryPoints);
    }

    private void OnConditionDoubleClick(object? sender, EventArgs e) {
        if (_conditionsList.SelectedItems.Count == 0) return;
        if (_selectedElement is not Branch branch) return;

        var condition = (Condition)_conditionsList.SelectedItems[0].Tag;
        var editor = new ConditionEditorForm(_vm, condition, new(branch));
        if (editor.ShowDialog() == DialogResult.OK) {
            UpdateControls(); 
        }
    }

    private void OnAddCondition(object? sender, EventArgs e) {
        if (_selectedElement is not Branch branch) return;

        var condition = VmElement.CreateDefault<Condition>(_vm, branch);
        var editor = new ConditionEditorForm(_vm, condition, new(branch));
        if (editor.ShowDialog() == DialogResult.OK) {
            branch.BranchConditions.Add(new(condition));
            UpdateControls();
        } else {
            _vm.RemoveElement(condition);
        }
    }

    private void AddEntryPoints(List<EntryPoint> entryPoints) {
        var label = new Label { Text = "Entry Points:", Dock = DockStyle.Fill };
        var listBox = new ListBox { Dock = DockStyle.Fill, Height = 100 };
        
        foreach (var ep in entryPoints) {
            listBox.Items.Add(ep.Name);
        }

        var addButton = new Button { Text = "Add", Dock = DockStyle.Top };
        addButton.Click += (_, _) => {
            var entryPoint = VmElement.CreateDefault<EntryPoint>(_vm, _selectedElement!);
            entryPoint.Name = $"EntryPoint_{entryPoints.Count + 1}";
            entryPoints.Add(entryPoint);
            listBox.Items.Add(entryPoint.Name);
        };

        _propertiesTable.Controls.Add(label, 0, _propertiesTable.RowCount);
        _propertiesTable.Controls.Add(listBox, 1, _propertiesTable.RowCount++);
        _propertiesTable.Controls.Add(addButton, 1, _propertiesTable.RowCount++);
    }

    private void AddProperty(string name, string value, Action<string>? onValueChanged) {
        var label = new Label { 
            Text = name, 
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };

        Control valueControl;
        if (onValueChanged == null) {
            valueControl = new Label {
                Text = value,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
        } else {
            var textBox = new TextBox {
                Text = value,
                Dock = DockStyle.Fill
            };
            textBox.TextChanged += (_, _) => onValueChanged(textBox.Text);
            valueControl = textBox;
        }

        _propertiesTable.Controls.Add(label, 0, _propertiesTable.RowCount);
        _propertiesTable.Controls.Add(valueControl, 1, _propertiesTable.RowCount++);
    }

    private void OnParentGraphClick(object? sender, EventArgs e) {
        if (_parentGraphButton.Tag is Graph parentGraph) {
            NavigateToGraph?.Invoke(parentGraph);
        }
    }

    private void OnChildGraphClick(object? sender, EventArgs e) {
        if (_childGraphButton.Tag is Graph childGraph) {
            NavigateToGraph?.Invoke(childGraph);
        }
    }
}