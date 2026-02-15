using System.Runtime.CompilerServices;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.RawPointerExtensions;

namespace P2XMLEditor.Parsing.Element.RawPointerParsers;

// ASSUMPTIONS:
public class RawPointerGameStringLoader : IParser<RawGameStringData> {

	public unsafe void ProcessFile(string filePath, List<RawGameStringData> raws) {
		var data = File.ReadAllBytes(filePath);
		var n = data.Length;

		fixed (byte* ptr = data) {
			var p = ptr + 106;
			var end = ptr + n - 10;

			while (p + 78 < end) {
				p += 12;
				var id = ParseUlong16(p);
				p += 16;

				p += 16;
				var parent = ParseParent(p, out var digits);
				p += digits + 22;

				raws.Add(new RawGameStringData { Id = id, ParentId = parent });
			}
		}
	}
	
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static unsafe ulong ParseParent(byte* p, out int len) {
		switch ((char)p[0]) {
			case '2':
				len = 15;
				return ParseUlong15(p);
			case '1':
				len = 17;
				return ParseUlong17(p);
			default:
				len = 16;
				return ParseUlong16(p);
		}
	}
}