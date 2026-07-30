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

public class Speech(ulong id) : VmElement(id), IFiller<RawSpeechData> {
	public List<Reply> Replies { get; set; }
	public GameString Text { get; set; }
	public VmEither<Blueprint, Character> AuthorGuid { get; set; }
	public bool OnlyOnce { get; set; }
	public bool IsTrade { get; set; }
	public List<EntryPoint> EntryPoints { get; set; }
	public bool IgnoreBlock { get; set; }
	public VmEither<Blueprint, Character> Owner { get; set; }
	public List<GraphLink>? InputLinks { get; set; }
	public List<GraphLink>? OutputLinks { get; set; }
	public bool Initial { get; set; }
	public string Name { get; set; }
	public Talking Parent { get; set; }

	public void FillFromRawData(RawSpeechData data, VirtualMachine vm) {
		Replies = [];
		foreach (var replyId in data.ReplyIds) 
			Replies.Add(vm.GetElement<Reply>(replyId));
		Text = vm.GetElement<GameString>(data.TextId);
		AuthorGuid = vm.GetElement<Blueprint, Character>(data.AuthorGuidId);
		OnlyOnce = data.OnlyOnce;
		IsTrade = data.IsTrade;
		EntryPoints = [];
		foreach (var entryPointId in data.EntryPointIds) 
			EntryPoints.Add(vm.GetElement<EntryPoint>(entryPointId));
		IgnoreBlock = data.IgnoreBlock;
		Owner = vm.GetElement<Blueprint, Character>(data.OwnerId);
		InputLinks = [];
		if (data.InputLinkIds != null) {
			foreach (var inputLinkId in data.InputLinkIds)
				InputLinks.Add(vm.GetElement<GraphLink>(inputLinkId));
		}
		OutputLinks = [];
		if (data.OutputLinkIds != null) {
			foreach (var outputLinkId in data.OutputLinkIds)
				OutputLinks.Add(vm.GetElement<GraphLink>(outputLinkId));
		}
		Initial = data.Initial;
		Name = data.Name;
		Parent = vm.GetElement<Talking>(data.ParentId);
	}
	
	public override bool IsOrphaned() => Parent.States.All(r => r.Element != this);
}
