using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.Editors;
using P2XMLEditor.GameData.Templates;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Services;

namespace P2XMLEditor.Forms.MainForm.MindMapViewer;

public class NodePropertiesPanel : Panel {
	private readonly Dictionary<string, string> EngineIdToImageMap = new();
		
	private readonly VirtualMachine _vm;
	private MindMapNode? _currentNode;

	private readonly TableLayoutPanel _nodePropertiesPanel;
	private readonly TextBox _nameBox;
	private readonly ComboBox _logicMapNodeTypeComboBox;

	private readonly SplitContainer _contentSplitContainer;
	private readonly TableLayoutPanel _contentListPanel;
	private readonly FlowLayoutPanel _contentButtonPanel;
	private readonly ListBox _contentList;
	private readonly Button _addContentButton;
	private readonly Button _removeContentButton;

	private readonly TableLayoutPanel _contentDetailsPanel;
	private readonly TextBox _contentNameBox;
	private readonly ComboBox _contentTypeComboBox;
	private readonly Label _contentDescriptionPreview;
	private readonly ComboBox _contentPictureComboBox;
	private readonly Label _contentConditionPreview;
	private readonly ToolTip _toolTip;
	private MindMapNodeContent? _selectedContent;

	private bool _updatingPictureComboBox;
	
