using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

namespace P2XMLEditor.Suggestions.Refactoring;

[Refactoring("Refactor/Clear default boolean values")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public class ClearDefaultBooleanValues(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		foreach (var parameterHolder in Vm.GetElementsByType<ParameterHolder>()) {
			if (parameterHolder.Static == false)
				parameterHolder.Static = null;
		}
		foreach (var gameObject in Vm.GetElementsByType<GameObject>()) 
			if (gameObject.Instantiated == false)
				gameObject.Instantiated = null;
		foreach (var actionLine in Vm.GetElementsByType<ActionLine>())
			if (actionLine.LoopInfo is { Random: false }) {
				var al = actionLine.LoopInfo.Value;
				al.Random = null;
				actionLine.LoopInfo = al;
			}
		foreach (var graphElement in Vm.GetElementsByType<Branch>().Cast<IGraphElement>().
			         Concat(Vm.GetElementsByType<Graph>()).Concat(Vm.GetElementsByType<State>())) {
			if (graphElement.IgnoreBlock == false)
				graphElement.IgnoreBlock = null;
			if (graphElement.Initial == false)
				graphElement.Initial = null;
		}
		foreach (var talking in Vm.GetElementsByType<Talking>()) {
			if (talking.IgnoreBlock == false)
				talking.IgnoreBlock = null;
			if (talking.Initial == false)
				talking.Initial = null;
		}
		foreach (var speech in Vm.GetElementsByType<Speech>()) {
			if (speech.IgnoreBlock == false)
				speech.IgnoreBlock = null;
			if (speech.Initial == false)
				speech.Initial = null;
			if (speech.OnlyOnce == false)
				speech.OnlyOnce = null;
			if (speech.IsTrade == false)
				speech.IsTrade = null;
		}
		foreach (var reply in Vm.GetElementsByType<Reply>()) {
			if (reply.OnlyOneReply == false)
				reply.OnlyOneReply = null;
			if (reply.Default == false)
				reply.Default = null;
			if (reply.OnlyOnce == false)
				reply.OnlyOnce = null;
		}
		foreach (var parameter in Vm.GetElementsByType<Parameter>()) {
			if (parameter.Implicit == false)
				parameter.Implicit = null;
			if (parameter.Custom == false)
				parameter.Custom = null;
		}
		foreach (var @event in Vm.GetElementsByType<Event>()) {
			if (@event.Manual == true) // INTENDED
				@event.Manual = null;
			if (@event.Repeated == true) // INTENDED
				@event.Repeated = null;
			if (@event.ChangeTo == true) // INTENDED
				@event.ChangeTo = null;
		}
		foreach (var @event in Vm.GetElementsByType<GraphLink>()) {
			if (@event.Enabled == true) // INTENDED
				@event.Enabled = null;
		}
		foreach (var expression in Vm.GetElementsByType<Expression>()) 
			if (expression.Inversion == false)
				expression.Inversion = null;
		foreach (var functionalComponent in Vm.GetElementsByType<FunctionalComponent>()) 
			if (functionalComponent.Main == false)
				functionalComponent.Main = null;
		foreach (var gameMode in Vm.GetElementsByType<GameMode>()) 
			if (gameMode.IsMain == false)
				gameMode.IsMain = null;
		foreach (var gameMode in Vm.GetElementsByType<GameMode>()) 
			if (gameMode.IsMain == false)
				gameMode.IsMain = null;
	}
}