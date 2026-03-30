using System.Collections.Generic;

namespace P2XMLEditor.Data;

public class WriterSettings {
	public WriterFormat Format { get; set; } = WriterFormat.Release;
	public bool CleanUpOrphanedElements { get; set; } = false;
	public bool CleanUpUnusedProperties { get; set; } = false;
	public bool CleanUpNames { get; set; } = false;
	public bool CleanUpEmptyStrings { get; set; } = false;
	public bool MergeConstants { get; set; } = false;
	public List<string> Languages { get; set; } = [];
	
	public VmVersionSettings? VmMetadata { get; set; }
}