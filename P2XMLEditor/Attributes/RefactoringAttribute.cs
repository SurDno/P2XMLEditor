namespace P2XMLEditor.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class RefactoringAttribute : Attribute
{
	public string MenuPath { get; }

	public RefactoringAttribute(string menuPath)
	{
		MenuPath = menuPath;
	}
}