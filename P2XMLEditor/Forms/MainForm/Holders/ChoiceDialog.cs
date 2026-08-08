using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace P2XMLEditor.Forms.MainForm.Holders;

/// <summary>
/// Picks one string from a list. <see cref="Editors.Actions.VmElementPicker"/> does this for
/// elements, but a component is a name rather than an element — the object does not have one
/// until it is chosen — so there is nothing there to pick from.
/// </summary>
public sealed class ChoiceDialog : Form {
	private readonly ListBox _list;
	private readonly TextBox _filter;
	private readonly List<string> _values;
	private readonly Func<string, string> _display;

	private ChoiceDialog(string title, IEnumerable<string> values, Func<string, string> display) {
		_values = values.ToList();
		_display = display;

		Text = title;
		Size = new Size(460, 480);
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		MaximizeBox = false;
		ShowInTaskbar = false;

		_list = new ListBox { Dock = DockStyle.Fill, IntegralHeight = false };
		_list.DoubleClick += (_, _) => {
			if (_list.SelectedItem != null) DialogResult = DialogResult.OK;
		};

		_filter = new TextBox { Dock = DockStyle.Top };
		_filter.TextChanged += (_, _) => Reload();

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 44, Padding = new Padding(8)
		};
		var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
		var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Margin = new Padding(8, 0, 0, 0) };
		buttons.Controls.AddRange([cancel, ok]);
		AcceptButton = ok;
		CancelButton = cancel;

		var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
		host.Controls.Add(_list);
		host.Controls.Add(_filter);

		Controls.Add(host);
		Controls.Add(buttons);

		Reload();
	}

	private void Reload() {
		_list.BeginUpdate();
		_list.Items.Clear();
		foreach (var value in _values)
			if (_filter.Text.Length == 0 || _display(value).Contains(_filter.Text, StringComparison.OrdinalIgnoreCase))
				_list.Items.Add(new Item(value, _display(value)));
		if (_list.Items.Count > 0) _list.SelectedIndex = 0;
		_list.EndUpdate();
	}

	public static bool TryPick(IWin32Window? owner, string title, IEnumerable<string> values,
		Func<string, string> display, out string? result) {
		using var dialog = new ChoiceDialog(title, values, display);
		if (dialog.ShowDialog(owner) != DialogResult.OK || dialog._list.SelectedItem is not Item item) {
			result = null;
			return false;
		}
		result = item.Value;
		return true;
	}

	private sealed class Item(string value, string label) {
		public string Value { get; } = value;
		public override string ToString() => label;
	}
}
