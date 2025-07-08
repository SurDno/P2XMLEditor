using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm.Combinations;

public sealed class CombinationEditorForm : Form {
    private readonly VirtualMachine _vm;
    private readonly DataGridView _gridView;
    private readonly List<ICombinationPart> _originalEntries;
    private readonly List<ParameterHolder> _comboItems;
    private readonly List<ParameterHolder> _storableItems;
    private bool _suppressEvents;

    public CombinationEditorForm(VirtualMachine vm, string initialValue, List<ParameterHolder> availableStorables, List<ParameterHolder> availableCombinations, Action<string> onSaveCallback) {
        _vm = vm;
        _originalEntries = CombinationHelper.Parse(_vm, initialValue);

        _comboItems = availableCombinations;
        _storableItems = availableStorables;

        Text = "Edit Combination Data";
        Size = new Size(1200, 600);
        StartPosition = FormStartPosition.CenterParent;

        _gridView = new DataGridView {
            Location = new Point(12, 42),
            Size = new Size(1160, 450),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            EditMode = DataGridViewEditMode.EditOnEnter,
            ColumnHeadersVisible = false, 
            MultiSelect = false,
            AllowUserToResizeColumns = false,
            AllowUserToResizeRows = false
        };

        _gridView.EditingControlShowing += GridView_EditingControlShowing;
        _gridView.CurrentCellDirtyStateChanged += (_, _) => {
            if (_gridView.IsCurrentCellDirty) _gridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
        };
        _gridView.CellValueChanged += GridView_CellValueChanged;
        _gridView.DataError += GridView_DataError;

        var addItemButton = CreateButton("New Item", 12, 150, AnchorStyles.Left);
        addItemButton.Click += AddItemButton_Click;

        var addGroupButton = CreateButton("New Group", 172, 150, AnchorStyles.Left);
        addGroupButton.Click += AddGroupButton_Click;

        var addToGroupButton = CreateButton("Add to Group", 332, 150, AnchorStyles.Left, false);
        addToGroupButton.Click += AddToGroupButton_Click;
        
        var removeButton = CreateButton("Remove Entry", 492, 150, AnchorStyles.Left);
        removeButton.Click += RemoveButton_Click;
        AcceptButton = CreateButton("Save", 1000, 75, AnchorStyles.Right, true, DialogResult.OK);
        ((Button)AcceptButton).Click += (_, _) => onSaveCallback(CombinationHelper.Serialize(_originalEntries));
        CancelButton = CreateButton("Cancel", 1085, 75, AnchorStyles.Right, true, DialogResult.Cancel);

        Controls.AddRange([_gridView, addItemButton, addGroupButton, addToGroupButton, removeButton, 
            (Control)AcceptButton, (Control)CancelButton]);

        SetupGridColumns();
        PopulateGrid(_originalEntries);

        _gridView.SelectionChanged += (_, _) => addToGroupButton.Enabled =
            _gridView.CurrentRow?.Tag is CombinationGroup && _gridView.CurrentCell.ColumnIndex != 0;
        _gridView.SelectionChanged += (_, _) => removeButton.Enabled =
            _gridView.CurrentRow?.Tag is CombinationEntry || _gridView.CurrentCell.ColumnIndex != 0;
        Paint += (_, e) => DrawCustomHeaders(e.Graphics);
    }
    

    private static Button CreateButton(string text, int x, int width, AnchorStyles anchor, bool enabled = true,
        DialogResult dialogResult = DialogResult.None) => new() {
            Text = text,
            Location = new Point(x, 500),
            Size = new Size(width, 35),
            Anchor = AnchorStyles.Bottom | anchor,
            Enabled = enabled,
            DialogResult = dialogResult
        };

