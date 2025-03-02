using P2XMLEditor.Core;

namespace P2XMLEditor.Abstract;

public abstract class RefactoringSuggestion(VirtualMachine vm) {
	public VirtualMachine _vm = vm;

	public abstract void Execute();
}