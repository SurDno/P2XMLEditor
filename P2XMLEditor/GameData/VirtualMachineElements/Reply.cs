using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.Data;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.Interfaces;
using P2XMLEditor.Helper;
using P2XMLEditor.Parsing.RawData;
using static P2XMLEditor.Helper.XmlParsingHelper;

#pragma warning disable CS8618

namespace P2XMLEditor.GameData.VirtualMachineElements;

public class Reply(ulong id) : VmElement(id), IFiller<RawReplyData> {
	public string Name { get; set; }
	public GameString Text { get; set; }
	public bool? OnlyOnce { get; set; }
	public bool? OnlyOneReply { get; set; }
	public bool? Default { get; set; }
	public Condition? EnableCondition { get; set; }
	public ActionLine? ActionLine { get; set; }
	public int OrderIndex { get; set; }
	public Speech Parent { get; set; }
	
	public void FillFromRawData(RawReplyData data, VirtualMachine vm) {
		Name = data.Name;
		Text = vm.GetElement<GameString>(data.TextId);
		OnlyOnce = data.OnlyOnce;
		OnlyOneReply = data.OnlyOneReply;
		Default = data.Default;
		OrderIndex = data.OrderIndex;
		Parent = vm.GetElement<Speech>(data.ParentId);
		EnableCondition = data.EnableConditionId != null ? vm.GetElement<Condition>(data.EnableConditionId.Value) : null;
		ActionLine = data.ActionLineId != null ? vm.GetElement<ActionLine>(data.ActionLineId.Value) : null;
	}
	
	public override bool IsOrphaned() => Parent.Replies.All(r => r != this);
}
