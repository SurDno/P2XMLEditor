using System;
using System.Diagnostics;

namespace P2XMLEditor.Logging;

public sealed class PerformanceLogger : IDisposable {
	private readonly string _context;
	private readonly long _startTimestamp;

	private PerformanceLogger(string context) {
		_context = context;
		_startTimestamp = Stopwatch.GetTimestamp();
		
		Logger.Log(LogLevel.Trace, $"[{_context}] Starting.");
	}

	public void Dispose() {
		var endTimestamp = Stopwatch.GetTimestamp();
		var delta = endTimestamp - _startTimestamp;
		var elapsedMs = delta * (0.0001d);
		
		var perfLevel = elapsedMs switch {
			< 5 => LogLevel.Trace,
			< 1000 => LogLevel.Performance,
			< 5000 => LogLevel.Warning,
			_ => LogLevel.Error
		};
		
		Logger.Log(perfLevel, $"[{_context}] Completed in {elapsedMs}ms");
	}
	
	public static PerformanceLogger Log(string method, string type) => new($"{type}.{method}");
}