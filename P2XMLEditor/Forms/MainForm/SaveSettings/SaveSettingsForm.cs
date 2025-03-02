using P2XMLEditor.Data;

namespace P2XMLEditor.Forms.MainForm.SaveSettings;

public class SaveSettingsForm : Form {
    private readonly CheckBox _cleanUpOrphanedElements;
    private readonly CheckBox _cleanUpUnusedProperties;
    private readonly CheckBox _cleanUpNames;
    private readonly CheckBox _cleanUpEmptyStrings;
    private readonly Button _okButton;
    private readonly Button _cancelButton;

    public WriterSettings Settings { get; private set; }

    public SaveSettingsForm() {
        Text = "Save Virtual Machine...";
        Size = new Size(800, 350);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;

        _cleanUpOrphanedElements = new CheckBox {
            Text = "Clean up orphaned elements",
            Location = new Point(20, 20),
            Width = 350
        };

        _cleanUpUnusedProperties = new CheckBox {
            Text = "Clean up unused properties",
            Location = new Point(20, 60),
            Width = 350
        };

        _cleanUpNames = new CheckBox {
            Text = "Clean up names",
            Location = new Point(20, 100),
            Width = 350
        };

        _cleanUpEmptyStrings = new CheckBox {
            Text = "Clean up empty strings",
            Location = new Point(20, 140),
            Width = 350
        };

        _okButton = new Button {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(200, 180),
            Width = 75,
            Height = 30
        };

        _cancelButton = new Button {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(290, 170),
            Width = 75,
            Height = 30
        };

        Controls.AddRange([_cleanUpOrphanedElements, _cleanUpUnusedProperties, _cleanUpNames, _cleanUpEmptyStrings, _okButton, _cancelButton]);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        _okButton.Click += OkButton_Click;
    }

    private void OkButton_Click(object? sender, EventArgs e) {
        Settings = new WriterSettings {
            CleanUpOrphanedElements = _cleanUpOrphanedElements.Checked,
            CleanUpUnusedProperties = _cleanUpUnusedProperties.Checked,
            CleanUpNames = _cleanUpNames.Checked,
            CleanUpEmptyStrings = _cleanUpEmptyStrings.Checked
        };
    }
}