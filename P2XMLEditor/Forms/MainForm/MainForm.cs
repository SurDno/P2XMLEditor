using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.MainForm.Actions;
using P2XMLEditor.Forms.MainForm.Combinations;
using P2XMLEditor.Forms.MainForm.Dialogs;
using P2XMLEditor.Forms.MainForm.FiniteStateMachines;
using P2XMLEditor.Forms.MainForm.MindMapViewer;
using P2XMLEditor.Forms.MainForm.Templates;
using P2XMLEditor.Forms.PathSelection;
using P2XMLEditor.Logging;
using P2XMLEditor.Services;

namespace P2XMLEditor.Forms.MainForm;

public class MainForm : Form {
	private VirtualMachine? _virtualMachine;
	private PathSelectionForm.Paths? _paths;
	private readonly TabControl _tabControl;
	private MenuStripManager? _menuStripManager;
	
	private readonly Dictionary<string, TabPage> _loadedTabs = new();
	
	private readonly Dictionary<string, Func<Control>> _tabFactories = new();
	
	public VirtualMachine? VirtualMachine => _virtualMachine;
	public PathSelectionForm.Paths? Paths => _paths;
	
	private StatusStrip _statusStrip;
	private ToolStripStatusLabel _logStatusLabel;
	private LogViewerForm? _logViewerForm;

	public MainForm() {
		_tabControl = new TabControl { Dock = DockStyle.Fill }; 
		_tabControl.Height = Height - 25; 
		Controls.Add(_tabControl); 
		_statusStrip = new StatusStrip { Height = 25 };
		_logStatusLabel = new ToolStripStatusLabel { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
		_logStatusLabel.Click += OnLogStatusClick;
		Logger.LogMessageAdded += OnLogMessageAdded;
		_statusStrip.Items.Add(_logStatusLabel);
		Controls.Add(_statusStrip);
		InitializeTabs();
		_menuStripManager = new MenuStripManager(this);
		ShowPathSelection();
	}
	
	private void ShowPathSelection() {
		var pathForm = new PathSelectionForm();
		if (pathForm.ShowDialog() != DialogResult.OK) {
			Logger.Log(LogLevel.Info, $"Closing P2XMLEditor as paths were not specified.");
			Environment.Exit(0);
		}
		if (pathForm.SelectedPaths != null) 
			LoadVirtualMachine(pathForm.SelectedPaths!);
	}

	public void LoadVirtualMachine(PathSelectionForm.Paths paths) {
		_paths = paths;
		var reader = new VirtualMachineReader(_paths!.VmPath, _paths.TemplatesPath, _paths.Mode, _paths.Parallel);
		_virtualMachine = reader.LoadVirtualMachine();
		PreviewLanguageService.Initialise(languages: _virtualMachine.Languages);
		
		Logger.Log(LogLevel.Info, $"DataCapacity: {_virtualMachine.GetDataCapacity()}");
		
		_tabControl.TabPages.Clear();
		_loadedTabs.Clear();
		_tabFactories.Clear();
		
		RegisterTabFactory("Mind Maps", () => new MindMapTabControl(_virtualMachine) { Dock = DockStyle.Fill });
		RegisterTabFactory("FSM Graphs", () => new FSMBrowser(_virtualMachine) { Dock = DockStyle.Fill });
		RegisterTabFactory("Combinations", () => new CombinationsBrowser(_virtualMachine) { Dock = DockStyle.Fill });
		RegisterTabFactory("Templates", () => new TemplatesViewer(_virtualMachine.TemplateManagerInst) { Dock = DockStyle.Fill });
		RegisterTabFactory("Actions", () => new ActionsBrowser(_virtualMachine) { Dock = DockStyle.Fill });
		RegisterTabFactory("Dialogs", () => new DialogBrowser(_virtualMachine) { Dock = DockStyle.Fill });
		
		// Show Mind Maps tab by default (this will create it)
		ShowTab("Mind Maps");
	}
	
	private void RegisterTabFactory(string name, Func<Control> factory) {
		_tabFactories[name] = factory;
	}
	
	public void ShowTab(string tabName) {
		if (!_tabFactories.ContainsKey(tabName)) return;
		
		// Check if tab is already loaded
		if (!_loadedTabs.ContainsKey(tabName)) {
			// Create the tab on demand
			Logger.Log(LogLevel.Info, $"Loading tab: {tabName}");
			var tabPage = new TabPage(tabName);
			var content = _tabFactories[tabName]();
			tabPage.Controls.Add(content);
			_loadedTabs[tabName] = tabPage;
		}
		
		var tab = _loadedTabs[tabName];
		if (!_tabControl.TabPages.Contains(tab)) {
			_tabControl.TabPages.Add(tab);
		}
		_tabControl.SelectedTab = tab;
	}
	
	public void HideTab(string tabName) {
		if (!_loadedTabs.ContainsKey(tabName)) return;
		
		var tabPage = _loadedTabs[tabName];
		if (_tabControl.TabPages.Contains(tabPage)) {
			_tabControl.TabPages.Remove(tabPage);
		}
	}
	
	public bool IsTabVisible(string tabName) {
		if (!_loadedTabs.ContainsKey(tabName)) return false;
		return _tabControl.TabPages.Contains(_loadedTabs[tabName]);
	}
	
	public bool IsTabLoaded(string tabName) {
		return _loadedTabs.ContainsKey(tabName);
	}
	
	public IEnumerable<string> GetAvailableTabNames() {
		return _tabFactories.Keys;
	}
	
	private void InitializeTabs() {
		var versionInfo = Assembly.GetExecutingAssembly().GetName().Version!;
		var currentVersion = $"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Build}";
		Text = $"P2XMLEditor {currentVersion}";
		Size = new(1920, 1080);
		MinimumSize = new(600, 600); 
	}
	
	private void OnLogMessageAdded(string message) {
		if (InvokeRequired) {
			BeginInvoke(() => OnLogMessageAdded(message));
			return;
		}
	
		var displayMessage = message;
		if (message.Contains("] ")) 
			displayMessage = message[(message.IndexOf("] ", StringComparison.Ordinal) + 2)..];
	
		_logStatusLabel.Text = displayMessage.Length > 100 ? $"{displayMessage[..97]}..." : displayMessage;
	}

	private void OnLogStatusClick(object? sender, EventArgs e) {
		if (_logViewerForm == null || _logViewerForm.IsDisposed) 
			_logViewerForm = new LogViewerForm();

		_logViewerForm.Show();
	}
	
	protected override void Dispose(bool disposing) {
		if (disposing) 
			Logger.LogMessageAdded -= OnLogMessageAdded;
		base.Dispose(disposing);
	}
}