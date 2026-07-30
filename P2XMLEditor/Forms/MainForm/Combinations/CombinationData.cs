using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Placeholders;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Forms.MainForm.Combinations;

public interface ICombinationPart {
	int? Probability { get; set; }
}

public sealed partial class CombinationEntry(VmEither<Item, Other> target) : ICombinationPart {
	private int? _probability = 100;

	public VmEither<Item, Other> Target { get; set; } = target;

	public ulong ItemId => Target.Element.Id;

	public int MinAmount { get; set; } = 1;
	public int MaxAmount { get; set; } = 1;
	public int Weight { get; set; } = 1;
	public int MinDurability { get; set; } = 100;
	public int MaxDurability { get; set; } = 100;

	public int? Probability {
		get => _probability;
		set => _probability = value.HasValue ? Math.Max(1, Math.Min(100, value.Value)) : null;
	}

	public override string ToString() {
		return $"{ItemId}END&PAR{MinAmount}END&PAR{MaxAmount}END&PAR{Weight}END&PAR" +
			   $"&CI&PARAMS&{MinDurability}&CI&PARAMS&{MaxDurability}&CI&PARAMS&END&PAR";
	}
	
	[SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
	private static int ExtractInt(string s) {
		var val = 0;
		foreach (var c in s) {
			if ((uint)(c - '0') <= 9)
				val = val * 10 + (c - '0');
		}
		return val;
	}

	public static CombinationEntry? Parse(VirtualMachine vm, string element) {
		try {
			var parts = element.Split(["END&PAR"], StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length < 4) return null;
			
			var durMatch = DurabilityRegex().Matches(element);
			if (durMatch.Count < 2) return null;
			
			
			var minAmount = ExtractInt(parts[1]);
			var maxAmount = ExtractInt(parts[2]);
			var weight = ExtractInt(parts[3]);
			var minDurability = int.Parse(durMatch[0].Groups[1].Value);
			var maxDurability = int.Parse(durMatch[1].Groups[1].Value);
			
			var id = ulong.Parse(parts[0]);
			var target = vm.GetNullableElement<Item, Other>(id)
			             ?? new VmEither<Item, Other>(vm.Register(new ItemPlaceholder(id)));

			return new CombinationEntry(target) {
				MinAmount = minAmount,
				MaxAmount = maxAmount,
				Weight = weight,
				MinDurability = minDurability,
				MaxDurability = maxDurability
			};
		} catch (Exception ex) {
			Logger.Log(LogLevel.Error, $"Failed to parse combination entry: {ex.Message}");
			return null;
		}
	}
	
	public static CombinationEntry New(VmEither<Item, Other> item) => new(item) {
		MinAmount = 1,
		MaxAmount = 1,
		Weight = 1,
		MinDurability = 100,
		MaxDurability = 100,
		Probability = 100
	};
	
	[GeneratedRegex(@"&CI&PARAMS&(\d+)", RegexOptions.Compiled)]
	private static partial Regex DurabilityRegex();
}

public sealed class CombinationGroup : ICombinationPart {
	private int? _probability = 100;
	
	public List<CombinationEntry> Items { get; init; } = [];
	public bool IsCollapsed { get; set; } = true;
	
	public int? Probability {
		get => _probability;
		set => _probability = value.HasValue ? Math.Max(1, Math.Min(100, value.Value)) : null;
	}

	public override string ToString() {
		var elements = new List<string>();
		
		for (var i = 0; i < Items.Count - 1; i++) 
			elements.Add(Items[i] + "END&VAR");
		
		if (Items.Count != 0)
			elements.Add(Items.Last() + $"END&VARProbability_{Probability}END&ELEM");
			
		return string.Join("", elements);
	}
}
