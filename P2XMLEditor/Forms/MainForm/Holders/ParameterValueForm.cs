using System;
using System.Drawing;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Forms.MainForm.Holders;

/// <summary>
/// A parameter's name and its value.
///
/// The value control is the one the expression editor uses for a constant: a parameter's value is
/// exactly that — a declared type and a literal of it — and building a second control for the
/// same job is how two places end up disagreeing about which types can be written.
///
/// The name is read-only for a standard parameter. Its key is "&lt;Component&gt;.&lt;Param&gt;"
/// and the component half has to match the component that declares it, which holds for all 58888
/// standard parameters across the two corpora; renaming one here would break that agreement with
/// no way to see it had happened.
/// </summary>
public sealed class ParameterValueForm : Form {
	private readonly TextBox _name;
	private readonly ConstantEditor _value;

	public ParameterValueForm(VirtualMachine vm, string title, Parameter? parameter) {
		Text = title;
		Size = new Size(560, 220);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		MaximizeBox = false;
		ShowInTaskbar = false;

		var editable = parameter is null or { Custom: true };

		_name = new TextBox {
			Dock = DockStyle.Fill, Text = parameter?.Name ?? "NewParam", ReadOnly = !editable
		};
		_value = new ConstantEditor(vm) { Dock = DockStyle.Fill };
		if (parameter != null) _value.Load(parameter);

		var rows = new TableLayoutPanel {
			Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(12, 12, 12, 0)
		};
		rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
		rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
		rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
		rows.RowStyles.Add(new RowStyle(SizeType.Absolute, ConstantEditor.PreferredHeight + 8));
		rows.Controls.Add(Label("Name"), 0, 0);
		rows.Controls.Add(_name, 1, 0);
		rows.Controls.Add(Label("Value"), 0, 1);
		rows.Controls.Add(_value, 1, 1);

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 48, Padding = new Padding(10)
		};
		var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
		var ok = new Button { Text = "OK", Margin = new Padding(8, 0, 0, 0) };
		ok.Click += (_, _) => {
			if (_name.Text.Trim().Length == 0) {
				MessageBox.Show(this, "A parameter needs a name.", "Name", MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				return;
			}
			DialogResult = DialogResult.OK;
		};
		buttons.Controls.AddRange([cancel, ok]);
		AcceptButton = ok;
		CancelButton = cancel;

		Controls.Add(rows);
		Controls.Add(buttons);
	}

	private static Label Label(string text) =>
		new() { Text = text, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };

	public string ParameterName => _name.Text.Trim();

	/// <summary>The value as typed, or null when it does not build — the old one is then kept.</summary>
	public ParameterValue? Value => _value.Build();
}
