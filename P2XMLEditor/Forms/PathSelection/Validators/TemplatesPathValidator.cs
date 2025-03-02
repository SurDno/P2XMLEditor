using P2XMLEditor.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.PathSelection.Validators;

public class TemplatesPathValidator : PathValidator {
	protected override string GetLabelText() => "Templates directory:";
    
	public override PathValidationResult Validate() {
		if (string.IsNullOrEmpty(PathBox.Text))
			return Hint("Templates folder is usually located at Pathologic/Data/Templates, " +
			            "but you may choose to supply your own directory if it is modded.");
		
		if (!Directory.Exists(PathBox.Text))
			return Error("Directory does not exist");
        
		if (Directory.GetFiles(PathBox.Text, "*.xml.gz").Length == 0) 
			return Error("No xml.gz files found in the directory.");
		
		return Success("The templates directory is valid.");
	}

	protected override void Locate() {
		var install = InstallationLocator.FindInstall();
		if (install == null) return;
		
		var path = Path.Combine(install, @"Data\Templates\");
		if (!Directory.Exists(path)) return;
		
		PathBox.Text = path;
	}
}