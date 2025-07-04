using P2XMLEditor.Core;

namespace P2XMLEditor.Abstract;

public abstract class Suggestion(VirtualMachine vm) {
	public VirtualMachine _vm = vm;

	public abstract void Execute();
}