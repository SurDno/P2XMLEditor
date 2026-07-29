using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.Forms.MainForm.SaveSettings;
using P2XMLEditor.Forms.PathSelection;
using P2XMLEditor.Helper;
using P2XMLEditor.Services;
using P2XMLEditor.Suggestions;

namespace P2XMLEditor.Forms.MainForm;

public class MenuStripManager {
   private readonly MainForm _mainForm;
   private readonly MenuStrip _menuStrip;

   public MenuStripManager(MainForm mainForm) {
	   _mainForm = mainForm;
	   _menuStrip = new();
	   InitializeMenuStrip();
	   _mainForm.Controls.Add(_menuStrip);
	   _mainForm.MainMenuStrip = _menuStrip;
   }

   [SuppressMessage("ReSharper", "UseCollectionExpression")]
   private void InitializeMenuStrip() {
	   var fileMenu = new ToolStripMenuItem("File");
	   var loadVmMenuItem = new ToolStripMenuItem("Load another virtual machine...");
	   loadVmMenuItem.Click += LoadVmMenuItem_Click;
	   
	   var saveMenu = new ToolStripMenuItem("Save...");
	   var saveSameFolder = new ToolStripMenuItem("in the same folder");
	   saveSameFolder.Click += SaveInSameFolder_Click;
	   var saveNewVm = new ToolStripMenuItem("as a new virtual machine");
	   saveNewVm.Click += SaveAsNewVm_Click;
	   var saveMod = new ToolStripMenuItem("as a P2ModLoader mod");
	   saveMod.Click += SaveAsMod_Click;
	   
	   saveMenu.DropDownItems.AddRange(new ToolStripItem[] { saveSameFolder, saveNewVm, saveMod });
	   
	   fileMenu.DropDownItems.AddRange(new ToolStripItem[] { loadVmMenuItem, saveMenu });
	   _menuStrip.Items.Add(fileMenu);

	   var displayMenu = new ToolStripMenuItem("Display");
	   _menuStrip.Items.Add(displayMenu);
	   displayMenu.DropDownOpening += (_, _) => UpdateDisplayMenu(displayMenu);

	   var windowMenu = new ToolStripMenuItem("Window");
	   _menuStrip.Items.Add(windowMenu);
	   
	   var allTypes = typeof(Suggestion).Assembly.GetTypes()
		   .Where(t => typeof(Suggestion).IsAssignableFrom(t) && !t.IsAbstract).ToList();

	   var refactorMenu = new ToolStripMenuItem("Refactor");
	   var refactoringTypes = allTypes.Where(t => t.GetCustomAttribute<RefactoringAttribute>() != null);
	   SetupSuggestionMenu(refactorMenu, refactoringTypes, t => t.GetCustomAttribute<RefactoringAttribute>()!.MenuPath);
	   
	   var cleanupMenu = new ToolStripMenuItem("Cleanup");
	   var cleanupTypes = allTypes.Where(t => t.GetCustomAttribute<CleanupAttribute>() != null);
	   SetupSuggestionMenu(cleanupMenu, cleanupTypes, t => t.GetCustomAttribute<CleanupAttribute>()!.MenuPath);

	   _menuStrip.Items.Add(refactorMenu);
	   _menuStrip.Items.Add(cleanupMenu);
	   
	   windowMenu.DropDownOpening += (_, _) => UpdateWindowMenu(windowMenu);
   }

   private void UpdateDisplayMenu(ToolStripMenuItem displayMenu) {
	   displayMenu.DropDownItems.Clear();

	   var langMenu = new ToolStripMenuItem("Game String Preview Language");
	   displayMenu.DropDownItems.Add(langMenu);

	   var vm = _mainForm.VirtualMachine;
	   if (vm == null || vm.Languages.Count == 0) {
		   var noLangs = new ToolStripMenuItem("(No VM loaded)") { Enabled = false };
		   langMenu.DropDownItems.Add(noLangs);
		   return;
	   }

	   foreach (var lang in vm.Languages) {
		   var langCopy = lang; 
		   var item = new ToolStripMenuItem(langCopy) {
			   CheckOnClick = false,
			   Checked = PreviewLanguageService.CurrentLanguage == langCopy
		   };
		   item.Click += (_, _) => {
			   PreviewLanguageService.SetLanguage(langCopy);
			   foreach (ToolStripMenuItem mi in langMenu.DropDownItems)
				   mi.Checked = mi.Text == PreviewLanguageService.CurrentLanguage;
		   };
		   langMenu.DropDownItems.Add(item);
	   }
   }

   private void UpdateWindowMenu(ToolStripMenuItem windowMenu) {
	   windowMenu.DropDownItems.Clear();
	   
	   if (_mainForm.VirtualMachine == null) {
		   var noVmItem = new ToolStripMenuItem("No virtual machine loaded") { Enabled = false };
		   windowMenu.DropDownItems.Add(noVmItem);
		   return;
	   }

	   foreach (var tabName in _mainForm.GetAvailableTabNames()) {
		   var menuItem = new ToolStripMenuItem(tabName) {
			   CheckOnClick = true,
			   Checked = _mainForm.IsTabVisible(tabName)
		   };
		   
		   if (_mainForm.IsTabLoaded(tabName) && !_mainForm.IsTabVisible(tabName)) {
			   menuItem.Font = new Font(menuItem.Font, FontStyle.Italic);
		   }
		   
		   menuItem.Click += (_, _) => {
			   if (menuItem.Checked) {
				   _mainForm.ShowTab(tabName);
			   } else {
				   _mainForm.HideTab(tabName);
			   }
		   };
		   
		   windowMenu.DropDownItems.Add(menuItem);
	   }
	   
	   windowMenu.DropDownItems.Add(new ToolStripSeparator());
	   
	   var showAllItem = new ToolStripMenuItem("Show All");
	   showAllItem.Click += (_, _) => {
		   foreach (var tabName in _mainForm.GetAvailableTabNames()) {
			   _mainForm.ShowTab(tabName);
		   }
	   };
	   windowMenu.DropDownItems.Add(showAllItem);
	   
	   var hideAllItem = new ToolStripMenuItem("Hide All");
	   hideAllItem.Click += (_, _) => {
		   foreach (var tabName in _mainForm.GetAvailableTabNames()) {
			   _mainForm.HideTab(tabName);
		   }
	   };
	   windowMenu.DropDownItems.Add(hideAllItem);
   }

