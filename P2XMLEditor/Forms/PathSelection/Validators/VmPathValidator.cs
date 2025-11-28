using System.IO;
using System.Windows.Forms;
using P2XMLEditor.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.PathSelection.Validators;

public class VmPathValidator : PathValidator {
	protected override string GetLabelText() => "Virtual machine to edit:";
    
	public override PathValidationResult Validate() {
		if (string.IsNullOrEmpty(PathBox.Text)) 
			return Hint("VirtualMachine is one of the folders in at Pathologic/Data/VirtualMachine/, " +
			            "but you may choose to supply your own directory if it is modded.");
		
		if (!Directory.Exists(PathBox.Text)) 
			return Error("Directory does not exist");
        
		if (Directory.GetFiles(PathBox.Text, "*.xml.gz").Length != 0) 
			return Error("The directory contains .xml.gz files. Pathologic 2 Demo virtual machine is not supported.");
		
		if (Directory.Exists(Path.Combine(PathBox.Text, "PathologicSandbox"))) 
			return Error("This is a directory containing virtual machines, not a specific virtual machine. Select " +
			             "one of the nested directories.");
		
		if (Directory.GetFiles(PathBox.Text, "*.xml").Length == 0) 
			return Error("Not a valid VirtualMachine directory. No XML files found");
		if (!Directory.Exists(Path.Combine(PathBox.Text, "Localizations"))) 
			return Error("Not a valid VirtualMachine directory. No Localizations folder found");
		
		return Success("The virtual machine is valid.");
	}
	
	protected override void Locate() {
		var install = InstallationLocator.FindInstall();
		if (install == null) return;
        
		var path = Path.Combine(install, @"Data\VirtualMachine\");
		if (!Directory.Exists(path)) return;

		using var form = new VmSelectionForm(path);
		if (form.ShowDialog() == DialogResult.OK)
			PathBox.Text = form.GetSelectedPath(path);
	}
}
