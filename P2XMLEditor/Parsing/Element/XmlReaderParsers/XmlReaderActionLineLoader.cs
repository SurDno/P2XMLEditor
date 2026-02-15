using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using P2XMLEditor.GameData.VirtualMachineElements.Enums;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Parsing.Helpers.XmlReaderExtensions;

namespace P2XMLEditor.Parsing.Element.XmlReaderParsers;

public class XmlReaderActionLineLoader : IParser<RawActionLineData> {
    [PerformanceLogHook]
    public void ProcessFile(string filePath, List<RawActionLineData> raws) {
        using var xr = InitializeFullFileReader(filePath);
        xr.SkipDeclarationAndRoot();
        
        while (xr.Read()) {
            if (xr.EndOfContainerReached()) break;

            var raw = new RawActionLineData();
            raw.Id = xr.GetIdAndEnter();
            raw.ActionIds = xr.Name == "Actions" ? xr.GetULongListAndAdvance() : null;
            raw.ActionLineType = xr.GetStringValueAndAdvance().Deserialize<ActionLineType>();
            if (xr.Name == "ActionLoopInfo") {
                ReadLoopInfo(xr, ref raw);
            }
            raw.Name = xr.GetOptionalStringValueAndAdvance();
            raw.LocalContextId = xr.GetULongValueAndAdvance();
            raw.OrderIndex = xr.GetIntValueAndAdvance();

            raws.Add(raw);
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ReadLoopInfo(XmlReader xr, ref RawActionLineData raw) {
        xr.Read();
        raw.LoopInfoName = xr.GetStringValueAndAdvance();
        raw.LoopInfoStart = xr.GetStringValueAndAdvance();
        raw.LoopInfoEnd = xr.GetStringValueAndAdvance();
        raw.LoopInfoRandom = xr.Name == "Random" ? bool.Parse(xr.GetStringValueAndAdvance()) : null;
        xr.Read();
    }
}
