using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Forms.MainForm.MindMapViewer;


public class MindMapTabControl : TabControl {
	private readonly VirtualMachine _vm;
	private readonly TabPage _newTabButton;

	public MindMapTabControl(VirtualMachine vm) {
		_vm = vm;
        
		_newTabButton = new TabPage("+");
		TabPages.Add(_newTabButton);

		foreach (var mindMap in vm.GetElementsByType<MindMap>())
			AddMindMapTab(mindMap);
		
		Selected += OnTabSelected;
		Logger.Log(LogLevel.Info, $"Loaded {TabPages.Count - 1} mind maps");
	}

	private void OnTabSelected(object? sender, TabControlEventArgs e) {
		if (e.TabPage == _newTabButton) 
			CreateNewMindMap();
	}

	private void CreateNewMindMap() {
		var mindMap = VmElement.CreateDefault<MindMap>(_vm, _vm.First<GameRoot>(_ => true));
		AddMindMapTab(mindMap);
	}

	private void AddMindMapTab(MindMap mindMap) {
		var tab = new TabPage(mindMap.Name);
		var viewer = new MindMapViewer(_vm, mindMap) {
			Dock = DockStyle.Fill,
			MinimumSize = new Size(300, 300) 
		};
		tab.Controls.Add(viewer);
        
		TabPages.Insert(TabPages.Count - 1, tab);
		SelectedTab = tab;
	}
}