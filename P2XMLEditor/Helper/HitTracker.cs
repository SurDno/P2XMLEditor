using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace P2XMLEditor.Helper;

public static class HitTracker {
	private sealed class Entry {
		public int Count;
		public string? FirstSample;
		public string? LastSample;
	}

	private static readonly ConcurrentDictionary<(string File, int Line), Entry> Entries = new();

	public static void Hit(string? sample = null, [CallerFilePath] string file = "", [CallerLineNumber] int line = 0) {
		var entry = Entries.GetOrAdd((ShortName(file), line), _ => new Entry());
		lock (entry) {
			entry.Count++;
			entry.FirstSample ??= sample;
			if (sample != null) entry.LastSample = sample;
		}
	}

	public static void Reset() => Entries.Clear();

	public static IEnumerable<string> Report(int sampleLength = 90) {
		foreach (var kvp in Entries.OrderBy(e => e.Key.File).ThenBy(e => e.Key.Line)) {
			var e = kvp.Value;
			var sample = e.FirstSample is null ? "" : $"  |  {Truncate(e.FirstSample, sampleLength)}";
			yield return $"{kvp.Key.File}:{kvp.Key.Line,-5} {e.Count,8:N0}{sample}";
		}
	}

	public static string ReportText() => string.Join(Environment.NewLine, Report());

	private static string ShortName(string path) {
		var slash = path.LastIndexOfAny(['\\', '/']);
		return slash >= 0 ? path[(slash + 1)..] : path;
	}

	private static string Truncate(string s, int max) =>
		s.Length <= max ? s : string.Concat(s.AsSpan(0, max - 1), "…");
}
