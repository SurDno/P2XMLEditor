using P2XMLEditor.Abstract;
using P2XMLEditor.Forms.PathSelection.Validators;
using P2XMLEditor.Parsing;

namespace P2XMLEditor.Forms.PathSelection;

public sealed class PathSelectionForm : Form {
    private readonly PathValidator[] _validators;

    public record Paths(string VmPath, string TemplatesPath, string AssetDbPath, ParsingMode Mode);

    public Paths? SelectedPaths { get; private set; }

    private ComboBox _parsingModeCombo;

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

        var bottomPanel = new TableLayoutPanel {
            Dock = DockStyle.Bottom,
            Height = 60,
            ColumnCount = 2,
            Padding = new Padding(10)
        };

        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        var modePanel = new FlowLayoutPanel {
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Fill,
            AutoSize = false
        };

        var modeLabel = new Label {
            Text = "Parsing Mode:",
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(0, 6, 0, 0)
        };

        _parsingModeCombo = new ComboBox {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 220
        };

        _parsingModeCombo.Items.Add("XML Reader (fastest)");
        _parsingModeCombo.Items.Add("XElement (legacy)");
        _parsingModeCombo.SelectedIndex = 0;

        modePanel.Controls.Add(modeLabel);
        modePanel.Controls.Add(_parsingModeCombo);
        bottomPanel.Controls.Add(modePanel, 0, 0);

        var okPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };

        var okButton = new Button {
            Text = "OK",
            Width = 120,
            Height = 35,
            Enabled = false
        };

        okButton.Click += (_, _) => {
            var mode = _parsingModeCombo.SelectedIndex == 0 ? ParsingMode.XmlReader : ParsingMode.XElement;

            SelectedPaths = new(
                _validators[0].PathBox.Text,
                _validators[1].PathBox.Text,
                _validators[2].PathBox.Text,
                mode
            );

            DialogResult = DialogResult.OK;
            Close();
        };

        okPanel.Controls.Add(okButton);
        bottomPanel.Controls.Add(okPanel, 1, 0);

        Controls.Add(bottomPanel);

        foreach (var validator in _validators)
            validator.PathBox.TextChanged += (_, _) => okButton.Enabled = _validators.All(v => v.Validate().IsValid);
    }
}