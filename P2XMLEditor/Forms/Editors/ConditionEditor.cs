using System;
using System.Collections.Generic;
using System.Drawing;
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
    private FlowLayoutPanel flowConditionType;
    private LinkLabel lnkFirstExpression;
    private LinkLabel lnkSecondExpression;
    private Panel pnlFirstExpression;
    private Panel pnlSecondExpression;
    private PartCondition _currentPartCondition;
    
    // Snapshot state for Cancel
    private bool _snapshotActive = true;
    private ConditionOperation _originalOperation;
    private List<VmEither<Condition, PartCondition>> _originalPredicates;
    private Dictionary<PartCondition, PartConditionSnapshot> _originalPartConditions = new();
    private List<VmElement> _addedElements = new();

    private class PartConditionSnapshot {
        public string Name;
        public ConditionType ConditionType;
        public Expression FirstExpression;
        public Expression SecondExpression;
    }

	public ConditionEditorForm(VirtualMachine vm, Condition condition,
		VmEither<Branch, Event, MindMapNode, Speech, State> localContext) {
		StartPosition = FormStartPosition.CenterParent;
		_vm = vm;
		_condition = condition;
		_localContext = localContext;
		
		_originalOperation = condition.Operation;
		_originalPredicates = condition.Predicates.ToList();
		foreach (var pred in condition.Predicates) {
			if (pred.Element is PartCondition pc) {
				_originalPartConditions[pc] = new PartConditionSnapshot {
					Name = pc.Name,
					ConditionType = pc.ConditionType,
					FirstExpression = pc.FirstExpression,
					SecondExpression = pc.SecondExpression
				};
			}
		}
		
		InitializeComponents();
		LoadData();
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
        
		cmbOperation = new ComboBox { Dock = DockStyle.Top, Enabled = false, DropDownStyle = ComboBoxStyle.DropDownList };
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
			var newPc = VmElement.CreateDefault<PartCondition>(_vm, _localContext.Element);
			_addedElements.Add(newPc);
			_condition.Predicates.Add(new(newPc));
			LoadData();
		};
		btnAddNestedCondition = new Button { Text = "Add Nested Condition", Dock = DockStyle.Top, Height = 40 };
		btnAddNestedCondition.Click += (_, _) => {
			var newCond = VmElement.CreateDefault<Condition>(_vm, _localContext.Element);
			_addedElements.Add(newCond);
			_condition.Predicates.Add(new(newCond));
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
		
		var buttonsPanel = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 52, Padding = new Padding(10) };
		var btnCancel = new Button { Text = "Cancel", Size = new Size(100, 32), DialogResult = DialogResult.Cancel, Margin = new Padding(0) };
		btnOK = new Button { Text = "Save", Size = new Size(100, 32), Margin = new Padding(8, 0, 0, 0) };
		btnOK.Click += BtnOK_Click;
		buttonsPanel.Controls.Add(btnOK);
		buttonsPanel.Controls.Add(btnCancel);
		
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
        
        var typePanel = new GroupBox { Text = "Condition Type", Dock = DockStyle.Top, Height = 100 };
		flowConditionType = new FlowLayoutPanel { Dock = DockStyle.Fill };
		foreach (ConditionType t in Enum.GetValues(typeof(ConditionType))) {
            string label = t switch {
                ConditionType.ValueEqual => "==",
                ConditionType.ValueNotEqual => "!=",
                ConditionType.ValueLess => "<",
                ConditionType.ValueLessEqual => "<=",
                ConditionType.ValueLarger => ">",
                ConditionType.ValueLargerEqual => ">=",
                ConditionType.ValueExpression => "Expression",
                ConditionType.ConstTrue => "True",
                ConditionType.ConstFalse => "False",
                _ => t.ToString()
            };
			var rb = new RadioButton { Text = label, Tag = t, AutoSize = true };
			rb.CheckedChanged += (_, _) => {
				if (!rb.Checked) return;
				if (_currentPartCondition == null) return;
				var selectedType = (ConditionType)rb.Tag;
				var oldType = _currentPartCondition.ConditionType;
				_currentPartCondition.ConditionType = selectedType;
				
				if (selectedType is ConditionType.ConstTrue or ConditionType.ConstFalse) {
					_vm.RemoveElement(_currentPartCondition.FirstExpression);
					_currentPartCondition.FirstExpression = null;
					_vm.RemoveElement(_currentPartCondition.SecondExpression);
					_currentPartCondition.SecondExpression = null;
				} else if (oldType is ConditionType.ConstTrue or ConditionType.ConstFalse) {
					_vm.RemoveElement(_currentPartCondition.FirstExpression);
					_vm.RemoveElement(_currentPartCondition.SecondExpression);
					_currentPartCondition.FirstExpression = null;
					_currentPartCondition.SecondExpression = null;
				}
				UpdateExpressionsView();
				RefreshSelectedPredicatePreview();
			};
			flowConditionType.Controls.Add(rb);
		}
		typePanel.Controls.Add(flowConditionType);
        
        var exprPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 10, 0, 0) };
        
        pnlFirstExpression = new Panel { Dock = DockStyle.Top, Height = 60 };
        var lblFirst = new Label { Text = "First Expression:", Font = new Font(Font, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Top };
        
        var innerFirst = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 5) };
        lnkFirstExpression = new LinkLabel { AutoSize = false, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        lnkFirstExpression.LinkClicked += (_, _) => OpenExpressionEditor(true);
        var btnClearFirst = new Button { Text = "Clear", Width = 70, Dock = DockStyle.Right };
        btnClearFirst.Click += (_, _) => ClearExpression(true);
        innerFirst.Controls.Add(lnkFirstExpression);
        innerFirst.Controls.Add(btnClearFirst);
        pnlFirstExpression.Controls.Add(innerFirst);
        pnlFirstExpression.Controls.Add(lblFirst);
        
        pnlSecondExpression = new Panel { Dock = DockStyle.Top, Height = 60 };
        var lblSecond = new Label { Text = "Second Expression:", Font = new Font(Font, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Top };
        
        var innerSecond = new Panel { Dock = DockStyle.Fill, Padding = new Padding(0, 5, 0, 5) };
        lnkSecondExpression = new LinkLabel { AutoSize = false, TextAlign = System.Drawing.ContentAlignment.MiddleLeft, Dock = DockStyle.Fill };
        lnkSecondExpression.LinkClicked += (_, _) => OpenExpressionEditor(false);
        var btnClearSecond = new Button { Text = "Clear", Width = 70, Dock = DockStyle.Right };
        btnClearSecond.Click += (_, _) => ClearExpression(false);
        innerSecond.Controls.Add(lnkSecondExpression);
        innerSecond.Controls.Add(btnClearSecond);
        pnlSecondExpression.Controls.Add(innerSecond);
        pnlSecondExpression.Controls.Add(lblSecond);
        
        exprPanel.Controls.Add(pnlSecondExpression);
        exprPanel.Controls.Add(pnlFirstExpression);
        
        pnlPartConditionSettings.Controls.Add(exprPanel);
        pnlPartConditionSettings.Controls.Add(typePanel);
        pnlPartConditionSettings.Controls.Add(namePanel);
        
        splitContainer.Panel2.Controls.Add(pnlPartConditionSettings);
        
		Controls.Add(splitContainer);
		Controls.Add(buttonsPanel);
		
		AcceptButton = btnOK;
		CancelButton = btnCancel;
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
            foreach (RadioButton rb in flowConditionType.Controls) {
                if (rb.Tag is ConditionType t && t == partCond.ConditionType) {
                    rb.Checked = true;
                    break;
                }
            }
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
            lnkFirstExpression.Text = PreviewHelper.Preview(_currentPartCondition.FirstExpression);
            if (showSecond)
                lnkSecondExpression.Text = PreviewHelper.Preview(_currentPartCondition.SecondExpression);
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
		
		var currentSet = new HashSet<VmElement>(_condition.Predicates.Select(p => p.Element));
		
		// Remove elements that were originally there but got deleted
		foreach (var orig in _originalPredicates) {
			if (!currentSet.Contains(orig.Element)) {
				if (orig.Element is PartCondition pc) _vm.RemoveElement(pc);
				else if (orig.Element is Condition cond) {
					cond.OnDestroy(_vm);
					_vm.RemoveElement(cond);
				}
			}
		}
		
		// Remove added elements that were subsequently deleted before Save
		foreach (var added in _addedElements) {
			if (!currentSet.Contains(added)) {
				if (added is PartCondition pc) _vm.RemoveElement(pc);
				else if (added is Condition cond) {
					cond.OnDestroy(_vm);
					_vm.RemoveElement(cond);
				}
			}
		}
		
		_condition.Operation = _condition.Predicates.Count <= 1 ? ConditionOperation.Root : (ConditionOperation)cmbOperation.SelectedItem!;
		UpdatePredicateOrderIndices();
		
		_snapshotActive = false;
		DialogResult = DialogResult.OK;
		Close();
	}
	
	protected override void OnFormClosing(FormClosingEventArgs e) {
		if (_snapshotActive && DialogResult != DialogResult.OK) {
			_condition.Operation = _originalOperation;
			_condition.Predicates = _originalPredicates;
			
			foreach (var kvp in _originalPartConditions) {
				kvp.Key.Name = kvp.Value.Name;
				kvp.Key.ConditionType = kvp.Value.ConditionType;
				kvp.Key.FirstExpression = kvp.Value.FirstExpression;
				kvp.Key.SecondExpression = kvp.Value.SecondExpression;
			}
			
			foreach (var added in _addedElements) {
				if (added is PartCondition pc) _vm.RemoveElement(pc);
				else if (added is Condition cond) {
					cond.OnDestroy(_vm);
					_vm.RemoveElement(cond);
				}
			}
		}
		base.OnFormClosing(e);
	}
}
