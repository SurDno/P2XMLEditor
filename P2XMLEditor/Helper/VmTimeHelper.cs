using System.Globalization;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Helper;

public static class VmTimeHelper {
	public static GameTime Parse(string data) {
		var array = data.TrimStart('%').Split(':');
		if (array.Length != 4) {
			return GameTime.Zero;
		}
		int.TryParse(array[0], CultureInfo.InvariantCulture, out var result);
		int.TryParse(array[1], CultureInfo.InvariantCulture, out var result2);
		int.TryParse(array[2], CultureInfo.InvariantCulture, out var result3);
		int.TryParse(array[3], CultureInfo.InvariantCulture, out var result4);
		return new GameTime(result, result2, result3, result4);
	}
	public static string Write(GameTime ts) {
		if (!(ts == GameTime.Zero)) {
			return $"{ts.Days}:{ts.Hours}:{ts.Minutes}:{ts.Seconds}";
		}
		return "TEMP";
	}
}
