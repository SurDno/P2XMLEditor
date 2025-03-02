using P2XMLEditor.Core;
using P2XMLEditor.Forms.MainForm.SaveSettings;
using P2XMLEditor.Forms.PathSelection;
using P2XMLEditor.Helper;
using P2XMLEditor.Refactoring;

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

   private void InitializeMenuStrip() {
       var fileMenu = new ToolStripMenuItem("File");
       var loadVmMenuItem = new ToolStripMenuItem("Load another virtual machine...");
       loadVmMenuItem.Click += LoadVmMenuItem_Click;
       var saveVmMenuItem = new ToolStripMenuItem("Save virtual machine...");
       saveVmMenuItem.Click += SaveVmMenuItem_Click;
       fileMenu.DropDownItems.AddRange([loadVmMenuItem, saveVmMenuItem]);
       
       var refactorMenu = new ToolStripMenuItem("Refactor");
       var simplifyTrueComparisonsItem = new ToolStripMenuItem("Simplify true comparisons");
       simplifyTrueComparisonsItem.Click += (_, _) => 
           new SimplifyTrueComparisons(_mainForm.VirtualMachine).Execute();
       var flattenRootConditionsItem = new ToolStripMenuItem("Flatten root conditions");
       flattenRootConditionsItem.Click += (_, _) => 
           new FlattenSinglePredicateRootConditions(_mainForm.VirtualMachine).Execute();
       var mergeNestedConditionsItem = new ToolStripMenuItem("Merge nested conditions");
       mergeNestedConditionsItem.Click += (_, _) =>
           new MergeNestedConditions(_mainForm.VirtualMachine).Execute();
       var a = new ToolStripMenuItem("Remove empty event graphs");
       a.Click += (_, _) =>
           new RemoveEmptyEventGraphs(_mainForm.VirtualMachine).Execute();
       var b = new ToolStripMenuItem("Remove orphaned game strings");
       b.Click += (_, _) =>
           new RemoveOrphanedGameStrings(_mainForm.VirtualMachine).Execute();
       refactorMenu.DropDownItems.AddRange([
           simplifyTrueComparisonsItem, 
           flattenRootConditionsItem, 
           mergeNestedConditionsItem,
           a, b
       ]);
       
       _menuStrip.Items.AddRange([fileMenu, refactorMenu]);
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