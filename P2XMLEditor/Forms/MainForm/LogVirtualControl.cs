using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Forms.MainForm;

public sealed class LogVirtualControl : Control {
	private record ParsedLogEntry(string Raw, string Timestamp, string Level, string Message, LogLevel LogLevel);
	private readonly List<ParsedLogEntry> _entries = [];
	private readonly VScrollBar _vScrollBar;
	private int _itemHeight = 22;
	private readonly Font _boldFont;
	private readonly Font _regularFont;
	private int _hoverIndex = -1;
	private readonly HashSet<int> _selectedIndices = [];
	private int _selectionAnchorIndex = -1;
	private List<int> _searchResultIndices = [];
	private int _currentSearchResultIndex = -1;
	private static readonly Regex LogRegex = new Regex("^\\[(?<timestamp>.*?)\\]\\s+(?<level>\\w+):\\s+(?<message>.*)$", RegexOptions.Compiled);
	public event Action<string>? LogEntrySelected;
	public LogVirtualControl() {
		DoubleBuffered = true;
		SetStyle(ControlStyles.Selectable | ControlStyles.UserMouse, value: true);
		_regularFont = new Font("Consolas", 9f);
		_boldFont = new Font("Consolas", 9f, FontStyle.Bold);
		_vScrollBar = new VScrollBar {
			Dock = DockStyle.Right,
			SmallChange = 1,
			LargeChange = 10
		};
		_vScrollBar.ValueChanged += delegate {
			InvalidatedWithScroll();
		};
		Controls.Add(_vScrollBar);
		BackColor = Color.White;
		ForeColor = Color.Black;
	}
	public void AddLog(string raw) {
		var match = LogRegex.Match(raw);
		ParsedLogEntry item;
		if (match.Success) {
			var value = match.Groups["level"].Value;
			if (!Enum.TryParse<LogLevel>(value, ignoreCase: true, out var result)) {
				result = LogLevel.Info;
			}
			item = new ParsedLogEntry(raw, match.Groups["timestamp"].Value, value, match.Groups["message"].Value, result);
		} else {
			item = new ParsedLogEntry(raw, "", "", raw, LogLevel.Info);
		}
		lock (_entries) {
			_entries.Add(item);
			UpdateScroll();
		}
		if (_vScrollBar.Value >= _vScrollBar.Maximum - _vScrollBar.LargeChange - 1) {
			ScrollToBottom();
		} else {
			Invalidate();
		}
	}
	public void AddLogs(IEnumerable<string> raws) {
		lock (_entries) {
			foreach (var raw in raws) {
				var match = LogRegex.Match(raw);
				if (match.Success) {
					var value = match.Groups["level"].Value;
					if (!Enum.TryParse<LogLevel>(value, ignoreCase: true, out var result)) {
						result = LogLevel.Info;
					}
					_entries.Add(new ParsedLogEntry(raw, match.Groups["timestamp"].Value, value, match.Groups["message"].Value, result));
				} else {
					_entries.Add(new ParsedLogEntry(raw, "", "", raw, LogLevel.Info));
				}
			}
			UpdateScroll();
		}
		ScrollToBottom();
	}
	private void UpdateScroll() {
		if (InvokeRequired) {
			BeginInvoke(UpdateScroll);
			return;
		}
		_vScrollBar.Maximum = Math.Max(0, _entries.Count);
		_vScrollBar.LargeChange = Height / _itemHeight;
	}
	public void ScrollToBottom() {
		if (InvokeRequired) {
			BeginInvoke(ScrollToBottom);
			return;
		}
		if (_vScrollBar.Maximum > _vScrollBar.LargeChange) {
			_vScrollBar.Value = _vScrollBar.Maximum - _vScrollBar.LargeChange;
		}
		Invalidate();
	}
	private void InvalidatedWithScroll() {
		Invalidate();
	}
	protected override void OnResize(EventArgs e) {
		base.OnResize(e);
		UpdateScroll();
		Refresh();
	}
	protected override void OnMouseWheel(MouseEventArgs e) {
		base.OnMouseWheel(e);
		var value = _vScrollBar.Value - e.Delta / 120 * 3;
		_vScrollBar.Value = Math.Clamp(value, 0, Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange));
	}
	protected override void OnPaint(PaintEventArgs e) {
		var graphics = e.Graphics;
		var value = _vScrollBar.Value;
		var num = Height / _itemHeight + 1;
		var num2 = Math.Min(_entries.Count, value + num);
		for (var i = value; i < num2; i++) {
			var parsedLogEntry = _entries[i];
			var rect = new Rectangle(0, (i - value) * _itemHeight, Width - _vScrollBar.Width, _itemHeight);
			var color = GetBackgroundColor(parsedLogEntry.LogLevel);
			if (_selectedIndices.Contains(i)) {
				color = Color.FromArgb(200, 210, 230);
			} else if (i == _hoverIndex) {
				color = Color.FromArgb(245, 245, 245);
			}
			using (var brush = new SolidBrush(color)) {
				graphics.FillRectangle(brush, rect);
			}
			var num3 = _searchResultIndices.Count > 0;
			var flag = _searchResultIndices.Contains(i);
			var alpha = ((!num3 || flag) ? 255 : 60);
			if (flag && i == GetCurrentSearchResultIndexInList()) {
				using var pen = new Pen(Color.FromArgb(120, 0, 120, 215), 1f);
				graphics.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);
			}
			DrawIcon(graphics, parsedLogEntry.LogLevel, rect.X + 5, rect.Y + (rect.Height - 16) / 2, alpha);
			var num4 = 25;
			var text = (string.IsNullOrEmpty(parsedLogEntry.Timestamp) ? "" : ("[" + parsedLogEntry.Timestamp + "] "));
			if (!string.IsNullOrEmpty(text)) {
				using (var brush2 = new SolidBrush(Color.FromArgb(alpha, Color.DimGray))) {
					graphics.DrawString(text, _boldFont, brush2, num4, rect.Y + (rect.Height - graphics.MeasureString(text, _boldFont).Height) / 2f);
				}
				num4 += (int)graphics.MeasureString(text, _boldFont).Width + 2;
			}
			var s = (string.IsNullOrEmpty(parsedLogEntry.Level) ? parsedLogEntry.Raw : (parsedLogEntry.Level + ": " + parsedLogEntry.Message));
			var foreColor = GetForeColor(parsedLogEntry.LogLevel);
			using var brush3 = new SolidBrush(Color.FromArgb(alpha, foreColor));
			using var format = new StringFormat {
				Trimming = StringTrimming.EllipsisCharacter,
				FormatFlags = StringFormatFlags.NoWrap,
				LineAlignment = StringAlignment.Center
			};
			graphics.DrawString(layoutRectangle: new Rectangle(num4, rect.Y, rect.Width - num4 + 5, rect.Height), s: s, font: _regularFont, brush: brush3, format: format);
		}
	}
	private int GetCurrentSearchResultIndexInList() {
		if (_currentSearchResultIndex >= 0 && _currentSearchResultIndex < _searchResultIndices.Count) {
			return _searchResultIndices[_currentSearchResultIndex];
		}
		return -1;
	}
	private static Color GetBackgroundColor(LogLevel level) {
		switch (level) {
			case LogLevel.Fatal:
			case LogLevel.Error:
				return Color.FromArgb(255, 235, 235);
			case LogLevel.Warning:
				return Color.FromArgb(255, 250, 225);
			case LogLevel.Success:
				return Color.FromArgb(235, 255, 235);
			case LogLevel.Performance:
				return Color.FromArgb(240, 245, 255);
			default:
				return Color.Transparent;
		}
	}
	private static Color GetForeColor(LogLevel level) {
		switch (level) {
			case LogLevel.Fatal:
			case LogLevel.Error:
				return Color.FromArgb(180, 0, 0);
			case LogLevel.Warning:
				return Color.FromArgb(160, 100, 0);
			case LogLevel.Success:
				return Color.FromArgb(0, 120, 0);
			case LogLevel.Performance:
				return Color.FromArgb(0, 0, 180);
			default:
				return Color.Black;
		}
	}
	private void DrawIcon(Graphics g, LogLevel level, int x, int y, int alpha = 255) {
		g.SmoothingMode = SmoothingMode.AntiAlias;
		var foreColor = GetForeColor(level);
		using var pen = new Pen(Color.FromArgb(alpha, foreColor), 2f);
		switch (level) {
			case LogLevel.Fatal:
			case LogLevel.Error: {
					using var icon3 = new Icon(SystemIcons.Error, 16, 16);
					g.DrawIcon(icon3, new Rectangle(x, y, 16, 16));
					break;
				}
			case LogLevel.Warning: {
					using var icon2 = new Icon(SystemIcons.Warning, 16, 16);
					g.DrawIcon(icon2, new Rectangle(x, y, 16, 16));
					break;
				}
			case LogLevel.Success: {
					g.DrawEllipse(pen, x, y, 14, 14);
					using var pen2 = new Pen(Color.FromArgb(alpha, Color.LimeGreen), 2f);
					g.DrawLine(pen2, x + 3, y + 7, x + 6, y + 10);
					g.DrawLine(pen2, x + 6, y + 10, x + 11, y + 4);
					break;
				}
			case LogLevel.Performance:
				g.DrawEllipse(pen, x, y, 14, 14);
				g.DrawLine(pen, x + 7, y + 7, x + 7, y + 3);
				g.DrawLine(pen, x + 7, y + 7, x + 11, y + 7);
				break;
			case LogLevel.Trace: {
					using var brush = new SolidBrush(Color.FromArgb(alpha, 120, 120, 120));
					g.FillRectangle(brush, x + 5, y + 5, 6, 6);
					break;
				}
			default: {
					using var icon = new Icon(SystemIcons.Information, 16, 16);
					g.DrawIcon(icon, new Rectangle(x, y, 16, 16));
					break;
				}
		}
	}
	public void PerformSearch(string query, bool useRegex, bool caseSensitive) {
		_searchResultIndices.Clear();
		_currentSearchResultIndex = -1;
		if (string.IsNullOrEmpty(query)) {
			Invalidate();
			return;
		}
		lock (_entries) {
			for (var i = 0; i < _entries.Count; i++) {
				var flag = false;
				if (useRegex) {
					try {
						var options = ((!caseSensitive) ? RegexOptions.IgnoreCase : RegexOptions.None);
						flag = Regex.IsMatch(_entries[i].Raw, query, options);
					} catch {
					}
				} else {
					var comparisonType = (caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
					flag = _entries[i].Raw.Contains(query, comparisonType);
				}
				if (flag) {
					_searchResultIndices.Add(i);
				}
			}
		}
		if (_searchResultIndices.Count > 0) {
			_currentSearchResultIndex = 0;
			JumpToSearchResult(0);
		}
		Invalidate();
	}
	public void NextSearchResult() {
		if (_searchResultIndices.Count != 0) {
			_currentSearchResultIndex = (_currentSearchResultIndex + 1) % _searchResultIndices.Count;
			JumpToSearchResult(_currentSearchResultIndex);
		}
	}
	public void PrevSearchResult() {
		if (_searchResultIndices.Count != 0) {
			_currentSearchResultIndex = (_currentSearchResultIndex - 1 + _searchResultIndices.Count) % _searchResultIndices.Count;
			JumpToSearchResult(_currentSearchResultIndex);
		}
	}
	private void JumpToSearchResult(int resultIndex) {
		var num = _searchResultIndices[resultIndex];
		if (num < _vScrollBar.Value || num >= _vScrollBar.Value + _vScrollBar.LargeChange) {
			_vScrollBar.Value = Math.Clamp(num - _vScrollBar.LargeChange / 2, 0, Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange));
		}
		Invalidate();
	}
	public (int current, int total) GetSearchStatus() => (current: _currentSearchResultIndex + 1, total: _searchResultIndices.Count);
	public void ClearSelection() {
		_selectedIndices.Clear();
		_selectionAnchorIndex = -1;
		Invalidate();
	}
	protected override void OnMouseDown(MouseEventArgs e) {
		Focus();
		base.OnMouseDown(e);
		var num = _vScrollBar.Value + e.Y / _itemHeight;
		if (num < 0 || num >= _entries.Count) {
			return;
		}
		if (ModifierKeys.HasFlag(Keys.Shift) && _selectionAnchorIndex != -1) {
			_selectedIndices.Clear();
			var num2 = Math.Min(_selectionAnchorIndex, num);
			var num3 = Math.Max(_selectionAnchorIndex, num);
			for (var i = num2; i <= num3; i++) {
				_selectedIndices.Add(i);
			}
		} else if (ModifierKeys.HasFlag(Keys.Control)) {
			if (_selectedIndices.Contains(num)) {
				_selectedIndices.Remove(num);
			} else {
				_selectedIndices.Add(num);
			}
			_selectionAnchorIndex = num;
		} else {
			_selectedIndices.Clear();
			_selectedIndices.Add(num);
			_selectionAnchorIndex = num;
		}
		LogEntrySelected?.Invoke(_entries[num].Raw);
		Invalidate();
	}
	protected override void OnKeyDown(KeyEventArgs e) {
		if (e.Control && e.KeyCode == Keys.C) {
			var selectedText = GetSelectedText();
			if (!string.IsNullOrEmpty(selectedText)) {
				Clipboard.SetText(selectedText);
			}
			e.Handled = true;
		} else if (e.Control && e.KeyCode == Keys.A) {
			_selectedIndices.Clear();
			for (var i = 0; i < _entries.Count; i++) {
				_selectedIndices.Add(i);
			}
			Invalidate();
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}
	public string GetSelectedText() {
		if (_selectedIndices.Count == 0) {
			return "";
		}
		lock (_entries) {
			IEnumerable<string> values = from i in _selectedIndices
										 orderby i
										 where i >= 0 && i < _entries.Count
										 select _entries[i].Raw;
			return string.Join(Environment.NewLine, values);
		}
	}
	public static void SelectAll() {
	}
	public string GetAllText() => string.Join(Environment.NewLine, _entries.Select(e => e.Raw));
	protected override void Dispose(bool disposing) {
		if (disposing) {
			_regularFont.Dispose();
			_boldFont.Dispose();
		}
		base.Dispose(disposing);
	}
}
