using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// Searchable element chooser. The corpus has ~7300 parameter holders and ~59500 parameters,
/// so the candidate list is held virtually and only the filtered slice is ever realised — a
/// plain ComboBox would stall the form on open.
///
/// A type dropdown narrows the list to one kind of holder; on "All" the kinds stay visible as
/// bold separator rows, so scrolling through everything still reads as grouped.
/// </summary>
public sealed class VmElementPicker : Form {
	private const string AllTypes = "All";

	private readonly List<VmElement> _candidates;
	private readonly Func<VmElement, string> _display;
	private readonly Func<VmElement, string?>? _note;
	private readonly ListView _list;
	private readonly SearchControl _search;
	private readonly ComboBox _typeFilter;
	private readonly Font _headerFont;

	private List<Row> _rows = [];

	public VmElement? Selected { get; private set; }

	/// <summary>A list line: either a selectable element or a bold type separator.</summary>
	private readonly record struct Row(VmElement? Element, string Text, bool IsHeader);

	private VmElementPicker(string title, IEnumerable<VmElement> candidates, Func<VmElement, string> display,
		VmElement? current, Func<VmElement, string?>? note) {
		_candidates = candidates.ToList();
		// Callers narrow the list to what is worth authoring; what the element already says is
		// not always in that set, and dropping it would mean the current value is invisible
		// here and silently replaced by whatever the user picks instead.
		if (current != null && !_candidates.Contains(current)) _candidates.Insert(0, current);
		_display = display;
		_note = note;

		Text = title;
		Size = new Size(980, 620);
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
		_list.Columns.Add("Element", note == null ? 600 : 380);
		_list.Columns.Add("Id", note == null ? 320 : 200);
		if (note != null) _list.Columns.Add("Note", 360);
		_list.RetrieveVirtualItem += OnRetrieveVirtualItem;
		_list.DoubleClick += (_, _) => Accept();
		_headerFont = new Font(_list.Font, FontStyle.Bold);

		_typeFilter = new ComboBox {
			DropDownStyle = ComboBoxStyle.DropDownList, IntegralHeight = false,
			Location = new Point(10, 8), Width = 220
		};
		foreach (var name in TypeNames()) _typeFilter.Items.Add(name);
		_typeFilter.SelectedIndex = 0;
		_typeFilter.SelectedIndexChanged += (_, _) => ApplyFilter();

		var typeRow = new Panel { Dock = DockStyle.Top, Height = 38 };
		typeRow.Controls.Add(new Label {
			Text = "Type:", Location = new Point(10, 12), Size = new Size(45, 20),
			TextAlign = ContentAlignment.MiddleLeft
		});
		_typeFilter.Location = new Point(60, 8);
		typeRow.Controls.Add(_typeFilter);
		// With a single kind on offer the dropdown would only ever say "All".
		typeRow.Visible = _typeFilter.Items.Count > 2;

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
		Controls.Add(typeRow);
		Controls.Add(_search);

		ApplyFilter();
		if (current != null) SelectElement(current);
	}

	/// <summary>
	/// Returns true when the user confirmed; <paramref name="result"/> is null if they cleared.
	/// <paramref name="note"/> annotates a row where the choice carries a caveat worth knowing
	/// before it is made rather than after.
	/// </summary>
	public static bool TryPick(IWin32Window? owner, string title, IEnumerable<VmElement> candidates,
		Func<VmElement, string> display, VmElement? current, out VmElement? result,
		Func<VmElement, string?>? note = null) {
		using var picker = new VmElementPicker(title, candidates, display, current, note);
		if (picker.ShowDialog(owner) != DialogResult.OK) {
			result = null;
			return false;
		}
		result = picker.Selected;
		return true;
	}

	/// <summary>
	/// The element's name, with its owner for a parameter. Deliberately just the name: in the
	/// list the type is already the group header and the filter above it, and the id has its
	/// own column, so repeating both inside the name only crowds it out.
	/// </summary>
	public static string Describe(VmElement? element, VirtualMachine? vm = null) {
		if (element == null) return "";

		// A Sample carries no name of its own — only the engine GUID of the template it stands
		// for, which is where the name lives.
		if (element is Sample sample && TemplateName(sample, vm) is { } templateName)
			return templateName;

		var name = element switch {
			INamedElement named => named.Name,
			Parameter p => p.Name,
			_ => null
		};
		var label = string.IsNullOrEmpty(name) ? element.GetType().Name : name;
		return element is Parameter { Parent.Element: ParameterHolder owner } ? $"{owner.Name}.{label}" : label;
	}

