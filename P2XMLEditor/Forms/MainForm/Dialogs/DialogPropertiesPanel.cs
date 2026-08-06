using System;
using System.Drawing;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.Forms.Editors;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
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
			case Branch branch:
				SetupBranchProperties(branch);
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
		
		var textLabel = new Label { 
			Text = "Text:", 
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.TopLeft,
			Height = 20
		};
		var textDisplay = new TextBox {
			Text = textPreview,
			Multiline = true,
			ReadOnly = true,
			Dock = DockStyle.Fill,
			ScrollBars = ScrollBars.Vertical
		};
		var editButton = new Button {
			Text = "Edit",
			Dock = DockStyle.Top,
			Height = 35
		};
		editButton.Click += (_, _) => {
			using var editor = new GameStringEditor(speech.Text, _vm);
			editor.ShowDialog();
			textDisplay.Text = speech.Text.GetText(PreviewLanguageService.CurrentLanguage);
		};

		_propertiesTable.Controls.Add(textLabel, 0, _propertiesTable.RowCount);
		_propertiesTable.SetColumnSpan(textLabel, 2);
		_propertiesTable.RowCount++;

		var textPanel = new Panel { Dock = DockStyle.Fill, Height = 350 };
		textPanel.Controls.Add(textDisplay);
		textPanel.Controls.Add(editButton);
		editButton.BringToFront();
		
		_propertiesTable.Controls.Add(textPanel, 0, _propertiesTable.RowCount);
		_propertiesTable.SetColumnSpan(textPanel, 2);
		_propertiesTable.RowCount++;

		// Flags
		AddCheckbox("Only Once", speech.OnlyOnce, 
			value => speech.OnlyOnce = value);
		AddCheckbox("Is Trade", speech.IsTrade, 
			value => speech.IsTrade = value);
		AddCheckbox("Initial", speech.Initial, 
			value => {
				speech.Initial = value;
				if (value && speech.Parent is Talking talking) {
					foreach (var stateRef in talking.States) {
						if (stateRef.Element is Speech otherSpeech && otherSpeech != speech) {
							otherSpeech.Initial = false;
						}
					}
					UpdateControls();
				}
			});
	}

	private void SetupReplyProperties(Reply reply) {
		AddHeader("Reply");

		// Text
		var textPreview = reply.Text.GetText(PreviewLanguageService.CurrentLanguage);
		
		var textLabel = new Label { 
			Text = "Text:", 
			Dock = DockStyle.Fill,
			TextAlign = ContentAlignment.TopLeft,
			Height = 20
		};
		var textDisplay = new TextBox {
			Text = textPreview,
			Multiline = true,
			ReadOnly = true,
			Dock = DockStyle.Fill,
			ScrollBars = ScrollBars.Vertical
		};
		var editButton = new Button {
			Text = "Edit",
			Dock = DockStyle.Top,
			Height = 35
		};
		editButton.Click += (_, _) => {
			using var editor = new GameStringEditor(reply.Text, _vm);
			editor.ShowDialog();
			textDisplay.Text = reply.Text.GetText(PreviewLanguageService.CurrentLanguage);
		};

		_propertiesTable.Controls.Add(textLabel, 0, _propertiesTable.RowCount);
		_propertiesTable.SetColumnSpan(textLabel, 2);
		_propertiesTable.RowCount++;

		var textPanel = new Panel { Dock = DockStyle.Fill, Height = 350 };
		textPanel.Controls.Add(textDisplay);
		textPanel.Controls.Add(editButton);
		editButton.BringToFront();
		
		_propertiesTable.Controls.Add(textPanel, 0, _propertiesTable.RowCount);
		_propertiesTable.SetColumnSpan(textPanel, 2);
		_propertiesTable.RowCount++;

		// Flags
		AddCheckbox("Only Once", reply.OnlyOnce, 
			value => reply.OnlyOnce = value);
		AddCheckbox("Only One Reply", reply.OnlyOneReply, 
			value => reply.OnlyOneReply = value);
		AddCheckbox("Default", reply.Default, 
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
				Height = 35
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
		} else {
			var addCondButton = new Button { Text = "Add Condition", Dock = DockStyle.Fill, Height = 35 };
			addCondButton.Click += (_, _) => {
				reply.EnableCondition = Condition.New(_vm, Core.IdGenerator.GetNewId<Condition>(_vm), reply);
				_vm.AddElement(reply.EnableCondition, typeof(Condition));
				UpdateControls();
				// Also request parent redraw via some event if needed, but right now UpdateControls works
			};
			_propertiesTable.Controls.Add(new Label { Text = "Condition:" }, 0, _propertiesTable.RowCount);
			_propertiesTable.Controls.Add(addCondButton, 1, _propertiesTable.RowCount++);
		}

		// ActionLine
		if (reply.ActionLine != null) {
			AddHeader("ActionLine");
			AddCheckbox("Is Loop Line", reply.ActionLine.ActionLineType == ActionLineType.Loop, isLoop => {
				reply.ActionLine.ActionLineType = isLoop ? ActionLineType.Loop : ActionLineType.Common;
				if (isLoop && reply.ActionLine.LoopInfo == null) {
					reply.ActionLine.LoopInfo = new ActionLoopInfo(
						ParameterSource.Create("", _vm),
						ParameterSource.Create("0", _vm, null, P2XMLEditor.GameData.VmTypeInfo.Int32),
						ParameterSource.Create("10", _vm, null, P2XMLEditor.GameData.VmTypeInfo.Int32),
						false
					);
				}
				UpdateControls();
			});
			AddProperty("Action Type", reply.ActionLine.ActionLineType.Serialize(), null);
		} else {
			var addActionButton = new Button { Text = "Add ActionLine", Dock = DockStyle.Fill, Height = 35 };
			addActionButton.Click += (_, _) => {
				reply.ActionLine = ActionLine.New(_vm, Core.IdGenerator.GetNewId<ActionLine>(_vm), reply);
				_vm.AddElement(reply.ActionLine, typeof(ActionLine));
				UpdateControls();
			};
			_propertiesTable.Controls.Add(new Label { Text = "ActionLine:" }, 0, _propertiesTable.RowCount);
			_propertiesTable.Controls.Add(addActionButton, 1, _propertiesTable.RowCount++);
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

	private void SetupBranchProperties(Branch branch) {
		AddHeader("Branch");
		AddProperty("Name", branch.Name, v => branch.Name = v);
		AddProperty("Type", branch.BranchType.ToString(), null);
		
		var condCount = branch.BranchConditions?.Count ?? 0;
		AddProperty("Condition Arms", condCount.ToString(), null);

		for (var i = 0; i < condCount; i++) {
			var condOrPart = branch.BranchConditions![i].Element;
			var condLabel = new Label {
				Text = $"Arm [{i}]:",
				Dock = DockStyle.Fill,
				TextAlign = ContentAlignment.TopLeft
			};
			var condPreview = new TextBox {
				Text = PreviewHelper.Preview(condOrPart),
				Multiline = true,
				Height = 50,
				ReadOnly = true,
				Dock = DockStyle.Fill,
				ScrollBars = ScrollBars.Vertical
			};
			_propertiesTable.Controls.Add(condLabel, 0, _propertiesTable.RowCount);
			_propertiesTable.Controls.Add(condPreview, 1, _propertiesTable.RowCount++);
		}
		
		AddProperty("[else]", "(last arm, no condition)", null);
		AddCheckbox("Initial", branch.Initial, v => branch.Initial = v);
		AddCheckbox("Ignore Block", branch.IgnoreBlock, v => branch.IgnoreBlock = v);
	}

	private void SetupActionLineProperties(ActionLine actionLine) {
		AddHeader("ActionLine");
		
		AddComboBox("Line Type", actionLine.ActionLineType, newType => {
			actionLine.ActionLineType = newType;
			if (newType == ActionLineType.Loop && actionLine.LoopInfo == null) {
				actionLine.LoopInfo = new ActionLoopInfo(
					ParameterSource.Create("", _vm),
					ParameterSource.Create("0", _vm, null, P2XMLEditor.GameData.VmTypeInfo.Int32),
					ParameterSource.Create("10", _vm, null, P2XMLEditor.GameData.VmTypeInfo.Int32),
					false
				);
			}
			UpdateControls();
		});

		AddCheckbox("Is Loop", actionLine.ActionLineType == ActionLineType.Loop, isLoop => {
			actionLine.ActionLineType = isLoop ? ActionLineType.Loop : ActionLineType.Common;
			if (isLoop && actionLine.LoopInfo == null) {
				actionLine.LoopInfo = new ActionLoopInfo(
					ParameterSource.Create("", _vm),
					ParameterSource.Create("0", _vm, null, P2XMLEditor.GameData.VmTypeInfo.Int32),
					ParameterSource.Create("10", _vm, null, P2XMLEditor.GameData.VmTypeInfo.Int32),
					false
				);
			}
			UpdateControls();
		});

		if (actionLine.ActionLineType == ActionLineType.Loop && actionLine.LoopInfo != null) {
			AddHeader("Loop Parameters");
			AddProperty("Param Name", actionLine.LoopInfo.Name.Write(), val => {
				actionLine.LoopInfo = new ActionLoopInfo(
					ParameterSource.Create(val, _vm),
					actionLine.LoopInfo.Start,
					actionLine.LoopInfo.End,
					actionLine.LoopInfo.Random
				);
			});
			AddProperty("Start Value", actionLine.LoopInfo.Start.Write(), val => {
				actionLine.LoopInfo = new ActionLoopInfo(
					actionLine.LoopInfo.Name,
					ParameterSource.Create(val, _vm, null, VmTypeInfo.Int32),
					actionLine.LoopInfo.End,
					actionLine.LoopInfo.Random
				);
			});
			AddProperty("End Value", actionLine.LoopInfo.End.Write(), val => {
				actionLine.LoopInfo = new ActionLoopInfo(
					actionLine.LoopInfo.Name,
					actionLine.LoopInfo.Start,
					ParameterSource.Create(val, _vm, null, VmTypeInfo.Int32),
					actionLine.LoopInfo.Random
				);
			});
			AddCheckbox("Random Loop", actionLine.LoopInfo.Random, isRandom => {
				actionLine.LoopInfo.Random = isRandom;
			});
		}

		if (actionLine.Actions != null) {
			AddProperty("Action Count", actionLine.Actions.Count.ToString(), null);
			for (var i = 0; i < actionLine.Actions.Count; i++) {
				if (actionLine.Actions[i].Element is P2XMLEditor.GameData.VirtualMachineElements.Action a) {
					AddProperty($"[{i}] Type", a.ActionType.Serialize(), null);
					AddProperty($"[{i}] Target", a.TargetObject.Kind.ToString(), null);
					if (a.EventToRaise != null)
						AddProperty($"[{i}] Event", a.EventToRaise.Name, null);
				}
			}
		} else {
			AddProperty("Action Count", "0", null);
		}
		
		var addActionButton = new Button { Text = "Add Action", Dock = DockStyle.Fill, Height = 35 };
		addActionButton.Click += (_, _) => {
			MessageBox.Show("Adding individual actions requires a full Action Editor. Currently not implemented in Properties Panel.");
		};
		_propertiesTable.Controls.Add(new Label { Text = "" }, 0, _propertiesTable.RowCount);
		_propertiesTable.Controls.Add(addActionButton, 1, _propertiesTable.RowCount++);
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

		private void AddComboBox<T>(string name, T currentValue, Action<T> onValueChanged) where T : struct, Enum {
		var label = new Label { Text = name, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
		var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
		foreach (var val in Enum.GetValues<T>()) {
			combo.Items.Add(val);
		}
		combo.SelectedItem = currentValue;
		combo.SelectedIndexChanged += (_, _) => {
			if (combo.SelectedItem is T selected) {
				onValueChanged(selected);
			}
		};
		_propertiesTable.Controls.Add(label, 0, _propertiesTable.RowCount);
		_propertiesTable.Controls.Add(combo, 1, _propertiesTable.RowCount++);
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
