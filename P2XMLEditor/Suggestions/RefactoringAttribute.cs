namespace P2XMLEditor.Suggestions;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RefactoringAttribute(string menuPath) : Attribute {
	public string MenuPath { get; } = menuPath;
}