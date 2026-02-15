using System;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.MainForm.Dialogs;

public class DialogBrowser : SplitContainer {
    private readonly VirtualMachine _vm;
    private readonly SearchControl _searchControl;
    private readonly ListView _dialogList;
    private DialogGraphViewer? _currentViewer;
    
    public DialogBrowser(VirtualMachine vm) {
        _vm = vm;
        Dock = DockStyle.Fill;
        Orientation = Orientation.Vertical;
        SplitterDistance = 400;
        
        var leftPanel = new Panel { 
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };
        Panel1.Controls.Add(leftPanel);
        
        _searchControl = new SearchControl(enableRegex: false) { Dock = DockStyle.Top };
        _searchControl.SearchChanged += (_, _) => LoadDialogs();
        leftPanel.Controls.Add(_searchControl);
        
        _dialogList = new ListView {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            Top = _searchControl.Bottom + 5
        };
        _dialogList.Columns.Add("Dialog", 400);
        _dialogList.SelectedIndexChanged += OnDialogSelected;
        leftPanel.Controls.Add(_dialogList);
        
        LoadDialogs();
    }

    private void LoadDialogs() {
        _dialogList.Items.Clear();

        var talkings = _vm.GetElementsByType<Talking>()
            .OrderBy(t => t.Name)
            .ToList();

        var displayedCount = 0;

        foreach (var talking in talkings) {
            var searchText = talking.Name;

            if (!_searchControl.IsMatchAny(searchText, talking.Id.ToString()))
                continue;

            var item = new ListViewItem(talking.Name) { Tag = talking };
            _dialogList.Items.Add(item);
            displayedCount++;
        }

        _searchControl.StatusText = $"Displaying {displayedCount}/{talkings.Count} dialogs.";
    }

    private void OnDialogSelected(object? sender, EventArgs e) {
        if (_dialogList.SelectedItems.Count == 0) return;
        
        var talking = (Talking)_dialogList.SelectedItems[0].Tag;
        LoadDialog(talking);
    }

    public void LoadDialog(Talking talking) {
        _currentViewer?.Dispose();
        
        _currentViewer = new DialogGraphViewer(_vm, talking) {
            Dock = DockStyle.Fill
        };
        Panel2.Controls.Clear();
        Panel2.Controls.Add(_currentViewer);
    }
}