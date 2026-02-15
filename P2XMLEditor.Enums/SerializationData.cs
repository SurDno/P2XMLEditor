namespace P2XMLEditor.Enums;

[AttributeUsage(AttributeTargets.Field)]
public class SerializationData(string value) : Attribute {
	public string Value { get; } = value;
}