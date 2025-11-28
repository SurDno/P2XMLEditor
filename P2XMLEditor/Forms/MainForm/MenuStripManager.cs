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
       var allTypes = typeof(Suggestion).Assembly.GetTypes().Where(t => typeof(Suggestion).IsAssignableFrom(t) && !t.IsAbstract).ToList();

       var refactorMenu = new ToolStripMenuItem("Refactor");
       var refactoringTypes = allTypes.Where(t => t.GetCustomAttribute<RefactoringAttribute>() != null);
       SetupSuggestionMenu(refactorMenu, refactoringTypes, t => t.GetCustomAttribute<RefactoringAttribute>()!.MenuPath);
       
       var cleanupMenu = new ToolStripMenuItem("Cleanup");
       var cleanupTypes = allTypes.Where(t => t.GetCustomAttribute<CleanupAttribute>() != null);
       SetupSuggestionMenu(cleanupMenu, cleanupTypes, t => t.GetCustomAttribute<CleanupAttribute>()!.MenuPath);

       _menuStrip.Items.Add(refactorMenu);
       _menuStrip.Items.Add(cleanupMenu);
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