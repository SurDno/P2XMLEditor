using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
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
        Height = 400;

        cmbOperation = new ComboBox { Dock = DockStyle.Top, Enabled = false };
        UpdateOperationComboBox();

        lstPredicates = new ListBox { Dock = DockStyle.Fill };
        lstPredicates.DoubleClick += (_, _) => {
            switch (_condition.Predicates[lstPredicates.SelectedIndex].Element) {
                case PartCondition partCond: {
                    using var partEditor = new PartConditionEditorForm(_vm, partCond, _localContext);
                    partEditor.ShowDialog();
                    break;
                }
                case Condition nestedCondition: {
                    using var condEditor = new ConditionEditorForm(_vm, nestedCondition, _localContext);
                    condEditor.ShowDialog();
                    break;
                }
            }
            LoadData();
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

        Controls.Add(lstPredicates);
        Controls.Add(topPanel);
        Controls.Add(btnOK);
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
        lstPredicates.Items.Clear();
        foreach (var pred in _condition.Predicates) 
            lstPredicates.Items.Add(PreviewHelper.Preview(pred.Element));
        
        if (lstPredicates.Items.Count > 0 && lstPredicates.SelectedIndex < 0)
            lstPredicates.SelectedIndex = 0;

        UpdateOperationComboBox();
        UpdatePredicateOrderIndices();
    }

    private void BtnOK_Click(object sender, EventArgs e) {
        _condition.Operation = _condition.Predicates.Count <= 1 ? ConditionOperation.Root : (ConditionOperation)cmbOperation.SelectedItem!;

        UpdatePredicateOrderIndices();
        DialogResult = DialogResult.OK;
        Close();
    }
}