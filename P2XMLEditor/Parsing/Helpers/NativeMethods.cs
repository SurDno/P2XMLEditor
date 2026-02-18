using System.Runtime.InteropServices;
using P2XMLEditor.Parsing.RawData;

namespace P2XMLEditor.Parsing.Helpers;

internal static class NativeMethods {
	[SuppressGCTransition]
	[DllImport("P2XMLEditor.NativeParsers.dll", CallingConvention = CallingConvention.Cdecl)]
	public static extern unsafe ulong ParseBuffer(byte* data, nuint size, RawGameStringData* output);
}