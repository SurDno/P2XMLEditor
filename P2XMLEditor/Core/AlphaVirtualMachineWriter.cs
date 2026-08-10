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
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Writing.Element.AlphaXElementWriters;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

/// <summary>
/// Writes the alpha corpus back out: one plain-xml file per type in its own upper-cased directory,
/// GameRoot.xml at the root, the object id in a &lt;Guid&gt; child rather than an attribute, and every
/// type string put back into the alpha spelling of the engine namespace (the Cyrillic 'с') that
/// <see cref="AlphaFormat"/> normalised away on load. Like the demo, localizations ride inline in
/// GameString, so there is no separate folder to write.
/// </summary>
public class AlphaVirtualMachineWriter(string vmPath, VirtualMachine virtualMachine)
	: VirtualMachineWriterBase(vmPath, virtualMachine) {

	// The two files whose spelling the upper-casing would not produce on its own.
	private static readonly Dictionary<string, string> FileNameOverrides = new() {
		{ "Blueprint", "BluePrint" },
		{ "MindMapLink", "MindMaLink" },
	};

	private static readonly Dictionary<Type, IAlphaXElementWriter> Writers = new() {
		{ typeof(Action),               new AlphaXElementActionWriter() },
		{ typeof(ActionLine),           new AlphaXElementActionLineWriter() },
		{ typeof(Blueprint),            new AlphaXElementBlueprintWriter() },
		{ typeof(Branch),               new AlphaXElementBranchWriter() },
		{ typeof(Character),            new AlphaXElementCharacterWriter() },
		{ typeof(Condition),            new AlphaXElementConditionWriter() },
		{ typeof(CustomType),           new AlphaXElementCustomTypeWriter() },
		{ typeof(EntryPoint),           new AlphaXElementEntryPointWriter() },
		{ typeof(Event),                new AlphaXElementEventWriter() },
		{ typeof(Expression),           new AlphaXElementExpressionWriter() },
		{ typeof(FunctionalComponent),  new AlphaXElementFunctionalComponentWriter() },
		{ typeof(GameMode),             new AlphaXElementGameModeWriter() },
		{ typeof(GameRoot),             new AlphaXElementGameRootWriter() },
		{ typeof(GameString),           new AlphaXElementGameStringWriter() },
		{ typeof(Geom),                 new AlphaXElementGeomWriter() },
		{ typeof(Graph),                new AlphaXElementGraphWriter() },
		{ typeof(GraphLink),            new AlphaXElementGraphLinkWriter() },
		{ typeof(Item),                 new AlphaXElementItemWriter() },
		{ typeof(MindMap),              new AlphaXElementMindMapWriter() },
		{ typeof(MindMapLink),          new AlphaXElementMindMapLinkWriter() },
		{ typeof(MindMapNode),          new AlphaXElementMindMapNodeWriter() },
		{ typeof(MindMapNodeContent),   new AlphaXElementMindMapNodeContentWriter() },
		{ typeof(Other),                new AlphaXElementOtherWriter() },
		{ typeof(Parameter),            new AlphaXElementParameterWriter() },
		{ typeof(PartCondition),        new AlphaXElementPartConditionWriter() },
		{ typeof(Quest),                new AlphaXElementQuestWriter() },
		{ typeof(Reply),                new AlphaXElementReplyWriter() },
		{ typeof(Sample),               new AlphaXElementSampleWriter() },
		{ typeof(Scene),                new AlphaXElementSceneWriter() },
		{ typeof(Speech),               new AlphaXElementSpeechWriter() },
		{ typeof(State),                new AlphaXElementStateWriter() },
		{ typeof(Talking),              new AlphaXElementTalkingWriter() },
	};

	protected override void SaveFile(string baseFileName, IEnumerable<VmElement> elements, Type elementType, WriterSettings settings) {
		try {
			var list = elements.OrderBy(e => e.Id).ToList();
			if (list.Count == 0) return;

			if (!Writers.TryGetValue(elementType, out var writer)) {
				Logger.Log(LogLevel.Warning, $"No Alpha writer found for {elementType.Name}. Skipping.");
				return;
			}

			var baseName = Path.GetFileNameWithoutExtension(baseFileName);
			var fileBase = FileNameOverrides.GetValueOrDefault(baseName, baseName);
			var filePath = baseName == "GameRoot"
				? Path.Combine(VmPath, fileBase + ".xml")
				: Path.Combine(VmPath, fileBase.ToUpperInvariant(), fileBase + ".xml");
			EnsureDirectory(filePath);

			// The root tag is the alpha file name, which is the type name apart from BluePrint and
			// MindMaLink.
			var root = new XElement(fileBase, list.Select(e => writer.ToXml(e, settings)));

			var body = new StringBuilder();
			using (var stringWriter = new StringWriter(body))
			using (var xmlWriter = XmlWriter.Create(stringWriter, XmlSettings))
				root.Save(xmlWriter);

			// The one thing serialization cannot do itself: put the engine namespace back into the
			// alpha spelling in every type string. The swap is a specific substring, so it never
			// touches the Cyrillic letter where it stands for itself in GameString's Russian text.
			var text = AlphaFormat.Denormalize(body.ToString());

			using var fileStream = File.Create(filePath);
			fileStream.Write(Encoding.UTF8.GetPreamble());
			fileStream.Write(Encoding.UTF8.GetBytes("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n"));
			fileStream.Write(Encoding.UTF8.GetBytes(text));

			Logger.Log(LogLevel.Info, $"Saved {list.Count} elements of type {elementType.Name} to {filePath}");
		} catch (Exception ex) {
			Logger.Log(LogLevel.Error, $"Error saving {baseFileName}: {ex.Message}\n{ex.StackTrace}");
		}
	}

	/// <summary>Alpha keeps localizations inline in GameString, so there is no folder to write.</summary>
	protected override void SaveLocalizations(WriterSettings settings) { }
}
