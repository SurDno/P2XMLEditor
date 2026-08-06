using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements.Helper;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Picks a spot in the built world rather than an object.
///
/// The distinction matters because a hierarchy guid names a placement, not a template: a scene
/// mounted in nineteen houses is one object with nineteen paths, and picking the object leaves
/// the question of which one unanswered. So the list is one row per placement, and what comes
/// back is a path <see cref="WorldHierarchy"/> says the engine's builder actually produces.
///
/// Held virtually for the same reason as <see cref="VmElementPicker"/> — the Sandbox has ~70k
/// placements — but the row text is built once up front, since filtering rebuilds it otherwise
/// on every keystroke.
/// </summary>
public sealed class HierarchyPicker : Form {
	private readonly List<Row> _all;
	private readonly ListView _list;
	private readonly SearchControl _search;

	private List<Row> _rows;

	public ulong[]? Selected { get; private set; }

	private sealed record Row(ulong[] Path, string Leaf, string Where, string Written);

	private HierarchyPicker(VirtualMachine vm, string title, ulong[]? current) {
		var world = WorldHierarchy.For(vm);
		_all = BuildRows(vm, world);
		_rows = _all;

		Text = title;
		Size = new Size(1100, 640);
		StartPosition = FormStartPosition.CenterParent;
		MinimizeBox = false;
		ShowInTaskbar = false;

		_list = new ListView {
			Dock = DockStyle.Fill, View = View.Details, VirtualMode = true,
			FullRowSelect = true, MultiSelect = false, HideSelection = false
		};
		_list.Columns.Add("Object", 260);
		_list.Columns.Add("Where", 520);
		_list.Columns.Add("Path", 280);
		_list.RetrieveVirtualItem += OnRetrieveVirtualItem;
		_list.DoubleClick += (_, _) => Accept();

		_search = new SearchControl();
		_search.SearchChanged += (_, _) => ApplyFilter();

		var buttons = new FlowLayoutPanel {
			Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 50,
			Padding = new Padding(10, 8, 10, 8)
		};
		var cancel = new Button { Text = "Cancel", Size = new Size(100, 32), DialogResult = DialogResult.Cancel };
		var ok = new Button { Text = "Select", Size = new Size(100, 32), Margin = new Padding(8, 0, 0, 0) };
		ok.Click += (_, _) => Accept();
		var clear = new Button { Text = "Clear", Size = new Size(100, 32), Margin = new Padding(8, 0, 0, 0) };
		clear.Click += (_, _) => {
			Selected = null;
			DialogResult = DialogResult.OK;
		};
		buttons.Controls.AddRange([cancel, ok, clear]);

		AcceptButton = ok;
		CancelButton = cancel;

		Controls.Add(_list);
		Controls.Add(buttons);
		Controls.Add(_search);

		ApplyFilter();
		if (current != null) SelectPath(current);
	}

	/// <summary>Returns true when the user confirmed; <paramref name="result"/> is null if they cleared.</summary>
	public static bool TryPick(IWin32Window? owner, VirtualMachine vm, string title, HierarchyGuid? current,
		out HierarchyGuid? result) {
		var currentPath = current?.Elements.Select(e => e.Id).ToArray();
		using var picker = new HierarchyPicker(vm, title, currentPath);
		result = null;
		if (picker.ShowDialog(owner) != DialogResult.OK) return false;
		if (picker.Selected == null) return true;

		HierarchyGuid.TryParse(string.Join("H", picker.Selected), vm, out result);
		return true;
	}

	/// <summary>
	/// One row per placement, ordered by object name so the list reads like the flat picker.
	/// Single-id placements are left out: on the wire a lone id is read as a base guid, never a
	/// hierarchy, so there would be no way to spell what the row stands for.
	/// </summary>
	private static List<Row> BuildRows(VirtualMachine vm, WorldHierarchy world) {
		var names = new Dictionary<ulong, string>();

		string NameOf(ulong id) {
			if (names.TryGetValue(id, out var cached)) return cached;
			var element = vm.GetNullableElement(id);
			// Most of the world exists only in the engine's own data — 14102 of the Sandbox's
			// 20459 placed objects have no element here, and stand in as placeholders whose
			// name would read "ScenePlaceholder" for every one of them. The id is all there is
			// to tell them apart, and the data does reference them, so they stay pickable.
			var name = element is null or IPlaceholder ? null : VmElementPicker.Describe(element, vm);
			return names[id] = string.IsNullOrEmpty(name) ? id.ToString() : name;
		}

		var rows = new List<Row>(world.AllPlacements.Count);
		foreach (var placement in world.AllPlacements) {
			if (placement.Path.Length < 2) continue;
			rows.Add(new Row(
				placement.Path,
				NameOf(placement.LeafId),
				string.Join(" → ", placement.Path[..^1].Select(NameOf)),
				placement.Write()));
		}

		rows.Sort((a, b) => {
			var byLeaf = string.CompareOrdinal(a.Leaf, b.Leaf);
			return byLeaf != 0 ? byLeaf : string.CompareOrdinal(a.Where, b.Where);
		});
		return rows;
	}

	private void OnRetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) {
		if (e.ItemIndex < 0 || e.ItemIndex >= _rows.Count) {
			e.Item = new ListViewItem("");
			return;
		}
		var row = _rows[e.ItemIndex];
		var item = new ListViewItem(row.Leaf);
		item.SubItems.Add(row.Where);
		item.SubItems.Add(row.Written);
		e.Item = item;
	}

	private void ApplyFilter() {
		_rows = string.IsNullOrEmpty(_search.SearchText)
			? _all
			: _all.Where(r => _search.IsMatchAny(r.Leaf, r.Where, r.Written)).ToList();

		_list.VirtualListSize = _rows.Count;
		_list.Invalidate();
		_search.StatusText = $"{_rows.Count}/{_all.Count} placements";
	}

	private void SelectPath(ulong[] path) {
		var written = string.Join("H", path);
		var index = _rows.FindIndex(r => r.Written == written);
		if (index < 0) return;
		_list.SelectedIndices.Clear();
		_list.SelectedIndices.Add(index);
		_list.EnsureVisible(index);
	}

	private void Accept() {
		if (_list.SelectedIndices.Count == 0) return;
		var index = _list.SelectedIndices[0];
		if (index < 0 || index >= _rows.Count) return;
		Selected = _rows[index].Path;
		DialogResult = DialogResult.OK;
	}
}
