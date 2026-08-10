using System;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.Forms.Editors;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;
using P2XMLEditor.Services;
using VmAction = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Forms.MainForm.Dialogs;

public class DialogPropertiesPanel : Panel {
	private readonly VirtualMachine _vm;
	private VmElement? _selectedElement;
	private readonly TableLayoutPanel _propertiesTable;

	/// <summary>
	/// Raised when an edit here changed the graph's shape — an action added or removed, a
	/// condition edited, an action line created. The viewer relays the actual content, so it
	/// relays this to a redraw rather than the panel repainting a graph it does not own.
	/// </summary>
	public event EventHandler? Changed;

	private void NotifyChanged() => Changed?.Invoke(this, EventArgs.Empty);

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
				EnsureLoopInfo(reply.ActionLine);
				UpdateControls();
			});
			AddActionListEditor(reply.ActionLine);
		} else {
			AddButtonRow("Add ActionLine", () => {
				// The line's context is the speech, not the reply: an action inside it resolves its
				// variables against the speaker, which is where the shipped data points every reply
				// action line's LocalContext too.
				reply.ActionLine = ActionLine.New(_vm, Core.IdGenerator.GetNewId<ActionLine>(_vm), reply.Parent);
				_vm.AddElement(reply.ActionLine, typeof(ActionLine));
				UpdateControls();
				NotifyChanged();
			});
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

		// The panel showed the condition and gave no way to change it — the one editor that opens a
		// condition, predicates and all, was reachable from a reply but not from the condition node
		// itself. Its scope is whatever owns it: a reply reads its condition in the speaker's
		// context, a branch in its own.
		AddButtonRow("Edit Condition", () => {
			using var editor = new ConditionEditorForm(_vm, condition, ResolveConditionContext(condition));
			editor.ShowDialog(FindForm());
			previewText.Text = PreviewHelper.Preview(condition);
			NotifyChanged();
		});
	}

	/// <summary>
	/// What a condition is read in the context of. A reply's enable condition resolves against the
	/// speech it answers, a branch arm against the branch. Found by asking who points at it rather
	/// than stored, because a condition does not name its owner.
	/// </summary>
	private VmEither<Branch, Event, MindMapNode, Speech, State> ResolveConditionContext(Condition condition) {
		foreach (var reply in _vm.GetElementsByType<Reply>())
			if (reply.EnableCondition == condition)
				return new(reply.Parent);

		foreach (var branch in _vm.GetElementsByType<Branch>())
			if (branch.BranchConditions?.Any(c => c.Element == condition) == true)
				return new(branch);

		// Nothing owns it — a condition just made, or one detached. A speech from this dialog is
		// the only context on offer; a Root condition with no variable references needs none anyway.
		return new(_vm.First<Speech>());
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

			// A whole-condition arm opens in the condition editor, in the branch's own context. A
			// bare PartCondition arm — 918 of the corpus's 4386 — has no standalone editor, so it
			// stays a preview rather than a button that would open nothing.
			if (condOrPart is Condition armCondition)
				AddButtonRow($"Edit Arm [{i}]", () => {
					using var editor = new ConditionEditorForm(_vm, armCondition, new(branch));
					editor.ShowDialog(FindForm());
					condPreview.Text = PreviewHelper.Preview(armCondition);
					NotifyChanged();
				});
		}
		
		AddProperty("[else]", "(last arm, no condition)", null);
		AddCheckbox("Initial", branch.Initial, v => branch.Initial = v);
		AddCheckbox("Ignore Block", branch.IgnoreBlock, v => branch.IgnoreBlock = v);
	}

	private void SetupActionLineProperties(ActionLine actionLine) {
		AddHeader("ActionLine");
		
		AddComboBox("Line Type", actionLine.ActionLineType, newType => {
			actionLine.ActionLineType = newType;
			EnsureLoopInfo(actionLine);
			UpdateControls();
		});

		AddCheckbox("Is Loop", actionLine.ActionLineType == ActionLineType.Loop, isLoop => {
			actionLine.ActionLineType = isLoop ? ActionLineType.Loop : ActionLineType.Common;
			EnsureLoopInfo(actionLine);
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

		AddActionListEditor(actionLine);
	}

	/// <summary>
	/// The line's own bounds, seeded when it becomes a loop. const_0 to const_2147483647 is the
	/// whole list, which is what almost every shipped loop runs; 0 to 10 — what this panel used to
	/// seed — silently stops after ten and appears nowhere in the data.
	/// </summary>
	private void EnsureLoopInfo(ActionLine line) {
		if (line.ActionLineType != ActionLineType.Loop || line.LoopInfo != null) return;
		line.LoopInfo = new ActionLoopInfo(
			ParameterSource.Create("", _vm),
			ParameterSource.Create("const_0", _vm, null, VmTypeInfo.Int32),
			ParameterSource.Create("const_2147483647", _vm, null, VmTypeInfo.Int32),
			false);
	}

	/// <summary>
	/// The actions of a line, each editable in the full action editor, with add and remove. This
	/// replaces a read-only list and an "Add Action" button that only ever showed a "not
	/// implemented" box — the one thing this panel most needed to do it could not do.
	/// </summary>
	private void AddActionListEditor(ActionLine line) {
		var actions = line.Actions ?? [];
		AddProperty("Action Count", actions.Count(a => a.Element is VmAction).ToString(), null);

		for (var i = 0; i < actions.Count; i++) {
			if (actions[i].Element is not VmAction action) {
				// A nested action line, not an action — shown, since it counts, but edited by
				// selecting it as its own node rather than through this row.
				AddProperty($"[{i}]", actions[i].Element is ActionLine nested ? $"[line] {nested.Name}" : "(line)", null);
				continue;
			}

			var row = new FlowLayoutPanel {
				Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, WrapContents = false,
				AutoSize = true, Margin = Padding.Empty, Padding = Padding.Empty
			};
			var edit = new Button { Text = "Edit", Width = 48, Height = 26, Margin = new Padding(0, 0, 4, 0) };
			edit.Click += (_, _) => EditAction(action);
			var remove = new Button { Text = "✕", Width = 30, Height = 26, Margin = Padding.Empty };
			remove.Click += (_, _) => {
				line.Actions?.RemoveAll(a => a.Element == action);
				_vm.RemoveElement(action);
				UpdateControls();
				NotifyChanged();
			};
			row.Controls.Add(edit);
			row.Controls.Add(remove);

			var preview = new Label {
				Text = $"[{i}] {PreviewHelper.Preview(action)}", Dock = DockStyle.Fill,
				AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft, Height = 30
			};

			_propertiesTable.Controls.Add(preview, 0, _propertiesTable.RowCount);
			_propertiesTable.Controls.Add(row, 1, _propertiesTable.RowCount++);
		}

		AddButtonRow("Add Action", () => {
			var action = VmElement.CreateDefault<VmAction>(_vm, line);
			(line.Actions ??= []).Add(new(action));

			using var editor = new ActionEditorForm(_vm, action);
			if (editor.ShowDialog(FindForm()) != DialogResult.OK) {
				line.Actions.RemoveAll(a => a.Element == action);
				_vm.RemoveElement(action);
			}
			UpdateControls();
			NotifyChanged();
		});
	}

	private void EditAction(VmAction action) {
		using var editor = new ActionEditorForm(_vm, action);
		if (editor.ShowDialog(FindForm()) != DialogResult.OK) return;
		UpdateControls();
		NotifyChanged();
	}

	/// <summary>A full-width button spanning both columns, for an action that is not a field.</summary>
	private void AddButtonRow(string text, System.Action onClick) {
		var button = new Button { Text = text, Dock = DockStyle.Fill, Height = 32 };
		button.Click += (_, _) => onClick();
		_propertiesTable.Controls.Add(button, 0, _propertiesTable.RowCount);
		_propertiesTable.SetColumnSpan(button, 2);
		_propertiesTable.RowCount++;
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
