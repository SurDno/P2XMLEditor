using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.WindowsFormsExtensions;

namespace P2XMLEditor.Forms.MainForm.Hierarchy;

public class HierarchyBrowser : Panel {
	private readonly VirtualMachine _vm;
	private TreeView _treeView;
	private SearchControl _searchControl;
	private ContextMenuStrip _contextMenu;

	[PerformanceLogHook]
	public HierarchyBrowser(VirtualMachine vm) {
		var methodFromHandle = MethodBase.GetCurrentMethod()!;
		var customAttributes = Attribute.GetCustomAttributes(methodFromHandle, typeof(PerformanceLogHookAttribute));
		var performanceLogHookAttribute =
			(PerformanceLogHookAttribute)customAttributes[(nint)customAttributes.LongLength - 1];
		performanceLogHookAttribute.Init(null, methodFromHandle, [vm]);
		performanceLogHookAttribute.OnEntry();
		try {
			_vm = vm;
			Dock = DockStyle.Fill;
			SetupControls();
			LoadHierarchy();
			performanceLogHookAttribute.OnExit();
		} catch (Exception exception) {
			performanceLogHookAttribute.OnException(exception);
			throw;
		}
	}

	private void SetupControls() {
		_searchControl = new SearchControl { Dock = DockStyle.Top };
		_searchControl.SearchChanged += delegate { LoadHierarchy(); };
		_treeView = new TreeView {
			Location = new Point(10, 45),
			Size = new Size(Width - 20, Height - 55),
			Anchor = (AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right),
			FullRowSelect = true,
			HideSelection = false,
			ShowLines = true,
			ShowPlusMinus = true,
			ShowRootLines = true,
			BorderStyle = BorderStyle.None,
			BackColor = Color.FromArgb(250, 250, 250),
			Font = new Font("Segoe UI", 9f)
		};
		_treeView.NodeMouseDoubleClick += OnNodeDoubleClick;
		SetupContextMenu();
		_treeView.ContextMenuStrip = _contextMenu;
		Controls.AddRange([_searchControl, _treeView]);
	}

	private void SetupContextMenu() {
		_contextMenu = new ContextMenuStrip();
		var toolStripMenuItem = new ToolStripMenuItem("Expand All");
		toolStripMenuItem.Click += delegate { _treeView.ExpandAll(); };
		var toolStripMenuItem2 = new ToolStripMenuItem("Collapse All");
		toolStripMenuItem2.Click += delegate { _treeView.CollapseAll(); };
		_contextMenu.Items.AddRange([toolStripMenuItem, toolStripMenuItem2]);
	}

	private GameRoot GetGameRoot() =>
		_vm.GetElementsByType<GameRoot>().FirstOrDefault() ?? throw new Exception("GameRoot not found");

	private void LoadHierarchy() {
		var rawHierarchy = GetGameRoot().HierarchyScenesStructure;
		if (rawHierarchy == null) {
			_searchControl.StatusText = "No hierarchy data found.";
			return;
		}

		VmEither<Geom, Other, Scene, ScenePlaceholder, TemplatePlaceholder> GetOrCreate(ulong id) {
			var nullableElement = _vm.GetNullableElement<Geom, Other, Scene, ScenePlaceholder, TemplatePlaceholder>(id);
			if (nullableElement.HasValue) {
				return nullableElement.Value;
			}

			if (GetGameRoot().BaseToEngineGuidsTable != null &&
			    GetGameRoot().BaseToEngineGuidsTable.ContainsKey(id.ToString())) {
				return new VmEither<Geom, Other, Scene, ScenePlaceholder, TemplatePlaceholder>(
					_vm.Register(new TemplatePlaceholder(id)));
			}

			return new VmEither<Geom, Other, Scene, ScenePlaceholder, TemplatePlaceholder>(
				_vm.Register(new ScenePlaceholder(id)));
		}

		var hierarchyScenesStructure = rawHierarchy.ToDictionary(
			kv => GetOrCreate(kv.Key),
			kv => kv.Value.ToDictionary(
				ckv => ckv.Key,
				ckv => ckv.Value.Select(GetOrCreate).ToArray()
			)
		);
		_treeView.BeginUpdate();
		_treeView.Nodes.Clear();
		HashSet<ulong> allChildren = [];
		foreach (var value in hierarchyScenesStructure.Values) {
			foreach (var value2 in value.Values) {
				foreach (var vmEither in value2) {
					allChildren.Add(vmEither.Id);
				}
			}
		}

		var list = hierarchyScenesStructure.Keys.Where(k => !allChildren.Contains(k.Id)).ToList();
		var num = 0;
		List<TreeNode> list2 = [];
		foreach (var item in list) {
			var treeNode = BuildNodeIfVisible(item, hierarchyScenesStructure);
			if (treeNode != null) {
				list2.Add(treeNode);
				num++;
			}
		}

		_treeView.Nodes.AddRange(list2.ToArray());
		_treeView.EndUpdate();
		_searchControl.StatusText = $"Displaying {num} root nodes.";
	}

