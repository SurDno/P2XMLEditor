using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.Templates;
using P2XMLEditor.GameData.Templates.Abstract;

namespace P2XMLEditor.Forms.MainForm.SaveSettings;

public class SaveSettingsForm : Form {
	private readonly RadioButton _formatRelease;
	private readonly RadioButton _formatDemo;
	private readonly RadioButton _formatAlpha;
	private readonly CheckBox _cleanUpOrphanedElements;
	private readonly CheckBox _removeDefaultValueTypes;
	private readonly CheckBox _stripNames;
	private readonly CheckBox _stripEditorOnlyTags;
	private readonly Button _okButton;
	private readonly Button _cancelButton;

	// Metadata Fields (only for New VM)
	private readonly TextBox? _pathBox;
	private readonly TextBox? _gameNameBox;
	private readonly ComboBox? _sceneCombo;
	private readonly ComboBox? _weatherCombo;
	
	private readonly NumericUpDown? _solarDayNum;
	private readonly DateTimePicker? _solarTimePicker;
	
	private readonly NumericUpDown? _skyRotationNum;
	private readonly NumericUpDown? _loadingDayNum;
	private readonly CheckBox? _hideLoadingCheck;
	private readonly TextBox? _loadingScreenBox;

	public WriterSettings Settings { get; private set; }

