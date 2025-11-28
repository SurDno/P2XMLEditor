using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.PathSelection;

public class VmSelectionForm : Form {
    private ListBox? _listBox;

    public VmSelectionForm(string basePath) {
        InitializeComponents();
        LoadDirectories(basePath);
    }

    private void InitializeComponents() {
        Text = "Select Virtual Machine";
        Size = new(600, 300);
        StartPosition = FormStartPosition.CenterParent;

        _listBox = new() { Dock = DockStyle.Fill, Margin = new(10) };

        FlowLayoutPanel panel = new() { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 40 };
        var okButton = new Button { Text = "OK", Height = 35, DialogResult = DialogResult.OK, Enabled = false };
        CancelButton = new Button { Text = "Cancel", Height = 35, DialogResult = DialogResult.Cancel };
        panel.Controls.AddRange([(Control)CancelButton, okButton]);
        
        Controls.AddRange([_listBox, panel]);

        _listBox.SelectedIndexChanged += (_, _) => { okButton.Enabled = _listBox.SelectedItem != null; };

        _listBox.DoubleClick += (_, _) => {
            if (_listBox.SelectedItem != null)
                DialogResult = DialogResult.OK;
        };
    }

    private void LoadDirectories(string basePath) {
        try {
            _listBox!.Items.AddRange(Directory.GetDirectories(basePath).Select(Path.GetFileName).ToArray());
        } catch (Exception ex) {
            ErrorHandler.Handle("Error loading directories", ex);
        }
    }

    public string? GetSelectedPath(string basePath) => 
        _listBox?.SelectedItem != null ? Path.Combine(basePath, _listBox.SelectedItem.ToString()!) : null;
}