    private void SetupGridColumns() {
        _gridView.Columns.Clear();

        var expandColumn = new DataGridViewTextBoxColumn {
            Name = "Expand",
            HeaderText = string.Empty,
            FillWeight = 10,
            ReadOnly = true,
            Resizable = DataGridViewTriState.False,
            ValueType = typeof(string)
        };

        var itemTypeColumn = new DataGridViewComboBoxColumn {
            Name = "ItemType",
            HeaderText = "Type",
            DataSource = new[] { "Combination", "Storable" },
            ValueType = typeof(string),
            FillWeight = 50
        };

        var itemColumn = new DataGridViewComboBoxColumn {
            Name = "Item",
            HeaderText = "Item",
            DisplayMember = "Name",
            ValueMember = "Id",
            ValueType = typeof(string),
            FillWeight = 100
        };

        _gridView.Columns.AddRange(expandColumn, itemTypeColumn, itemColumn,
            NewTextColumn("MinAmount", "Min", 25, typeof(int)),
            NewTextColumn("MaxAmount", "Max", 25, typeof(int)),
            NewTextColumn("Weight", "Weight", 30, typeof(int)),
            NewTextColumn("MinDurability", "Min", 25, typeof(int)),
            NewTextColumn("MaxDurability", "Max", 25, typeof(int)),
            NewTextColumn("Probability", "Probability (%)", 40, typeof(int))
        );

        _gridView.CellClick += GridView_CellClick;
        _gridView.CellEndEdit += GridView_CellEndEdit;
        _gridView.CellBeginEdit += GridView_CellBeginEdit;
        _gridView.CellPainting += GridView_CellPainting;
    }
    
    private static DataGridViewTextBoxColumn NewTextColumn(string name, string header, int fillWeight, Type valueType) {
        return new DataGridViewTextBoxColumn {
            Name = name,
            HeaderText = header,
            FillWeight = fillWeight,
            Resizable = DataGridViewTriState.False,
            ValueType = valueType
        };
    }

