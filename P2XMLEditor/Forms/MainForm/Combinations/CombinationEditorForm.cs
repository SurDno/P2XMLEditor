namespace P2XMLEditor.Forms.MainForm.Combinations;

public sealed class CombinationEditorForm : Form {
    private readonly Action<string> _onSaveCallback;
    private readonly DataGridView _gridView;
    private readonly List<ICombinationPart> _originalEntries;
    private readonly Dictionary<string, string> _availableItems;
    
    public CombinationEditorForm(string initialValue, Dictionary<string, string> availableItems, Action<string> onSaveCallback) {
        _onSaveCallback = onSaveCallback;
        _availableItems = availableItems;
        _originalEntries = CombinationDataParser.Parse(initialValue);

        Text = "Edit Combination Data";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterParent;

        _gridView = new DataGridView {
            Location = new Point(12, 12),
            Size = new Size(860, 480),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.CellSelect,
            EditMode = DataGridViewEditMode.EditOnEnter
        };

        var addItemButton = CreateButton("New Item", 12, 150, AnchorStyles.Left);
        addItemButton.Click += AddItemButton_Click;

        var addGroupButton = CreateButton("New Group", 172, 150, AnchorStyles.Left);
        addGroupButton.Click += AddGroupButton_Click;

        var addToGroupButton = CreateButton("Add to Group", 332, 150, AnchorStyles.Left, false);
        addToGroupButton.Click += AddToGroupButton_Click;

        var saveButton = CreateButton("Save", 700, 75, AnchorStyles.Right, true, DialogResult.OK);
        saveButton.Click += (_, _) => _onSaveCallback(CombinationDataParser.Serialize(_originalEntries));

        var cancelButton = CreateButton("Cancel", 785, 75, AnchorStyles.Right, true, DialogResult.Cancel);

        Controls.AddRange([_gridView, addItemButton, addGroupButton, addToGroupButton, saveButton, cancelButton]);

        SetupGridColumns();
        PopulateGrid(_originalEntries);

        _gridView.SelectionChanged += (_, _) => addToGroupButton.Enabled = _gridView.CurrentRow?.Tag is CombinationGroup; 
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
            HeaderText = "",
            Width = 30, 
            FillWeight = 15, 
            ReadOnly = true 
        };

        var itemColumn = new DataGridViewComboBoxColumn {
            Name = "Item",
            HeaderText = "Item",
            DataPropertyName = "Item",
            ValueType = typeof(string),
            FillWeight = 100
        };

        var itemList = _availableItems.Select(p => new { Id = p.Key, Name = p.Value }).OrderBy(x => x.Name).ToList();
        
        itemColumn.DataSource = itemList;
        itemColumn.DisplayMember = "Name";
        itemColumn.ValueMember = "Id";

        _gridView.Columns.AddRange(expandColumn, itemColumn, GetColumn("Min Amount"), GetColumn("Max Amount"),
            GetColumn("Weight"), GetColumn("Min Durability"), GetColumn("Max Durability"), GetColumn("Probability"));

