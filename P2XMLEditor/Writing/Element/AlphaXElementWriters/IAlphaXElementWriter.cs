using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Writing.Element.AlphaXElementWriters;

public interface IAlphaXElementWriter {
	XElement ToXml(VmElement element, WriterSettings settings);
}

public interface IAlphaXElementWriter<in T> : IAlphaXElementWriter where T : VmElement {
	XElement ToXml(T element, WriterSettings settings);
	XElement IAlphaXElementWriter.ToXml(VmElement element, WriterSettings settings) => ToXml((T)element, settings);
}
