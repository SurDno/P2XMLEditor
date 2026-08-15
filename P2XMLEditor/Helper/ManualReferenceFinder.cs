using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Helper;

public static class ManualReferenceFinder {
    public static IEnumerable<object> ExtractReferences(VmElement obj) {
        switch(obj.GetType().FullName) {
            case "P2XMLEditor.GameData.VirtualMachineElements.Action":
                var t_Action = (P2XMLEditor.GameData.VirtualMachineElements.Action)obj;
                if (t_Action.EventToRaise != null) yield return t_Action.EventToRaise;
                if (t_Action.SourceExpression != null) yield return t_Action.SourceExpression;
                if (t_Action.TargetObject.Holder != null) yield return t_Action.TargetObject.Holder;
                if (t_Action.TargetObject.ParameterRef != null) yield return t_Action.TargetObject.ParameterRef;
                if (t_Action.TargetObject.Hierarchy != null) yield return t_Action.TargetObject.Hierarchy;
                if (t_Action.Source.HasValue) yield return t_Action.Source.Value;
                if (t_Action.EventParams != null) foreach (var item in t_Action.EventParams) yield return item;
                if (t_Action.Name != null) yield return t_Action.Name;
                if (t_Action.LocalContext.Element != null) yield return t_Action.LocalContext.Element;
                if (t_Action.TargetFuncName != null) yield return t_Action.TargetFuncName;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.ActionLine":
                var t_ActionLine = (P2XMLEditor.GameData.VirtualMachineElements.ActionLine)obj;
                if (t_ActionLine.Actions != null) foreach (var item in t_ActionLine.Actions) if (item.Element != null) yield return item.Element;
                if (t_ActionLine.Name != null) yield return t_ActionLine.Name;
                if (t_ActionLine.LocalContext.Element != null) yield return t_ActionLine.LocalContext.Element;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Blueprint":
                var t_Blueprint = (P2XMLEditor.GameData.VirtualMachineElements.Blueprint)obj;
                if (t_Blueprint.FunctionalComponents != null) foreach (var item in t_Blueprint.FunctionalComponents) if (item != null) yield return item;
                if (t_Blueprint.EventGraph != null) yield return t_Blueprint.EventGraph;
                if (t_Blueprint.StandartParams != null) foreach (var kvp in t_Blueprint.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Blueprint.CustomParams != null) foreach (var kvp in t_Blueprint.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Blueprint.GameTimeContext != null) yield return t_Blueprint.GameTimeContext;
                if (t_Blueprint.Name != null) yield return t_Blueprint.Name;
                if (t_Blueprint.Parent != null) yield return t_Blueprint.Parent;
                if (t_Blueprint.InheritanceInfo != null) foreach (var item in t_Blueprint.InheritanceInfo) if (item != null) yield return item;
                if (t_Blueprint.Events != null) foreach (var item in t_Blueprint.Events) if (item != null) yield return item;
                if (t_Blueprint.ChildObjects != null) foreach (var item in t_Blueprint.ChildObjects) if (item != null) yield return item;
                if (t_Blueprint.ParamId != null) yield return t_Blueprint.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Branch":
                var t_Branch = (P2XMLEditor.GameData.VirtualMachineElements.Branch)obj;
                if (t_Branch.BranchConditions != null) foreach (var item in t_Branch.BranchConditions) if (item.Element != null) yield return item.Element;
                if (t_Branch.Parent.Element != null) yield return t_Branch.Parent.Element;
                if (t_Branch.EntryPoints != null) foreach (var item in t_Branch.EntryPoints) if (item != null) yield return item;
                if (t_Branch.InputLinks != null) foreach (var item in t_Branch.InputLinks) if (item != null) yield return item;
                if (t_Branch.OutputLinks != null) foreach (var item in t_Branch.OutputLinks) if (item != null) yield return item;
                if (t_Branch.Owner != null) yield return t_Branch.Owner;
                if (t_Branch.Name != null) yield return t_Branch.Name;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Character":
                var t_Character = (P2XMLEditor.GameData.VirtualMachineElements.Character)obj;
                if (t_Character.WorldPositionGuid != null) yield return t_Character.WorldPositionGuid;
                if (t_Character.EngineTemplateId != null) yield return t_Character.EngineTemplateId;
                if (t_Character.EngineBaseTemplateId != null) yield return t_Character.EngineBaseTemplateId;
                if (t_Character.FunctionalComponents != null) foreach (var item in t_Character.FunctionalComponents) if (item != null) yield return item;
                if (t_Character.EventGraph != null) yield return t_Character.EventGraph;
                if (t_Character.StandartParams != null) foreach (var kvp in t_Character.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Character.CustomParams != null) foreach (var kvp in t_Character.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Character.GameTimeContext != null) yield return t_Character.GameTimeContext;
                if (t_Character.Name != null) yield return t_Character.Name;
                if (t_Character.Parent != null) yield return t_Character.Parent;
                if (t_Character.InheritanceInfo != null) foreach (var item in t_Character.InheritanceInfo) if (item != null) yield return item;
                if (t_Character.Events != null) foreach (var item in t_Character.Events) if (item != null) yield return item;
                if (t_Character.ChildObjects != null) foreach (var item in t_Character.ChildObjects) if (item != null) yield return item;
                if (t_Character.ParamId != null) yield return t_Character.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Condition":
                var t_Condition = (P2XMLEditor.GameData.VirtualMachineElements.Condition)obj;
                if (t_Condition.Predicates != null) foreach (var item in t_Condition.Predicates) if (item.Element != null) yield return item.Element;
                if (t_Condition.Name != null) yield return t_Condition.Name;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.CustomType":
                var t_CustomType = (P2XMLEditor.GameData.VirtualMachineElements.CustomType)obj;
                if (t_CustomType.Name != null) yield return t_CustomType.Name;
                if (t_CustomType.Parent != null) yield return t_CustomType.Parent;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.EntryPoint":
                var t_EntryPoint = (P2XMLEditor.GameData.VirtualMachineElements.EntryPoint)obj;
                if (t_EntryPoint.Name != null) yield return t_EntryPoint.Name;
                if (t_EntryPoint.ActionLine != null) yield return t_EntryPoint.ActionLine;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Event":
                var t_Event = (P2XMLEditor.GameData.VirtualMachineElements.Event)obj;
                if (t_Event.Name != null) yield return t_Event.Name;
                if (t_Event.Parent.Element != null) yield return t_Event.Parent.Element;
                if (t_Event.EventParameter != null) yield return t_Event.EventParameter;
                if (t_Event.Condition != null) yield return t_Event.Condition;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Expression":
                var t_Expression = (P2XMLEditor.GameData.VirtualMachineElements.Expression)obj;
                if (t_Expression.Const != null) yield return t_Expression.Const;
                if (t_Expression.LocalContext.Element != null) yield return t_Expression.LocalContext.Element;
                if (t_Expression.FormulaChilds != null) foreach (var item in t_Expression.FormulaChilds) if (item != null) yield return item;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.FunctionalComponent":
                var t_FunctionalComponent = (P2XMLEditor.GameData.VirtualMachineElements.FunctionalComponent)obj;
                if (t_FunctionalComponent.Events != null) foreach (var item in t_FunctionalComponent.Events) if (item != null) yield return item;
                if (t_FunctionalComponent.Name != null) yield return t_FunctionalComponent.Name;
                if (t_FunctionalComponent.Parent != null) yield return t_FunctionalComponent.Parent;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.GameMode":
                var t_GameMode = (P2XMLEditor.GameData.VirtualMachineElements.GameMode)obj;
                if (t_GameMode.PlayerRef != null) yield return t_GameMode.PlayerRef;
                if (t_GameMode.Name != null) yield return t_GameMode.Name;
                if (t_GameMode.Parent != null) yield return t_GameMode.Parent;
                if (t_GameMode.ParamId != null) yield return t_GameMode.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.GameRoot":
                var t_GameRoot = (P2XMLEditor.GameData.VirtualMachineElements.GameRoot)obj;
                if (t_GameRoot.Samples != null) foreach (var item in t_GameRoot.Samples) if (item != null) yield return item;
                if (t_GameRoot.LogicMaps != null) foreach (var item in t_GameRoot.LogicMaps) if (item != null) yield return item;
                if (t_GameRoot.GameModes != null) foreach (var item in t_GameRoot.GameModes) if (item != null) yield return item;
                if (t_GameRoot.BaseToEngineGuidsTable != null) foreach (var kvp in t_GameRoot.BaseToEngineGuidsTable) if (kvp.Value != null) yield return kvp.Value;
                if (t_GameRoot.HierarchyEngineGuidsTable != null) foreach (var item in t_GameRoot.HierarchyEngineGuidsTable) if (item != null) yield return item;
                if (t_GameRoot.FunctionalComponents != null) foreach (var item in t_GameRoot.FunctionalComponents) if (item != null) yield return item;
                if (t_GameRoot.EventGraph != null) yield return t_GameRoot.EventGraph;
                if (t_GameRoot.StandartParams != null) foreach (var kvp in t_GameRoot.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_GameRoot.CustomParams != null) foreach (var kvp in t_GameRoot.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_GameRoot.GameTimeContext != null) yield return t_GameRoot.GameTimeContext;
                if (t_GameRoot.Name != null) yield return t_GameRoot.Name;
                if (t_GameRoot.Parent != null) yield return t_GameRoot.Parent;
                if (t_GameRoot.InheritanceInfo != null) foreach (var item in t_GameRoot.InheritanceInfo) if (item != null) yield return item;
                if (t_GameRoot.Events != null) foreach (var item in t_GameRoot.Events) if (item != null) yield return item;
                if (t_GameRoot.ChildObjects != null) foreach (var item in t_GameRoot.ChildObjects) if (item != null) yield return item;
                if (t_GameRoot.ParamId != null) yield return t_GameRoot.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.GameString":
                var t_GameString = (P2XMLEditor.GameData.VirtualMachineElements.GameString)obj;
                if (t_GameString.Parent.Element != null) yield return t_GameString.Parent.Element;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Geom":
                var t_Geom = (P2XMLEditor.GameData.VirtualMachineElements.Geom)obj;
                if (t_Geom.WorldPositionGuid != null) yield return t_Geom.WorldPositionGuid;
                if (t_Geom.EngineTemplateId != null) yield return t_Geom.EngineTemplateId;
                if (t_Geom.EngineBaseTemplateId != null) yield return t_Geom.EngineBaseTemplateId;
                if (t_Geom.FunctionalComponents != null) foreach (var item in t_Geom.FunctionalComponents) if (item != null) yield return item;
                if (t_Geom.EventGraph != null) yield return t_Geom.EventGraph;
                if (t_Geom.StandartParams != null) foreach (var kvp in t_Geom.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Geom.CustomParams != null) foreach (var kvp in t_Geom.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Geom.GameTimeContext != null) yield return t_Geom.GameTimeContext;
                if (t_Geom.Name != null) yield return t_Geom.Name;
                if (t_Geom.Parent != null) yield return t_Geom.Parent;
                if (t_Geom.InheritanceInfo != null) foreach (var item in t_Geom.InheritanceInfo) if (item != null) yield return item;
                if (t_Geom.Events != null) foreach (var item in t_Geom.Events) if (item != null) yield return item;
                if (t_Geom.ChildObjects != null) foreach (var item in t_Geom.ChildObjects) if (item != null) yield return item;
                if (t_Geom.ParamId != null) yield return t_Geom.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Graph":
                var t_Graph = (P2XMLEditor.GameData.VirtualMachineElements.Graph)obj;
                if (t_Graph.States != null) foreach (var item in t_Graph.States) if (item.Element != null) yield return item.Element;
                if (t_Graph.EventLinks != null) foreach (var item in t_Graph.EventLinks) if (item != null) yield return item;
                if (t_Graph.Parent.Element != null) yield return t_Graph.Parent.Element;
                if (t_Graph.EntryPoints != null) foreach (var item in t_Graph.EntryPoints) if (item != null) yield return item;
                if (t_Graph.InputLinks != null) foreach (var item in t_Graph.InputLinks) if (item != null) yield return item;
                if (t_Graph.OutputLinks != null) foreach (var item in t_Graph.OutputLinks) if (item != null) yield return item;
                if (t_Graph.Owner != null) yield return t_Graph.Owner;
                if (t_Graph.Name != null) yield return t_Graph.Name;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.GraphLink":
                var t_GraphLink = (P2XMLEditor.GameData.VirtualMachineElements.GraphLink)obj;
                if (t_GraphLink.Event != null) yield return t_GraphLink.Event;
                if (t_GraphLink.SourceParams != null) foreach (var item in t_GraphLink.SourceParams) if (item != null) yield return item;
                if (t_GraphLink.Name != null) yield return t_GraphLink.Name;
                if (t_GraphLink.Parent.Element != null) yield return t_GraphLink.Parent.Element;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Item":
                var t_Item = (P2XMLEditor.GameData.VirtualMachineElements.Item)obj;
                if (t_Item.WorldPositionGuid != null) yield return t_Item.WorldPositionGuid;
                if (t_Item.EngineTemplateId != null) yield return t_Item.EngineTemplateId;
                if (t_Item.EngineBaseTemplateId != null) yield return t_Item.EngineBaseTemplateId;
                if (t_Item.FunctionalComponents != null) foreach (var item in t_Item.FunctionalComponents) if (item != null) yield return item;
                if (t_Item.EventGraph != null) yield return t_Item.EventGraph;
                if (t_Item.StandartParams != null) foreach (var kvp in t_Item.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Item.CustomParams != null) foreach (var kvp in t_Item.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Item.GameTimeContext != null) yield return t_Item.GameTimeContext;
                if (t_Item.Name != null) yield return t_Item.Name;
                if (t_Item.Parent != null) yield return t_Item.Parent;
                if (t_Item.InheritanceInfo != null) foreach (var item in t_Item.InheritanceInfo) if (item != null) yield return item;
                if (t_Item.Events != null) foreach (var item in t_Item.Events) if (item != null) yield return item;
                if (t_Item.ChildObjects != null) foreach (var item in t_Item.ChildObjects) if (item != null) yield return item;
                if (t_Item.ParamId != null) yield return t_Item.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.MindMap":
                var t_MindMap = (P2XMLEditor.GameData.VirtualMachineElements.MindMap)obj;
                if (t_MindMap.Name != null) yield return t_MindMap.Name;
                if (t_MindMap.Title != null) yield return t_MindMap.Title;
                if (t_MindMap.Parent != null) yield return t_MindMap.Parent;
                if (t_MindMap.Nodes != null) foreach (var item in t_MindMap.Nodes) if (item != null) yield return item;
                if (t_MindMap.Links != null) foreach (var item in t_MindMap.Links) if (item != null) yield return item;
                if (t_MindMap.TextObjects != null) foreach (var item in t_MindMap.TextObjects) if (item != null) yield return item;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.MindMapLink":
                var t_MindMapLink = (P2XMLEditor.GameData.VirtualMachineElements.MindMapLink)obj;
                if (t_MindMapLink.Parent != null) yield return t_MindMapLink.Parent;
                if (t_MindMapLink.Source != null) yield return t_MindMapLink.Source;
                if (t_MindMapLink.Destination != null) yield return t_MindMapLink.Destination;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.MindMapNode":
                var t_MindMapNode = (P2XMLEditor.GameData.VirtualMachineElements.MindMapNode)obj;
                if (t_MindMapNode.Name != null) yield return t_MindMapNode.Name;
                if (t_MindMapNode.Parent != null) yield return t_MindMapNode.Parent;
                if (t_MindMapNode.Content != null) foreach (var item in t_MindMapNode.Content) if (item != null) yield return item;
                if (t_MindMapNode.InputLinks != null) foreach (var item in t_MindMapNode.InputLinks) if (item != null) yield return item;
                if (t_MindMapNode.OutputLinks != null) foreach (var item in t_MindMapNode.OutputLinks) if (item != null) yield return item;
                if (t_MindMapNode.NodeNameText != null) yield return t_MindMapNode.NodeNameText;
                if (t_MindMapNode.NodeDescriptionText != null) yield return t_MindMapNode.NodeDescriptionText;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.MindMapNodeContent":
                var t_MindMapNodeContent = (P2XMLEditor.GameData.VirtualMachineElements.MindMapNodeContent)obj;
                if (t_MindMapNodeContent.Parent != null) yield return t_MindMapNodeContent.Parent;
                if (t_MindMapNodeContent.ContentDescriptionText != null) yield return t_MindMapNodeContent.ContentDescriptionText;
                if (t_MindMapNodeContent.ContentPicture != null) yield return t_MindMapNodeContent.ContentPicture;
                if (t_MindMapNodeContent.ContentCondition != null) yield return t_MindMapNodeContent.ContentCondition;
                if (t_MindMapNodeContent.Name != null) yield return t_MindMapNodeContent.Name;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Other":
                var t_Other = (P2XMLEditor.GameData.VirtualMachineElements.Other)obj;
                if (t_Other.WorldPositionGuid != null) yield return t_Other.WorldPositionGuid;
                if (t_Other.EngineTemplateId != null) yield return t_Other.EngineTemplateId;
                if (t_Other.EngineBaseTemplateId != null) yield return t_Other.EngineBaseTemplateId;
                if (t_Other.FunctionalComponents != null) foreach (var item in t_Other.FunctionalComponents) if (item != null) yield return item;
                if (t_Other.EventGraph != null) yield return t_Other.EventGraph;
                if (t_Other.StandartParams != null) foreach (var kvp in t_Other.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Other.CustomParams != null) foreach (var kvp in t_Other.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Other.GameTimeContext != null) yield return t_Other.GameTimeContext;
                if (t_Other.Name != null) yield return t_Other.Name;
                if (t_Other.Parent != null) yield return t_Other.Parent;
                if (t_Other.InheritanceInfo != null) foreach (var item in t_Other.InheritanceInfo) if (item != null) yield return item;
                if (t_Other.Events != null) foreach (var item in t_Other.Events) if (item != null) yield return item;
                if (t_Other.ChildObjects != null) foreach (var item in t_Other.ChildObjects) if (item != null) yield return item;
                if (t_Other.ParamId != null) yield return t_Other.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Parameter":
                var t_Parameter = (P2XMLEditor.GameData.VirtualMachineElements.Parameter)obj;
                if (t_Parameter.Name != null) yield return t_Parameter.Name;
                if (t_Parameter.OwnerComponent != null) yield return t_Parameter.OwnerComponent;
                if (t_Parameter.Type != null) yield return t_Parameter.Type;
                if (t_Parameter.SerializedValue != null) yield return t_Parameter.SerializedValue;
                if (t_Parameter.ParamId != null) yield return t_Parameter.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.PartCondition":
                var t_PartCondition = (P2XMLEditor.GameData.VirtualMachineElements.PartCondition)obj;
                if (t_PartCondition.Name != null) yield return t_PartCondition.Name;
                if (t_PartCondition.FirstExpression != null) yield return t_PartCondition.FirstExpression;
                if (t_PartCondition.SecondExpression != null) yield return t_PartCondition.SecondExpression;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Quest":
                var t_Quest = (P2XMLEditor.GameData.VirtualMachineElements.Quest)obj;
                if (t_Quest.StartEvent != null) yield return t_Quest.StartEvent;
                if (t_Quest.FunctionalComponents != null) foreach (var item in t_Quest.FunctionalComponents) if (item != null) yield return item;
                if (t_Quest.EventGraph != null) yield return t_Quest.EventGraph;
                if (t_Quest.StandartParams != null) foreach (var kvp in t_Quest.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Quest.CustomParams != null) foreach (var kvp in t_Quest.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Quest.GameTimeContext != null) yield return t_Quest.GameTimeContext;
                if (t_Quest.Name != null) yield return t_Quest.Name;
                if (t_Quest.Parent != null) yield return t_Quest.Parent;
                if (t_Quest.InheritanceInfo != null) foreach (var item in t_Quest.InheritanceInfo) if (item != null) yield return item;
                if (t_Quest.Events != null) foreach (var item in t_Quest.Events) if (item != null) yield return item;
                if (t_Quest.ChildObjects != null) foreach (var item in t_Quest.ChildObjects) if (item != null) yield return item;
                if (t_Quest.ParamId != null) yield return t_Quest.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Reply":
                var t_Reply = (P2XMLEditor.GameData.VirtualMachineElements.Reply)obj;
                if (t_Reply.Name != null) yield return t_Reply.Name;
                if (t_Reply.Text != null) yield return t_Reply.Text;
                if (t_Reply.EnableCondition != null) yield return t_Reply.EnableCondition;
                if (t_Reply.ActionLine != null) yield return t_Reply.ActionLine;
                if (t_Reply.Parent != null) yield return t_Reply.Parent;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Sample":
                var t_Sample = (P2XMLEditor.GameData.VirtualMachineElements.Sample)obj;
                if (t_Sample.EngineId != null) yield return t_Sample.EngineId;
                if (t_Sample.ParamId != null) yield return t_Sample.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Scene":
                var t_Scene = (P2XMLEditor.GameData.VirtualMachineElements.Scene)obj;
                if (t_Scene.WorldPositionGuid != null) yield return t_Scene.WorldPositionGuid;
                if (t_Scene.EngineTemplateId != null) yield return t_Scene.EngineTemplateId;
                if (t_Scene.EngineBaseTemplateId != null) yield return t_Scene.EngineBaseTemplateId;
                if (t_Scene.FunctionalComponents != null) foreach (var item in t_Scene.FunctionalComponents) if (item != null) yield return item;
                if (t_Scene.EventGraph != null) yield return t_Scene.EventGraph;
                if (t_Scene.StandartParams != null) foreach (var kvp in t_Scene.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Scene.CustomParams != null) foreach (var kvp in t_Scene.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_Scene.GameTimeContext != null) yield return t_Scene.GameTimeContext;
                if (t_Scene.Name != null) yield return t_Scene.Name;
                if (t_Scene.Parent != null) yield return t_Scene.Parent;
                if (t_Scene.InheritanceInfo != null) foreach (var item in t_Scene.InheritanceInfo) if (item != null) yield return item;
                if (t_Scene.Events != null) foreach (var item in t_Scene.Events) if (item != null) yield return item;
                if (t_Scene.ChildObjects != null) foreach (var item in t_Scene.ChildObjects) if (item != null) yield return item;
                if (t_Scene.ParamId != null) yield return t_Scene.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Speech":
                var t_Speech = (P2XMLEditor.GameData.VirtualMachineElements.Speech)obj;
                if (t_Speech.Replies != null) foreach (var item in t_Speech.Replies) if (item != null) yield return item;
                if (t_Speech.Text != null) yield return t_Speech.Text;
                if (t_Speech.AuthorGuid.Element != null) yield return t_Speech.AuthorGuid.Element;
                if (t_Speech.EntryPoints != null) foreach (var item in t_Speech.EntryPoints) if (item != null) yield return item;
                if (t_Speech.Owner.Element != null) yield return t_Speech.Owner.Element;
                if (t_Speech.InputLinks != null) foreach (var item in t_Speech.InputLinks) if (item != null) yield return item;
                if (t_Speech.OutputLinks != null) foreach (var item in t_Speech.OutputLinks) if (item != null) yield return item;
                if (t_Speech.Name != null) yield return t_Speech.Name;
                if (t_Speech.Parent != null) yield return t_Speech.Parent;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.State":
                var t_State = (P2XMLEditor.GameData.VirtualMachineElements.State)obj;
                if (t_State.Parent != null) yield return t_State.Parent;
                if (t_State.EntryPoints != null) foreach (var item in t_State.EntryPoints) if (item != null) yield return item;
                if (t_State.InputLinks != null) foreach (var item in t_State.InputLinks) if (item != null) yield return item;
                if (t_State.OutputLinks != null) foreach (var item in t_State.OutputLinks) if (item != null) yield return item;
                if (t_State.Owner != null) yield return t_State.Owner;
                if (t_State.Name != null) yield return t_State.Name;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Talking":
                var t_Talking = (P2XMLEditor.GameData.VirtualMachineElements.Talking)obj;
                if (t_Talking.States != null) foreach (var item in t_Talking.States) if (item.Element != null) yield return item.Element;
                if (t_Talking.EventLinks != null) foreach (var item in t_Talking.EventLinks) if (item != null) yield return item;
                if (t_Talking.EntryPoints != null) foreach (var item in t_Talking.EntryPoints) if (item != null) yield return item;
                if (t_Talking.InputLinks != null) foreach (var item in t_Talking.InputLinks) if (item != null) yield return item;
                if (t_Talking.Owner.Element != null) yield return t_Talking.Owner.Element;
                if (t_Talking.Name != null) yield return t_Talking.Name;
                if (t_Talking.Parent != null) yield return t_Talking.Parent;
                if (t_Talking.ParamId != null) yield return t_Talking.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ActionLinePlaceholder":
                var t_ActionLinePlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ActionLinePlaceholder)obj;
                if (t_ActionLinePlaceholder.Actions != null) foreach (var item in t_ActionLinePlaceholder.Actions) if (item.Element != null) yield return item.Element;
                if (t_ActionLinePlaceholder.Name != null) yield return t_ActionLinePlaceholder.Name;
                if (t_ActionLinePlaceholder.LocalContext.Element != null) yield return t_ActionLinePlaceholder.LocalContext.Element;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.CharacterPlaceholder":
                var t_CharacterPlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.CharacterPlaceholder)obj;
                if (t_CharacterPlaceholder.WorldPositionGuid != null) yield return t_CharacterPlaceholder.WorldPositionGuid;
                if (t_CharacterPlaceholder.EngineTemplateId != null) yield return t_CharacterPlaceholder.EngineTemplateId;
                if (t_CharacterPlaceholder.EngineBaseTemplateId != null) yield return t_CharacterPlaceholder.EngineBaseTemplateId;
                if (t_CharacterPlaceholder.FunctionalComponents != null) foreach (var item in t_CharacterPlaceholder.FunctionalComponents) if (item != null) yield return item;
                if (t_CharacterPlaceholder.EventGraph != null) yield return t_CharacterPlaceholder.EventGraph;
                if (t_CharacterPlaceholder.StandartParams != null) foreach (var kvp in t_CharacterPlaceholder.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_CharacterPlaceholder.CustomParams != null) foreach (var kvp in t_CharacterPlaceholder.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_CharacterPlaceholder.GameTimeContext != null) yield return t_CharacterPlaceholder.GameTimeContext;
                if (t_CharacterPlaceholder.Name != null) yield return t_CharacterPlaceholder.Name;
                if (t_CharacterPlaceholder.Parent != null) yield return t_CharacterPlaceholder.Parent;
                if (t_CharacterPlaceholder.InheritanceInfo != null) foreach (var item in t_CharacterPlaceholder.InheritanceInfo) if (item != null) yield return item;
                if (t_CharacterPlaceholder.Events != null) foreach (var item in t_CharacterPlaceholder.Events) if (item != null) yield return item;
                if (t_CharacterPlaceholder.ChildObjects != null) foreach (var item in t_CharacterPlaceholder.ChildObjects) if (item != null) yield return item;
                if (t_CharacterPlaceholder.ParamId != null) yield return t_CharacterPlaceholder.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.GraphPlaceholder":
                var t_GraphPlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.GraphPlaceholder)obj;
                if (t_GraphPlaceholder.States != null) foreach (var item in t_GraphPlaceholder.States) if (item.Element != null) yield return item.Element;
                if (t_GraphPlaceholder.EventLinks != null) foreach (var item in t_GraphPlaceholder.EventLinks) if (item != null) yield return item;
                if (t_GraphPlaceholder.Parent.Element != null) yield return t_GraphPlaceholder.Parent.Element;
                if (t_GraphPlaceholder.EntryPoints != null) foreach (var item in t_GraphPlaceholder.EntryPoints) if (item != null) yield return item;
                if (t_GraphPlaceholder.InputLinks != null) foreach (var item in t_GraphPlaceholder.InputLinks) if (item != null) yield return item;
                if (t_GraphPlaceholder.OutputLinks != null) foreach (var item in t_GraphPlaceholder.OutputLinks) if (item != null) yield return item;
                if (t_GraphPlaceholder.Owner != null) yield return t_GraphPlaceholder.Owner;
                if (t_GraphPlaceholder.Name != null) yield return t_GraphPlaceholder.Name;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ItemPlaceholder":
                var t_ItemPlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ItemPlaceholder)obj;
                if (t_ItemPlaceholder.WorldPositionGuid != null) yield return t_ItemPlaceholder.WorldPositionGuid;
                if (t_ItemPlaceholder.EngineTemplateId != null) yield return t_ItemPlaceholder.EngineTemplateId;
                if (t_ItemPlaceholder.EngineBaseTemplateId != null) yield return t_ItemPlaceholder.EngineBaseTemplateId;
                if (t_ItemPlaceholder.FunctionalComponents != null) foreach (var item in t_ItemPlaceholder.FunctionalComponents) if (item != null) yield return item;
                if (t_ItemPlaceholder.EventGraph != null) yield return t_ItemPlaceholder.EventGraph;
                if (t_ItemPlaceholder.StandartParams != null) foreach (var kvp in t_ItemPlaceholder.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_ItemPlaceholder.CustomParams != null) foreach (var kvp in t_ItemPlaceholder.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_ItemPlaceholder.GameTimeContext != null) yield return t_ItemPlaceholder.GameTimeContext;
                if (t_ItemPlaceholder.Name != null) yield return t_ItemPlaceholder.Name;
                if (t_ItemPlaceholder.Parent != null) yield return t_ItemPlaceholder.Parent;
                if (t_ItemPlaceholder.InheritanceInfo != null) foreach (var item in t_ItemPlaceholder.InheritanceInfo) if (item != null) yield return item;
                if (t_ItemPlaceholder.Events != null) foreach (var item in t_ItemPlaceholder.Events) if (item != null) yield return item;
                if (t_ItemPlaceholder.ChildObjects != null) foreach (var item in t_ItemPlaceholder.ChildObjects) if (item != null) yield return item;
                if (t_ItemPlaceholder.ParamId != null) yield return t_ItemPlaceholder.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ParameterPlaceholder":
                var t_ParameterPlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ParameterPlaceholder)obj;
                if (t_ParameterPlaceholder.Name != null) yield return t_ParameterPlaceholder.Name;
                if (t_ParameterPlaceholder.OwnerComponent != null) yield return t_ParameterPlaceholder.OwnerComponent;
                if (t_ParameterPlaceholder.Type != null) yield return t_ParameterPlaceholder.Type;
                if (t_ParameterPlaceholder.SerializedValue != null) yield return t_ParameterPlaceholder.SerializedValue;
                if (t_ParameterPlaceholder.ParamId != null) yield return t_ParameterPlaceholder.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ScenePlaceholder":
                var t_ScenePlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ScenePlaceholder)obj;
                if (t_ScenePlaceholder.WorldPositionGuid != null) yield return t_ScenePlaceholder.WorldPositionGuid;
                if (t_ScenePlaceholder.EngineTemplateId != null) yield return t_ScenePlaceholder.EngineTemplateId;
                if (t_ScenePlaceholder.EngineBaseTemplateId != null) yield return t_ScenePlaceholder.EngineBaseTemplateId;
                if (t_ScenePlaceholder.FunctionalComponents != null) foreach (var item in t_ScenePlaceholder.FunctionalComponents) if (item != null) yield return item;
                if (t_ScenePlaceholder.EventGraph != null) yield return t_ScenePlaceholder.EventGraph;
                if (t_ScenePlaceholder.StandartParams != null) foreach (var kvp in t_ScenePlaceholder.StandartParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_ScenePlaceholder.CustomParams != null) foreach (var kvp in t_ScenePlaceholder.CustomParams) if (kvp.Value != null) yield return kvp.Value;
                if (t_ScenePlaceholder.GameTimeContext != null) yield return t_ScenePlaceholder.GameTimeContext;
                if (t_ScenePlaceholder.Name != null) yield return t_ScenePlaceholder.Name;
                if (t_ScenePlaceholder.Parent != null) yield return t_ScenePlaceholder.Parent;
                if (t_ScenePlaceholder.InheritanceInfo != null) foreach (var item in t_ScenePlaceholder.InheritanceInfo) if (item != null) yield return item;
                if (t_ScenePlaceholder.Events != null) foreach (var item in t_ScenePlaceholder.Events) if (item != null) yield return item;
                if (t_ScenePlaceholder.ChildObjects != null) foreach (var item in t_ScenePlaceholder.ChildObjects) if (item != null) yield return item;
                if (t_ScenePlaceholder.ParamId != null) yield return t_ScenePlaceholder.ParamId;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.StatePlaceholder":
                var t_StatePlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.StatePlaceholder)obj;
                if (t_StatePlaceholder.Parent != null) yield return t_StatePlaceholder.Parent;
                if (t_StatePlaceholder.EntryPoints != null) foreach (var item in t_StatePlaceholder.EntryPoints) if (item != null) yield return item;
                if (t_StatePlaceholder.InputLinks != null) foreach (var item in t_StatePlaceholder.InputLinks) if (item != null) yield return item;
                if (t_StatePlaceholder.OutputLinks != null) foreach (var item in t_StatePlaceholder.OutputLinks) if (item != null) yield return item;
                if (t_StatePlaceholder.Owner != null) yield return t_StatePlaceholder.Owner;
                if (t_StatePlaceholder.Name != null) yield return t_StatePlaceholder.Name;
                break;
            case "P2XMLEditor.GameData.VirtualMachineElements.Placeholders.TemplatePlaceholder":
                var t_TemplatePlaceholder = (P2XMLEditor.GameData.VirtualMachineElements.Placeholders.TemplatePlaceholder)obj;
                break;
        }
    }
    private static bool IsReferenceType(Type t) { return typeof(VmElement).IsAssignableFrom(t) || t == typeof(ParameterSource) || t == typeof(string) || t == typeof(HierarchyGuid); }
}
