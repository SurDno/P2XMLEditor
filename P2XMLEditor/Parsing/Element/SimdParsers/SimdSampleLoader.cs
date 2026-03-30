using System.Collections.Generic;
using System.IO;
using System.Text;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.SimdExtensions;
namespace P2XMLEditor.Parsing.Element.SimdParsers;

public class SimdSampleLoader : IParser<RawSampleData> {
	private static readonly byte[] ITEM = "<Item id=\""u8.ToArray();
	private static readonly byte[] SAMPLE = "<SampleType>"u8.ToArray();
	private static readonly byte[] ENGINE = "<EngineID>"u8.ToArray();

	[PerformanceLogHook]
	public unsafe void ProcessFile(string filePath, List<RawSampleData> raws) {
		var data = File.ReadAllBytes(filePath);

		fixed (byte* ptr = data) {
			var p = ptr;
			var end = ptr + data.Length;

			while (true) {
				var item = FindSimd(p, end, ITEM);
				if (item == null) break;
				p = item + ITEM.Length;

				ulong id = 0;
				while (*p != (byte)'"') {
					id = id * 10 + (ulong)(*p - (byte)'0');
					p++;
				}
				p++;

				var sTag = FindSimd(p, end, SAMPLE);
				p = sTag + SAMPLE.Length;

				var sStart = p;
				while (*p != (byte)'<') p++;
				var sampleTypeStr = Encoding.UTF8.GetString(sStart, (int)(p - sStart));

				var eTag = FindSimd(p, end, ENGINE);
				p = eTag + ENGINE.Length;

				var eStart = p;
				while (*p != (byte)'<') p++;
				var engineId = Encoding.UTF8.GetString(eStart, (int)(p - eStart));

				raws.Add(new RawSampleData {
					Id		 = id,
					SampleType = sampleTypeStr.Deserialize<SampleType>(),
					EngineId   = engineId
				});
			}
		}
	}
}