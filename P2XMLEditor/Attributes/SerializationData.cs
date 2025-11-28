using System;

namespace P2XMLEditor.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class SerializationData(string value) : Attribute {
	public string Value { get; } = value;
}