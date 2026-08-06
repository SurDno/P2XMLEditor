using System;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Forms.MainForm.Dialogs;

public class CreateDialogForm : Form {
	public string DialogName { get; private set; } = string.Empty;
	public Graph? ParentGraph { get; private set; }

	public CreateDialogForm(VirtualMachine vm) {
		Text = "Create New Dialog";
		Width = 400;
		Height = 200;
		FormBorderStyle = FormBorderStyle.FixedDialog;
		StartPosition = FormStartPosition.CenterParent;
		MaximizeBox = false;
		MinimizeBox = false;

		var nameLabel = new Label {
			Text = "Dialog Name:",
			Left = 20,
			Top = 20,
			AutoSize = true
		};

		var nameTextBox = new TextBox {
			Left = 120,
			Top = 18,
			Width = 240
		};

		var ownerLabel = new Label {
			Text = "Parent / Owner:",
			Left = 20,
			Top = 60,
			AutoSize = true
		};

		var ownerComboBox = new ComboBox {
			Left = 120,
			Top = 58,
			Width = 240,
			DropDownStyle = ComboBoxStyle.DropDownList
		};

		// Populate all graphs that could act as parents. We look for root graphs.
		var rootGraphs = vm.GetElementsByType<Graph>()
			.Where(g => !(g is Talking)) // don't parent a talking inside a talking
			.OrderBy(g => g.Owner?.Name ?? g.Name)
			.ToList();

		foreach (var graph in rootGraphs) {
			var ownerName = graph.Owner?.Name ?? "Unknown Owner";
			ownerComboBox.Items.Add(new ComboBoxItem(graph, $"{ownerName} ({graph.Name})"));
		}

		if (ownerComboBox.Items.Count > 0)
			ownerComboBox.SelectedIndex = 0;

		var okButton = new Button {
			Text = "Create",
			Left = 180,
			Top = 110,
			Width = 80,
			DialogResult = DialogResult.OK
		};
		okButton.Click += (s, e) => {
			if (string.IsNullOrWhiteSpace(nameTextBox.Text)) {
				MessageBox.Show("Please enter a dialog name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				DialogResult = DialogResult.None;
				return;
			}
			DialogName = nameTextBox.Text.Trim();
			if (ownerComboBox.SelectedItem is ComboBoxItem item) {
				ParentGraph = item.Graph;
			}
		};

		var cancelButton = new Button {
			Text = "Cancel",
			Left = 280,
			Top = 110,
			Width = 80,
			DialogResult = DialogResult.Cancel
		};

		Controls.Add(nameLabel);
		Controls.Add(nameTextBox);
		Controls.Add(ownerLabel);
		Controls.Add(ownerComboBox);
		Controls.Add(okButton);
		Controls.Add(cancelButton);

		AcceptButton = okButton;
		CancelButton = cancelButton;
	}

	private class ComboBoxItem {
		public Graph Graph { get; }
		public string DisplayName { get; }

		public ComboBoxItem(Graph graph, string displayName) {
			Graph = graph;
			DisplayName = displayName;
		}

		public override string ToString() => DisplayName;
	}
}
