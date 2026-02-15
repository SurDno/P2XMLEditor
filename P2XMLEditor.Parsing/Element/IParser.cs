namespace P2XMLEditor.Parsing.Element;

public interface IParser<TRaw> where TRaw : struct {
	void ProcessFile(string filePath, List<TRaw> raws);
}