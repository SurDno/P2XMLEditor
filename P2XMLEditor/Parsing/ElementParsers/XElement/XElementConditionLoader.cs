using System.Xml.Linq;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Parsing.ElementParsers.XElement;

public class XElementConditionLoader : IParser<RawConditionData> {
	public void ProcessFile(string filePath, List<RawConditionData> raws) {
		foreach (var element in XDocument.Load(filePath).Root!.Elements()) {
			var id = ulong.Parse(element.Attribute(XNameCache.IdAttribute)!.Value);

			var raw = new RawConditionData {
				Id = id,
				PredicateIds = ParseListElementAsUlong(element, XNameCache.Predicates),
				Operation = element.Element(XNameCache.Operation)!.Value.Deserialize<ConditionOperation>(),
				Name = element.Element(XNameCache.Name)!.Value,
				OrderIndex = int.Parse(element.Element(XNameCache.OrderIndex)!.Value)
			};

			raws.Add(raw);
		}
	}
}