using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace P2XMLEditor.Parsing.Helpers;

public static class SimdExtensions {
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static unsafe byte* FindSimd(byte* p, byte* end, byte[] pat) {
		var len = pat.Length;

		var first = Vector256.Create(pat[0]);

		while (p + len <= end) {
			var block = Avx.LoadVector256(p);
			var eq = Avx2.CompareEqual(block, first);
			var mask = (uint)Avx2.MoveMask(eq);

			while (mask != 0) {
				var bit = BitOperations.TrailingZeroCount(mask);
				var cand = p + bit;

				if (cand + len <= end) {
					var match = true;
					for (var i = 0; i < len; i++)
						if (cand[i] != pat[i]) {
							match = false;
							break;
						}

					if (match) return cand;
				}
				mask &= mask - 1;
			}
			p += 32;
		}

		return null;
	}
}
