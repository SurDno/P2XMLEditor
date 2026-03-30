using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;
using P2XMLEditor.Writing.Element.ReleaseXElementWriters;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;

namespace P2XMLEditor.Core;

public class VirtualMachineWriter(string vmPath, VirtualMachine virtualMachine) {
    private readonly Dictionary<Type, (string file, Func<VirtualMachine, IEnumerable<VmElement>> Elements)> _typeMapping = new() {
            { typeof(Action), ("Action.xml", vm => vm.GetElementsByType<Action>()) },
            { typeof(ActionLine), ("ActionLine.xml", vm => vm.GetElementsByType<ActionLine>()) },
            { typeof(Blueprint), ("Blueprint.xml", vm => vm.GetElementsByType<Blueprint>()) },
            { typeof(Branch), ("Branch.xml", vm => vm.GetElementsByType<Branch>()) },
            { typeof(Character), ("Character.xml", vm => vm.GetElementsByType<Character>()) },
            { typeof(Condition), ("Condition.xml", vm => vm.GetElementsByType<Condition>()) },
            { typeof(CustomType), ("CustomType.xml", vm => vm.GetElementsByType<CustomType>()) },
            { typeof(EntryPoint), ("EntryPoint.xml", vm => vm.GetElementsByType<EntryPoint>()) },
            { typeof(Event), ("Event.xml", vm => vm.GetElementsByType<Event>()) },
            { typeof(Expression), ("Expression.xml", vm => vm.GetElementsByType<Expression>()) },
            { typeof(FunctionalComponent), ("FunctionalComponent.xml", vm => vm.GetElementsByType<FunctionalComponent>()) },
            { typeof(GameMode), ("GameMode.xml", vm => vm.GetElementsByType<GameMode>()) },
            { typeof(GameRoot), ("GameRoot.xml", vm => vm.GetElementsByType<GameRoot>()) },
            { typeof(GameString), ("GameString.xml", vm => vm.GetElementsByType<GameString>()) },
            { typeof(Geom), ("Geom.xml", vm => vm.GetElementsByType<Geom>()) },
            { typeof(Graph), ("Graph.xml", vm => vm.GetElementsByType<Graph>()) },
            { typeof(GraphLink), ("GraphLink.xml", vm => vm.GetElementsByType<GraphLink>()) },
            { typeof(Item), ("Item.xml", vm => vm.GetElementsByType<Item>()) },
            { typeof(MindMap), ("MindMap.xml", vm => vm.GetElementsByType<MindMap>()) },
            { typeof(MindMapLink), ("MindMapLink.xml", vm => vm.GetElementsByType<MindMapLink>()) },
            { typeof(MindMapNode), ("MindMapNode.xml", vm => vm.GetElementsByType<MindMapNode>()) },
            { typeof(MindMapNodeContent), ("MindMapNodeContent.xml", vm => vm.GetElementsByType<MindMapNodeContent>()) },
            { typeof(Other), ("Other.xml", vm => vm.GetElementsByType<Other>()) },
            { typeof(Parameter), ("Parameter.xml", vm => vm.GetElementsByType<Parameter>()) },
            { typeof(PartCondition), ("PartCondition.xml", vm => vm.GetElementsByType<PartCondition>()) },
            { typeof(Quest), ("Quest.xml", vm => vm.GetElementsByType<Quest>()) },
            { typeof(Reply), ("Reply.xml", vm => vm.GetElementsByType<Reply>()) },
            { typeof(Sample), ("Sample.xml", vm => vm.GetElementsByType<Sample>()) },
            { typeof(Scene), ("Scene.xml", vm => vm.GetElementsByType<Scene>()) },
            { typeof(Speech), ("Speech.xml", vm => vm.GetElementsByType<Speech>()) },
            { typeof(State), ("State.xml", vm => vm.GetElementsByType<State>()) },
            { typeof(Talking), ("Talking.xml", vm => vm.GetElementsByType<Talking>()) }
    };

