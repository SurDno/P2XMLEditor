using System;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Forms.Editors.Actions;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors;

public class ConditionEditorForm : Form {
	private readonly VirtualMachine _vm;
	private readonly Condition _condition;
	private readonly VmEither<Branch, Event, MindMapNode, Speech, State> _localContext;
	private ComboBox cmbOperation;
	private ListBox lstPredicates;
	private Button btnAddPartCondition;
	private Button btnAddNestedCondition;
	private Button btnRemovePredicate;
	private Button btnOK;
    // PartCondition settings panel
    private Panel pnlPartConditionSettings;
    private TextBox txtName;
    private ComboBox cmbConditionType;
    private LinkLabel lnkFirstExpression;
    private LinkLabel lnkSecondExpression;
    private FlowLayoutPanel pnlFirstExpression;
    private FlowLayoutPanel pnlSecondExpression;
    private PartCondition _currentPartCondition;

	public ConditionEditorForm(VirtualMachine vm, Condition condition,
		VmEither<Branch, Event, MindMapNode, Speech, State> localContext) {
		_vm = vm;
		_condition = condition;
		_localContext = localContext;
		UpdateConditionOrderIndex();
		InitializeComponents();
		LoadData();
	}
	private void UpdateConditionOrderIndex() { 
		foreach (var condition in _vm.GetElementsByType<Condition>()) {
			for (var i = 0; i < condition.Predicates.Count; i++) {
				if (condition.Predicates[i].Element != _condition) continue;
				_condition.OrderIndex = (byte)i;
				return;
			}
		}
		_condition.OrderIndex = 0;
	}
	private void UpdatePredicateOrderIndices() {
		for (var i = 0; i < _condition.Predicates.Count; i++) {
			var element = _condition.Predicates[i].Element;
			switch (element) {
				case PartCondition partCond:
					partCond.OrderIndex = (byte)i;
					break;
				case Condition cond:
					cond.OrderIndex = (byte)i;
					break;
			}
		}
	}
	private void InitializeComponents() {
		Text = "Edit Condition";
		Width = 900;
		Height = 500;
        
        var splitContainer = new SplitContainer { Dock = DockStyle.Fill };
        var leftPanel = new Panel { Dock = DockStyle.Fill };
        
		cmbOperation = new ComboBox { Dock = DockStyle.Top, Enabled = false };
		UpdateOperationComboBox();
		lstPredicates = new ListBox { Dock = DockStyle.Fill };
        lstPredicates.SelectedIndexChanged += (_, _) => {
            UpdatePartConditionPanel();
        };
		lstPredicates.DoubleClick += (_, _) => {
			if (lstPredicates.SelectedIndex < 0) return;
			switch (_condition.Predicates[lstPredicates.SelectedIndex].Element) {
				case Condition nestedCondition: {
					using var condEditor = new ConditionEditorForm(_vm, nestedCondition, _localContext);
					condEditor.ShowDialog();
					LoadData();
					break;
				}
			}
		};
		btnAddPartCondition = new Button { Text = "Add PartCondition", Dock = DockStyle.Top, Height = 40 };
		btnAddPartCondition.Click += (_, _) => {
			_condition.Predicates.Add(new(VmElement.CreateDefault<PartCondition>(_vm, _localContext.Element)));
			LoadData();
		};
		btnAddNestedCondition = new Button { Text = "Add Nested Condition", Dock = DockStyle.Top, Height = 40 };
		btnAddNestedCondition.Click += (_, _) => {
			_condition.Predicates.Add(new(VmElement.CreateDefault<Condition>(_vm, _localContext.Element)));
			LoadData();
		};
		btnRemovePredicate = new Button { Text = "Remove Predicate", Dock = DockStyle.Top, Height = 40 };
		btnRemovePredicate.Click += (_, _) => {
			var index = lstPredicates.SelectedIndex;
			if (index < 0)
				return;
			_condition.Predicates.RemoveAt(index);
			LoadData();
		};
		btnOK = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 40 };
		btnOK.Click += BtnOK_Click;
		var topPanel = new Panel { Dock = DockStyle.Top, Height = 160 };
		topPanel.Controls.Add(btnRemovePredicate);
		topPanel.Controls.Add(btnAddNestedCondition);
		topPanel.Controls.Add(btnAddPartCondition);
		topPanel.Controls.Add(cmbOperation);
		leftPanel.Controls.Add(lstPredicates);
		leftPanel.Controls.Add(topPanel);
        
        splitContainer.Panel1.Controls.Add(leftPanel);
        
        // Right Panel (PartCondition Settings)
        pnlPartConditionSettings = new Panel { Dock = DockStyle.Fill, Visible = false };
        
