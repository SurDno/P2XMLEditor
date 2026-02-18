using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.Element;
using P2XMLEditor.Parsing.Helpers;
using P2XMLEditor.Parsing.RawData;

public sealed class NativeGameStringLoader : IParser<RawGameStringData>
{
	[PerformanceLogHook]
	public unsafe void ProcessFile(string filePath, List<RawGameStringData> raws) {
		raws.Clear();

		var data = File.ReadAllBytes(filePath);

		const int capacity = 22000;
		raws.Capacity = capacity;

		CollectionsMarshal.SetCount(raws, capacity);

		Span<RawGameStringData> span = CollectionsMarshal.AsSpan(raws);

		ulong count;

		fixed (byte* pData = data)
			fixed (RawGameStringData* pOut = span)
			{
				count = NativeMethods.ParseBuffer(
					pData,
					(nuint)data.Length,
					pOut);
			}

		CollectionsMarshal.SetCount(raws, (int)count);
	}
}