	public SaveSettingsForm(GameType detectedType = GameType.Release, bool isNewVm = false, TemplateManager? templateManager = null, string? defaultPath = null, VmVersionSettings? defaultMeta = null) {
		Text = isNewVm ? "Save as New Virtual Machine" : "Save Virtual Machine";
		ClientSize = isNewVm ? new Size(1080, 720) : new Size(460, 420);
		FormBorderStyle = FormBorderStyle.FixedDialog;
		MaximizeBox = false;
		MinimizeBox = false;
		StartPosition = FormStartPosition.CenterParent;

		var mainLayout = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			Padding = new Padding(20),
			RowCount = 3,
			ColumnCount = 2
		};
		mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
		mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
		mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
		mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
		mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));

		// --- Path Selection (Top) ---
		if (isNewVm) {
			var pathPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 1 };
			pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
			pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			pathPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 50));
			pathPanel.Controls.Add(new Label { Text = "Output Path:", AutoSize = true, TextAlign = ContentAlignment.MiddleLeft, Dock = DockStyle.Fill, Margin = new Padding(0) }, 0, 0);
			pathPanel.Controls.Add(_pathBox = new TextBox { Text = defaultPath, Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) }, 1, 0);
			var browseBtn = new Button { Text = "...", Dock = DockStyle.Fill, Margin = new Padding(5, 0, 0, 0) };
			browseBtn.Click += (_, _) => {
				using var fbd = new FolderBrowserDialog();
				if (fbd.ShowDialog() == DialogResult.OK) _pathBox.Text = fbd.SelectedPath;
			};
			pathPanel.Controls.Add(browseBtn, 2, 0);
			mainLayout.Controls.Add(pathPanel, 0, 0);
			mainLayout.SetColumnSpan(pathPanel, 2);
		}

		// --- Left Side: Format & Cleanup ---
		var leftPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
		var formatGroup = new GroupBox { Text = "Output Format", Size = new Size(380, 130), Margin = new Padding(0, 0, 0, 20) };
		_formatRelease = new RadioButton { Text = "Release (.xml, <Root> wrapper)", Location = new Point(15, 30), AutoSize = true, Checked = detectedType == GameType.Release };
		_formatDemo = new RadioButton { Text = "Demo (.xml.gz, GZip compressed)", Location = new Point(15, 60), AutoSize = true, Checked = detectedType == GameType.Demo };
		_formatAlpha = new RadioButton { Text = "Alpha (.xml, one directory per type)", Location = new Point(15, 90), AutoSize = true, Checked = detectedType == GameType.Alpha };
		formatGroup.Controls.AddRange([_formatRelease, _formatDemo, _formatAlpha]);
		leftPanel.Controls.Add(formatGroup);

		var cleanupGroup = new GroupBox { Text = "Minification Options", Size = new Size(380, 160) };
		_cleanUpOrphanedElements = new CheckBox { Text = "Clean up orphaned elements", Location = new Point(15, 30), AutoSize = true };
		_removeDefaultValueTypes = new CheckBox { Text = "Remove tags with default values", Location = new Point(15, 60), AutoSize = true };
		_stripNames = new CheckBox { Text = "Strip names", Location = new Point(15, 90), AutoSize = true };
		_stripEditorOnlyTags = new CheckBox { Text = "Strip editor-only tags", Location = new Point(15, 120), AutoSize = true };
		cleanupGroup.Controls.AddRange([_cleanUpOrphanedElements, _removeDefaultValueTypes, _stripNames, _stripEditorOnlyTags]);
		leftPanel.Controls.Add(cleanupGroup);
		mainLayout.Controls.Add(leftPanel, 0, 1);

		// --- Right Side: VM Metadata ---
		if (isNewVm && templateManager != null) {
			var metadataGroup = new GroupBox { Text = "Virtual Machine Metadata (Version.xml)", Dock = DockStyle.Fill };
			var metaLayout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15), RowCount = 8, ColumnCount = 2 };
			metaLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
			metaLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
			
			// SET EXPLICIT ROW STYLES for perfect height distribution
			for (var i = 0; i < 8; i++) metaLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 12.5f));

			var r = 0;
			AddMetaRow(metaLayout, r++, "Game Name:", _gameNameBox = new TextBox { Text = defaultMeta?.GameName ?? "Haruspex", Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) });
			AddMetaRow(metaLayout, r++, "Scene:", _sceneCombo = CreateTemplateCombo<SceneObject>(templateManager, defaultMeta?.Scene ?? new Guid("1d70fc8a-a74d-5144-693c-ae5769293269")));
			AddMetaRow(metaLayout, r++, "Weather Snapshot:", _weatherCombo = CreateTemplateCombo<WeatherSnapshot>(templateManager, defaultMeta?.WeatherSnapshot ?? new Guid("16de4259-4406-48d7-9244-84a87cbbc369")));
			
			var timeRow = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
			timeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 60));
			timeRow.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			timeRow.Controls.Add(_solarDayNum = new NumericUpDown { Minimum = 1, Maximum = 100, Value = defaultMeta?.SolarTime.Day ?? 1, Dock = DockStyle.Fill, Margin = new Padding(0, 5, 5, 0) }, 0, 0);
			var solarTimeForPicker = defaultMeta?.SolarTime;
			var pickerValue = solarTimeForPicker.HasValue
				? new DateTime(2000, 1, 1, solarTimeForPicker.Value.Hour, solarTimeForPicker.Value.Minute, solarTimeForPicker.Value.Second)
				: new DateTime(2000, 1, 1, 7, 30, 0);
			timeRow.Controls.Add(_solarTimePicker = new DateTimePicker { 
				Format = DateTimePickerFormat.Custom, 
				CustomFormat = "HH:mm:ss", 
				ShowUpDown = true, 
				Value = pickerValue,
				Dock = DockStyle.Fill, 
				Margin = new Padding(0, 5, 0, 0)
			}, 1, 0);
			AddMetaRow(metaLayout, r++, "Solar Time (Day.Time):", timeRow);
			
			AddMetaRow(metaLayout, r++, "Sky Rotation:", _skyRotationNum = new NumericUpDown { Minimum = -36000, Maximum = 36000, Value = defaultMeta?.SkyRotation ?? 145, Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) });
			AddMetaRow(metaLayout, r++, "Loading Window Day:", _loadingDayNum = new NumericUpDown { Minimum = -100, Maximum = 100, Value = defaultMeta?.LoadingWindowGameDay ?? -1, Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) });
			AddMetaRow(metaLayout, r++, "Hide Loading Window:", _hideLoadingCheck = new CheckBox { Checked = defaultMeta?.HideLoadingWindow ?? false, Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) });
			AddMetaRow(metaLayout, r++, "Loading Screen:", _loadingScreenBox = new TextBox { Text = defaultMeta?.LoadingScreenName ?? "PathologicSandbox", Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) });

			metadataGroup.Controls.Add(metaLayout);
			mainLayout.Controls.Add(metadataGroup, 1, 1);
		}

		var buttonPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
		_okButton = new Button { Text = "Save", DialogResult = DialogResult.OK, Size = new Size(120, 40) };
		_cancelButton = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Size = new Size(120, 40) };
		buttonPanel.Controls.AddRange([_cancelButton, _okButton]);
		mainLayout.Controls.Add(buttonPanel, 1, 2);

		Controls.Add(mainLayout);
		AcceptButton = _okButton;
		CancelButton = _cancelButton;
		_okButton.Click += OkButton_Click;
	}

	private void AddMetaRow(TableLayoutPanel layout, int row, string labelText, Control control) {
		layout.Controls.Add(new Label { 
			Text = labelText, 
			AutoSize = true, 
			TextAlign = ContentAlignment.BottomLeft, 
			Dock = DockStyle.Fill, 
			Margin = new Padding(0) 
		}, 0, row);
		layout.Controls.Add(control, 1, row);
	}

	private ComboBox CreateTemplateCombo<T>(TemplateManager tm, Guid? defaultGuid = null) where T : TemplateObject {
		var combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(0, 5, 0, 0) };
		var items = tm.Templates.Values.OfType<T>()
			.Select(t => new TemplateItem(t.Id, t.Name))
			.OrderBy(i => i.Name)
			.ToArray();
		
		combo.Items.AddRange(items.Cast<object>().ToArray());
		
		if (defaultGuid.HasValue) {
			var index = -1;
			for(var i = 0; i < items.Length; i++) {
				if (items[i].Id != defaultGuid.Value) continue;
				index = i; break;
			}
			if (index >= 0) combo.SelectedIndex = index;
			else if (combo.Items.Count > 0) combo.SelectedIndex = 0;
		} else if (combo.Items.Count > 0) {
			combo.SelectedIndex = 0;
		}
		
		return combo;
	}

	private record TemplateItem(Guid Id, string Name) {
		public override string ToString() => Name;
	}

	private void OkButton_Click(object? sender, EventArgs e) {
		VmVersionSettings? metadata = null;
		if (_pathBox != null) {
			var time = _solarTimePicker!.Value;
			var combinedDateTime = new DateTime(1, 1, (int)_solarDayNum!.Value, time.Hour, time.Minute, time.Second);

			metadata = new VmVersionSettings(
				_pathBox.Text,
				_gameNameBox!.Text,
				((TemplateItem)_sceneCombo!.SelectedItem).Id,
				((TemplateItem)_weatherCombo!.SelectedItem).Id,
				combinedDateTime,
				(int)_skyRotationNum!.Value,
				(int)_loadingDayNum!.Value,
				_hideLoadingCheck!.Checked,
				_loadingScreenBox!.Text
			);
		}

		Settings = new WriterSettings {
			Format = _formatDemo.Checked ? WriterFormat.Demo : _formatAlpha.Checked ? WriterFormat.Alpha : WriterFormat.Release,
			CleanUpOrphanedElements = _cleanUpOrphanedElements.Checked,
			RemoveDefaultValueTypes = _removeDefaultValueTypes.Checked,
			StripNames = _stripNames.Checked,
			StripEditorOnlyTags = _stripEditorOnlyTags.Checked,
			VmMetadata = metadata
		};
	}
}
