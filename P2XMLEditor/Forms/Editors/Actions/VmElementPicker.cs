using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Searchable element chooser. The corpus has ~7300 parameter holders and ~59500 parameters,
/// so the candidate list is held virtually and only the filtered slice is ever realised —
/// a plain ComboBox would stall the form on open.
/// </summary>
public sealed class VmElementPicker : Form {
	private readonly List<VmElement> _candidates;
	private readonly Func<VmElement, string> _display;
	private List<VmElement> _filtered;
	private readonly ListView _list;
	private readonly SearchControl _search;

	public VmElement? Selected { get; private set; }

	private VmElementPicker(string title, IEnumerable<VmElement> candidates, Func<VmElement, string> display,
		VmElement? current) {
		_candidates = candidates.ToList();
		_display = display;
		_filtered = _candidates;

		Text = title;
		Size = new Size(620, 520);
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		ShowInTaskbar = false;

		_list = new ListView {
			Dock = DockStyle.Fill,
			View = View.Details,
			VirtualMode = true,
			FullRowSelect = true,
			MultiSelect = false,
			HideSelection = false
		};
		_list.Columns.Add("Element", 420);
		_list.Columns.Add("Id", 150);
		_list.RetrieveVirtualItem += OnRetrieveVirtualItem;
		_list.DoubleClick += (_, _) => Accept();
		_list.KeyDown += (_, e) => {
			if (e.KeyCode == Keys.Enter) Accept();
		};

		_search = new SearchControl();
		_search.SearchChanged += (_, _) => ApplyFilter();

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom,
			FlowDirection = FlowDirection.RightToLeft,
			Height = 40,
			Padding = new Padding(5)
		};
		var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };
		var ok = new Button { Text = "Select" };
		ok.Click += (_, _) => Accept();
		var clear = new Button { Text = "Clear", Width = 80 };
		clear.Click += (_, _) => {
			Selected = null;
			DialogResult = DialogResult.OK;
			Close();
		};
		buttons.Controls.AddRange([cancel, ok, clear]);

		AcceptButton = ok;
		CancelButton = cancel;

		Controls.Add(_list);
		Controls.Add(buttons);
		Controls.Add(_search);

		ApplyFilter();
		if (current != null) SelectElement(current);
	}

	/// <summary>Returns true when the user confirmed; <paramref name="result"/> is null if they cleared.</summary>
	public static bool TryPick(IWin32Window? owner, string title, IEnumerable<VmElement> candidates,
		Func<VmElement, string> display, VmElement? current, out VmElement? result) {
		using var picker = new VmElementPicker(title, candidates, display, current);
		if (picker.ShowDialog(owner) != DialogResult.OK) {
			result = null;
			return false;
		}
		result = picker.Selected;
		return true;
	}

	/// <summary>Human-readable label for an element, falling back to type and id.</summary>
	public static string Describe(VmElement? element) {
		if (element == null) return "";
		var name = element switch {
			INamedElement named => named.Name,
			Parameter p => p.Name,
			_ => null
		};
		var owner = element switch {
			Parameter { Parent.Element: ParameterHolder holder } => holder.Name,
			_ => null
		};
		var label = string.IsNullOrEmpty(name) ? element.GetType().Name : name;
		if (!string.IsNullOrEmpty(owner)) label = $"{owner}.{label}";
		return $"{label}  [{element.GetType().Name} {element.Id}]";
	}

	private void OnRetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) {
		if (e.ItemIndex < 0 || e.ItemIndex >= _filtered.Count) {
			e.Item = new ListViewItem("");
			return;
		}
		var element = _filtered[e.ItemIndex];
		var item = new ListViewItem(_display(element)) { Tag = element };
		item.SubItems.Add(element.Id.ToString());
		e.Item = item;
	}

	private void ApplyFilter() {
		_filtered = string.IsNullOrEmpty(_search.SearchText)
			? _candidates
			: _candidates.Where(c => _search.IsMatchAny(_display(c), c.Id.ToString())).ToList();

		_list.VirtualListSize = _filtered.Count;
		_list.Invalidate();
		_search.StatusText = $"{_filtered.Count}/{_candidates.Count}";
	}

	private void SelectElement(VmElement element) {
		var index = _filtered.IndexOf(element);
		if (index < 0) return;
		_list.SelectedIndices.Clear();
		_list.SelectedIndices.Add(index);
		_list.EnsureVisible(index);
	}

	private void Accept() {
		if (_list.SelectedIndices.Count == 0) return;
		var index = _list.SelectedIndices[0];
		if (index < 0 || index >= _filtered.Count) return;
		Selected = _filtered[index];
		DialogResult = DialogResult.OK;
		Close();
	}
}
