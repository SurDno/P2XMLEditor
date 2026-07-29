using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using static P2XMLEditor.Helper.XmlParsingHelper;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public class ReleaseXElementExpressionWriter : IReleaseXElementWriter<Expression> {
	public XElement ToXml(Expression element, WriterSettings settings) {
		var xElement = CreateBaseElement(element.Id);

		xElement.Add(new XElement("ExpressionType", element.ExpressionType.Serialize()));
		if (element.Function != null)
			xElement.Add(CreateSelfClosingElement("TargetFunctionName", element.Function.Name));
		xElement.Add( new XElement("TargetObject", element.TargetObject.Write()));
		
		if (element.TargetParam != null)
			xElement.Add(new XElement("TargetParam", element.TargetParam.Value.Write()));
		if (element.Function?.GetParamStrings() is { Count: > 0 } sourceParams)
			xElement.Add(CreateListElement("SourceParams", sourceParams));
		if (element.Const != null)
			xElement.Add(new XElement("Const", element.Const.Id));

		xElement.Add(new XElement("LocalContext", element.LocalContext.Id));
		if (element.FormulaChilds?.Count > 0) {
			xElement.Add(CreateListElement("FormulaChilds", element.FormulaChilds.Select(c => c.Id.ToString())));
			xElement.Add(CreateListElement("FormulaOperations", element.FormulaOperations!.Select(o => o.Serialize())));
		}

		if (element.Inversion != null)
			xElement.Add(CreateBoolElement("Inversion", (bool)element.Inversion));
		return xElement;
	}
}
