using System;
using System.Windows.Forms;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Helper;

public static class ErrorHandler {
	
	public static void Handle(string msg, Exception? e, bool skipLogging = false) {
		var message = e != null ? ": " + e.Message : string.Empty;
		var stackTrace = e != null ? e.StackTrace + "\n\n" : string.Empty;
		
		if (e != null && !skipLogging)
			Logger.Log(LogLevel.Error, $"{message}\n{stackTrace}");
		
		MessageBox.Show($"{msg}{message}.\n\n{stackTrace}See P2XMLEditor.log in your Logs directory for more info.",
			"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
	}
}