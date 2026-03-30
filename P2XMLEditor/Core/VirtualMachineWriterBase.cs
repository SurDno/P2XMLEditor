using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

/// <summary>
/// Shared element-type-to-filename mapping and localization saving, used by both concrete writers.
/// </summary>
public abstract class VirtualMachineWriterBase(string vmPath, VirtualMachine virtualMachine) {

	protected readonly VirtualMachine Vm = virtualMachine;
	protected readonly string VmPath = vmPath;

	protected static readonly Dictionary<Type, (string File, Func<VirtualMachine, IEnumerable<VmElement>> Elements)> TypeMapping = new() {
		{ typeof(Action),               ("Action.xml",               vm => vm.GetElementsByType<Action>()) },
		{ typeof(ActionLine),           ("ActionLine.xml",           vm => vm.GetElementsByType<ActionLine>()) },
		{ typeof(Blueprint),            ("Blueprint.xml",            vm => vm.GetElementsByType<Blueprint>()) },
		{ typeof(Branch),               ("Branch.xml",               vm => vm.GetElementsByType<Branch>()) },
		{ typeof(Character),            ("Character.xml",            vm => vm.GetElementsByType<Character>()) },
		{ typeof(Condition),            ("Condition.xml",            vm => vm.GetElementsByType<Condition>()) },
		{ typeof(CustomType),           ("CustomType.xml",           vm => vm.GetElementsByType<CustomType>()) },
		{ typeof(EntryPoint),           ("EntryPoint.xml",           vm => vm.GetElementsByType<EntryPoint>()) },
		{ typeof(Event),                ("Event.xml",                vm => vm.GetElementsByType<Event>()) },
		{ typeof(Expression),           ("Expression.xml",           vm => vm.GetElementsByType<Expression>()) },
		{ typeof(FunctionalComponent),  ("FunctionalComponent.xml",  vm => vm.GetElementsByType<FunctionalComponent>()) },
		{ typeof(GameMode),             ("GameMode.xml",             vm => vm.GetElementsByType<GameMode>()) },
		{ typeof(GameRoot),             ("GameRoot.xml",             vm => vm.GetElementsByType<GameRoot>()) },
		{ typeof(GameString),           ("GameString.xml",           vm => vm.GetElementsByType<GameString>()) },
		{ typeof(Geom),                 ("Geom.xml",                 vm => vm.GetElementsByType<Geom>()) },
		{ typeof(Graph),                ("Graph.xml",                vm => vm.GetElementsByType<Graph>()) },
		{ typeof(GraphLink),            ("GraphLink.xml",            vm => vm.GetElementsByType<GraphLink>()) },
		{ typeof(Item),                 ("Item.xml",                 vm => vm.GetElementsByType<Item>()) },
		{ typeof(MindMap),              ("MindMap.xml",              vm => vm.GetElementsByType<MindMap>()) },
		{ typeof(MindMapLink),          ("MindMapLink.xml",          vm => vm.GetElementsByType<MindMapLink>()) },
		{ typeof(MindMapNode),          ("MindMapNode.xml",          vm => vm.GetElementsByType<MindMapNode>()) },
		{ typeof(MindMapNodeContent),   ("MindMapNodeContent.xml",   vm => vm.GetElementsByType<MindMapNodeContent>()) },
		{ typeof(Other),                ("Other.xml",                vm => vm.GetElementsByType<Other>()) },
		{ typeof(Parameter),            ("Parameter.xml",            vm => vm.GetElementsByType<Parameter>()) },
		{ typeof(PartCondition),        ("PartCondition.xml",        vm => vm.GetElementsByType<PartCondition>()) },
		{ typeof(Quest),                ("Quest.xml",                vm => vm.GetElementsByType<Quest>()) },
		{ typeof(Reply),                ("Reply.xml",                vm => vm.GetElementsByType<Reply>()) },
		{ typeof(Sample),               ("Sample.xml",               vm => vm.GetElementsByType<Sample>()) },
		{ typeof(Scene),                ("Scene.xml",                vm => vm.GetElementsByType<Scene>()) },
		{ typeof(Speech),               ("Speech.xml",               vm => vm.GetElementsByType<Speech>()) },
		{ typeof(State),                ("State.xml",                vm => vm.GetElementsByType<State>()) },
		{ typeof(Talking),              ("Talking.xml",              vm => vm.GetElementsByType<Talking>()) },
	};

	protected static XmlWriterSettings XmlSettings => new() {
		Encoding = Encoding.UTF8,
		Indent = true,
		OmitXmlDeclaration = true,
		NewLineChars = "\r\n"
	};

	[PerformanceLogHook]
	public void SaveVirtualMachine(WriterSettings settings) {
		Logger.Log(LogLevel.Info, $"Saving virtual machine.");
		settings.Languages = Vm.Languages.ToList();
		foreach (var (type, (fileName, getElements)) in TypeMapping)
			SaveFile(fileName, getElements(Vm), type, settings);
		SaveLocalizations(settings);
	}

	/// <summary>Saves one element-type file. Implemented differently for Release vs Demo.</summary>
	protected abstract void SaveFile(string baseFileName, IEnumerable<VmElement> elements, Type elementType, WriterSettings settings);

	/// <summary>Saves language localization data. Override for Demo (inline) or Release (external files).</summary>
	protected abstract void SaveLocalizations(WriterSettings settings);

	/// <summary>Ensures the directory for a given file path exists.</summary>
	protected static void EnsureDirectory(string filePath) {
		var dir = Path.GetDirectoryName(filePath);
		if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
			Directory.CreateDirectory(dir);
	}
}
