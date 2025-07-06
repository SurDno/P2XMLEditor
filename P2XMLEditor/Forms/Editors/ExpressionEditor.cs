using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.Editors;

public class ExpressionEditorForm : Form {
    private readonly VirtualMachine _vm;
    private readonly Expression _expression;

    private ComboBox cmbExpressionType;
    private Panel pnlParamEdit;
    private CheckBox chkDirectEdit;
    private TextBox txtTargetObject;
    private TextBox txtTargetParam;
    private ComboBox cmbCategory;
    private ComboBox cmbHolder;
    private ComboBox cmbParameter;
    private Panel pnlFunction;
    private TextBox txtFuncTargetObject;
    private ComboBox cmbFunction;
    private ListBox lstFunctionParams;
    private Button btnAddParam;
    private Button btnRemoveParam;
    private Panel pnlConst;
    private TextBox txtConstType;
    private TextBox txtConstValue;
    private Panel pnlComplex;
    private ListBox lstFormulaChilds;
    private ComboBox cmbOperations;
    private Button btnAddChild;
    private Button btnRemoveChild;
    private Button btnSave;
    private Button btnCancel;

    private static readonly string[] Categories = ["Item", "Blueprint", "Quest", "Character", "Scene", "Geom", "Other"];

    public ExpressionEditorForm(VirtualMachine vm, Expression expression) {
        _vm = vm;
        _expression = expression;
        InitializeComponents();
        LoadData();
    }

