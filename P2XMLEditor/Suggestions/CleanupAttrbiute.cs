namespace P2XMLEditor.Suggestions;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CleanupAttribute(string menuPath) : Attribute {
	public string MenuPath { get; } = menuPath;
}