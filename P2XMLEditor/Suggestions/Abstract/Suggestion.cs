using P2XMLEditor.Core;

namespace P2XMLEditor.Suggestions.Abstract;

public abstract class Suggestion(VirtualMachine vm) {
	protected readonly VirtualMachine Vm = vm;

	public abstract void Execute();
}