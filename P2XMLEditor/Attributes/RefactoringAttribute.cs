namespace P2XMLEditor.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RefactoringAttribute(string menuPath) : Attribute {
	public string MenuPath { get; } = menuPath;
}