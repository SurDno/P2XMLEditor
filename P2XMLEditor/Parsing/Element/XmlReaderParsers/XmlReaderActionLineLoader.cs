using System.Runtime.CompilerServices;
using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderActionLineLoader : IParser<RawActionLineData> {
    [PerformanceLogHook]
    public void ProcessFile(string filePath, List<RawActionLineData> raws) {
        using var xr = InitializeFullFileReader(filePath);
        xr.SkipDeclarationAndRoot();
        
        while (xr.Read()) {
            if (xr.EndOfContainerReached()) break;

            var raw = new RawActionLineData {
                Id = xr.GetIdAndEnter(),
                ActionIds = xr.Name == "Actions" ? xr.GetULongListAndAdvance() : null,
                ActionLineType = xr.GetStringValueAndAdvance().Deserialize<ActionLineType>(),
                LoopInfo = xr.Name == "ActionLoopInfo" ? ReadLoopInfo(xr) : null,
                Name = xr.GetOptionalStringValueAndAdvance(),
                LocalContextId = xr.GetULongValueAndAdvance(),
                OrderIndex = xr.GetIntValueAndAdvance()
            };

            raws.Add(raw);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ActionLine.ActionLoopInfo ReadLoopInfo(XmlReader xr) {
        var info = new ActionLine.ActionLoopInfo();
        xr.Read();
        info.Name = xr.GetStringValueAndAdvance();
        info.Start = xr.GetStringValueAndAdvance();
        info.End = xr.GetStringValueAndAdvance();
        info.Random = xr.Name == "Random" ? bool.Parse(xr.GetStringValueAndAdvance()) : null;
        xr.Read();
        return info;
    }
}
