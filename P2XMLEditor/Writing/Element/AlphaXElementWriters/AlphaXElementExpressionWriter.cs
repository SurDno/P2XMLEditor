using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.DemoXmlParsingHelper;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public class AlphaXElementExpressionWriter : IAlphaXElementWriter<Expression> {
	public XElement ToXml(Expression element, WriterSettings settings) {
		var obj = new XElement("object");

		obj.Add(
			new XElement("ExpressionType", element.ExpressionType.Serialize()),
			CreateDemoStringElement("TargetFunctionName", element.Function?.Name ?? ""),
			CreateDemoStringElement("TargetObjUniName", element.TargetObject.Write()),
			CreateDemoStringElement("TargetParamName", element.TargetParam?.Write() ?? "")
		);

		if (element.Const != null)
			obj.Add(new XElement("Const", element.Const.Id));

		obj.Add(CreateDemoListElement("SourceParamNames", element.Function?.GetParamStrings() ?? []));
		obj.Add(new XElement("LocalContext", element.LocalContext.Id));

		if (!settings.RemoveDefaultValueTypes || element.Inversion) obj.Add(CreateDemoBoolElement("Inversion", element.Inversion));

		obj.Add(
			CreateDemoListElementAsLong("FormulaChilds", element.FormulaChilds?.Select(c => c.Id) ?? []),
			CreateDemoListElement("FormulaOperations", element.FormulaOperations?.Select(o => o.Serialize()) ?? []),
			CreateGuidElement(element.Id)
		);

		return obj;
	}
}
