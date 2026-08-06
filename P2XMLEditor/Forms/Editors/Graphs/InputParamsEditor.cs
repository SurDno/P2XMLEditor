using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors.Graphs;

/// <summary>
/// A graph's input parameters — the values a link has to supply when it enters.
///
/// Order is the contract: a link passes its arguments positionally, so parameter <c>i</c> is
/// filled by argument <c>i</c> and nothing else names them. That makes adding one at the end
/// safe and removing one from the middle a change to every link that enters the graph, which
/// is why removal says how many links it affects and offers to fix them.
///
/// Renaming is deliberately absent. A parameter's Name is written verbatim and referenced by
/// that string from anywhere inside the graph — the engine stores it flat via
/// AddSubgraphLocalVariable and reads it back by name — so a rename would have to rewrite every
/// reference in every action and expression under the graph. Retyping is offered, because the
/// type is only read where the value is used and the editor re-derives it there.
/// </summary>
public sealed class InputParamsEditor : UserControl {
	private readonly VirtualMachine _vm;
	private readonly ListView _list;
	private readonly Button _add;
	private readonly Button _retype;
	private readonly Button _remove;

	private Graph? _graph;

	public event EventHandler? Changed;

	public InputParamsEditor(VirtualMachine vm) {
		_vm = vm;

		_list = new ListView {
			Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, MultiSelect = false,
			HideSelection = false
		};
		_list.Columns.Add("#", 34);
		_list.Columns.Add("Name", 160);
		_list.Columns.Add("Type", 160);
		_list.DoubleClick += (_, _) => Retype();
		_list.SelectedIndexChanged += (_, _) => UpdateButtons();

		_add = NewButton("Add", Add);
		_retype = NewButton("Change type…", Retype);
		_remove = NewButton("Remove", Remove);

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.LeftToRight, AutoSize = true,
			WrapContents = true, Padding = new Padding(0, 4, 0, 0)
		};
		buttons.Controls.AddRange([_add, _retype, _remove]);