    private void GridView_EditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e) {
        if (e.Control is not ComboBox comboBox) return;
        comboBox.SelectedIndexChanged -= TypeSelector_SelectedIndexChanged;
        if (_gridView.CurrentCell?.ColumnIndex != 1) return;
        comboBox.SelectedIndexChanged += TypeSelector_SelectedIndexChanged;
    }
    
    private static void GridView_DataError(object? sender, DataGridViewDataErrorEventArgs e) {
        if (e is { Exception: ArgumentException, Context: DataGridViewDataErrorContexts.Formatting }) 
            e.Cancel = true;
    }

    private void TypeSelector_SelectedIndexChanged(object? sender, EventArgs e) {
        if (_gridView.CurrentRow is not { Tag: CombinationEntry entry } || _suppressEvents || 
            _gridView.CurrentCell?.OwningColumn?.Name != "ItemType")
            return;
        
        var comboBox = sender as ComboBox;
        var selectedType = comboBox?.SelectedItem?.ToString();

        _suppressEvents = true;

        try {
            var targetList = selectedType == "Storable" ? _storableItems : _comboItems;

            if (targetList.Count <= 0) return;
            var newId = targetList[0].Id;

            entry.Target = _vm.GetElement<Item, Other>(newId);

            if (_gridView.Rows[_gridView.CurrentRow.Index].Cells[2] is not DataGridViewComboBoxCell itemCell) return;
            itemCell.DataSource = targetList;
            itemCell.DisplayMember = "Name";
            itemCell.ValueMember = "Id";
                    
            var currentValue = itemCell.Value?.ToString();

            if (targetList.Any(ph => ph.Id == currentValue)) return;
            itemCell.Value = newId;
            entry.Target = _vm.GetElement<Item, Other>(newId);
        } finally {
            _suppressEvents = false;
        }
    }

    private void GridView_CellValueChanged(object? sender, DataGridViewCellEventArgs e) {
        if (_suppressEvents || e.RowIndex < 0) return;

        var row = _gridView.Rows[e.RowIndex];

        if (e.ColumnIndex != 2 || row.Tag is not CombinationEntry itemEntry) return;
        var newItemId = row.Cells[2].Value?.ToString();
        if (string.IsNullOrEmpty(newItemId)) return;
        itemEntry.Target = _vm.GetElement<Item, Other>(newItemId);
    }


    private void AddEntryToGrid(CombinationEntry entry, bool isGrouped, bool isLastInGroup = false) {
        var rowIndex = _gridView.Rows.Add();
        var row = _gridView.Rows[rowIndex];

        row.Tag = entry;
        
        row.Cells[0].Value = isGrouped ? (isLastInGroup ? "└─" : "├─") : "";

        var isStorable = _storableItems.Any(ph => ph.Id == entry.ItemId);
        var actualType = isStorable ? "Storable" : "Combination";
        var sourceList = isStorable ? _storableItems : _comboItems;
        
        row.Cells[1].Value = actualType;

        if (row.Cells[2] is DataGridViewComboBoxCell itemCell) {
            itemCell.DataSource = sourceList;
            itemCell.DisplayMember = "Name";
            itemCell.ValueMember = "Id";
            
            if (sourceList.Any(ph => ph.Id == entry.ItemId)) {
                itemCell.Value = entry.ItemId;
            } else if (sourceList.Count > 0) {
                itemCell.Value = sourceList[0].Id;
                entry.Target = _vm.GetElement<Item, Other>(sourceList[0].Id);
            }
        }

        row.Cells[3].Value = entry.MinAmount;
        row.Cells[4].Value = entry.MaxAmount;
        row.Cells[5].Value = entry.Weight;
        row.Cells[6].Value = entry.MinDurability;
        row.Cells[7].Value = entry.MaxDurability;
        row.Cells[8].Value = isGrouped ? null : entry.Probability;

        row.DefaultCellStyle.BackColor = isGrouped ? Color.FromArgb(240, 240, 240) : row.DefaultCellStyle.BackColor;

        row.Cells[0].ReadOnly = true;
        row.Cells[8].ReadOnly = isGrouped;
        row.Cells[8].Style.BackColor = isGrouped ? Color.LightGray : row.DefaultCellStyle.BackColor;
    }
    
    private void DrawCustomHeaders(Graphics g) {
        using var headerFont = new Font(_gridView.Font.FontFamily, _gridView.Font.Size, FontStyle.Bold);
        using var subHeaderFont = new Font(_gridView.Font.FontFamily, _gridView.Font.Size - 1);
        using var brush = new SolidBrush(Color.Black);
        using var pen = new Pen(Color.Gray);
        using var headerBrush = new SolidBrush(SystemColors.Control);

        const int headerHeight = 40, subHeaderHeight = 20;
        var gridLeft = _gridView.Left;
        var gridTop = _gridView.Top;

        var columns = new List<(string name, int x, int width)>();
        var currentX = gridLeft;
        
        foreach (DataGridViewColumn col in _gridView.Columns) {
            var displayRect = _gridView.GetColumnDisplayRectangle(col.Index, false);
            if (displayRect.Width <= 0) continue;
            columns.Add((col.Name, currentX, displayRect.Width));
            currentX += displayRect.Width;
        }

        var headerRect = new Rectangle(gridLeft, gridTop - headerHeight, _gridView.Width, headerHeight);
        g.FillRectangle(headerBrush, headerRect);
        g.DrawRectangle(pen, headerRect);

        var maxAmountCol = columns.FirstOrDefault(c => c.name == "MaxAmount");
        var maxDurabilityCol = columns.FirstOrDefault(c => c.name == "MaxDurability");

        foreach (var (name, x, width) in columns) {
            switch (name) {
                case "Expand":
                    var expandRect = new Rectangle(x, gridTop - headerHeight, width, headerHeight);
                    g.DrawLine(pen, expandRect.Right, expandRect.Top, expandRect.Right, expandRect.Bottom);
                    break;
                case "ItemType":
                    if (columns.FirstOrDefault(c => c.name == "Item") is var itemCol) {
                        var rect = new Rectangle(x, gridTop - headerHeight, width + itemCol.width, headerHeight);
                        DrawCenteredText(g, "Item", headerFont, brush, rect);
                        g.DrawRectangle(pen, rect);
                    }
                    break;
                case "MinAmount":
                    if (maxAmountCol != default) {
                        var amountWidth = width + maxAmountCol.width;
                        var rect = new Rectangle(x, gridTop - headerHeight, amountWidth, subHeaderHeight);
                        DrawCenteredText(g, "Amount", headerFont, brush, rect);
                        g.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom + subHeaderHeight);
                        g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
                    }
                    break;
                case "Weight":
                    var weightRect = new Rectangle(x, gridTop - headerHeight, width, headerHeight);
                    DrawCenteredText(g, "Weight", headerFont, brush, weightRect);
                    g.DrawLine(pen, weightRect.Right, weightRect.Top, weightRect.Right, weightRect.Bottom);
                    break;
                case "MinDurability":
                    if (maxDurabilityCol != default) {
                        var durabilityWidth = width + maxDurabilityCol.width;
                        var rect = new Rectangle(x, gridTop - headerHeight, durabilityWidth, subHeaderHeight);
                        DrawCenteredText(g, "Durability", headerFont, brush, rect);
                        g.DrawLine(pen, rect.Right, rect.Top, rect.Right, rect.Bottom + subHeaderHeight);
                        g.DrawLine(pen, rect.Left, rect.Bottom, rect.Right, rect.Bottom);
                    }
                    break;
                case "Probability":
                    var probRect = new Rectangle(x, gridTop - headerHeight, width, headerHeight);
                    DrawCenteredText(g, "Probability (%)", headerFont, brush, probRect);
                    break;
            }
        }

        foreach (var (name, x, width) in columns) {
            switch (name) {
                case "MinAmount":
                    var minAmountRect = new Rectangle(x, gridTop - subHeaderHeight, width, subHeaderHeight);
                    DrawCenteredText(g, "Min", subHeaderFont, brush, minAmountRect);
                    g.DrawLine(pen, x + width, gridTop - subHeaderHeight, x + width, gridTop);
                    break;
                    
                case "MaxAmount":
                    var maxAmountRect = new Rectangle(x, gridTop - subHeaderHeight, width, subHeaderHeight);
                    DrawCenteredText(g, "Max", subHeaderFont, brush, maxAmountRect);
                    break;
                    
                case "MinDurability":
                    var minDurRect = new Rectangle(x, gridTop - subHeaderHeight, width, subHeaderHeight);
                    DrawCenteredText(g, "Min", subHeaderFont, brush, minDurRect);
                    g.DrawLine(pen, x + width, gridTop - subHeaderHeight, x + width, gridTop);
                    break;
                    
                case "MaxDurability":
                    var maxDurRect = new Rectangle(x, gridTop - subHeaderHeight, width, subHeaderHeight);
                    DrawCenteredText(g, "Max", subHeaderFont, brush, maxDurRect);
                    break;
            }
        }
    }

    private static void DrawCenteredText(Graphics g, string text, Font font, Brush brush, Rectangle rect) {
        var size = g.MeasureString(text, font);
        g.DrawString(text, font, brush, rect.X + (rect.Width - size.Width) / 2, rect.Y + (rect.Height - size.Height) / 2);
    }

    private void GridView_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e) {
        if (e is not { ColumnIndex: 0, RowIndex: >= 0 }) return;
        if (_gridView.Rows[e.RowIndex].Tag is not CombinationGroup group) return;
        e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
        using var font = new Font("Arial", 8, FontStyle.Bold);
        var buttonText = group.IsCollapsed ? "+" : "–";
        var textSize = e.Graphics!.MeasureString(buttonText, font);
        var x = e.CellBounds.Left + (e.CellBounds.Width - textSize.Width) / 2;
        var y = e.CellBounds.Top + (e.CellBounds.Height - textSize.Height) / 2;
        e.Graphics.DrawString(buttonText, font, Brushes.Black, x, y);
        e.Handled = true;
    }

    private void GridView_CellEndEdit(object? sender, DataGridViewCellEventArgs e) {
        var row = _gridView.Rows[e.RowIndex];
        if (row.Tag is CombinationEntry entry) {
            var newItemId = row.Cells[2].Value?.ToString();
            if (!string.IsNullOrEmpty(newItemId)) 
                entry.Target = _vm.GetElement<Item, Other>(newItemId);
            
            if (int.TryParse(row.Cells[3].Value?.ToString(), out var minAmount))
                entry.MinAmount = minAmount;
            if (int.TryParse(row.Cells[4].Value?.ToString(), out var maxAmount))
                entry.MaxAmount = maxAmount;
            if (int.TryParse(row.Cells[5].Value?.ToString(), out var weight))
                entry.Weight = weight;
            if (int.TryParse(row.Cells[6].Value?.ToString(), out var minDur))
                entry.MinDurability = minDur;
            if (int.TryParse(row.Cells[7].Value?.ToString(), out var maxDur))
                entry.MaxDurability = maxDur;
            if (int.TryParse(row.Cells[8].Value?.ToString(), out var prob))
                entry.Probability = prob;
        } else if (row.Tag is CombinationGroup group) {
            if (int.TryParse(row.Cells[8].Value?.ToString(), out var prob))
                group.Probability = prob;
        }
    }

    private void GridView_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e) {
        if (e.RowIndex < 0 || _gridView.Rows[e.RowIndex].Tag is not CombinationGroup || e.ColumnIndex == 8) return;
        e.Cancel = true;
    }

    private void GridView_CellClick(object? sender, DataGridViewCellEventArgs e) {
        if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
        var row = _gridView.Rows[e.RowIndex];
        if (row.Tag is not CombinationGroup group) return;
        group.IsCollapsed = !group.IsCollapsed;
        _suppressEvents = true;
        PopulateGrid(_originalEntries);
        _suppressEvents = false;
    }

    private void AddItemButton_Click(object? sender, EventArgs e) {
        var firstItem = _comboItems.FirstOrDefault() ?? _storableItems.FirstOrDefault();
        if (firstItem == null) return;
        var entry = CombinationEntry.New(_vm.GetElement<Item, Other>(firstItem.Id));
        _originalEntries.Add(entry);
        _suppressEvents = true;
        PopulateGrid(_originalEntries);
        _suppressEvents = false;
    }

    private void AddGroupButton_Click(object? sender, EventArgs e) {
        var groupHead = new CombinationGroup { Probability = 100 };
        
        _originalEntries.Add(groupHead);
        _suppressEvents = true;
        PopulateGrid(_originalEntries);
        _suppressEvents = false;
    }

    private void AddToGroupButton_Click(object? sender, EventArgs e) {
        if (_gridView.CurrentRow?.Tag is not CombinationGroup group) return;
        
        var firstItem = _comboItems.FirstOrDefault() ?? _storableItems.FirstOrDefault();
        if (firstItem == null) return;
        
        var newItem = CombinationEntry.New(_vm.GetElement<Item, Other>(firstItem.Id));
        group.Items.Add(newItem);
        group.IsCollapsed = false;
        _suppressEvents = true;
        PopulateGrid(_originalEntries);
        _suppressEvents = false;
    }
    
    private void RemoveButton_Click(object? sender, EventArgs e) {
        if (_gridView.CurrentRow == null)
            return;
        switch (_gridView.CurrentRow.Tag) {
            case CombinationGroup group:
                _originalEntries.Remove(group);
                break;
            case CombinationEntry entry: {
                foreach (var part in _originalEntries) {
                    if (part is CombinationGroup grp && grp.Items.Remove(entry))
                        break;
                }
                _originalEntries.Remove(entry);
                break;
            }
        }
        PopulateGrid(_originalEntries);
    }

    private void PopulateGrid(List<ICombinationPart> entries) {
        _gridView.SuspendLayout();
        _gridView.Rows.Clear();
    
        foreach (var entry in entries) {
            switch (entry) {
                case CombinationGroup group: 
                    AddGroupHeaderToGrid(group);
                    if (group.IsCollapsed) continue;
                    for (var i = 0; i < group.Items.Count; i++) 
                        AddEntryToGrid(group.Items[i], true, isLastInGroup: i == group.Items.Count - 1);
                    break;
                case CombinationEntry singleItem:
                    AddEntryToGrid(singleItem, false);
                    break;
            }
        }
    
        _gridView.ResumeLayout();
        Invalidate();
    }

    private void AddGroupHeaderToGrid(CombinationGroup group) {
        var rowIndex = _gridView.Rows.Add();
        var row = _gridView.Rows[rowIndex];

        row.Tag = group;
        row.Cells[0].Value = DBNull.Value;

        for (var i = 0; i < _gridView.Columns.Count; i++) {
            var cell = row.Cells[i];

            switch (i) {
                case 0 or 8:
                    continue;
                case 1:
                    row.Cells[i] = new DataGridViewTextBoxCell { Value = "[GROUP]" };
                    row.Cells[i].ReadOnly = true;
                    row.Cells[i].Style.BackColor = Color.LightGray;
                    break;
                case 2:
                    row.Cells[i] = new DataGridViewTextBoxCell { Value = "One of the following:" };
                    row.Cells[i].ReadOnly = true;
                    row.Cells[i].Style.BackColor = Color.LightGray;
                    break;
                default:
                    cell.ReadOnly = true;
                    cell.Style.BackColor = Color.LightGray;
                    break;
            }
        }

        row.Cells[8].Value = group.Probability;
    }
}