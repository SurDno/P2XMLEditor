using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace P2XMLEditor.WindowsFormsExtensions;

public class SearchControl : Panel {
    private readonly TextBox _searchBox;
    private readonly CheckBox _regexCheckBox;
    private readonly Label _statusLabel;
    
    public event EventHandler? SearchChanged;
    
    public string SearchText => _searchBox.Text;
    
    public bool IsRegexEnabled =>_regexCheckBox is { Checked: true };
    
    public string StatusText {
        get => _statusLabel.Text;
        set => _statusLabel.Text = value;
    }
    
    public SearchControl(bool enableRegex = true) {
        Height = 40;
        Dock = DockStyle.Top;
        
        var searchLabel = new Label {
            Text = "Search:",
            Location = new Point(10, 10),
            Size = new Size(70, 20),
            TextAlign = ContentAlignment.MiddleLeft
        };
        
        _searchBox = new TextBox {
            Location = new Point(85, 9),
            Size = new Size(300, 15)
        };
        _searchBox.TextChanged += (_, _) => OnSearchChanged();

        if (enableRegex) {
            _regexCheckBox = new CheckBox {
                Text = "Regex",
                Location = new Point(405, 11),
                Size = new Size(60, 20),
                AutoSize = true
            };
            _regexCheckBox.CheckedChanged += (_, _) => OnSearchChanged();
        }
        

        _statusLabel = new Label {
            Height = 30,
            AutoSize = true,
            TextAlign = ContentAlignment.MiddleRight,
            Anchor = AnchorStyles.Right,
            Padding = new Padding(0, 9, 10, 0)
        };

        _statusLabel.SizeChanged += (_, _) => {
            _statusLabel.Left = Width - _statusLabel.Width - 10;
        };
        
        Controls.AddRange(enableRegex ? [searchLabel, _searchBox, _regexCheckBox!, _statusLabel] : 
            [searchLabel, _searchBox, _statusLabel]);
    }
    
    public bool IsMatch(string text) {
        if (string.IsNullOrEmpty(SearchText))
            return true;
            
        if (string.IsNullOrEmpty(text))
            return false;

        if (!IsRegexEnabled) 
            return text.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase);
        
        try {
            return Regex.IsMatch(text, SearchText, RegexOptions.IgnoreCase);
        } catch (ArgumentException) {
            return text.Contains(SearchText, StringComparison.InvariantCultureIgnoreCase);
        }
    }
    
    public bool IsMatchAny(params string[] texts) => texts.Any(IsMatch);

    public void ClearSearch() => _searchBox.Text = string.Empty;

    public void FocusSearchBox() => _searchBox.Focus();

    private void OnSearchChanged() => SearchChanged?.Invoke(this, EventArgs.Empty);
}