   private void SetupSuggestionMenu(ToolStripMenuItem parentMenu, IEnumerable<Type> suggestionTypes, Func<Type, string> getMenuPath) {
	   var executeAllItem = new ToolStripMenuItem("Execute All");
	   executeAllItem.Click += (_, _) => {
		   foreach (var type in suggestionTypes) 
			   ((Suggestion)Activator.CreateInstance(type, _mainForm.VirtualMachine)!).Execute();
	   };
	   parentMenu.DropDownItems.Add(executeAllItem);
	   parentMenu.DropDownItems.Add(new ToolStripSeparator());

	   var menuMap = new Dictionary<string, ToolStripMenuItem>();
	   foreach (var type in suggestionTypes) {
		   var menuPath = getMenuPath(type);
		   var pathParts = menuPath.Split('/');
		   var currentMenu = parentMenu;

		   for (var i = 0; i < pathParts.Length - 1; i++) {
			   var key = string.Join("/", pathParts.Take(i + 1));
			   if (!menuMap.TryGetValue(key, out var submenu)) {
				   submenu = new ToolStripMenuItem(pathParts[i]);
				   currentMenu.DropDownItems.Add(submenu);
				   menuMap[key] = submenu;
			   }

			   currentMenu = submenu;
		   }

		   var leaf = new ToolStripMenuItem(pathParts.Last());
		   leaf.Click += (_, _) => ((Suggestion)Activator.CreateInstance(type, _mainForm.VirtualMachine)!).Execute();
		   currentMenu.DropDownItems.Add(leaf);
	   }
   }

   private void LoadVmMenuItem_Click(object? sender, EventArgs e) {
	   var pathForm = new PathSelectionForm();
	   if (pathForm.ShowDialog() != DialogResult.OK || pathForm.SelectedPaths == null) return;
	   _mainForm.LoadVirtualMachine(pathForm.SelectedPaths);
   }

   private void SaveInSameFolder_Click(object? sender, EventArgs e) {
	   if (_mainForm.Paths == null || _mainForm.VirtualMachine == null) {
		   ErrorHandler.Handle("No virtual machine is currently loaded.", null);
		   return;
	   }

	   if (MessageBox.Show("Are you sure you want to overwrite the existing virtual machine files?", "Confirm Save", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

	   var detectedType = _mainForm.VirtualMachine.Type;
	   var saveSettingsForm = new SaveSettingsForm(detectedType);
	   if (saveSettingsForm.ShowDialog() != DialogResult.OK) return;

	   var settings = saveSettingsForm.Settings;
	   var outputPath = _mainForm.Paths.VmPath;

	   VirtualMachineWriterBase writer = settings.Format == WriterFormat.Demo
		   ? new DemoVirtualMachineWriter(outputPath, _mainForm.VirtualMachine)
		   : new ReleaseVirtualMachineWriter(outputPath, _mainForm.VirtualMachine);

	   writer.SaveVirtualMachine(settings);
   }

   private void SaveAsNewVm_Click(object? sender, EventArgs e) {
	   if (_mainForm.Paths == null || _mainForm.VirtualMachine == null) {
		   ErrorHandler.Handle("No virtual machine is currently loaded.", null);
		   return;
	   }

	   var detectedType = _mainForm.VirtualMachine.Type;
	   var defaultPath = _mainForm.Paths.VmPath + "Recreation";
	   var saveSettingsForm = new SaveSettingsForm(detectedType, true, _mainForm.VirtualMachine.TemplateManagerInst, defaultPath);
	   if (saveSettingsForm.ShowDialog() != DialogResult.OK) return;

	   var settings = saveSettingsForm.Settings;
	   var vmSettings = settings.VmMetadata;
	   if (vmSettings == null) return;
	   
	   VirtualMachineWriterBase writer = settings.Format == WriterFormat.Demo
		   ? new DemoVirtualMachineWriter(vmSettings.OutputPath, _mainForm.VirtualMachine)
		   : new ReleaseVirtualMachineWriter(vmSettings.OutputPath, _mainForm.VirtualMachine);

	   writer.SaveVirtualMachine(settings);
	   
	   VersionXmlGenerator.Generate(vmSettings.OutputPath, vmSettings, _mainForm.VirtualMachine.GetDataCapacity());
   }

   private void SaveAsMod_Click(object? sender, EventArgs e) {
	   MessageBox.Show("Saving as a P2ModLoader mod is not supported yet.", "Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Information);
   }

   [Obsolete("Use specific save handlers instead")]
   private void SaveVmMenuItem_Click(object? sender, EventArgs e) {
	   SaveAsNewVm_Click(sender, e);
   }
}
