using P2XMLEditor.Logging;

namespace P2XMLEditor.Parsing.Element;

public interface IParser<TRaw> where TRaw : struct {
	[PerformanceLogHook]
	void ProcessFile(string filePath, List<TRaw> raws);
}