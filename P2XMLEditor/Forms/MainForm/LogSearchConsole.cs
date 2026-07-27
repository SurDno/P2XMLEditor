using System;
using System.Drawing;
using System.Windows.Forms;

namespace P2XMLEditor.Forms.MainForm;

public sealed class LogSearchConsole : UserControl {
	private readonly TextBox _searchBox;
	private readonly Label _statusLabel;
	private readonly CheckBox _regexToggle;
	private readonly Label _nextBtn;
	private readonly Label _prevBtn;
	private readonly Label _closeBtn;
	public event Action<string, bool, bool>? SearchRequested;
	public event Action? NextRequested;
	public event Action? PrevRequested;
	public event Action? CloseRequested;

	public LogSearchConsole() {
		Size = new Size(520, 32);
		BackColor = SystemColors.Control;
		BorderStyle = BorderStyle.FixedSingle;
		var value = new Label {
			Text = "Search:", AutoSize = true, Location = new Point(5, 8), TextAlign = ContentAlignment.MiddleLeft
		};
		_searchBox = new TextBox {
			Location = new Point(55, 4),
			Size = new Size(150, 23),
			BackColor = SystemColors.Window,
			ForeColor = SystemColors.WindowText,
			BorderStyle = BorderStyle.Fixed3D,
			Font = new Font("Segoe UI", 9f)
		};
		_searchBox.TextChanged += delegate { OnSearchChanged(); };
		_searchBox.KeyDown += delegate(object? s, KeyEventArgs e) {
			if (e.KeyCode == Keys.Return) {
				if (e.Shift) {
					PrevRequested?.Invoke();
				} else {
					NextRequested?.Invoke();
				}

				e.SuppressKeyPress = true;
			} else if (e.KeyCode == Keys.Escape) {
				CloseRequested?.Invoke();
			}
		};
		_regexToggle = new CheckBox {
			Text = "Regex", FlatStyle = FlatStyle.System, AutoSize = true, Location = new Point(235, 7)
		};
		_regexToggle.CheckedChanged += delegate { OnSearchChanged(); };
		_statusLabel = new Label {
			Location = new Point(310, 8),
			Size = new Size(95, 18),
			ForeColor = SystemColors.ControlDarkDark,
			TextAlign = ContentAlignment.MiddleRight,
			Font = new Font("Segoe UI", 9f, FontStyle.Bold)
		};
		_prevBtn = CreateSymbolButton("▲", new Point(415, 3));
		_nextBtn = CreateSymbolButton("▼", new Point(445, 3));
		_closeBtn = CreateSymbolButton("✕", new Point(480, 3));
		_nextBtn.Click += delegate { NextRequested?.Invoke(); };
		_prevBtn.Click += delegate { PrevRequested?.Invoke(); };
		_closeBtn.Click += delegate { CloseRequested?.Invoke(); };
		Controls.Add(_searchBox);
		Controls.Add(value);
		Controls.Add(_regexToggle);
		Controls.Add(_statusLabel);
		Controls.Add(_prevBtn);
		Controls.Add(_nextBtn);
		Controls.Add(_closeBtn);
	}

	private static Label CreateSymbolButton(string text, Point location) {
		var label = new Label {
			Text = text,
			Size = new Size(27, 27),
			Location = location,
			TextAlign = ContentAlignment.MiddleCenter,
			Font = new Font("Segoe UI", 9f),
			Cursor = Cursors.Hand,
			Padding = new Padding(4, 0, 0, 6)
		};
		label.MouseEnter += (_, _) =>  label.BackColor = Color.LightGray;
		label.MouseLeave += (_, _) => label.BackColor = Color.Transparent;
		return label;
	}

	private void OnSearchChanged() {
		SearchRequested?.Invoke(_searchBox.Text, _regexToggle.Checked, arg3: false);
	}

	public void SetStatus(int current, int total) {
		_statusLabel.Text = ((total == 0) ? "No results" : $"{current} of {total}");
	}

	public void FocusSearch() {
		_searchBox.Focus();
		_searchBox.SelectAll();
	}
}