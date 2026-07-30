using System;
using P2XMLEditor.Core;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;


public class Message(string name, string type, Event owner) : IEquatable<Message> {
	public string Name { get; } = name;
	public string Type { get; set; } = type;
	public Event Event { get; } = owner;

	public string ParamName => Name.Split(["_message_"], StringSplitOptions.None)[^1];

	public string ParamId => Name;

	public static Message FromApi(Event owner, string paramName, string type) =>
		new($"{owner.Name}_message_{paramName}", type, owner);

	public static bool TryParse(string input, VirtualMachine vm, out Message? result) {
		result = null;
		return input.Contains("_message_") && vm.TryResolveMessage(input, out result);
	}

	public bool Equals(Message? other) => other != null && Name == other.Name && ReferenceEquals(Event, other.Event);

	public override bool Equals(object? obj) => Equals(obj as Message);
	public override int GetHashCode() => HashCode.Combine(Name, Event.Id);
	public override string ToString() => $"{Name} @ {Event.Id}";
}