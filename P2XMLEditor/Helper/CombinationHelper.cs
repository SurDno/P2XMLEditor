using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using P2XMLEditor.Core;
using P2XMLEditor.Forms.MainForm.Combinations;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Helper;

public static partial class CombinationHelper {
	public const string CombinationKey = "Combination.CombinationData", StorableKey = "Storable.DefaultStackCount";

	public static List<ICombinationPart> Parse(VirtualMachine vm, string value) {
		var entries = new List<ICombinationPart>();
		if (string.IsNullOrEmpty(value)) return entries;

		try {
			var groups = value.Split(["END&ELEM"], StringSplitOptions.RemoveEmptyEntries);
			
			foreach (var groupData in groups) {
				var probMatch = ProbabilityRegex().Match(groupData);
				int? probability = null;
				var itemData = groupData;
				
				if (probMatch.Success) {
					probability = int.Parse(probMatch.Groups[1].Value);
					itemData = groupData[..probMatch.Index];
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
			Logger.Log(LogLevel.Error, $"Failed to parse combination data: {ex.Message}");
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

	public static string FormatReadable(ParameterValue? value, Dictionary<ulong, string> itemNames, VirtualMachine vm) {
		if (value is CombinationDataValue combo) {
			return FormatEntries(combo.TypedValue, itemNames);
		}
		if (value is BasicValue<string> s) {
			var entries = Parse(vm, s.TypedValue);
			return FormatEntries(entries, itemNames);
		}
		return value?.Serialize() ?? string.Empty;
	}

	private static string FormatEntries(List<ICombinationPart> entries, Dictionary<ulong, string> itemNames) {
		return string.Join(", ", entries.Select(entry => entry switch {
			CombinationGroup group => $"[{string.Join("|", group.Items.Select(i => GetName(itemNames, i) + GetCount(i)))}]",
			CombinationEntry item => $"{GetName(itemNames, item) + GetCount(item)}",
			_ => "Unknown"
		} + (entry.Probability == 100 ? string.Empty : $" ({entry.Probability}%)")));
	}

	private static string GetName(Dictionary<ulong, string> names, CombinationEntry entry) {
		return names.TryGetValue(entry.ItemId!, out var name) ? name : entry.ItemId.ToString();
	}

	private static string GetCount(CombinationEntry entry) {
		if (entry is { MinAmount: 1, MaxAmount: 1 })
			return string.Empty;

		return entry.MinAmount == entry.MaxAmount ? $" x{entry.MinAmount}" : $" x{entry.MinAmount}-{entry.MaxAmount}";
	}
	
	public static void RemoveFromPotentialCombinations(VirtualMachine vm, ParameterHolder ph) {
		if (!ph.StandartParams.ContainsKey(StorableKey) && !ph.StandartParams.ContainsKey(CombinationKey)) return;

		foreach (var combo in GetCombinationsWithItem(vm, ph)) {
			Logger.Log(LogLevel.Info, $"Removing {ph.Name} from combination: {combo.Name}.");
			var parsedList = ((CombinationDataValue)combo.StandartParams[CombinationKey].Value).TypedValue;
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
		}
	}

	public static List<ParameterHolder> GetCombinationsWithItem(VirtualMachine vm, ParameterHolder ph) {
		return vm.GetElementsByType<Item>().Cast<ParameterHolder>().Concat(vm.GetElementsByType<Other>()).
			Where(item => {
				if (!item.StandartParams.TryGetValue(CombinationKey, out var param) || item == ph) return false;
				var list = ((CombinationDataValue)param.Value).TypedValue;
				return list.Any(part => part switch {
					CombinationEntry entry => entry.Target.Element == ph,
					CombinationGroup group => group.Items.Any(i => i.Target.Element == ph),
					_ => false
				});
			}).ToList();
	}

	[GeneratedRegex(@"Probability_(\d+)", RegexOptions.Compiled)]
	private static partial Regex ProbabilityRegex();
}
