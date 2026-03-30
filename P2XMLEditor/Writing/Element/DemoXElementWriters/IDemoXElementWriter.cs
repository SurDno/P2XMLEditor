using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Writing.Element.DemoXElementWriters;

public interface IDemoXElementWriter {
	XElement ToXml(VmElement element, WriterSettings settings);
}

public interface IDemoXElementWriter<in T> : IDemoXElementWriter where T : VmElement {
	XElement ToXml(T element, WriterSettings settings);
	XElement IDemoXElementWriter.ToXml(VmElement element, WriterSettings settings) => ToXml((T)element, settings);
}
