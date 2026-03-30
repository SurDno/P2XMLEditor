using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;
using P2XMLEditor.Writing.Element.DemoXElementWriters;

namespace P2XMLEditor.Core;

public class DemoVirtualMachineWriter(string vmPath, VirtualMachine virtualMachine)
	: VirtualMachineWriterBase(vmPath, virtualMachine) {

	private static readonly Dictionary<string, string> FileNameOverrides = new() {
		{ "MindMapLink", "MindMaLink" },
		{ "Blueprint",   "BluePrint" },
	};

	private static readonly Dictionary<Type, IDemoXElementWriter> Writers = new() {
		{ typeof(GameData.VirtualMachineElements.Action),      new DemoXElementActionWriter() },
		{ typeof(ActionLine),           new DemoXElementActionLineWriter() },
		{ typeof(Blueprint),            new DemoXElementBlueprintWriter() },
		{ typeof(Branch),               new DemoXElementBranchWriter() },
		{ typeof(Character),            new DemoXElementCharacterWriter() },
		{ typeof(Condition),            new DemoXElementConditionWriter() },
		{ typeof(CustomType),           new DemoXElementCustomTypeWriter() },
		{ typeof(EntryPoint),           new DemoXElementEntryPointWriter() },
		{ typeof(Event),                new DemoXElementEventWriter() },
		{ typeof(Expression),           new DemoXElementExpressionWriter() },
		{ typeof(FunctionalComponent),  new DemoXElementFunctionalComponentWriter() },
		{ typeof(GameMode),             new DemoXElementGameModeWriter() },
		{ typeof(GameRoot),             new DemoXElementGameRootWriter() },
		{ typeof(GameString),           new DemoXElementGameStringWriter() },
		{ typeof(Geom),                 new DemoXElementGeomWriter() },
		{ typeof(Graph),                new DemoXElementGraphWriter() },
		{ typeof(GraphLink),            new DemoXElementGraphLinkWriter() },
		{ typeof(Item),                 new DemoXElementItemWriter() },
		{ typeof(MindMap),              new DemoXElementMindMapWriter() },
		{ typeof(MindMapLink),          new DemoXElementMindMapLinkWriter() },
		{ typeof(MindMapNode),          new DemoXElementMindMapNodeWriter() },
		{ typeof(MindMapNodeContent),   new DemoXElementMindMapNodeContentWriter() },
		{ typeof(Other),                new DemoXElementOtherWriter() },
		{ typeof(Parameter),            new DemoXElementParameterWriter() },
		{ typeof(PartCondition),        new DemoXElementPartConditionWriter() },
		{ typeof(Quest),                new DemoXElementQuestWriter() },
		{ typeof(Reply),                new DemoXElementReplyWriter() },
		{ typeof(Sample),               new DemoXElementSampleWriter() },
		{ typeof(Scene),                new DemoXElementSceneWriter() },
		{ typeof(Speech),               new DemoXElementSpeechWriter() },
		{ typeof(State),                new DemoXElementStateWriter() },
		{ typeof(Talking),              new DemoXElementTalkingWriter() },
	};

	protected override void SaveFile(string baseFileName, IEnumerable<VmElement> elements, Type elementType, WriterSettings settings) {
		try {
			var list = elements.ToList();
			if (list.Count == 0) return;

			if (!Writers.TryGetValue(elementType, out var writer)) {
				Logger.Log(LogLevel.Warning, $"No Demo writer found for {elementType.Name}. Skipping.");
				return;
			}

			// Apply Demo filename overrides and always use .xml.gz
			var baseName = Path.GetFileNameWithoutExtension(baseFileName);
			if (FileNameOverrides.TryGetValue(baseName, out var overrideName))
				baseName = overrideName;
			var finalFileName = baseName + ".xml.gz";

			var filePath = Path.Combine(VmPath, finalFileName);
			EnsureDirectory(filePath);

			// Root element name is the type name (e.g. <Action>, <Branch>)
			// Demo specifically uses MindMaLink typo for root tag
			var rootName = elementType.Name;
			if (rootName == "MindMapLink") rootName = "MindMaLink";
			var root = new XElement(rootName, list.Select(e => writer.ToXml(e, settings)));

			// Write the XML declaration directly to the GZip stream before creating the XmlWriter
			// so the writer doesn't encounter a second document-start token.
			using var fileStream = File.Create(filePath);
			using var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal);
			gzipStream.Write("""
			                 <?xml version="1.0" encoding="UTF-8"?>

			                 """u8.ToArray());
			using var xmlWriter = XmlWriter.Create(gzipStream, XmlSettings);
			root.Save(xmlWriter);

			Logger.Log(LogLevel.Info, $"Saved {list.Count} elements of type {elementType.Name} to {finalFileName}");
		} catch (Exception ex) {
			Logger.Log(LogLevel.Error, $"Error saving {baseFileName}: {ex.Message}\n{ex.StackTrace}");
		}
	}

	/// <summary>
	/// Demo localizations are stored inline inside GameString.xml — no separate folder needed.
	/// </summary>
	protected override void SaveLocalizations(WriterSettings settings) { }
}
