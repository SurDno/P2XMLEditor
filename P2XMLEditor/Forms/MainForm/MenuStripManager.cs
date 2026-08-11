using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
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
using P2XMLEditor.Logging;
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

	   AddPlayButton();
   }

   private void AddPlayButton() {
	   var playButton = new ToolStripMenuItem("Play") {
		   Alignment = ToolStripItemAlignment.Right,
		   ToolTipText = "Play P2XMLEditorTest"
	   };

	   var bmp = new Bitmap(16, 16);
	   using (var g = Graphics.FromImage(bmp)) {
		   g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
		   g.FillPolygon(Brushes.Green, new Point[] { new(3, 2), new(13, 8), new(3, 14) });
	   }
	   playButton.Image = bmp;

	   playButton.Click += async (_, _) => {
		   if (_mainForm.Paths == null || _mainForm.VirtualMachine == null) return;
		   
		   var exePath = Path.GetFullPath(Path.Combine(_mainForm.Paths.VmPath, "..", "..", "..", "Pathologic.exe"));
		   if (!File.Exists(exePath)) {
			   MessageBox.Show($"Could not find executable at {exePath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			   return;
		   }

		   playButton.Enabled = false;
		   var testDir = Path.Combine(_mainForm.Paths.VmPath, "..", "P2XMLEditorTest");

		   try {
			   var settings = new WriterSettings {
				   Format = WriterFormat.Release,
				   VmMetadata = _mainForm.VirtualMachine.VmMetadata != null 
				       ? _mainForm.VirtualMachine.VmMetadata with { OutputPath = testDir }
				       : new VmVersionSettings(
					       OutputPath: testDir,
					       GameName: "Haruspex",
					       Scene: new Guid("1d70fc8a-a74d-5144-693c-ae5769293269"),
					       WeatherSnapshot: new Guid("16de4259-4406-48d7-9244-84a87cbbc369"),
					       SolarTime: new DateTime(1, 1, 1, 7, 30, 0),
					       SkyRotation: 145,
					       LoadingWindowGameDay: -1,
					       HideLoadingWindow: false,
					       LoadingScreenName: "PathologicSandbox"
				       )
			   };

			   var writer = new ReleaseVirtualMachineWriter(testDir, _mainForm.VirtualMachine);
			   writer.SaveVirtualMachine(settings);
			   VersionXmlGenerator.Generate(testDir, settings.VmMetadata, _mainForm.VirtualMachine.GetDataCapacity());

			   var processStartInfo = new ProcessStartInfo {
				   FileName = exePath,
				   Arguments = "-load \"P2XMLEditorTest\"",
				   WorkingDirectory = Path.GetDirectoryName(exePath)
			   };

			   using var process = Process.Start(processStartInfo);
			   if (process != null) {
				   await System.Threading.Tasks.Task.Run(process.WaitForExit);
			   }
		   } catch (Exception ex) {
			   MessageBox.Show($"Failed to play:\n{ex}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
		   } finally {
			   if (Directory.Exists(testDir)) {
				   try {
					   Directory.Delete(testDir, true);
				   } catch (Exception ex) {
					   Logger.Log(LogLevel.Warning, $"Failed to clean up test directory {testDir}: {ex.Message}");
				   }
			   }
			   playButton.Enabled = true;
		   }
	   };

	   _menuStrip.Items.Add(playButton);

	   _mainForm.Activated += (_, _) => {
		   if (_mainForm.Paths != null) {
			   var exePath = Path.GetFullPath(Path.Combine(_mainForm.Paths.VmPath, "..", "..", "..", "Pathologic.exe"));
			   playButton.Enabled = File.Exists(exePath);
		   } else {
			   playButton.Enabled = false;
		   }
	   };
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
		   try {
			   foreach (var type in suggestionTypes)
				   ((Suggestion)Activator.CreateInstance(type, _mainForm.VirtualMachine)!).Execute();
		   } finally {
			   // A pass deletes and rewires elements the open tabs are showing, so they are
			   // rebuilt from the current VM once the batch has run. In a finally because a pass
			   // that threw partway still changed the data the tabs are displaying.
			   _mainForm.RefreshLoadedTabs();
		   }
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
		   leaf.Click += (_, _) => {
			   try {
				   ((Suggestion)Activator.CreateInstance(type, _mainForm.VirtualMachine)!).Execute();
			   } finally {
				   _mainForm.RefreshLoadedTabs();
			   }
		   };
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
	   var saveSettingsForm = new SaveSettingsForm(detectedType, false, null, null, _mainForm.VirtualMachine.VmMetadata);
	   if (saveSettingsForm.ShowDialog() != DialogResult.OK) return;

	   var settings = saveSettingsForm.Settings;
	   var outputPath = _mainForm.Paths.VmPath;

	   try {
		   VirtualMachineWriterBase writer = settings.Format switch {
			   WriterFormat.Demo => new DemoVirtualMachineWriter(outputPath, _mainForm.VirtualMachine),
			   WriterFormat.Alpha => new AlphaVirtualMachineWriter(outputPath, _mainForm.VirtualMachine),
			   _ => new ReleaseVirtualMachineWriter(outputPath, _mainForm.VirtualMachine)
		   };

		   writer.SaveVirtualMachine(settings);
		   MessageBox.Show("Virtual machine saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
	   } catch (Exception ex) {
		   MessageBox.Show($"Failed to save virtual machine:\n{ex}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	   }
   }

   private void SaveAsNewVm_Click(object? sender, EventArgs e) {
	   if (_mainForm.Paths == null || _mainForm.VirtualMachine == null) {
		   ErrorHandler.Handle("No virtual machine is currently loaded.", null);
		   return;
	   }

	   var detectedType = _mainForm.VirtualMachine.Type;
	   var defaultPath = _mainForm.Paths.VmPath + "Recreation";
	   var saveSettingsForm = new SaveSettingsForm(detectedType, true, _mainForm.VirtualMachine.TemplateManagerInst, defaultPath, _mainForm.VirtualMachine.VmMetadata);
	   if (saveSettingsForm.ShowDialog() != DialogResult.OK) return;

	   var settings = saveSettingsForm.Settings;
	   var vmSettings = settings.VmMetadata;
	   if (vmSettings == null) return;
	   
	   try {
		   VirtualMachineWriterBase writer = settings.Format switch {
			   WriterFormat.Demo => new DemoVirtualMachineWriter(vmSettings.OutputPath, _mainForm.VirtualMachine),
			   WriterFormat.Alpha => new AlphaVirtualMachineWriter(vmSettings.OutputPath, _mainForm.VirtualMachine),
			   _ => new ReleaseVirtualMachineWriter(vmSettings.OutputPath, _mainForm.VirtualMachine)
		   };

		   writer.SaveVirtualMachine(settings);
		   
		   VersionXmlGenerator.Generate(vmSettings.OutputPath, vmSettings, _mainForm.VirtualMachine.GetDataCapacity());
		   MessageBox.Show("Virtual machine saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
	   } catch (Exception ex) {
		   MessageBox.Show($"Failed to save virtual machine:\n{ex}", "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	   }
   }

   private void SaveAsMod_Click(object? sender, EventArgs e) {
	   MessageBox.Show("Saving as a P2ModLoader mod is not supported yet.", "Not Supported", MessageBoxButtons.OK, MessageBoxIcon.Information);
   }

   [Obsolete("Use specific save handlers instead")]
   private void SaveVmMenuItem_Click(object? sender, EventArgs e) {
	   SaveAsNewVm_Click(sender, e);
   }
}
