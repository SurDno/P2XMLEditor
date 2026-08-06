using System;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Logging;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.MainForm.Dialogs;

public class DialogBrowser : SplitContainer {
	private readonly VirtualMachine _vm;
	private readonly SearchControl _searchControl;
	private readonly ListView _dialogList;
	private DialogGraphViewer? _currentViewer;
	
	[PerformanceLogHook]
	public DialogBrowser(VirtualMachine vm) {
		_vm = vm;
		Dock = DockStyle.Fill;
		Orientation = Orientation.Vertical;
		FixedPanel = FixedPanel.Panel1;
		SplitterDistance = 250;
		
		var leftPanel = new Panel { 
			Dock = DockStyle.Fill,
			Padding = new Padding(5)
		};
		Panel1.Controls.Add(leftPanel);
		
		_searchControl = new SearchControl(enableRegex: true) { Dock = DockStyle.Top };
		_searchControl.SearchChanged += (_, _) => LoadDialogs();
		leftPanel.Controls.Add(_searchControl);
		
		_dialogList = new ListView {
			Dock = DockStyle.Fill,
			View = View.Details,
			FullRowSelect = true,
			MultiSelect = false,
			Top = _searchControl.Bottom + 5
		};
		_dialogList.Columns.Add("Dialog", 220);
		_dialogList.SelectedIndexChanged += OnDialogSelected;
		leftPanel.Controls.Add(_dialogList);

		var contextMenu = new ContextMenuStrip();
		var createItem = contextMenu.Items.Add("Create New Dialog");
		createItem.Click += (_, _) => CreateNewDialog();
		var deleteItem = contextMenu.Items.Add("Delete Dialog");
		deleteItem.Click += (_, _) => DeleteSelectedDialog();
		
		contextMenu.Opening += (_, _) => {
			deleteItem.Enabled = _dialogList.SelectedItems.Count > 0;
		};
		_dialogList.ContextMenuStrip = contextMenu;
		
		LoadDialogs();
	}

	private void CreateNewDialog() {
		using var form = new CreateDialogForm(_vm);
		if (form.ShowDialog() != DialogResult.OK) return;

		var ownerNode = form.ParentGraph!.Owner;
		VmEither<Blueprint, Character> talkingOwner = new();
		if (ownerNode is Character c) talkingOwner = new(c);
		else if (ownerNode is Blueprint b) talkingOwner = new(b);

		var newTalking = new Talking(Core.IdGenerator.GetNewId<Talking>(_vm)) {
			Name = form.DialogName,
			Parent = form.ParentGraph!,
			States = new(),
			EventLinks = new(),
			EntryPoints = new(),
			InputLinks = new(),
			Owner = talkingOwner
		};
		
		var initialSpeech = new Speech(Core.IdGenerator.GetNewId<Speech>(_vm)) {
			Name = "First Speech",
			Parent = newTalking,
			Replies = new(),
			EntryPoints = new(),
			InputLinks = new(),
			OutputLinks = new(),
			Initial = true
		};
		
		newTalking.States.Add(new VmEither<Branch, Speech, State>(initialSpeech));
		form.ParentGraph!.States.Add(new VmEither<State, Graph, Branch, Talking>(newTalking));
		
		_vm.AddElement(newTalking, typeof(Talking));
		_vm.AddElement(initialSpeech, typeof(Speech));
		
		LoadDialogs();
		
		foreach (ListViewItem item in _dialogList.Items) {
			if (item.Tag == newTalking) {
				item.Selected = true;
				item.EnsureVisible();
				break;
			}
		}
	}

	private void DeleteSelectedDialog() {
		if (_dialogList.SelectedItems.Count == 0) return;
		var talking = (Talking)_dialogList.SelectedItems[0].Tag;
		
		if (MessageBox.Show($"Are you sure you want to delete dialog '{talking.Name}'?", "Confirm Delete", 
			MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes) {
			_vm.RemoveElement(talking);
			Panel2.Controls.Clear();
			_currentViewer?.Dispose();
			_currentViewer = null;
			LoadDialogs();
		}
	}

	private void LoadDialogs() {
		_dialogList.Items.Clear();

		var talkings = _vm.GetElementsByType<Talking>()
			.OrderBy(t => t.Name)
			.ToList();

		var displayedCount = 0;

		foreach (var talking in talkings) {
			var searchText = talking.Name;

			if (!_searchControl.IsMatchAny(searchText, talking.Id.ToString()))
				continue;

			var item = new ListViewItem(talking.Name) { Tag = talking };
			_dialogList.Items.Add(item);
			displayedCount++;
		}

		_searchControl.StatusText = $"Displaying {displayedCount}/{talkings.Count} dialogs.";
	}

	private void OnDialogSelected(object? sender, EventArgs e) {
		if (_dialogList.SelectedItems.Count == 0) return;
		
		var talking = (Talking)_dialogList.SelectedItems[0].Tag;
		LoadDialog(talking);
	}

	public void LoadDialog(Talking talking) {
		_currentViewer?.Dispose();
		
		_currentViewer = new DialogGraphViewer(_vm, talking) {
			Dock = DockStyle.Fill
		};
		Panel2.Controls.Clear();
		Panel2.Controls.Add(_currentViewer);
	}
}
