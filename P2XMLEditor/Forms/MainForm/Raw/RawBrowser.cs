using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.MainForm.Raw;

public class RawBrowser : Panel {
    private readonly VirtualMachine _vm;
    private readonly Type _elementType;
    private ListBox _listBox;
    private TextBox _regexBox;
    private TreeView _treeView;
    private List<VmElement> _allElements;
    private Label _statusLabel;

    public RawBrowser(VirtualMachine vm, Type elementType) {
        _vm = vm;
        _elementType = elementType;
        Dock = DockStyle.Fill;

        SetupControls();
        LoadElements();
    }

    private void SetupControls() {
        var split = new SplitContainer {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 400
        };
        Controls.Add(split);

        var leftPanel = new Panel { Dock = DockStyle.Fill };
        
        var topPanel = new Panel { Dock = DockStyle.Top, Height = 45 };
        var lbl = new Label { Text = "Regex Search:", Dock = DockStyle.Top, Height = 15 };
        _regexBox = new TextBox { Dock = DockStyle.Top };
        _regexBox.TextChanged += (_, _) => FilterElements();
        topPanel.Controls.Add(_regexBox);
        topPanel.Controls.Add(lbl);
        
        _statusLabel = new Label { Dock = DockStyle.Bottom, Height = 20 };

        _listBox = new ListBox { Dock = DockStyle.Fill, FormattingEnabled = true };
        _listBox.SelectedIndexChanged += (_, _) => ShowReferences();
        
        leftPanel.Controls.Add(_listBox);
        leftPanel.Controls.Add(topPanel);
        leftPanel.Controls.Add(_statusLabel);

        _treeView = new TreeView {
            Dock = DockStyle.Fill,
            FullRowSelect = true,
            HideSelection = false,
            ShowLines = true,
            ShowPlusMinus = true,
        };
        _treeView.BeforeExpand += OnBeforeExpand;

        split.Panel1.Controls.Add(leftPanel);
        split.Panel2.Controls.Add(_treeView);
    }

    private void LoadElements() {
        _allElements = _vm.ElementsByType.TryGetValue(_elementType, out var list) ? list : new List<VmElement>();
        FilterElements();
    }

    private void FilterElements() {
        _listBox.BeginUpdate();
        _listBox.Items.Clear();

        Regex regex = null;
        if (!string.IsNullOrWhiteSpace(_regexBox.Text)) {
            try {
                regex = new Regex(_regexBox.Text, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            } catch {
                // Invalid regex, ignore
            }
        }

        var displayed = 0;
        foreach (var el in _allElements) {
            var name = el is INamedElement named ? named.Name : "";
            var text = $"{el.Id} - {name}";
            
            if (regex != null && !regex.IsMatch(text))
                continue;

            _listBox.Items.Add(new ListBoxItem(el, text));
            displayed++;
        }

        _listBox.EndUpdate();
        _statusLabel.Text = $"Displaying {displayed}/{_allElements.Count} items.";
    }

    private class ListBoxItem {
        public VmElement Element { get; }
        public string Text { get; }
        public ListBoxItem(VmElement el, string text) { Element = el; Text = text; }
        public override string ToString() => Text;
    }

    private void ShowReferences() {
        _treeView.BeginUpdate();
        _treeView.Nodes.Clear();

        if (_listBox.SelectedItem is ListBoxItem item) {
            var root = new TreeNode($"References to {item.Text}") { Tag = item.Element };
            PopulateChildren(root, item.Element);
            _treeView.Nodes.Add(root);
            root.Expand();
        }

        _treeView.EndUpdate();
    }

    // The reference graph is dense and cyclic — a whole VM's worth of actions, expressions and
    // links can lead back to one element — so the tree is built one level at a time and only where
    // the user looks. Each referrer that can itself have referrers is given a placeholder child so
    // it shows as expandable; the real children are computed in OnBeforeExpand when it is opened.
    // Eagerly walking the transitive closure (and re-walking every diamond) was what hung the form.
    private static readonly string Placeholder = "\0loading";

    private void OnBeforeExpand(object? sender, TreeViewCancelEventArgs e) {
        var node = e.Node;
        if (node?.Tag is not VmElement target) return;
        if (node.Nodes.Count != 1 || node.Nodes[0].Text != Placeholder) return; // already realised
        _treeView.BeginUpdate();
        node.Nodes.Clear();
        PopulateChildren(node, target);
        _treeView.EndUpdate();
    }

    private void PopulateChildren(TreeNode node, VmElement target) {
        // The chain from the root to this node — a referrer already on it is a cycle back, shown as
        // a leaf rather than expanded again.
        var ancestors = new HashSet<ulong>();
        for (var n = node; n != null; n = n.Parent)
            if (n.Tag is VmElement e) ancestors.Add(e.Id);

        foreach (var r in DomainReferenceFinder.GetDirectReferences(target, _vm)) {
            var name = r is INamedElement named ? named.Name : "";
            var child = new TreeNode($"{r.GetType().Name}: {r.Id} - {name}") { Tag = r };
            if (!ancestors.Contains(r.Id))
                child.Nodes.Add(new TreeNode(Placeholder)); // realised lazily on expand
            node.Nodes.Add(child);
        }
    }
}

