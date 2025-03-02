using P2XMLEditor.Abstract;
using P2XMLEditor.Helper;

namespace P2XMLEditor.Forms.PathSelection.Validators;

public class AssetDbPathValidator : PathValidator {
	protected override string GetLabelText() => "AssetDatabaseData.xml.gz:";
    
	public override PathValidationResult Validate() {
		if (string.IsNullOrEmpty(PathBox.Text)) 
			return Hint("The file is usually located at Pathologic/Data/Database/AssetDatabaseData.xml.gz, " +
			            "but you may choose to supply your own version of the file if it is modded.");
		
		if (!File.Exists(PathBox.Text)) 
			return Error("File does not exist");
		
		if (!PathBox.Text.EndsWith(".xml.gz")) 
			return Error("AssetDatabaseData must be an xml.gz file");
		
		return Success("The asset database is valid.");
	}
	
	protected override void Browse() {
		using var dialog = new OpenFileDialog();
		dialog.Filter = "AssetDatabaseData.xml.gz|*.xml.gz";
		if (dialog.ShowDialog() == DialogResult.OK)
			PathBox.Text = dialog.FileName;
	}

	protected override void Locate() {
		var install = InstallationLocator.FindInstall();
		if (install == null) return;
		
		var path = Path.Combine(install, @"Data\Database\AssetDatabaseData.xml.gz");
		if (!File.Exists(path)) return;
		
		PathBox.Text = path;
	}
}