	private TreeNode? BuildNodeIfVisible(VmEither<Geom, Other, Scene, ScenePlaceholder, TemplatePlaceholder> element,
		Dictionary<VmEither<Geom, Other, Scene, ScenePlaceholder, TemplatePlaceholder>, Dictionary<ChildContainerType,
			VmEither<Geom, Other, Scene, ScenePlaceholder, TemplatePlaceholder>[]>> structure) {
		var elementName = GetElementName(element.Element);
		var text = element.Id.ToString();
		var flag = _searchControl.IsMatchAny(elementName, text);
		List<TreeNode> list = [];
		if (structure.TryGetValue(element, out var value)) {
			foreach (var item in value) {
				item.Deconstruct(out var key, out var value2);
				var childContainerType = key;
				var array = value2;
				if (array.Length == 0) {
					continue;
				}

				List<TreeNode> list2 = [];
				value2 = array;
				foreach (var element2 in value2) {
					var treeNode = BuildNodeIfVisible(element2, structure);
					if (treeNode != null) {
						list2.Add(treeNode);
					}
				}

				if (list2.Count > 0) {
					var treeNode2 = new TreeNode(childContainerType.ToString()) { ForeColor = Color.Gray };
					treeNode2.Nodes.AddRange(list2.ToArray());
					list.Add(treeNode2);
				}
			}
		}

		if (flag || list.Count > 0) {
			var treeNode3 = new TreeNode($"{elementName} [{element.Id}] ({element.Element.GetType().Name})") {
				Tag = element.Element, ToolTipText = $"ID: {element.Id}\nType: {element.Element.GetType().Name}"
			};
			var element3 = element.Element;
			var foreColor = ((!(element3 is Scene) && !(element3 is ScenePlaceholder))
				? ((element3 is TemplatePlaceholder)
					? Color.Purple
					: ((element3 is Geom)
						? Color.DarkBlue
						: ((!(element3 is Other)) ? Color.Black : Color.DarkSlateGray)))
				: Color.DarkGreen);
			treeNode3.ForeColor = foreColor;
			if (flag) {
				treeNode3.BackColor = Color.Yellow;
			}

			if (list.Count > 0) {
				treeNode3.Nodes.AddRange(list.ToArray());
			}

			return treeNode3;
		}

		return null;
	}

	private static string GetElementName(VmElement el) {
		if (!(el is INamedElement namedElement)) {
			if (!(el is ScenePlaceholder scenePlaceholder)) {
				if (el is TemplatePlaceholder templatePlaceholder) {
					return $"TemplatePlaceholder_{templatePlaceholder.Id}";
				}

				return el.GetType().Name;
			}

			return $"ScenePlaceholder_{scenePlaceholder.Id}";
		}

		return namedElement.Name;
	}

	private void OnNodeDoubleClick(object? sender, TreeNodeMouseClickEventArgs e) {
		if (e.Node?.Tag is VmElement vmElement) {
			var logLevel = LogLevel.Info;
			bool shouldAppend;
			var handler = new LogInterpolatedStringHandler(25, 2, logLevel, out shouldAppend);
			if (shouldAppend) {
				handler.AppendLiteral("Selected element: ");
				handler.AppendFormatted(GetElementName(vmElement));
				handler.AppendLiteral(" (ID: ");
				handler.AppendFormatted(vmElement.Id);
				handler.AppendLiteral(")");
			}

			Logger.Log(logLevel, handler);
		}
	}
}