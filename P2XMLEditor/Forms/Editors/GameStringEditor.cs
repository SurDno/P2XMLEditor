using System;
using System.Collections.Generic;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;

namespace P2XMLEditor.Forms.Editors;

public class GameStringEditor : Form {
	private readonly GameString _gameString;
	private readonly VirtualMachine _vm;
	private readonly Dictionary<string, TextBox> _textBoxes;
	private readonly TabControl _tabControl;

	public GameStringEditor(GameString gameString, VirtualMachine vm) {
		_gameString = gameString;
		_vm = vm;
		_textBoxes = new Dictionary<string, TextBox>();

		Text = "Edit Game String";
		Width = 500;
		Height = 400;
		StartPosition = FormStartPosition.CenterParent;

		_tabControl = new TabControl {
			Dock = DockStyle.Fill
		};

		foreach (var lang in _vm.Languages) {
			var tabPage = new TabPage(lang);
			var textBox = new TextBox {
				Text = _gameString.GetText(lang),
				Multiline = true,
				ScrollBars = ScrollBars.Vertical,
				Dock = DockStyle.Fill,
				Margin = new Padding(10)
			};

			tabPage.Controls.Add(textBox);
			_tabControl.TabPages.Add(tabPage);
			_textBoxes[lang] = textBox;
		}

		var buttonPanel = new FlowLayoutPanel {
			FlowDirection = FlowDirection.LeftToRight,
			Dock = DockStyle.Bottom,
			Height = 50
		};

		buttonPanel.Controls.Add(new Panel { Width = 150 });

		var okButton = new Button {
			Text = "OK",
			DialogResult = DialogResult.OK,
			Width = 80,
			Height = 40
		};

		var cancelButton = new Button {
			Text = "Cancel",
			DialogResult = DialogResult.Cancel,
			Width = 80,
			Height = 40,
			Margin = new Padding(10, 0, 0, 0)
		};

		buttonPanel.Controls.Add(okButton);
		buttonPanel.Controls.Add(cancelButton);

		Controls.Add(_tabControl);
		Controls.Add(buttonPanel);

		AcceptButton = okButton;
		CancelButton = cancelButton;

		Resize += (_, _) => {
			var spacerWidth = (buttonPanel.Width - okButton.Width - cancelButton.Width - cancelButton.Margin.Left) / 2;
			buttonPanel.Controls[0].Width = Math.Max(0, spacerWidth);
		};
	}

	protected override void OnFormClosing(FormClosingEventArgs e) {
		base.OnFormClosing(e);
		if (DialogResult != DialogResult.OK) return;
		foreach (var kvp in _textBoxes)
			_gameString.SetText(kvp.Value.Text, kvp.Key);
	}
}