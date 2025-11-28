using System.Drawing;
using System.Windows.Forms;

namespace P2XMLEditor.Abstract;

public abstract class PathValidator {
    public TextBox PathBox { get; }
    private readonly Label _errorLabel = new() { AutoSize = true };
    private readonly FlowLayoutPanel _buttonPanel;

    public readonly record struct PathValidationResult(bool IsValid, string Text, Color Color);

    protected PathValidator() {
        PathBox = new();
        PathBox.ReadOnly = true;
        PathBox.Dock = DockStyle.Fill;
        PathBox.TextChanged += (_, _) => UpdateValidation();

        _buttonPanel = new() { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Anchor = AnchorStyles.Left };

        Button browseBtn = new() { Text = "Browse", Width = 100, Height = PathBox.PreferredHeight, Margin = new(0) };
        browseBtn.Click += (_, _) => Browse();
        _buttonPanel.Controls.Add(browseBtn);
        Button locateBtn = new() { Text = "Locate", Width = 100, Height = PathBox.PreferredHeight, Margin = new(0) };
        locateBtn.Click += (_, _) => Locate();
        _buttonPanel.Controls.Add(locateBtn);
    }

    public void AddToLayout(TableLayoutPanel layout, int row) {
        layout.Controls.Add(new Label { Text = GetLabelText(), Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
        layout.Controls.Add(PathBox, 1, row);
        layout.Controls.Add(_buttonPanel, 2, row);
        layout.Controls.Add(_errorLabel, 1, row + 1);
    }

    public void UpdateValidation() {
        var result = Validate();
        _errorLabel.Text = result.Text;
        _errorLabel.ForeColor = result.Color;
    }

    protected virtual void Browse() {
        using var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
            PathBox.Text = dialog.SelectedPath;
    }

    public abstract PathValidationResult Validate();
    protected abstract string GetLabelText();
    protected abstract void Locate();
    
    protected static PathValidationResult Error(string message) => new(false, message, Color.Red);
    protected static PathValidationResult Hint(string message) => new(false, message, Color.Black);
    protected static PathValidationResult Success(string message) => new(true, message, Color.Green);
}