        var namePanel = new Panel { Dock = DockStyle.Top, Height = 60 };
		var lblName = new Label { Text = "Name:", Dock = DockStyle.Top, Height = 30 };
		txtName = new TextBox { Dock = DockStyle.Top, Height = 30 };
        txtName.TextChanged += (_, _) => {
            if (_currentPartCondition == null) return;
            _currentPartCondition.Name = txtName.Text;
            RefreshSelectedPredicatePreview();
        };
		namePanel.Controls.Add(txtName);
		namePanel.Controls.Add(lblName);
        
        var typePanel = new Panel { Dock = DockStyle.Top, Height = 60 };
		var lblType = new Label { Text = "Condition Type:", Dock = DockStyle.Top, Height = 30 };
		cmbConditionType = new ComboBox { Dock = DockStyle.Top, Height = 30 };
		cmbConditionType.Items.AddRange(Enum.GetValues(typeof(ConditionType)).Cast<object>().ToArray());
		cmbConditionType.SelectedIndexChanged += (_, _) => {
            if (_currentPartCondition == null) return;
			if (cmbConditionType.SelectedItem is not ConditionType selectedType) return;
            var oldType = _currentPartCondition.ConditionType;
			_currentPartCondition.ConditionType = selectedType;
            
			if (selectedType is ConditionType.ConstTrue or ConditionType.ConstFalse) {
				_vm.RemoveElement(_currentPartCondition.FirstExpression);
				_currentPartCondition.FirstExpression = null;
				_vm.RemoveElement(_currentPartCondition.SecondExpression);
				_currentPartCondition.SecondExpression = null;
			} else if (oldType is ConditionType.ConstTrue or ConditionType.ConstFalse) {
                // Switching from Const to Binary: Initialize to null to avoid type deadlock
                _vm.RemoveElement(_currentPartCondition.FirstExpression);
                _vm.RemoveElement(_currentPartCondition.SecondExpression);
                _currentPartCondition.FirstExpression = null;
                _currentPartCondition.SecondExpression = null;
            }
			UpdateExpressionsView();
            RefreshSelectedPredicatePreview();
		};
		typePanel.Controls.Add(cmbConditionType);
		typePanel.Controls.Add(lblType);
        
