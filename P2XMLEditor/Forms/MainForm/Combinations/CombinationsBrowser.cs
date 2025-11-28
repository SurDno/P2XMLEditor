using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm.Combinations;

public class CombinationsBrowser : Panel {
    private VirtualMachine _vm;
    private ListView _comboList;
    private TextBox _searchBox;
    private Label _statusLabel;
    private ContextMenuStrip _contextMenu;

    private const string COMBINATION_KEY = "Combination.CombinationData", STORABLE_KEY = "Storable.DefaultStackCount";

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
        _comboList.KeyDown += (s, e) => { if (e.KeyCode == Keys.Delete) DeleteSelectedCombination(); };

        SetupContextMenu();

        _comboList.ContextMenuStrip = _contextMenu;

        Controls.AddRange([searchLabel, _searchBox, _statusLabel, _comboList]);
    }

    private void ResizeColumns() => _comboList.Columns[1].Width = _comboList.Width - _comboList.Columns[0].Width - 30;

    private void LoadCombinations() {
        _comboList.Items.Clear();

        var combos = _vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(_vm.GetElementsByType<Other>()).
                             Where(item => item.StandartParams.ContainsKey(COMBINATION_KEY)).ToList();

        var allNames = _vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(_vm.GetElementsByType<Other>()).
              Where(item => item.StandartParams.ContainsKey(STORABLE_KEY) ||
                            item.StandartParams.ContainsKey(COMBINATION_KEY)).ToDictionary(o => o.Id, o => o.Name);

        var text = _searchBox.Text.ToLowerInvariant();

        foreach (var el in combos) {
            var contents = CombinationHelper.FormatReadable(el.StandartParams[COMBINATION_KEY].Value, allNames, _vm);

            if (!string.IsNullOrEmpty(text) && !IsIn(el.Name, text) && !IsIn(el.Id.ToString(), text) && !IsIn(contents, text))
                continue;

            var listItem = new ListViewItem(el.Name) { Tag = el };
            listItem.SubItems.Add(contents);
            _comboList.Items.Add(listItem);
        }

        _statusLabel.Text = $"Displaying {_comboList.Items.Count}/{combos.Count} combinations.";
    }

    private static bool IsIn(string src, string val) => src.Contains(val, StringComparison.InvariantCultureIgnoreCase);

    private void OnCombinationDoubleClick(object? sender, EventArgs e) => EditSelectedCombination();

    private void EditSelectedCombination() {
        if (_comboList.SelectedItems.Count == 0) return;

        var selectedElement = (ParameterHolder)_comboList.SelectedItems[0].Tag!;
        var combinationParam = selectedElement.StandartParams[COMBINATION_KEY];

        var allNames = _vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(_vm.GetElementsByType<Other>()).ToList();
        var availableCombinations = allNames.Where(item => item.StandartParams.ContainsKey(COMBINATION_KEY)).ToList();
        var availableStorables = allNames.Where(item => item.StandartParams.ContainsKey(STORABLE_KEY)).Except(availableCombinations).ToList();

        new CombinationEditorForm(_vm, combinationParam.Value, availableStorables, availableCombinations, newValue => {
            combinationParam.Value = newValue;
            LoadCombinations();
        }).ShowDialog();
    }

    private void DeleteSelectedCombination() {
        if (_comboList.SelectedItems.Count == 0) return;

        var selectedElement = (ParameterHolder)_comboList.SelectedItems[0].Tag!;
        if (!selectedElement.StandartParams.ContainsKey(COMBINATION_KEY)) return;

        if (MessageBox.Show($"Are you sure you want to delete the combination '{selectedElement.Name}'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        _vm.RemoveElement(selectedElement);
        LoadCombinations();
    }

    [SuppressMessage("ReSharper", "UseCollectionExpression")]
    private void SetupContextMenu() {
        _contextMenu = new ContextMenuStrip();
        var editItem = new ToolStripMenuItem("Edit");
        var deleteItem = new ToolStripMenuItem("Delete");

        editItem.Click += (_, _) => EditSelectedCombination();
        deleteItem.Click += (_, _) => DeleteSelectedCombination();

        _contextMenu.Items.AddRange(new ToolStripItem[] { editItem, deleteItem });
    }
}
