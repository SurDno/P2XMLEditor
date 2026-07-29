using System;
using System.Windows.Forms;
using P2XMLEditor.Forms.MainForm;
using P2XMLEditor.Logging;

namespace P2XMLEditor;

internal static class Program {
	[STAThread]
	private static void Main() {
		Logger.Log(LogLevel.Info, $"Starting P2XMLEditor...");
		ApplicationConfiguration.Initialize();
		AppContext.SetSwitch("System.Xml.XmlConvert.SkipValidation", true);
		AppContext.SetSwitch("System.Xml.XmlConvert.SkipNameValidation", true);
		AppContext.SetSwitch("System.Xml.DisableFormatting", true);
		/*
		MessageBox.Show("P2XMLEditor is currently in Beta. Some of its functions have not been tested properly and " +
						"may cause virtual machine corruption, resulting in it not being loaded by the game " +
						"and/or the tool in the future. Make frequent backups of virtual machines you're editing " +
						"to avoid potential data loss.", "Disclaimer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						*/
		P2XMLEditor.Core.SettingsManager.Load();
		Application.Run(new MainForm());
	}
}
