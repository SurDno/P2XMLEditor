using System.Reflection;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.MainForm.Combinations;
using P2XMLEditor.Forms.MainForm.FiniteStateMachines;
using P2XMLEditor.Forms.MainForm.MindMapViewer;
using P2XMLEditor.Forms.MainForm.Templates;
using P2XMLEditor.Forms.PathSelection;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm;

public class MainForm : Form {
    private VirtualMachine? _virtualMachine;
    private PathSelectionForm.Paths? _paths;
    private readonly TabControl _tabControl;
    
    public VirtualMachine? VirtualMachine => _virtualMachine;
    public PathSelectionForm.Paths? Paths => _paths;
    
    private TemplateManager? _templateManager;

    public MainForm() {
        _tabControl = new TabControl {
            Dock = DockStyle.Fill
        };
        Controls.Add(_tabControl); 
        InitializeTabs();
        var menuStripManager = new MenuStripManager(this);
        ShowPathSelection();
    }
    
    private void ShowPathSelection() {
        var pathForm = new PathSelectionForm();
        if (pathForm.ShowDialog() != DialogResult.OK) {
            Logger.LogInfo("Closing P2XMLEditor as paths were not specified.");
            Environment.Exit(0);
        }
        if (pathForm.SelectedPaths != null) 
            LoadVirtualMachine(pathForm.SelectedPaths!);
    }

    public void LoadVirtualMachine(PathSelectionForm.Paths paths) {
        _paths = paths;
        var reader = new VirtualMachineReader(_paths!.VmPath);
        _virtualMachine = reader.LoadVirtualMachine();

        Logger.LogInfo($"DataCapacity: {_virtualMachine.GetDataCapacity()}");
        _tabControl.TabPages.Cast<TabPage>().ToList().ForEach(t => _tabControl.TabPages.Remove(t));
        
        var mindMapTab = new TabPage("Mind Maps");
        mindMapTab.Controls.Add(new MindMapTabControl(_virtualMachine) { Dock = DockStyle.Fill });
        _tabControl.TabPages.Add(mindMapTab);
        
        var fsmTab = new TabPage("FSM Graphs");
        fsmTab.Controls.Add(new FSMBrowser(_virtualMachine) { Dock = DockStyle.Fill });
        _tabControl.TabPages.Add(fsmTab);
        
        var combinationsTab = new TabPage("Combinations");
        combinationsTab.Controls.Add(new CombinationsBrowser(_virtualMachine) { Dock = DockStyle.Fill });
        _tabControl.TabPages.Add(combinationsTab);

        _templateManager = new TemplateManager(_paths.TemplatesPath);
        _templateManager.LoadTemplates();
        var templatesTab = new TabPage("Templates");
        templatesTab.Controls.Add(new TemplatesViewer(_templateManager) { Dock = DockStyle.Fill });
        _tabControl.TabPages.Add(templatesTab);
    }
    
    private void InitializeTabs() {
        var versionInfo = Assembly.GetExecutingAssembly().GetName().Version!;
        var currentVersion = $"{versionInfo.Major}.{versionInfo.Minor}.{versionInfo.Build}";
        Text = $"P2XMLEditor {currentVersion}";
        Size = new(1200, 800);
        MinimumSize = new(600, 600); 
    }
}