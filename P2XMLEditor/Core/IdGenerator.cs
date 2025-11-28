using System;
using System.Collections.Generic;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using Action = P2XMLEditor.GameData.VirtualMachineElements.Action;


namespace P2XMLEditor.Core;

public static class IdGenerator {
	public static readonly Dictionary<Type, byte> OffsetMultiplierByType = new Dictionary<Type, byte> {
		{ typeof(GameRoot), 1 },
		{ typeof(Parameter), 2 },
		{ typeof(Expression), 4},
		{ typeof(PartCondition), 5},
		{ typeof(Condition), 6},
		{ typeof(Action), 7},
		{ typeof(ActionLine), 8},
		{ typeof(EntryPoint), 9},
		{ typeof(Event), 10},
		{ typeof(State), 11},
		{ typeof(Branch), 12},
		{ typeof(Reply), 13},
		{ typeof(Speech), 14},
		{ typeof(GraphLink), 15},
		{ typeof(Graph), 16},
		{ typeof(Talking), 17},
		{ typeof(Quest), 18},
		{ typeof(Character), 19},
		{ typeof(Item), 20},
		{ typeof(Geom), 21},
		{ typeof(GameString), 22},
		{ typeof(Blueprint), 27},
		{ typeof(FunctionalComponent), 28},
		{ typeof(Sample), 31},
		{ typeof(GameMode), 32},
		{ typeof(Scene), 35},
		{ typeof(Other), 36},
		{ typeof(CustomType), 37},
//		{ typeof(Combination), 40},
		{ typeof(MindMap), 41},
		{ typeof(MindMapNode), 42},
		{ typeof(MindMapLink), 43},
		{ typeof(MindMapNodeContent), 44}
	};

	private static ulong GetMinIdForType(Type type) => 
		OffsetMultiplierByType.TryGetValue(type, out var v) ? v * 281470681677825UL : 
		throw new Exception($"Type not supported: {type}");

	public static ulong GetNewId<T>(VirtualMachine vm) {  
		var firstFreeId = GetMinIdForType(typeof(T));
		while (vm.GetNullableElement(firstFreeId) != null) 
			firstFreeId++;
		return firstFreeId;
	}
}