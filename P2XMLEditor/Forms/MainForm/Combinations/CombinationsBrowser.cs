using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Forms.MainForm.Combinations;

public class CombinationsBrowser : Panel {
    private VirtualMachine _vm;
    private ListView _comboList;
    private TextBox _searchBox;
    private Label _statusLabel;
    
    public CombinationsBrowser(VirtualMachine vm) {
        _vm = vm;
        Dock = DockStyle.Fill;
        
        SetupControls();
        LoadCombinations();
    }

    private void SetupControls() {
        var searchLabel = new Label { Text = "Search:", Location = new Point(10, 15), Size = new Size(70, 23) };
        
        _searchBox = new TextBox { Location = new Point(80, 12), Size = new Size(300, 30) };
        _searchBox.TextChanged += (_, _) => LoadCombinations();

        _statusLabel = new Label { Location = new Point(390, 15), Size = new Size(400, 30) };

        _comboList = new ListView {
            Location = new Point(10, 55),
            Size = new Size(Width - 20, Height - 65),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            GridLines = true
        };
        
        _comboList.Columns.Add("Item Name", 350);
        _comboList.Columns.Add("Combination Data", -2);
        _comboList.SizeChanged += (_, _) => ResizeColumns();
        _comboList.ColumnWidthChanged += (_, e) => { if (e.ColumnIndex == 0) ResizeColumns(); };
        
        _comboList.DoubleClick += OnCombinationDoubleClick;
        Controls.AddRange([searchLabel, _searchBox, _statusLabel, _comboList]);
    }
    
    private void ResizeColumns() => _comboList.Columns[1].Width = _comboList.Width - _comboList.Columns[0].Width - 30;

    private void LoadCombinations() {
        _comboList.Items.Clear();
        
        var combos = _vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(_vm.GetElementsByType<Other>()).
                                 Where(item => item.StandartParams.ContainsKey("Combination.CombinationData")).ToList();

        var allNames = _vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(_vm.GetElementsByType<Other>()).
              Where(item => item.StandartParams.ContainsKey("Storable.DefaultStackCount") ||
                            item.StandartParams.ContainsKey("Combination.CombinationData")).ToDictionary(o => o.Id, o => o.Name);
        
        var text = _searchBox.Text.ToLowerInvariant();

        foreach (var el in combos) {
            var contents = CombinationDataParser.FormatReadable(el.StandartParams["Combination.CombinationData"].Value, allNames);
            
            if (!string.IsNullOrEmpty(text) && !IsIn(el.Name, text) && !IsIn(el.Id, text) && !IsIn(contents, text))
                continue;

            var listItem = new ListViewItem(el.Name) { Tag = el };
            listItem.SubItems.Add(contents); 
            _comboList.Items.Add(listItem);
        }

        _statusLabel.Text = $"Displaying {_comboList.Items.Count}/{combos.Count} combinations.";
    }
    
    private static bool IsIn(string src, string val) => src.Contains(val, StringComparison.InvariantCultureIgnoreCase);
    
    private void OnCombinationDoubleClick(object? sender, EventArgs e) {
        if (_comboList.SelectedItems.Count == 0) return;

        var selectedElement = (ParameterHolder)_comboList.SelectedItems[0].Tag!;
        var combinationParam = selectedElement.StandartParams["Combination.CombinationData"];
    
        var allNames = _vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(_vm.GetElementsByType<Other>()).
            Where(item => item.StandartParams.ContainsKey("Storable.DefaultStackCount") ||
                          item.StandartParams.ContainsKey("Combination.CombinationData")).ToDictionary(o => o.Id, o => o.Name);

        new CombinationEditorForm(combinationParam.Value, allNames, newValue => {
            combinationParam.Value = newValue;
            LoadCombinations();
        }).ShowDialog();
    }
}