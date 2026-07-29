using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Abstract;
using P2XMLEditor.Data;
using P2XMLEditor.Forms.PathSelection.Validators;
using P2XMLEditor.Parsing;

namespace P2XMLEditor.Forms.PathSelection;

public sealed class PathSelectionForm : Form {
    private readonly PathValidator[] _validators;

    public record Paths(string VmPath, string TemplatesPath, string AssetDbPath, ParsingMode Mode, bool Parallel);

    public Paths? SelectedPaths { get; private set; }

    private ComboBox _parsingModeCombo;
    private CheckBox _parallelCheckBox;
    private Button _okButton;

    public PathSelectionForm() {
        Text = "Configure Paths";
        Size = new(1200, 450);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        _validators = [new VmPathValidator(), new TemplatesPathValidator(), new AssetDbPathValidator()];
        InitializeLayout();
        UpdateParsingModes();
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
            Height = 80,
            ColumnCount = 2,
            Padding = new Padding(10)
        };

        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        bottomPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var configPanel = new FlowLayoutPanel {
            FlowDirection = FlowDirection.LeftToRight,
            Dock = DockStyle.Fill,
            AutoSize = true
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

        _parallelCheckBox = new CheckBox {
            Text = "Parallel parser execution",
            Checked = true,
            AutoSize = true,
            Margin = new Padding(20, 4, 0, 0)
        };

        configPanel.Controls.Add(modeLabel);
        configPanel.Controls.Add(_parsingModeCombo);
        configPanel.Controls.Add(_parallelCheckBox);
        bottomPanel.Controls.Add(configPanel, 0, 0);

        var okPanel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };

        _okButton = new Button {
            Text = "OK",
            Width = 120,
            Height = 35,
            Enabled = false
        };

        _okButton.Click += (_, _) => {
            var mode = ParsingMode.Fastest;
            if (_parsingModeCombo.SelectedItem != null) {
                mode = _parsingModeCombo.SelectedItem.ToString() switch {
                    "XML Reader" => ParsingMode.XmlReader,
                    "XElement (legacy)" => ParsingMode.XElement,
                    "Demo XElement (native)" => ParsingMode.XElement,
                    _ => ParsingMode.Fastest
                };
            }

            SelectedPaths = new(
                _validators[0].PathBox.Text,
                _validators[1].PathBox.Text,
                _validators[2].PathBox.Text,
                mode,
                _parallelCheckBox.Checked
            );

            DialogResult = DialogResult.OK;
            Close();
        };

        okPanel.Controls.Add(_okButton);
        bottomPanel.Controls.Add(okPanel, 1, 0);

        Controls.Add(bottomPanel);

        foreach (var validator in _validators) {
            validator.PathBox.TextChanged += (_, _) => {
                _okButton.Enabled = _validators.All(v => v.Validate().IsValid);
                UpdateParsingModes();
            };
        }
    }

    private void UpdateParsingModes() {
        var path = _validators[0].PathBox.Text;
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) {
            ResetParsingModes();
            return;
        }

        var isDemo = false;
        if (File.Exists(Path.Combine(path, "Version.xml"))) {
            isDemo = false;
        } else if (Directory.GetFiles(path, "*.xml.gz").Length > 0) {
            isDemo = true;
        }

        if (isDemo) {
            SetDemoParsingModes();
        } else {
            ResetParsingModes();
        }
    }

    private void SetDemoParsingModes() {
        _parsingModeCombo.Items.Clear();
        _parsingModeCombo.Items.Add("Demo XElement (native)");
        _parsingModeCombo.SelectedIndex = 0;
        _parsingModeCombo.Enabled = false;
    }

    private void ResetParsingModes() {
        if (_parsingModeCombo.Enabled && _parsingModeCombo.Items.Count > 1) return;
        
        _parsingModeCombo.Enabled = true;
        var current = _parsingModeCombo.SelectedItem?.ToString();
        _parsingModeCombo.Items.Clear();
        _parsingModeCombo.Items.Add("Fastest");
        _parsingModeCombo.Items.Add("XML Reader");
        _parsingModeCombo.Items.Add("XElement (legacy)");
        
        if (current != null && _parsingModeCombo.Items.Contains(current))
            _parsingModeCombo.SelectedItem = current;
        else
            _parsingModeCombo.SelectedIndex = 0;
    }
}
