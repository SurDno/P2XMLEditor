using P2XMLEditor.Core;

namespace P2XMLEditor.GameData.VirtualMachineElements.Interfaces;

public interface IFiller<in TRaw> where TRaw : struct {
	void FillFromRawData(TRaw rawData, VirtualMachine vm);
}