	private static string? TemplateName(Sample sample, VirtualMachine? vm) {
		if (vm == null || string.IsNullOrEmpty(sample.EngineId)) return null;
		if (!Guid.TryParse(sample.EngineId, out var id)) return null;
		return vm.TemplateManagerInst.Templates.TryGetValue(id, out var template) &&
			   !string.IsNullOrEmpty(template.Name)
			? template.Name
			: null;
	}

	/// <summary>
	/// Name plus type and id, for the read-only boxes that show a chosen element on their own
	/// with no columns around them to say which one it is.
	/// </summary>
	public static string DescribeDetailed(VmElement? element, VirtualMachine? vm = null) =>
		element == null ? "" : $"{Describe(element, vm)}  [{element.GetType().Name} {element.Id}]";

	private IEnumerable<string> TypeNames() =>
		_candidates.Select(c => c.GetType().Name).Distinct(StringComparer.Ordinal)
			.OrderBy(n => n, StringComparer.Ordinal)
			.Prepend(AllTypes);

	private void OnRetrieveVirtualItem(object? sender, RetrieveVirtualItemEventArgs e) {
		if (e.ItemIndex < 0 || e.ItemIndex >= _rows.Count) {
			e.Item = new ListViewItem("");
			return;
		}

		var row = _rows[e.ItemIndex];
		var item = new ListViewItem(row.Text) { Tag = row.Element };
		if (row.IsHeader) {
			item.Font = _headerFont;
			item.SubItems.Add("");
		} else {
			item.SubItems.Add(row.Element!.Id.ToString());
		}
		if (_note != null) item.SubItems.Add(row.IsHeader ? "" : _note(row.Element!) ?? "");
		e.Item = item;
	}

	private void ApplyFilter() {
		var type = _typeFilter.SelectedItem as string ?? AllTypes;
		var matching = _candidates
			.Where(c => type == AllTypes || c.GetType().Name == type)
			// The note is searchable too, so "placed" narrows the list to the objects where
			// choosing the id over a hierarchy path is the decision being made.
			.Where(c => _search.IsMatchAny(_display(c), c.Id.ToString(), _note?.Invoke(c) ?? ""))
			.ToList();

		_rows = new List<Row>(matching.Count + 16);
		if (type == AllTypes) {
			// Grouped, with the kind called out, so a long scroll through everything still
			// reads as sections rather than one undifferentiated list.
			foreach (var group in matching.GroupBy(c => c.GetType().Name).OrderBy(g => g.Key, StringComparer.Ordinal)) {
				_rows.Add(new Row(null, $"── {group.Key} ({group.Count()}) ──", true));
				foreach (var element in group.OrderBy(_display, StringComparer.Ordinal))
					_rows.Add(new Row(element, _display(element), false));
			}
		} else {
			foreach (var element in matching.OrderBy(_display, StringComparer.Ordinal))
				_rows.Add(new Row(element, _display(element), false));
		}

		_list.VirtualListSize = _rows.Count;
		_list.Invalidate();
		_search.StatusText = $"{matching.Count}/{_candidates.Count}";
	}

	private void SelectElement(VmElement element) {
		var index = _rows.FindIndex(r => ReferenceEquals(r.Element, element));
		if (index < 0) return;
		_list.SelectedIndices.Clear();
		_list.SelectedIndices.Add(index);
		_list.EnsureVisible(index);
	}

	private void Accept() {
		if (_list.SelectedIndices.Count == 0) return;
		var index = _list.SelectedIndices[0];
		if (index < 0 || index >= _rows.Count) return;

		// A separator is a label, not a choice.
		var row = _rows[index];
		if (row.IsHeader || row.Element == null) return;

		Selected = row.Element;
		DialogResult = DialogResult.OK;
	}

	protected override void Dispose(bool disposing) {
		if (disposing) _headerFont.Dispose();
		base.Dispose(disposing);
	}
}