	public NodePropertiesPanel(VirtualMachine vm) {
		_vm = vm;
		Dock = DockStyle.Right;
		Width = 300;
		_toolTip = new ToolTip();
		var mainLayout = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			RowCount = 2
		};
		mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		Controls.Add(mainLayout);
		_nodePropertiesPanel = new TableLayoutPanel {
			Dock = DockStyle.Top,
			ColumnCount = 2,
			AutoSize = true
		};
		_nodePropertiesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
		_nodePropertiesPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
		var nameLabel = new Label { Text = "Name:", Anchor = AnchorStyles.Left, AutoSize = true };
		_nameBox = new TextBox { Dock = DockStyle.Fill };
		_nameBox.TextChanged += OnNameChanged;
		_nodePropertiesPanel.Controls.Add(nameLabel, 0, 0);
		_nodePropertiesPanel.Controls.Add(_nameBox, 1, 0);
		var typeLabel = new Label { Text = "Node Type:", Anchor = AnchorStyles.Left, AutoSize = true };
		_logicMapNodeTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
		_logicMapNodeTypeComboBox.DataSource = Enum.GetValues(typeof(LogicMapNodeType));
		_logicMapNodeTypeComboBox.SelectedIndexChanged += OnNodeTypeChanged;
		_nodePropertiesPanel.Controls.Add(typeLabel, 0, 1);
		_nodePropertiesPanel.Controls.Add(_logicMapNodeTypeComboBox, 1, 1);
		mainLayout.Controls.Add(_nodePropertiesPanel, 0, 0);
		_contentSplitContainer = new SplitContainer {
			Dock = DockStyle.Fill,
			Orientation = Orientation.Horizontal,
			SplitterDistance = 120
		};
		_contentListPanel = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			RowCount = 2
		};
		_contentListPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		_contentListPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
		_contentButtonPanel = new FlowLayoutPanel {
			Dock = DockStyle.Fill,
			AutoSize = false,
			Height = 35,
			FlowDirection = FlowDirection.LeftToRight,
			WrapContents = false
		};
		_addContentButton = new Button { Text = "Add Content", AutoSize = true, Height = 30 };
		_addContentButton.Click += OnAddContent;
		_removeContentButton = new Button { Text = "Remove Content", AutoSize = true, Height = 30 };
		_removeContentButton.Click += OnRemoveContent;
		_contentButtonPanel.Controls.Add(_addContentButton);
		_contentButtonPanel.Controls.Add(_removeContentButton);
		_contentList = new ListBox { Dock = DockStyle.Fill, Height = 20, FormattingEnabled = true, AutoSize = false};
		_contentList.Format += (_, e) => {
			if (e.ListItem is MindMapNodeContent content)
				e.Value = string.IsNullOrEmpty(content.Name) ? $"Content {content.Number}" : content.Name;
		};
		_contentList.SelectedIndexChanged += OnContentSelected;
		_contentListPanel.Controls.Add(_contentButtonPanel, 0, 0);
		_contentListPanel.Controls.Add(_contentList, 0, 1);
		_contentSplitContainer.Panel1.Controls.Add(_contentListPanel);
		_contentDetailsPanel = new TableLayoutPanel {
			Dock = DockStyle.Fill,
			ColumnCount = 2,
			RowCount = 5
		};
		_contentDetailsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
		_contentDetailsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
		_contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		_contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		_contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
		_contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
		_contentDetailsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
		_contentDetailsPanel.Controls.Add(
			new Label { Text = "Content Name:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 0);
		_contentNameBox = new TextBox { Dock = DockStyle.Fill };
		_contentNameBox.TextChanged += (_, _) => {
			if (_selectedContent == null) return;
			_selectedContent.Name = _contentNameBox.Text;
			RefreshContentList();
		};
		_contentDetailsPanel.Controls.Add(_contentNameBox, 1, 0);
		_contentDetailsPanel.Controls.Add(
			new Label { Text = "Content Type:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 1);
		_contentTypeComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
		_contentTypeComboBox.DataSource = Enum.GetValues(typeof(NodeContentType));
		_contentTypeComboBox.SelectedIndexChanged += (_, _) => {
			if (_selectedContent == null || _contentTypeComboBox.SelectedItem is not NodeContentType type) return;
			_selectedContent.ContentType = type;
		};
		_contentDetailsPanel.Controls.Add(_contentTypeComboBox, 1, 1);
		_contentDetailsPanel.Controls.Add(
			new Label { Text = "Description:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 2);
		_contentDescriptionPreview = new Label
			{ Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 60 };
		_contentDescriptionPreview.DoubleClick += OnEditDescriptionClicked;
		_toolTip.SetToolTip(_contentDescriptionPreview, "Double-click to edit description");
		_contentDetailsPanel.Controls.Add(_contentDescriptionPreview, 1, 2);
		_contentDetailsPanel.Controls.Add(new Label { Text = "Picture:", Anchor = AnchorStyles.Left, AutoSize = true },
			0, 3);
		_contentPictureComboBox = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList,
			FormattingEnabled = true  };
		_contentPictureComboBox.SelectedIndexChanged += OnContentPictureChanged;

		var samples = vm.GetElementsByType<Sample>().ToList();
		foreach (var template in vm.TemplateManagerInst.Templates.Select(kvp => kvp.Value)) {
			if (template is not MMPlaceholder) continue;
			var guid = template.Id.ToString().Replace("-", "");
			if (samples.All(s => s.EngineId != guid)) continue;
			var name = template.Name.Replace("MindMap_", "").ToLower();
			EngineIdToImageMap[guid] = name;
		}
		_contentPictureComboBox.Format += (_, e) => {
			if (e.ListItem is Sample sample) 
				e.Value = EngineIdToImageMap.TryGetValue(sample.EngineId, out var name) ? name : sample.EngineId;
		};
		_contentDetailsPanel.Controls.Add(_contentPictureComboBox, 1, 3);
		_contentDetailsPanel.Controls.Add(
			new Label { Text = "Condition:", Anchor = AnchorStyles.Left, AutoSize = true }, 0, 4);
		_contentConditionPreview = new Label
			{ Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, AutoSize = false, Height = 60 };
		_contentConditionPreview.DoubleClick += OnEditConditionClicked;
		_toolTip.SetToolTip(_contentConditionPreview, "Double-click to edit condition");
		_contentDetailsPanel.Controls.Add(_contentConditionPreview, 1, 4);
		_contentSplitContainer.Panel2.Controls.Add(_contentDetailsPanel);
		mainLayout.Controls.Add(_contentSplitContainer, 0, 1);
		_nodePropertiesPanel.Enabled = false;
		_contentButtonPanel.Enabled = false;
		_contentDetailsPanel.Visible = false;
	}
	
	public void SetNode(MindMapNode? node) {
		_currentNode = node;
		_nodePropertiesPanel.Enabled = node != null;
		_contentButtonPanel.Enabled = node != null;
		if (node != null) {
			_nameBox.Text = node.Name;
			_logicMapNodeTypeComboBox.SelectedItem = node.LogicMapNodeType;
		} else {
			_nameBox.Text = "";
		}

		RefreshContentList();
		
		if (node?.Content.Count > 0)
			_contentList.SelectedIndex = 0;
		else
			ClearContentDetails();
		
		Invalidate();
	}

	private void RefreshContentList() {
		_contentList.Items.Clear();
		if (_currentNode == null) return;
		foreach (var content in _currentNode.Content)
			_contentList.Items.Add(content);
	}
	
	public void RefreshLanguage() {
		RefreshContentDetails();
	}

	private void RefreshContentDetails() {
		if (_selectedContent == null) return;
		_contentDetailsPanel.Visible = true;
		_contentNameBox.Text = _selectedContent.Name;
		_contentTypeComboBox.SelectedItem = _selectedContent.ContentType;
		_contentDescriptionPreview.Text = _selectedContent.ContentDescriptionText.GetText(PreviewLanguageService.CurrentLanguage);
		_contentConditionPreview.Text = PreviewHelper.Preview(_selectedContent.ContentCondition);

		_updatingPictureComboBox = true;
		try {
			var samples = _vm.GetElementsByType<Sample>()
				.Where(s => s.SampleType == SampleType.MindMapPicture)
				.ToList();
			
			_contentPictureComboBox.DataSource = null;
			_contentPictureComboBox.DataSource = samples;

			if (_selectedContent.ContentPicture == null) return;
			var selectedSample = samples.FirstOrDefault(s => s.Id == _selectedContent.ContentPicture.Id);
			_contentPictureComboBox.SelectedItem = selectedSample;
			
		} finally {
			_updatingPictureComboBox = false;
		}
	}

	private void OnContentPictureChanged(object? sender, EventArgs e) {
		if (_updatingPictureComboBox) return;
		if (_selectedContent == null || _contentPictureComboBox.SelectedItem is not Sample sample) return;
		_selectedContent.ContentPicture = sample;
	}
	
	private void OnEditDescriptionClicked(object? sender, EventArgs e) {
		if (_selectedContent == null) return;
		var editor = new GameStringEditor(_selectedContent.ContentDescriptionText, _vm);
		if (editor.ShowDialog() == DialogResult.OK) {
			_contentDescriptionPreview.Text = _selectedContent.ContentDescriptionText.GetText(PreviewLanguageService.CurrentLanguage);
		}
	}


	private void OnNameChanged(object? sender, EventArgs e) {
		if (_currentNode != null) {
			_currentNode.Name = _nameBox.Text;
			if (Parent is MindMapViewer viewer)
				viewer.RefreshView();
		}
	}

	private void OnNodeTypeChanged(object? sender, EventArgs e) {
		if (_currentNode != null && _logicMapNodeTypeComboBox.SelectedItem is LogicMapNodeType newType) {
			_currentNode.LogicMapNodeType = newType;
			if (Parent is MindMapViewer viewer)
				viewer.RefreshView();
		}
	}

	private void OnAddContent(object? sender, EventArgs e) {
		if (_currentNode == null) return;
		var content = VmElement.CreateDefault<MindMapNodeContent>(_vm, _currentNode);
		_currentNode.Content.Add(content);
		RecalculateContentNumbers();
		RefreshContentList();
	}

	private void OnRemoveContent(object? sender, EventArgs e) {
		if (_currentNode == null || _contentList.SelectedItem is not MindMapNodeContent content)
			return;
		_currentNode.Content.Remove(content);
		_vm.RemoveElement(content);
		RecalculateContentNumbers();
		RefreshContentList();
		ClearContentDetails();
	}

	private void RecalculateContentNumbers() {
		if (_currentNode == null) return;
		for (var i = 0; i < _currentNode.Content.Count; i++)
			_currentNode.Content[i].Number = i;
	}

	private void OnContentSelected(object? sender, EventArgs e) {
		if (_contentList.SelectedItem is MindMapNodeContent content) {
			_selectedContent = content;
			RefreshContentDetails();
		} else {
			_selectedContent = null;
			ClearContentDetails();
		}
	}

	private void ClearContentDetails() {
		_contentDetailsPanel.Visible = false;
		_contentNameBox.Text = "";
		_contentTypeComboBox.SelectedIndex = -1;
		_contentDescriptionPreview.Text = "";
		_contentPictureComboBox.DataSource = null;
		_contentConditionPreview.Text = "";
	}

	private void OnEditConditionClicked(object? sender, EventArgs e) {
		if (_selectedContent == null) return;
		using var editor = new ConditionEditorForm(_vm, _selectedContent.ContentCondition, new(_currentNode!));
		if (editor.ShowDialog() == DialogResult.OK)
		{
			_contentConditionPreview.Text = PreviewHelper.Preview(_selectedContent.ContentCondition);
		}
	}
}