    private void InitializeComponents() {
        Text = "Expression Editor";
        Size = new Size(800, 600);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var mainLayout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            Padding = new Padding(10),
            RowCount = 3,
            ColumnCount = 2
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        mainLayout.Controls.Add(new Label {
            Text = "Expression Type:",
            TextAlign = ContentAlignment.MiddleLeft
        }, 0, 0);

        cmbExpressionType = new ComboBox {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        mainLayout.Controls.Add(cmbExpressionType, 1, 0);

        pnlParamEdit = new Panel {
            Dock = DockStyle.Fill,
            Visible = false
        };
        InitializeParamEditPanel();
        mainLayout.Controls.Add(pnlParamEdit, 1, 1);

        pnlFunction = new Panel {
            Dock = DockStyle.Fill,
            Visible = false
        };
        InitializeFunctionPanel();
        mainLayout.Controls.Add(pnlFunction, 1, 1);

        pnlConst = new Panel {
            Dock = DockStyle.Fill,
            Visible = false
        };
        InitializeConstPanel();
        mainLayout.Controls.Add(pnlConst, 1, 1);

        pnlComplex = new Panel {
            Dock = DockStyle.Fill,
            Visible = false
        };
        InitializeComplexPanel();
        mainLayout.Controls.Add(pnlComplex, 1, 1);

        var buttonPanel = new FlowLayoutPanel {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            Height = 40
        };

        btnCancel = new Button {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel
        };
        btnSave = new Button {
            Text = "Save"
        };

        buttonPanel.Controls.AddRange([btnCancel, btnSave]);
        Controls.Add(buttonPanel);
        Controls.Add(mainLayout);

        cmbExpressionType.SelectedIndexChanged += (_, _) => UpdateUI();
        btnSave.Click += Save_Click;
    }

    private void InitializeParamEditPanel() {
        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 6, ColumnCount = 2,
            AutoSize = true
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        for (var i = 0; i < layout.RowCount; i++) {
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        }

        chkDirectEdit = new CheckBox {
            Text = "Direct Edit",
            Checked = true,
            AutoSize = true
        };
        layout.Controls.Add(chkDirectEdit, 0, 0);
        layout.SetColumnSpan(chkDirectEdit, 2);

        txtTargetObject = new TextBox {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };
        txtTargetParam = new TextBox {
            Dock = DockStyle.Fill,
            Margin = new Padding(3)
        };

        layout.Controls.Add(new Label {
            Text = "Target Object:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        }, 0, 1);
        layout.Controls.Add(txtTargetObject, 1, 1);
        layout.Controls.Add(new Label {
            Text = "Target Parameter:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        }, 0, 2);
        layout.Controls.Add(txtTargetParam, 1, 2);

        cmbCategory = new ComboBox {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        cmbHolder = new ComboBox {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };
        cmbParameter = new ComboBox {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(3)
        };

        layout.Controls.Add(new Label {
            Text = "Category:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        }, 0, 3);
        layout.Controls.Add(cmbCategory, 1, 3);
        layout.Controls.Add(new Label {
            Text = "Holder:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        }, 0, 4);
        layout.Controls.Add(cmbHolder, 1, 4);
        layout.Controls.Add(new Label {
            Text = "Parameter:",
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        }, 0, 5);
        layout.Controls.Add(cmbParameter, 1, 5);

        pnlParamEdit.Controls.Add(layout);

        chkDirectEdit.CheckedChanged += (_, _) => UpdateParamEditMode();
        cmbCategory.SelectedIndexChanged += (_, _) => UpdateHolderCombo();
        cmbHolder.SelectedIndexChanged += (_, _) => UpdateParameterCombo();
    }

    private void InitializeFunctionPanel() {
        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 4,
            ColumnCount = 2
        };

        txtFuncTargetObject = new TextBox { Dock = DockStyle.Fill };
        cmbFunction = new ComboBox {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        lstFunctionParams = new ListBox { Dock = DockStyle.Fill };

        var paramButtons = new FlowLayoutPanel {
            FlowDirection = FlowDirection.LeftToRight
        };
        btnAddParam = new Button { Text = "Add" };
        btnRemoveParam = new Button { Text = "Remove" };
        paramButtons.Controls.AddRange([btnAddParam, btnRemoveParam]);

        layout.Controls.Add(new Label { Text = "Target Object:" }, 0, 0);
        layout.Controls.Add(txtFuncTargetObject, 1, 0);
        layout.Controls.Add(new Label { Text = "Function:" }, 0, 1);
        layout.Controls.Add(cmbFunction, 1, 1);
        layout.Controls.Add(new Label { Text = "Parameters:" }, 0, 2);
        layout.Controls.Add(lstFunctionParams, 1, 2);
        layout.Controls.Add(paramButtons, 1, 3);

        pnlFunction.Controls.Add(layout);

        btnAddParam.Click += (_, _) => lstFunctionParams.Items.Add("");
        btnRemoveParam.Click += (_, _) => {
            if (lstFunctionParams.SelectedIndex != -1)
                lstFunctionParams.Items.RemoveAt(lstFunctionParams.SelectedIndex);
        };
    }

    private void InitializeConstPanel() {
        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 2
        };

        txtConstType = new TextBox { Dock = DockStyle.Fill };
        txtConstValue = new TextBox { Dock = DockStyle.Fill };

        layout.Controls.Add(new Label { Text = "Type:" }, 0, 0);
        layout.Controls.Add(txtConstType, 1, 0);
        layout.Controls.Add(new Label { Text = "Value:" }, 0, 1);
        layout.Controls.Add(txtConstValue, 1, 1);

        pnlConst.Controls.Add(layout);
    }

    private void InitializeComplexPanel() {
        var layout = new TableLayoutPanel {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 2
        };

        lstFormulaChilds = new ListBox { Dock = DockStyle.Fill };
        cmbOperations = new ComboBox {
            Dock = DockStyle.Fill,
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        var buttons = new FlowLayoutPanel {
            FlowDirection = FlowDirection.LeftToRight
        };
        btnAddChild = new Button { Text = "Add Child" };
        btnRemoveChild = new Button { Text = "Remove Child" };
        buttons.Controls.AddRange([btnAddChild, btnRemoveChild]);

        layout.Controls.Add(new Label { Text = "Children:" }, 0, 0);
        layout.Controls.Add(lstFormulaChilds, 1, 0);
        layout.Controls.Add(new Label { Text = "Operation:" }, 0, 1);
        layout.Controls.Add(cmbOperations, 1, 1);
        layout.Controls.Add(buttons, 1, 2);

        pnlComplex.Controls.Add(layout);

        btnAddChild.Click += (_, _) => {
            var newChild = VmElement.CreateDefault<Expression>(_vm, _expression);
            if (_expression.FormulaChilds == null)
                _expression.FormulaChilds = [];
            if (_expression.FormulaOperations == null)
                _expression.FormulaOperations = [];

            _expression.FormulaChilds.Add(newChild);
            _expression.FormulaOperations.Add(FormulaOperation.Plus);
            lstFormulaChilds.Items.Add(newChild);
        };

        btnRemoveChild.Click += (_, _) => {
            if (lstFormulaChilds.SelectedIndex != -1) {
                var index = lstFormulaChilds.SelectedIndex;
                _expression.FormulaChilds?.RemoveAt(index);
                _expression.FormulaOperations?.RemoveAt(index);
                lstFormulaChilds.Items.RemoveAt(index);
            }
        };
    }

    private void LoadData() {
        cmbExpressionType.Items.AddRange(Enum.GetValues<ExpressionType>().Cast<object>().ToArray());
        cmbExpressionType.SelectedItem = _expression.ExpressionType;

        cmbCategory.Items.AddRange(Categories);
        foreach (var name in VmFunction.GetAvailableFunctions())
            cmbFunction.Items.Add(name);
        cmbOperations.Items.AddRange(Enum.GetValues<FormulaOperation>().Cast<object>().ToArray());

        if (_expression.Function != null)
            cmbFunction.SelectedItem = _expression.Function.Name;

        txtTargetObject.Text = _expression.TargetObject.Write();
        txtTargetParam.Text = _expression.TargetParam.Write();

        if (_expression.Const != null) {
            txtConstType.Text = _expression.Const.Type;
            txtConstValue.Text = _expression.Const.Value;
        }

        if (_expression.FormulaChilds != null) {
            foreach (var child in _expression.FormulaChilds)
                lstFormulaChilds.Items.Add(child);
        }

        if (_expression.FormulaOperations?.Count > 0)
            cmbOperations.SelectedItem = _expression.FormulaOperations[0];

        UpdateUI();
    }

    private void UpdateUI() {
        var type = (ExpressionType)cmbExpressionType.SelectedItem;

        pnlParamEdit.Visible = type == ExpressionType.Param;
        pnlFunction.Visible = type == ExpressionType.Function;
        pnlConst.Visible = type == ExpressionType.Const;
        pnlComplex.Visible = type == ExpressionType.Complex;

        if (type == ExpressionType.Param)
            UpdateParamEditMode();
    }

    private void UpdateParamEditMode() {
        var directMode = chkDirectEdit.Checked;
        txtTargetObject.Visible = txtTargetParam.Visible = directMode;
        cmbCategory.Visible = cmbHolder.Visible = cmbParameter.Visible = !directMode;
        if (directMode) return;

        try {
            var targetObj = CommonVariable.Read(txtTargetObject.Text, _vm);
            var targetParam = CommonVariable.Read(txtTargetParam.Text, _vm);
            if (targetObj.ContextParameter is not ParameterHolder ph) return;
            cmbCategory.SelectedItem = ph.GetType().Name;
            cmbHolder.SelectedItem = cmbHolder.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Value == ph);
            if (targetParam.VariableParameter is not Parameter p) return;
            cmbParameter.SelectedItem = cmbParameter.Items.Cast<ComboBoxItem>().FirstOrDefault(i => i.Value == p);
        } catch {
            Logger.LogWarning("Current values of TargetObject and TargetParam were not parsed.");
        }
    }

    private void UpdateHolderCombo() {
        cmbHolder.Items.Clear();
        cmbParameter.Items.Clear();

        if (cmbCategory.SelectedItem is not string category) return;
        var holders = _vm.GetElementsByType<ParameterHolder>().Where(h => category switch {
            "Item" => h is Item,
            "Blueprint" => h is Blueprint,
            "Quest" => h is Quest,
            "Character" => h is Character,
            "Scene" => h is Scene,
            "Geom" => h is Geom,
            "Other" => h is Other,
            _ => throw new ArgumentOutOfRangeException()
        }).ToList();

        foreach (var holder in holders)
            cmbHolder.Items.Add(new ComboBoxItem(holder.Name, holder));
    }

    private void UpdateParameterCombo() {
        cmbParameter.Items.Clear();

        if (cmbHolder.SelectedItem is not ComboBoxItem { Value: ParameterHolder holder }) return;
        foreach (var param in holder.StandartParams)
            cmbParameter.Items.Add(new ComboBoxItem(param.Key, param.Value));
        foreach (var param in holder.CustomParams)
            cmbParameter.Items.Add(new ComboBoxItem(param.Key, param.Value));
    }

    private void CleanupUnusedConst() {
        if (_expression.Const == null || _expression.ExpressionType == ExpressionType.Const) return;
        _vm.RemoveElement(_expression.Const);
        _expression.Const = null;
    }

    private void Save_Click(object sender, EventArgs e) {
        _expression.ExpressionType = (ExpressionType)(cmbExpressionType.SelectedItem ?? throw new Exception());

        switch (_expression.ExpressionType) {
            case ExpressionType.Param:
                if (chkDirectEdit.Checked) {
                    _expression.TargetObject = CommonVariable.Read(txtTargetObject.Text, _vm);
                    _expression.TargetParam = CommonVariable.Read(txtTargetParam.Text, _vm);
                } else if (cmbHolder.SelectedItem is ComboBoxItem holderItem &&
                           cmbParameter.SelectedItem is ComboBoxItem paramItem) {
                    _expression.TargetObject = CommonVariable.Read(((ParameterHolder)holderItem.Value).Id, _vm);
                    _expression.TargetParam = CommonVariable.Read($"%{((Parameter)paramItem.Value).Id}", _vm);
                }

                break;
            case ExpressionType.Function:
                if (cmbFunction.SelectedItem is string funcName) {
                    _expression.TargetObject = CommonVariable.Read(txtFuncTargetObject.Text, _vm);
                    var parameters = lstFunctionParams.Items.Cast<string>().ToList();
                    _expression.Function = VmFunction.GetFunction(funcName, _vm, parameters);
                }
                break;
            case ExpressionType.Const:
                _expression.Const ??= VmElement.CreateDefault<Parameter>(_vm, _expression);
                _expression.Const.Type = txtConstType.Text;
                _expression.Const.Value = txtConstValue.Text;
                break;
            case ExpressionType.Complex:
            default:
                break;
        }
        
        CleanupUnusedConst();

        DialogResult = DialogResult.OK;
        Close();
    }

    private class ComboBoxItem(string text, object value) {
        public string Text { get; } = text;
        public object Value { get; } = value;

        public override string ToString() => Text;
    }
}