namespace P2XMLEditor.Parsing.Helpers;

public static class SimdByteArrayCache {
	private static readonly byte[] ItemBytes = "<Item id=\""u8.ToArray();
	private static readonly byte[] ParentBytes = "<Parent>"u8.ToArray();
	private static readonly byte[] SAMPLE = "<SampleType>"u8.ToArray();
	private static readonly byte[] SAMPLE_END = "</SampleType>"u8.ToArray();
	private static readonly byte[] ENGINE = "<EngineID>"u8.ToArray();
	private static readonly byte[] ENGINE_END = "</EngineID>"u8.ToArray();
}