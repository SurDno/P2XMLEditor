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
using P2XMLEditor.Writing.Element.ReleaseXElementWriters;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class ReleaseVirtualMachineWriter(string vmPath, VirtualMachine virtualMachine)
	: VirtualMachineWriterBase(vmPath, virtualMachine) {

	private static readonly Dictionary<Type, IReleaseXElementWriter> Writers = new() {
		{ typeof(Action),      new ReleaseXElementActionWriter() },
		{ typeof(ActionLine),           new ReleaseXElementActionLineWriter() },
		{ typeof(Blueprint),            new ReleaseXElementBlueprintWriter() },
		{ typeof(Branch),               new ReleaseXElementBranchWriter() },
		{ typeof(Character),            new ReleaseXElementCharacterWriter() },
		{ typeof(Condition),            new ReleaseXElementConditionWriter() },
		{ typeof(CustomType),           new ReleaseXElementCustomTypeWriter() },
		{ typeof(EntryPoint),           new ReleaseXElementEntryPointWriter() },
		{ typeof(Event),                new ReleaseXElementEventWriter() },
		{ typeof(Expression),           new ReleaseXElementExpressionWriter() },
		{ typeof(FunctionalComponent),  new ReleaseXElementFunctionalComponentWriter() },
		{ typeof(GameMode),             new ReleaseXElementGameModeWriter() },
		{ typeof(GameRoot),             new ReleaseXElementGameRootWriter() },
		{ typeof(GameString),           new ReleaseXElementGameStringWriter() },
		{ typeof(Geom),                 new ReleaseXElementGeomWriter() },
		{ typeof(Graph),                new ReleaseXElementGraphWriter() },
		{ typeof(GraphLink),            new ReleaseXElementGraphLinkWriter() },
		{ typeof(Item),                 new ReleaseXElementItemWriter() },
		{ typeof(MindMap),              new ReleaseXElementMindMapWriter() },
		{ typeof(MindMapLink),          new ReleaseXElementMindMapLinkWriter() },
		{ typeof(MindMapNode),          new ReleaseXElementMindMapNodeWriter() },
		{ typeof(MindMapNodeContent),   new ReleaseXElementMindMapNodeContentWriter() },
		{ typeof(Other),                new ReleaseXElementOtherWriter() },
		{ typeof(Parameter),            new ReleaseXElementParameterWriter() },
		{ typeof(PartCondition),        new ReleaseXElementPartConditionWriter() },
		{ typeof(Quest),                new ReleaseXElementQuestWriter() },
		{ typeof(Reply),                new ReleaseXElementReplyWriter() },
		{ typeof(Sample),               new ReleaseXElementSampleWriter() },
		{ typeof(Scene),                new ReleaseXElementSceneWriter() },
		{ typeof(Speech),               new ReleaseXElementSpeechWriter() },
		{ typeof(State),                new ReleaseXElementStateWriter() },
		{ typeof(Talking),              new ReleaseXElementTalkingWriter() },
	};

	protected override void SaveFile(string baseFileName, IEnumerable<VmElement> elements, Type elementType, WriterSettings settings) {
		try {
			// Sort elements by ID for Release VM format
			var list = elements.OrderBy(e => e.Id).ToList();
			if (list.Count == 0) return;

			if (!Writers.TryGetValue(elementType, out var writer)) {
				Logger.Log(LogLevel.Warning, $"No Release writer found for {elementType.Name}. Skipping.");
				return;
			}

			var filePath = Path.Combine(VmPath, baseFileName);
			EnsureDirectory(filePath);

			var root = new XElement("Root",
				new XAttribute("xml_data_format_version", "14"),
				new XAttribute("default_type", elementType.Name),
				list.Select(e => writer.ToXml(e, settings))
			);

			using var fileStream = File.Create(filePath);
			fileStream.Write(Encoding.UTF8.GetPreamble());
			fileStream.Write("""
			                 <?xml version="1.0" encoding="UTF-8"?>

			                 """u8.ToArray());
			using var xmlWriter = XmlWriter.Create(fileStream, XmlSettings);
			root.Save(xmlWriter);

			Logger.Log(LogLevel.Info, $"Saved {list.Count} elements of type {elementType.Name} to {baseFileName}");
		} catch (Exception ex) {
			Logger.Log(LogLevel.Error, $"Error saving {baseFileName}: {ex.Message}\n{ex.StackTrace}");
		}
	}

	protected override void SaveLocalizations(WriterSettings settings) {
		var locPath = Path.Combine(VmPath, "Localizations");
		Directory.CreateDirectory(locPath);

		foreach (var lang in Vm.Languages) {
			using var writer = new StreamWriter(Path.Combine(locPath, $"{lang}.txt"), false, new UTF8Encoding(true));
			foreach (var gs in Vm.GetElementsByType<GameString>().OrderBy(g => g.Id))
				writer.WriteLine($"{gs.Id} {gs.GetText(lang)}");
		}
	}
}
