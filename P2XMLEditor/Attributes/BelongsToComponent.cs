using P2XMLEditor.GameData.Templates.InternalTypes.Interfaces;

namespace P2XMLEditor.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class BelongsToComponent(ITemplateComponent value) : Attribute {
	public ITemplateComponent Value { get; } = value;
}