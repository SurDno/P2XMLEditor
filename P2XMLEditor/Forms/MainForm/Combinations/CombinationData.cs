using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Helper;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Forms.MainForm.Combinations;

public interface ICombinationPart {
    int? Probability { get; set; }
}

public sealed partial class CombinationEntry(VmEither<Item, Other> target) : ICombinationPart {
    private int _minAmount = 1;
    private int _maxAmount = 1;
    private int _minDurability = 100;
    private int _maxDurability = 100;
    private int? _probability = 100;

    public VmEither<Item, Other> Target { get; set; } = target;

    public ulong ItemId => Target.Element.Id;

    public int MinAmount {
        get => _minAmount;
        set => _minAmount = Math.Max(0, value);
    }

    public int MaxAmount {
        get => _maxAmount;
        set => _maxAmount = Math.Max(MinAmount, value);
    }

    public int Weight { get; set; } = 1;

    public int MinDurability {
        get => _minDurability;
        set => _minDurability = Math.Max(0, Math.Min(100, value));
    }

    public int MaxDurability {
        get => _maxDurability;
        set => _maxDurability = Math.Max(_minDurability, Math.Min(100, value));
    }
    
    public int? Probability {
        get => _probability;
        set => _probability = value.HasValue ? Math.Max(1, Math.Min(100, value.Value)) : null;
    }

    public override string ToString() {
        return $"{ItemId}END&PAR{MinAmount}END&PAR{MaxAmount}END&PAR{Weight}END&PAR" +
               $"&CI&PARAMS&{MinDurability}&CI&PARAMS&{MaxDurability}&CI&PARAMS&END&PAR";
    }

    
    public static CombinationEntry? Parse(VirtualMachine vm, string element) {
        try {
            var parts = element.Split(["END&PAR"], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) return null;
            
            var durMatch = DurabilityRegex().Matches(element);
            if (durMatch.Count < 2) return null;
            
            var minAmount = int.Parse(new string(parts[1].Where(char.IsDigit).ToArray()));
            var maxAmount = int.Parse(new string(parts[2].Where(char.IsDigit).ToArray()));
            var weight = int.Parse(new string(parts[3].Where(char.IsDigit).ToArray()));
            
            var minDurability = int.Parse(durMatch[0].Groups[1].Value);
            var maxDurability = int.Parse(durMatch[1].Groups[1].Value);
            
            return new CombinationEntry(vm.GetElement<Item, Other>(ulong.Parse(parts[0]))) {
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