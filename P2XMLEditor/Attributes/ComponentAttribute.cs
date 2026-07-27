using System;

namespace P2XMLEditor.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class ComponentAttribute(string value) : Attribute {
	public string Value { get; } = value;
}