		Controls.Add(_list);
		Controls.Add(buttons);
	}

	private static Button NewButton(string text, System.Action onClick) {
		var button = new Button { Text = text, AutoSize = true, Margin = new Padding(0, 0, 6, 0) };
		button.Click += (_, _) => onClick();
		return button;
	}

	public void SetGraph(Graph? graph) {
		_graph = graph;
		Reload();
	}

	private void Reload() {
		_list.BeginUpdate();
		_list.Items.Clear();

		var parameters = _graph?.InputParams ?? [];
		for (var i = 0; i < parameters.Count; i++) {
			var item = new ListViewItem(i.ToString()) { Tag = parameters[i] };
			item.SubItems.Add(parameters[i].ParamName);
			item.SubItems.Add(parameters[i].Type);
			_list.Items.Add(item);
		}

		_list.EndUpdate();
		UpdateButtons();
	}

	private void UpdateButtons() {
		var selected = Selected() != null;
		_add.Enabled = _graph != null;
		_retype.Enabled = selected;
		_remove.Enabled = selected;
	}

	private InputParameter? Selected() =>
		_list.SelectedItems.Count == 0 ? null : _list.SelectedItems[0].Tag as InputParameter;

	// ---------------------------------------------------------------- commands

	private void Add() {
		if (_graph == null) return;

		using var dialog = new ParameterDialog(_vm, "New input parameter", "", "System.Boolean");
		if (dialog.ShowDialog(FindForm()) != DialogResult.OK || dialog.ParamName.Length == 0) return;

		// Created through the factory, so the name carries this graph's id — the uniqueness
		// prefix the engine expects, even though it never parses it back out.
		(_graph.InputParams ??= []).Add(InputParameter.Create(_graph, dialog.ParamName, dialog.TypeName));

		// Every link into the graph now owes one more argument. They are given an empty slot
		// rather than left short, so the count stays right and the user fills it in.
		foreach (var link in LinksInto(_graph))
			(link.SourceParams ??= []).Add("");

		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void Retype() {
		if (Selected() is not { } parameter) return;

		using var dialog = new ParameterDialog(_vm, "Change type", parameter.ParamName, parameter.Type) {
			NameReadOnly = true
		};
		if (dialog.ShowDialog(FindForm()) != DialogResult.OK) return;

		parameter.Type = dialog.TypeName;
		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	private void Remove() {
		if (_graph == null || Selected() is not { } parameter) return;

		var index = _graph.InputParams?.IndexOf(parameter) ?? -1;
		if (index < 0) return;

		var links = LinksInto(_graph).ToList();
		var message = $"Remove input parameter '{parameter.ParamName}'?";
		if (links.Count > 0)
			message += $"\n\n{links.Count} link(s) enter this graph and will have their matching argument removed.";
		message += "\n\nAnything inside the graph that reads it will stop resolving.";

		if (MessageBox.Show(this, message, "Remove input parameter", MessageBoxButtons.YesNo,
				MessageBoxIcon.Warning) != DialogResult.Yes)
			return;

		_graph.InputParams!.RemoveAt(index);
		foreach (var link in links)
			if (link.SourceParams is { } arguments && index < arguments.Count)
				arguments.RemoveAt(index);

		Reload();
		Changed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Links whose arguments this graph's parameters consume — those that enter it directly, and
	/// those that enter a graph substituting it, since a substitute is where a parameterless
	/// graph gets its parameters from.
	/// </summary>
	private IEnumerable<GraphLink> LinksInto(Graph graph) =>
		_vm.GetElementsByType<GraphLink>()
			.Where(l => GraphTopology.ParameterisedGraph(l.Destination?.Element) == graph);

	/// <summary>Name and type of one parameter.</summary>
	private sealed class ParameterDialog : Form {
		private readonly TextBox _name;
		private readonly ComboBox _type;

		public ParameterDialog(VirtualMachine vm, string title, string name, string type) {
			Text = title;
			Size = new Size(480, 190);
			FormBorderStyle = FormBorderStyle.FixedDialog;
			StartPosition = FormStartPosition.CenterParent;
			MinimizeBox = false;
			MaximizeBox = false;
			ShowInTaskbar = false;

			_name = new TextBox { Dock = DockStyle.Fill, Text = name };

			_type = new ComboBox {
				Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDown, IntegralHeight = false,
				AutoCompleteMode = AutoCompleteMode.SuggestAppend, AutoCompleteSource = AutoCompleteSource.ListItems
			};
			// Editable, because a parameter may be declared with a narrowing the plain enum
			// cannot spell — "IObjRef%cf_Region" and the like are ordinary in the data.
			foreach (var candidate in Enum.GetValues<VmType>()
						 .Where(t => t is not (VmType.Void or VmType.Unknown))
						 .Select(t => t.Serialize())
						 .OrderBy(t => t, StringComparer.Ordinal))
				_type.Items.Add(candidate);
			_type.Text = type;

			var rows = new TableLayoutPanel {
				Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2, Padding = new Padding(12, 12, 12, 0)
			};
			rows.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
			rows.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
			rows.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
			rows.Controls.Add(new Label { Text = "Name", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
			rows.Controls.Add(_name, 1, 0);
			rows.Controls.Add(new Label { Text = "Type", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
			rows.Controls.Add(_type, 1, 1);

			var buttons = new FlowLayoutPanel {
				Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 48,
				Padding = new Padding(10)
			};
			var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
			var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Margin = new Padding(8, 0, 0, 0) };
			buttons.Controls.AddRange([cancel, ok]);
			AcceptButton = ok;
			CancelButton = cancel;

			Controls.Add(rows);
			Controls.Add(buttons);
		}

		public bool NameReadOnly {
			set => _name.ReadOnly = value;
		}

		public string ParamName => _name.Text.Trim();
		public string TypeName => _type.Text.Trim();
	}
}