        var exprPanel = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown };
        
        pnlFirstExpression = new FlowLayoutPanel { Width = 500, Height = 35, FlowDirection = FlowDirection.LeftToRight };
        lnkFirstExpression = new LinkLabel { Width = 400, Height = 30, AutoSize = false, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        lnkFirstExpression.LinkClicked += (_, _) => OpenExpressionEditor(true);
        var btnClearFirst = new Button { Text = "Clear", Width = 50, Height = 30 };
        btnClearFirst.Click += (_, _) => ClearExpression(true);
        pnlFirstExpression.Controls.Add(lnkFirstExpression);
        pnlFirstExpression.Controls.Add(btnClearFirst);
        
        pnlSecondExpression = new FlowLayoutPanel { Width = 500, Height = 35, FlowDirection = FlowDirection.LeftToRight };
        lnkSecondExpression = new LinkLabel { Width = 400, Height = 30, AutoSize = false, TextAlign = System.Drawing.ContentAlignment.MiddleLeft };
        lnkSecondExpression.LinkClicked += (_, _) => OpenExpressionEditor(false);
        var btnClearSecond = new Button { Text = "Clear", Width = 50, Height = 30 };
        btnClearSecond.Click += (_, _) => ClearExpression(false);
        pnlSecondExpression.Controls.Add(lnkSecondExpression);
        pnlSecondExpression.Controls.Add(btnClearSecond);
        
        exprPanel.Controls.Add(pnlFirstExpression);
        exprPanel.Controls.Add(pnlSecondExpression);
        
        pnlPartConditionSettings.Controls.Add(exprPanel);
        pnlPartConditionSettings.Controls.Add(typePanel);
        pnlPartConditionSettings.Controls.Add(namePanel);
        
        splitContainer.Panel2.Controls.Add(pnlPartConditionSettings);
        
		Controls.Add(splitContainer);
		Controls.Add(btnOK);
	}
    
    private void UpdatePartConditionPanel() {
        if (lstPredicates.SelectedIndex < 0) {
            pnlPartConditionSettings.Visible = false;
            _currentPartCondition = null;
            return;
        }
        
        var selectedElement = _condition.Predicates[lstPredicates.SelectedIndex].Element;
        if (selectedElement is PartCondition partCond) {
            _currentPartCondition = partCond;
            pnlPartConditionSettings.Visible = true;
            
            txtName.Text = partCond.Name ?? string.Empty;
            cmbConditionType.SelectedItem = partCond.ConditionType;
            UpdateExpressionsView();
        } else {
            pnlPartConditionSettings.Visible = false;
            _currentPartCondition = null;
        }
    }
    
    private void RefreshSelectedPredicatePreview() {
        if (lstPredicates.SelectedIndex < 0) return;
        var idx = lstPredicates.SelectedIndex;
        var element = _condition.Predicates[idx].Element;
        lstPredicates.Items[idx] = PreviewHelper.Preview(element);
    }
    
    private void UpdateExpressionsView() {
        if (_currentPartCondition == null) return;
        bool isBinary = !(_currentPartCondition.ConditionType is ConditionType.ConstTrue or ConditionType.ConstFalse);
        bool showSecond = isBinary && _currentPartCondition.ConditionType is not ConditionType.ValueExpression;
        
        pnlFirstExpression.Visible = isBinary;
        pnlSecondExpression.Visible = showSecond;
        
        if (isBinary) {
            lnkFirstExpression.Text = "First Expression: " + PreviewHelper.Preview(_currentPartCondition.FirstExpression);
            if (showSecond)
                lnkSecondExpression.Text = "Second Expression: " + PreviewHelper.Preview(_currentPartCondition.SecondExpression);
        }
    }
    
    private void ClearExpression(bool firstSide) {
        if (_currentPartCondition == null) return;
        if (firstSide) {
            _vm.RemoveElement(_currentPartCondition.FirstExpression);
            _currentPartCondition.FirstExpression = null;
        } else {
            _vm.RemoveElement(_currentPartCondition.SecondExpression);
            _currentPartCondition.SecondExpression = null;
        }
        UpdateExpressionsView();
        RefreshSelectedPredicatePreview();
    }
    
    private void OpenExpressionEditor(bool firstSide) {
        if (_currentPartCondition == null) return;
        
        if (firstSide)
            _currentPartCondition.FirstExpression ??= VmElement.CreateDefault<Expression>(_vm, _localContext.Element);
        else
            _currentPartCondition.SecondExpression ??= VmElement.CreateDefault<Expression>(_vm, _localContext.Element);
            
        using var exprEditor = new ExpressionEditorForm(_vm,
            firstSide ? _currentPartCondition.FirstExpression : _currentPartCondition.SecondExpression,
            ExpressionTyping.ExpectedFor(_currentPartCondition, firstSide, _vm),
            _currentPartCondition.ConditionType, firstSide);
            
        if (exprEditor.ShowDialog() == DialogResult.OK) {
            UpdateExpressionsView();
            RefreshSelectedPredicatePreview();
        }
    }
    
	private void UpdateOperationComboBox() {
		var curSelection = cmbOperation.SelectedItem;
		cmbOperation.Items.Clear();
		if (_condition.Predicates.Count <= 1) {
			cmbOperation.Items.Add(ConditionOperation.Root);
			cmbOperation.SelectedItem = ConditionOperation.Root;
			cmbOperation.Enabled = false;
		} else {
			cmbOperation.Items.Add(ConditionOperation.And);
			cmbOperation.Items.Add(ConditionOperation.Or);
			cmbOperation.Items.Add(ConditionOperation.Xor);
			cmbOperation.Enabled = true;
			cmbOperation.SelectedItem = curSelection is ConditionOperation op && op != ConditionOperation.Root ? op :
				_condition.Operation != ConditionOperation.Root ? _condition.Operation : ConditionOperation.And;
		}
	}
	private void LoadData() {
        var lastSelectedIndex = lstPredicates.SelectedIndex;
		lstPredicates.Items.Clear();
		foreach (var pred in _condition.Predicates) 
			lstPredicates.Items.Add(PreviewHelper.Preview(pred.Element));
		if (lstPredicates.Items.Count > 0)
			lstPredicates.SelectedIndex = lastSelectedIndex >= 0 && lastSelectedIndex < lstPredicates.Items.Count ? lastSelectedIndex : 0;
        else
            UpdatePartConditionPanel();
		UpdateOperationComboBox();
		UpdatePredicateOrderIndices();
	}
	private void BtnOK_Click(object sender, EventArgs e) {
		foreach (var pred in _condition.Predicates) {
			if (pred.Element is PartCondition pc) {
				if (pc.ConditionType is not (ConditionType.ConstTrue or ConditionType.ConstFalse)) {
					if (pc.FirstExpression == null) {
						MessageBox.Show($"PartCondition '{pc.Name}' is missing its first expression.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
					if (pc.ConditionType != ConditionType.ValueExpression && pc.SecondExpression == null) {
						MessageBox.Show($"PartCondition '{pc.Name}' is missing its second expression.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
						return;
					}
				}
			}
		}
		_condition.Operation = _condition.Predicates.Count <= 1 ? ConditionOperation.Root : (ConditionOperation)cmbOperation.SelectedItem!;
		UpdatePredicateOrderIndices();
		DialogResult = DialogResult.OK;
		Close();
	}
}
