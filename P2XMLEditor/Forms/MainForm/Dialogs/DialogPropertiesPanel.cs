using System;
using System.Drawing;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Services;

namespace P2XMLEditor.Forms.MainForm.Dialogs;

public class DialogPropertiesPanel : Panel {
	private readonly VirtualMachine _vm;
	private VmElement? _selectedElement;
	private readonly TableLayoutPanel _propertiesTable;

	public DialogPropertiesPanel(VirtualMachine vm) {
		_vm = vm;
		AutoScroll = true;
		Dock = DockStyle.Right;
		Width = 350;
		Padding = new Padding(5);

		_propertiesTable = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			ColumnStyles = {
				new ColumnStyle(SizeType.Percent, 40),
				new ColumnStyle(SizeType.Percent, 60)
			},
			AutoSize = true,
			CellBorderStyle = TableLayoutPanelCellBorderStyle.None
		};
		Controls.Add(_propertiesTable);
	}

	public void SetElement(VmElement? element) {
		_selectedElement = element;
		UpdateControls();
	}

	private void UpdateControls() {
		_propertiesTable.Controls.Clear();
		_propertiesTable.RowStyles.Clear();

		if (_selectedElement == null) {
			Enabled = false;
			return;
		}

		Enabled = true;

		switch (_selectedElement) {
			case Speech speech:
				SetupSpeechProperties(speech);
				break;
			case Reply reply:
				SetupReplyProperties(reply);
				break;
			case Condition condition:
				SetupConditionProperties(condition);
				break;
			case ActionLine actionLine:
				SetupActionLineProperties(actionLine);
				break;
		}
	}

	private void SetupSpeechProperties(Speech speech) {
		AddHeader("Speech");

		// Author
		var authorName = speech.AuthorGuid.Element switch {
			Character c => c.Name,
			Blueprint b => b.Name,
			_ => "Unknown"
		};
		AddProperty("Author", authorName, null);

		// Text
		var textPreview = speech.Text.GetText(PreviewLanguageService.CurrentLanguage);
		if (textPreview.Length > 100) textPreview = textPreview[..97] + "...";
		
		var textLabel = new Label { 
			Text = "Text:", 
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.TopLeft
		};
		var textDisplay = new TextBox {
			Text = textPreview,
			Multiline = true,
			Height = 60,
			ReadOnly = true,
			Dock = DockStyle.Fill
		};
		var editButton = new Button {
			Text = "Edit",
			Dock = DockStyle.Top,
			Height = 25
		};
		editButton.Click += (_, _) => {
			using var editor = new GameStringEditor(speech.Text, _vm);
			editor.ShowDialog();
			textDisplay.Text = speech.Text.GetText(PreviewLanguageService.CurrentLanguage);
		};

		_propertiesTable.Controls.Add(textLabel, 0, _propertiesTable.RowCount);
		var textPanel = new Panel { Dock = DockStyle.Fill, Height = 85 };
		textPanel.Controls.Add(textDisplay);
		textPanel.Controls.Add(editButton);
		editButton.BringToFront();
		_propertiesTable.Controls.Add(textPanel, 1, _propertiesTable.RowCount++);

		// Flags
		AddCheckbox("Only Once", speech.OnlyOnce ?? false, 
			value => speech.OnlyOnce = value);
		AddCheckbox("Is Trade", speech.IsTrade ?? false, 
			value => speech.IsTrade = value);
		AddCheckbox("Initial", speech.Initial ?? false, 
			value => speech.Initial = value);

		// Reply count
		AddProperty("Replies", speech.Replies.Count.ToString(), null);
	}

	private void SetupReplyProperties(Reply reply) {
		AddHeader("Reply");

		// Text
		var textPreview = reply.Text.GetText(PreviewLanguageService.CurrentLanguage);
		if (textPreview.Length > 100) textPreview = textPreview[..97] + "...";
		
		var textLabel = new Label { 
			Text = "Text:", 
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.TopLeft
		};
		var textDisplay = new TextBox {
			Text = textPreview,
			Multiline = true,
			Height = 60,
			ReadOnly = true,
			Dock = DockStyle.Fill
		};
		var editButton = new Button {
			Text = "Edit",
			Dock = DockStyle.Top,
			Height = 25
		};
		editButton.Click += (_, _) => {
			using var editor = new GameStringEditor(reply.Text, _vm);
			editor.ShowDialog();
			textDisplay.Text = reply.Text.GetText(PreviewLanguageService.CurrentLanguage);
		};

		_propertiesTable.Controls.Add(textLabel, 0, _propertiesTable.RowCount);
		var textPanel = new Panel { Dock = DockStyle.Fill, Height = 85 };
		textPanel.Controls.Add(textDisplay);
		textPanel.Controls.Add(editButton);
		editButton.BringToFront();
		_propertiesTable.Controls.Add(textPanel, 1, _propertiesTable.RowCount++);

		// Flags
		AddCheckbox("Only Once", reply.OnlyOnce ?? false, 
			value => reply.OnlyOnce = value);
		AddCheckbox("Only One Reply", reply.OnlyOneReply ?? false, 
			value => reply.OnlyOneReply = value);
		AddCheckbox("Default", reply.Default ?? false, 
			value => reply.Default = value);

		// Condition
		if (reply.EnableCondition != null) {
			var condLabel = new Label { Text = "Condition:", Dock = DockStyle.Fill };
			var condPreview = new TextBox {
				Text = PreviewHelper.Preview(reply.EnableCondition),
				ReadOnly = true,
				Dock = DockStyle.Fill
			};
			var editCondButton = new Button {
				Text = "Edit",
				Dock = DockStyle.Top,
				Height = 25
			};
			editCondButton.Click += (_, _) => {
				using var editor = new ConditionEditorForm(_vm, reply.EnableCondition, new(reply.Parent));
				editor.ShowDialog();
				condPreview.Text = PreviewHelper.Preview(reply.EnableCondition);
			};

			_propertiesTable.Controls.Add(condLabel, 0, _propertiesTable.RowCount);
			var condPanel = new Panel { Dock = DockStyle.Fill, Height = 50 };
			condPanel.Controls.Add(condPreview);
			condPanel.Controls.Add(editCondButton);
			editCondButton.BringToFront();
			_propertiesTable.Controls.Add(condPanel, 1, _propertiesTable.RowCount++);
		}

		// ActionLine
		if (reply.ActionLine != null) {
			AddProperty("Has Actions", "Yes", null);
			AddProperty("Action Type", reply.ActionLine.ActionLineType.Serialize(), null);
		}
	}

	private void SetupConditionProperties(Condition condition) {
		AddHeader("Condition");
		AddProperty("Operation", condition.Operation.Serialize(), null);
		AddProperty("Predicates", condition.Predicates.Count.ToString(), null);

		var previewLabel = new Label { 
			Text = "Preview:", 
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.TopLeft
		};
		var previewText = new TextBox {
			Text = PreviewHelper.Preview(condition),
			Multiline = true,
			Height = 80,
			ReadOnly = true,
			Dock = DockStyle.Fill,
			ScrollBars = ScrollBars.Vertical
		};

		_propertiesTable.Controls.Add(previewLabel, 0, _propertiesTable.RowCount);
		_propertiesTable.Controls.Add(previewText, 1, _propertiesTable.RowCount++);
	}

	private void SetupActionLineProperties(ActionLine actionLine) {
		AddHeader("ActionLine");
		AddProperty("Type", actionLine.ActionLineType.Serialize(), null);
		
		if (actionLine.Actions != null) {
			AddProperty("Action Count", actionLine.Actions.Count.ToString(), null);
		}
	}

	private void AddHeader(string text) {
		var header = new Label {
			Text = text,
			Font = new Font(Font.FontFamily, Font.Size + 2, FontStyle.Bold),
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft,
			Height = 30,
			BackColor = SystemColors.ControlLight
		};
		_propertiesTable.Controls.Add(header, 0, _propertiesTable.RowCount);
		_propertiesTable.SetColumnSpan(header, 2);
		_propertiesTable.RowCount++;
	}

	private void AddProperty(string name, string value, Action<string>? onValueChanged) {
		var label = new Label { 
			Text = name, 
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		};

		Control valueControl;
		if (onValueChanged == null) {
			valueControl = new Label {
				Text = value,
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.MiddleLeft
			};
		} else {
			var textBox = new TextBox {
				Text = value,
				Dock = DockStyle.Fill
			};
			textBox.TextChanged += (_, _) => onValueChanged(textBox.Text);
			valueControl = textBox;
		}

		_propertiesTable.Controls.Add(label, 0, _propertiesTable.RowCount);
		_propertiesTable.Controls.Add(valueControl, 1, _propertiesTable.RowCount++);
	}

	private void AddCheckbox(string name, bool value, Action<bool> onValueChanged) {
		var label = new Label { 
			Text = name, 
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.MiddleLeft
		};

		var checkbox = new CheckBox {
			Checked = value,
			Dock = DockStyle.Fill
		};
		checkbox.CheckedChanged += (_, _) => onValueChanged(checkbox.Checked);

		_propertiesTable.Controls.Add(label, 0, _propertiesTable.RowCount);
		_propertiesTable.Controls.Add(checkbox, 1, _propertiesTable.RowCount++);
	}
}
