using System;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors;

public class PartConditionEditorForm : Form {
    private readonly VirtualMachine _vm;
    private readonly PartCondition _partCondition;
    private readonly VmEither<Branch, Event, MindMapNode, Speech, State> _localContext;

    private TextBox txtName;
    private ComboBox cmbConditionType;
    private ListBox lstExpressions;
    private Button btnOK;

    public PartConditionEditorForm(VirtualMachine vm, PartCondition partCondition,
        VmEither<Branch, Event, MindMapNode, Speech, State> localContext) {
        _vm = vm;
        _partCondition = partCondition;
        _localContext = localContext;
        InitializeComponents();
        LoadData();
    }

    private void InitializeComponents() {
        Text = "Edit PartCondition";
        Width = 600;
        Height = 400;

        var namePanel = new Panel { Dock = DockStyle.Top, Height = 60 };
        var lblName = new Label { Text = "Name:", Dock = DockStyle.Top, Height = 30 };
        txtName = new TextBox { Dock = DockStyle.Top, Height = 30 };
        namePanel.Controls.Add(txtName);
        namePanel.Controls.Add(lblName);

        var typePanel = new Panel { Dock = DockStyle.Top, Height = 60 };
        var lblType = new Label { Text = "Condition Type:", Dock = DockStyle.Top, Height = 30 };
        cmbConditionType = new ComboBox { Dock = DockStyle.Top, Height = 30 };
        cmbConditionType.Items.AddRange(Enum.GetValues(typeof(ConditionType)).Cast<object>().ToArray());
        cmbConditionType.SelectedIndexChanged += (_, _) => {
            if (cmbConditionType.SelectedItem is not ConditionType selectedType) return;
            _partCondition.ConditionType = selectedType;
            if (selectedType is ConditionType.ConstTrue or ConditionType.ConstFalse) {
                _vm.RemoveElement(_partCondition.FirstExpression);
                _partCondition.FirstExpression = null;
                _vm.RemoveElement(_partCondition.SecondExpression);
                _partCondition.SecondExpression = null;
            }
            LoadExpressionsList();
        };
        typePanel.Controls.Add(cmbConditionType);
        typePanel.Controls.Add(lblType);

        var expressionsPanel = new Panel { Dock = DockStyle.Fill };
        lstExpressions = new ListBox { Dock = DockStyle.Fill };
        lstExpressions.DoubleClick += (_, _) => {
            if (!lstExpressions.Enabled) return;
            _partCondition.FirstExpression ??= VmElement.CreateDefault<Expression>(_vm, _localContext.Element);
            _partCondition.SecondExpression ??= VmElement.CreateDefault<Expression>(_vm, _localContext.Element);
            using var exprEditor = new ExpressionEditorForm(_vm,
                lstExpressions.SelectedIndex == 0 ? _partCondition.FirstExpression : _partCondition.SecondExpression);
            if (exprEditor.ShowDialog() == DialogResult.OK)
                LoadExpressionsList();
        };
        expressionsPanel.Controls.Add(lstExpressions);

        btnOK = new Button { Text = "OK", Dock = DockStyle.Bottom, Height = 40 };
        btnOK.Click += (_, _) => {
            ApplyChanges();
            DialogResult = DialogResult.OK;
            Close();
        };

        Controls.Add(expressionsPanel);
        Controls.Add(typePanel);
        Controls.Add(namePanel);
        Controls.Add(btnOK);
    }

    private void LoadExpressionsList() {
        lstExpressions.Items.Clear();
        lstExpressions.Enabled = !(_partCondition.ConditionType is ConditionType.ConstTrue or ConditionType.ConstFalse);
        if (!lstExpressions.Enabled) return;
        lstExpressions.Items.Add("First Expression: " + PreviewHelper.Preview(_partCondition.FirstExpression));
        if(_partCondition.ConditionType is not ConditionType.ValueExpression)
            lstExpressions.Items.Add("Second Expression: " + PreviewHelper.Preview(_partCondition.SecondExpression));
    }

    private void LoadData() {
        txtName.Text = _partCondition.Name ?? string.Empty;
        cmbConditionType.SelectedItem = _partCondition.ConditionType;
        LoadExpressionsList();
    }
    
    private void ApplyChanges() {
        _partCondition.Name = txtName.Text;
        if (cmbConditionType.SelectedItem is not ConditionType selectedType) return;
        _partCondition.ConditionType = selectedType;
        if (selectedType is not (ConditionType.ConstTrue or ConditionType.ConstFalse)) return;
        _vm.RemoveElement(_partCondition.FirstExpression);
        _partCondition.FirstExpression = null;
        _vm.RemoveElement(_partCondition.SecondExpression);
        _partCondition.SecondExpression = null;
    }
}