        _gridView.CellClick += GridView_CellClick;
        _gridView.CellEndEdit += GridView_CellEndEdit;
        _gridView.CellBeginEdit += GridView_CellBeginEdit;
        _gridView.CellPainting += GridView_CellPainting;
    }

    private DataGridViewTextBoxColumn GetColumn(string name) => new() {
        Name = name.Replace(" ", ""), 
        HeaderText = name,
        ValueType = typeof(int), 
        FillWeight = 40
    };

    private void GridView_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e) {
        if (e is not { ColumnIndex: 0, RowIndex: >= 0 }) return;
        if (_gridView.Rows[e.RowIndex].Tag is not CombinationGroup group) return;
        e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
        using var font = new Font("Arial", 8, FontStyle.Bold);
        var buttonText = group.IsCollapsed ? "+" : "-";
        var textSize = e.Graphics.MeasureString(buttonText, font);
        var x = e.CellBounds.Left + (e.CellBounds.Width - textSize.Width) / 2;
        var y = e.CellBounds.Top + (e.CellBounds.Height - textSize.Height) / 2;
        e.Graphics.DrawString(buttonText, font, Brushes.Black, x, y);
        e.Handled = true;
    }

    private void GridView_CellEndEdit(object? sender, DataGridViewCellEventArgs e) {
        var row = _gridView.Rows[e.RowIndex];
        if (row.Tag is CombinationEntry entry) {
            entry.ItemId = row.Cells["Item"].Value?.ToString() ?? entry.ItemId;
            
            if (int.TryParse(row.Cells["MinAmount"].Value?.ToString(), out var minAmount))
                entry.MinAmount = minAmount;
            if (int.TryParse(row.Cells["MaxAmount"].Value?.ToString(), out var maxAmount))
                entry.MaxAmount = maxAmount;
            if (int.TryParse(row.Cells["Weight"].Value?.ToString(), out var weight))
                entry.Weight = weight;
            if (int.TryParse(row.Cells["MinDurability"].Value?.ToString(), out var minDur))
                entry.MinDurability = minDur;
            if (int.TryParse(row.Cells["MaxDurability"].Value?.ToString(), out var maxDur))
                entry.MaxDurability = maxDur;
            if (int.TryParse(row.Cells["Probability"].Value?.ToString(), out var prob))
                entry.Probability = prob;
        }
        else if (row.Tag is CombinationGroup group) {
            if (int.TryParse(row.Cells["Probability"].Value?.ToString(), out var prob))
                group.Probability = prob;
        }
    }

    private void GridView_CellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e) {
        if (e.RowIndex < 0) return;
        
        if (_gridView.Rows[e.RowIndex].Tag is not CombinationGroup) return;

        if (e.ColumnIndex != _gridView.Columns["Probability"]!.Index)
            e.Cancel = true;
    }

    private void GridView_CellClick(object? sender, DataGridViewCellEventArgs e) {
        if (e.RowIndex < 0 || e.ColumnIndex != 0) return;
            
        var row = _gridView.Rows[e.RowIndex];
        if (row.Tag is not CombinationGroup group) return;

        group.IsCollapsed = !group.IsCollapsed;
        PopulateGrid(_originalEntries);
    }

    private void AddItemButton_Click(object? sender, EventArgs e) {
        var firstItemId = _availableItems.Keys.FirstOrDefault() ?? "unknown";
        var entry = CombinationEntry.New(firstItemId);
    
        _originalEntries.Add(entry);
        PopulateGrid(_originalEntries);
    }

    private void AddGroupButton_Click(object? sender, EventArgs e) {
        var groupHead = new CombinationGroup { Probability = 100 };
        
        _originalEntries.Add(groupHead);
        PopulateGrid(_originalEntries);
    }

    private void AddToGroupButton_Click(object? sender, EventArgs e) {
        if (_gridView.CurrentRow?.Tag is not CombinationGroup group) return;
        var newItem = CombinationEntry.New(_availableItems.Keys.FirstOrDefault() ?? "unknown");
        group.Items.Add(newItem);
        group.IsCollapsed = false;
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
    }

    private void AddGroupHeaderToGrid(CombinationGroup group) {
        var rowIndex = _gridView.Rows.Add();
        var row = _gridView.Rows[rowIndex];
    
        row.Tag = group;
        row.Cells["Expand"].Value = DBNull.Value;
    
        foreach (DataGridViewCell cell in row.Cells) {
            if (cell.OwningColumn.Name is "Expand" or "Probability") continue;
            cell.ReadOnly = true;
            cell.Style.BackColor = Color.LightGray;
        }
    
        var textCell = new DataGridViewTextBoxCell { Value = "One of the following" };
        textCell.Style.BackColor = Color.LightGray;
        row.Cells["Item"] = textCell;
        row.Cells["Item"].ReadOnly = true;
    
        row.Cells["Probability"].Value = group.Probability;
    }

    private void AddEntryToGrid(CombinationEntry entry, bool isGrouped, bool isLastInGroup = false) {
        var rowIndex = _gridView.Rows.Add();
        var row = _gridView.Rows[rowIndex];

        row.Tag = entry;
        row.Cells["Expand"].Value = isGrouped ? (isLastInGroup ? "└─" : "├─") : "";
        row.Cells["Item"].Value = entry.ItemId;
        row.Cells["MinAmount"].Value = entry.MinAmount;
        row.Cells["MaxAmount"].Value = entry.MaxAmount;
        row.Cells["Weight"].Value = entry.Weight;
        row.Cells["MinDurability"].Value = entry.MinDurability;
        row.Cells["MaxDurability"].Value = entry.MaxDurability;
        row.Cells["Probability"].Value = isGrouped ? null : entry.Probability;

        row.DefaultCellStyle.BackColor = isGrouped ? Color.FromArgb(240, 240, 240) : row.DefaultCellStyle.BackColor;

        row.Cells["Expand"].ReadOnly = true;
        row.Cells["Probability"].ReadOnly = isGrouped;
    }
}