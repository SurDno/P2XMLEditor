using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Writing.Element.ReleaseXElementWriters;

public interface IReleaseXElementWriter {
	XElement ToXml(VmElement element, WriterSettings settings);
}

public interface IReleaseXElementWriter<in T> : IReleaseXElementWriter where T : VmElement {
	XElement ToXml(T element, WriterSettings settings);
	XElement IReleaseXElementWriter.ToXml(VmElement element, WriterSettings settings) => ToXml((T)element, settings);
}
