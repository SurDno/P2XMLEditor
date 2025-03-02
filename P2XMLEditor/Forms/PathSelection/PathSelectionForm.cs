using P2XMLEditor.Abstract;
using P2XMLEditor.Forms.PathSelection.Validators;

namespace P2XMLEditor.Forms.PathSelection;

public sealed class PathSelectionForm : Form {
    private readonly PathValidator[] _validators;

    public record Paths(string VmPath, string TemplatesPath, string AssetDbPath);
    public Paths? SelectedPaths { get; private set; }

    public PathSelectionForm() {
        Text = "Configure Paths";
        Size = new(1200, 450);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        _validators = [new VmPathValidator(), new TemplatesPathValidator(), new AssetDbPathValidator()];
        InitializeLayout();
    }

    private void InitializeLayout() {
        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            Padding = new(10),
            RowCount = 6,
            ColumnCount = 3
        };

        layout.ColumnStyles.Add(new(SizeType.Percent, 20));
        layout.ColumnStyles.Add(new(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new(SizeType.Percent, 20));

        for (var i = 0; i < _validators.Length; i++) {
            _validators[i].AddToLayout(layout, i * 2);
            _validators[i].UpdateValidation();
        }

        Controls.Add(layout);

        var btnPanel = new FlowLayoutPanel {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 60,
            Padding = new(10)
        };

        var okButton = new Button { Text = "OK", Width = 120, Height = 35, Enabled = false };
        okButton.Click += (_, _) => {
            SelectedPaths = new(
                _validators[0].PathBox.Text,
                _validators[1].PathBox.Text,
                _validators[2].PathBox.Text
            );
            DialogResult = DialogResult.OK;
            Close();
        };

        btnPanel.Controls.AddRange([okButton]);
        Controls.Add(btnPanel);

        foreach (var validator in _validators)
            validator.PathBox.TextChanged += (_, _) => okButton.Enabled = _validators.All(v => v.Validate().IsValid);
    }
}