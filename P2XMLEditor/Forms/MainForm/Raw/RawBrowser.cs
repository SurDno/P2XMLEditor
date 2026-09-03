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
            // Referrers come from a reverse index the VirtualMachine builds once, so the whole tree
            // is a series of dictionary lookups — fast enough to build eagerly. A shared visited set
            // means each element is expanded once (the first time it is reached); a later occurrence
            // is a leaf, which both bounds the tree and breaks the reference cycles.
            var budget = MaxNodes;
            AddReferences(root, item.Element, new HashSet<ulong> { item.Element.Id }, ref budget);
            _treeView.Nodes.Add(root);
            root.Expand();
        }

        _treeView.EndUpdate();
    }

    // A hub element (say a character much of the map reads) has a transitive closure of tens of
    // thousands of nodes — instant to compute but pointless to render in full — so the build stops
    // here and marks where it stopped.
    private const int MaxNodes = 20000;

    private void AddReferences(TreeNode node, VmElement target, HashSet<ulong> visited, ref int budget) {
        foreach (var r in DomainReferenceFinder.GetDirectReferences(target, _vm)) {
            if (budget <= 0) { node.Nodes.Add(new TreeNode("… (more not shown)")); return; }
            budget--;
            var name = r is INamedElement named ? named.Name : "";
            var child = new TreeNode($"{r.GetType().Name}: {r.Id} - {name}") { Tag = r };
            node.Nodes.Add(child);
            if (visited.Add(r.Id))
                AddReferences(child, r, visited, ref budget);
        }
    }
}

