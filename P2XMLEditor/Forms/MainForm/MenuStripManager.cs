using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using P2XMLEditor.Core;
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
       var saveVmMenuItem = new ToolStripMenuItem("Save virtual machine...");
       saveVmMenuItem.Click += SaveVmMenuItem_Click;
       fileMenu.DropDownItems.AddRange(new ToolStripItem[] { loadVmMenuItem, saveVmMenuItem });
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
           var langCopy = lang; // capture for closure
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
           
           // Show if tab is loaded but hidden
           if (_mainForm.IsTabLoaded(tabName) && !_mainForm.IsTabVisible(tabName)) {
               menuItem.Font = new System.Drawing.Font(menuItem.Font, System.Drawing.FontStyle.Italic);
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

   private void SaveVmMenuItem_Click(object? sender, EventArgs e) {
       if (_mainForm.Paths == null) {
           ErrorHandler.Handle("No virtual machine is currently loaded.", null);
           return;
       }

       var saveSettingsForm = new SaveSettingsForm();
       if (saveSettingsForm.ShowDialog() != DialogResult.OK) return;
       var writer = new VirtualMachineWriter(_mainForm.Paths.VmPath + "Recreation/", _mainForm.VirtualMachine);
       writer.SaveVirtualMachine(saveSettingsForm.Settings);
   }
}