    private readonly Dictionary<Type, IReleaseXElementWriter> _writers = new() {
        { typeof(Action), new ReleaseXElementActionWriter() },
        { typeof(ActionLine), new ReleaseXElementActionLineWriter() },
        { typeof(Blueprint), new ReleaseXElementBlueprintWriter() },
        { typeof(Branch), new ReleaseXElementBranchWriter() },
        { typeof(Character), new ReleaseXElementCharacterWriter() },
        { typeof(Condition), new ReleaseXElementConditionWriter() },
        { typeof(CustomType), new ReleaseXElementCustomTypeWriter() },
        { typeof(EntryPoint), new ReleaseXElementEntryPointWriter() },
        { typeof(Event), new ReleaseXElementEventWriter() },
        { typeof(Expression), new ReleaseXElementExpressionWriter() },
        { typeof(FunctionalComponent), new ReleaseXElementFunctionalComponentWriter() },
        { typeof(GameMode), new ReleaseXElementGameModeWriter() },
        { typeof(GameRoot), new ReleaseXElementGameRootWriter() },
        { typeof(GameString), new ReleaseXElementGameStringWriter() },
        { typeof(Geom), new ReleaseXElementGeomWriter() },
        { typeof(Graph), new ReleaseXElementGraphWriter() },
        { typeof(GraphLink), new ReleaseXElementGraphLinkWriter() },
        { typeof(Item), new ReleaseXElementItemWriter() },
        { typeof(MindMap), new ReleaseXElementMindMapWriter() },
        { typeof(MindMapLink), new ReleaseXElementMindMapLinkWriter() },
        { typeof(MindMapNode), new ReleaseXElementMindMapNodeWriter() },
        { typeof(MindMapNodeContent), new ReleaseXElementMindMapNodeContentWriter() },
        { typeof(Other), new ReleaseXElementOtherWriter() },
        { typeof(Parameter), new ReleaseXElementParameterWriter() },
        { typeof(PartCondition), new ReleaseXElementPartConditionWriter() },
        { typeof(Quest), new ReleaseXElementQuestWriter() },
        { typeof(Reply), new ReleaseXElementReplyWriter() },
        { typeof(Sample), new ReleaseXElementSampleWriter() },
        { typeof(Scene), new ReleaseXElementSceneWriter() },
        { typeof(Speech), new ReleaseXElementSpeechWriter() },
        { typeof(State), new ReleaseXElementStateWriter() },
        { typeof(Talking), new ReleaseXElementTalkingWriter() }
    };
    

    [PerformanceLogHook]
    public void SaveVirtualMachine(WriterSettings settings) {
        Logger.Log(LogLevel.Info, $"Saving virtual machine.");
        foreach (var (type, (fileName, getElements)) in _typeMapping) {
            SaveElements(fileName, getElements(virtualMachine), type, settings);
        }
        SaveLocalizations();
    }

    private void SaveElements(string fileName, IEnumerable<VmElement> elements, Type elementType, WriterSettings settings) {
        try {
            var elementsList = elements.ToList();
            if (elementsList.Count == 0) return;

            var filePath = Path.Combine(vmPath, fileName);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var root = new XElement("Root",
                new XAttribute("xml_data_format_version", "14"),
                new XAttribute("default_type", elementType.Name),
                elementsList.Select(element => _writers[elementType].ToXml(element, settings))
            );

            var xmlSettings = new XmlWriterSettings { 
                Encoding = Encoding.UTF8, 
                Indent = true, 
                OmitXmlDeclaration = true,
                NewLineChars = "\r\n"
            };
        
            using var stream = File.Create(filePath);
            stream.Write(Encoding.UTF8.GetPreamble());
            stream.Write("""
                         <?xml version="1.0" encoding="UTF-8"?>

                         """u8.ToArray());
            using var writer = XmlWriter.Create(stream, xmlSettings);
            root.Save(writer);
        
            Logger.Log(LogLevel.Info, $"Saved {elementsList.Count} elements of type {elementType.Name} to {fileName}");
        } catch (Exception ex) {
            Logger.Log(LogLevel.Error, $"Error saving {fileName}: {ex.Message}, {ex.StackTrace}");
        }
    }

    private void SaveLocalizations() {
        var locPath = Path.Combine(vmPath, "Localizations");
        Directory.CreateDirectory(locPath);

        foreach (var lang in virtualMachine.Languages) {
            using var writer = new StreamWriter(Path.Combine(locPath, $"{lang}.txt"), false, new UTF8Encoding(true));
            foreach (var gameString in virtualMachine.GetElementsByType<GameString>().OrderBy(gs => gs.Id))
                writer.WriteLine($"{gameString.Id} {gameString.GetText(lang)}");
        }
    }
}