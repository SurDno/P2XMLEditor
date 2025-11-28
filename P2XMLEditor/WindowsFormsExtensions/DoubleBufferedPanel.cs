using System.Windows.Forms;

namespace P2XMLEditor.WindowsFormsExtensions;

public sealed class DoubleBufferedPanel : Panel {
	public DoubleBufferedPanel() {
		DoubleBuffered = true;
	}
}