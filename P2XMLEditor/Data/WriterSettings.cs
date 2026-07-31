using System.Collections.Generic;

namespace P2XMLEditor.Data;

public class WriterSettings {
	public WriterFormat Format { get; set; } = WriterFormat.Release;
	public bool CleanUpOrphanedElements { get; set; } = false;
	public bool RemoveDefaultValueTypes { get; set; } = false;
	public bool StripNames { get; set; } = false;
	public bool StripEditorOnlyTags { get; set; } = false;
	public List<string> Languages { get; set; } = [];
	
	public VmVersionSettings? VmMetadata { get; set; }
}
