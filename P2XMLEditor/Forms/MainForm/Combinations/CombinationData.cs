using System.Text.RegularExpressions;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Helper;

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

    public string ItemId => Target.Element.Id;

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
            
            return new CombinationEntry(vm.GetElement<Item, Other>(parts[0])) {
                MinAmount = minAmount,
                MaxAmount = maxAmount,
                Weight = weight,
                MinDurability = minDurability,
                MaxDurability = maxDurability
            };
        } catch (Exception ex) {
            Logger.LogError($"Failed to parse combination entry: {ex.Message}");
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

public sealed partial class CombinationGroup : ICombinationPart {
    private int? _probability = 100;
    
    public List<CombinationEntry> Items { get; init; } = new();
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

public static class CombinationDataParser {
    private static readonly Regex ProbabilityRegex = new(@"Probability_(\d+)", RegexOptions.Compiled);
    private const string COMBINATION_KEY = "Combination.CombinationData", STORABLE_KEY = "Storable.DefaultStackCount";

    public static List<ICombinationPart> Parse(VirtualMachine vm, string value) {
        var entries = new List<ICombinationPart>();
        if (string.IsNullOrEmpty(value)) return entries;

        try {
            var groups = value.Split(["END&ELEM"], StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var groupData in groups) {
                var probMatch = ProbabilityRegex.Match(groupData);
                int? probability = null;
                var itemData = groupData;
                
                if (probMatch.Success) {
                    probability = int.Parse(probMatch.Groups[1].Value);
                    itemData = groupData.Substring(0, probMatch.Index);
                }

                var itemParts = itemData.Split(["END&VAR"], StringSplitOptions.RemoveEmptyEntries);
                var currentGroupEntries = itemParts.Select(i => CombinationEntry.Parse(vm, i)).ToList();

                switch (currentGroupEntries.Count) {
                    case <= 0:
                        continue;
                    case 1:
                        var singleItem = currentGroupEntries[0];
                        singleItem.Probability = probability;
                        entries.Add(singleItem);
                        break;
                    case > 1:
                        entries.Add(new CombinationGroup {
                            Items = currentGroupEntries,
                            Probability = probability
                        });
                        break;
                }
            }
        } catch (Exception ex) {
            Logger.LogError($"Failed to parse combination data: {ex.Message}");
        }

        return entries;
    }

    public static string Serialize(List<ICombinationPart> entries) {
        return string.Join("", entries.Select(entry => entry switch {
            CombinationGroup group => group.ToString(),
            CombinationEntry item => item + $"END&VARProbability_{item.Probability}END&ELEM",
            _ => string.Empty
        }));
    }

    public static string FormatReadable(string value, Dictionary<string, string> itemNames, VirtualMachine vm) {
        var entries = Parse(vm, value);

        return string.Join(", ", entries.Select(entry => entry switch {
            CombinationGroup group => $"[{string.Join("|", group.Items.Select(i => GetName(itemNames, i) + GetCount(i)))}]",
            CombinationEntry item => $"{GetName(itemNames, item) + GetCount(item)}",
            _ => "Unknown"
        } + (entry.Probability == 100 ? string.Empty : $" ({entry.Probability}%)")));
    }

    private static string GetName(Dictionary<string, string> names, CombinationEntry entry) {
        return names.TryGetValue(entry.ItemId!, out var name) ? name : entry.ItemId!;
    }

    private static string GetCount(CombinationEntry entry) {
        if (entry is { MinAmount: 1, MaxAmount: 1 })
            return string.Empty;

        return entry.MinAmount == entry.MaxAmount ? $" x{entry.MinAmount}" : $" x{entry.MinAmount}-{entry.MaxAmount}";
    }
    
    public static void RemoveFromPotentialCombinations(VirtualMachine vm, ParameterHolder ph) {
        if (!ph.StandartParams.ContainsKey(STORABLE_KEY) && !ph.StandartParams.ContainsKey(COMBINATION_KEY)) return;
        var combos = vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(vm.GetElementsByType<Other>()).
            Where(item => item.StandartParams.ContainsKey(COMBINATION_KEY) && item != ph).ToList();

        foreach (var combo in combos.Where(c => c.StandartParams[COMBINATION_KEY].Value.Contains($"{ph.Id}END"))) {
            Logger.LogInfo($"Removing {ph.Name} from combination: {combo.Name}.");
            var parsedList = Parse(vm, combo.StandartParams[COMBINATION_KEY].Value);
            List<ICombinationPart> toRemoveFromCombo = [];
            foreach (var element in parsedList) {
                switch (element) {
                    case CombinationGroup group:
                        group.Items.RemoveAll(item => item.Target.Element == ph);
                        if (group.Items.Count == 0)
                            toRemoveFromCombo.Add(group);
                        break;
                    case CombinationEntry entry:
                        if (entry.Target.Element == ph) 
                            toRemoveFromCombo.Add(entry);
                        break;
                }
            }
            foreach (var combinationPart in toRemoveFromCombo) 
                parsedList.Remove(combinationPart);
            combo.StandartParams[COMBINATION_KEY].Value = Serialize(parsedList);
        }
    }
}