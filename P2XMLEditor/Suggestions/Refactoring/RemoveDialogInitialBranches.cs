using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.Logging;

namespace P2XMLEditor.Suggestions.Refactoring;

/// <summary>
/// Finds all dialog initial branches (branches inside a Talking graph that have
/// zero conditions — i.e. only the unconditional "else" arm — and are marked
/// Initial or only receive flow from the Talking entry), then:
///   1. Marks the Speech they point to as Initial.
///   2. Removes the branch's output link to that Speech (and the link from the
///      branch's InputLinks, if any).
///   3. Removes the branch's EntryPoints and the branch itself.
///   4. Removes the branch from the Talking.States list.
/// </summary>
[Refactoring("Refactor/Dialogs/Remove dialog initial branches"), SuppressMessage("ReSharper", "UnusedType.Global")]
public class RemoveDialogInitialBranches(VirtualMachine vm) : Suggestion(vm) {
	public override void Execute() {
		var talkings = Vm.GetElementsByType<Talking>().ToList();
		var removed = 0;

		foreach (var talking in talkings) {
			// Collect all initial, no-condition branches in this talking.
			// We may need multiple passes if branches chain (branch → branch → speech).
			bool madeProgress;
			do {
				madeProgress = false;
				var statesCopy = talking.States.ToList();

				foreach (var stateRef in statesCopy) {
					if (stateRef.Element is not Branch branch) continue;
					if (!IsSimpleBranch(branch)) continue;

					// Follow the single output link to its destination.
					var outputLink = branch.OutputLinks?
						.Where(l => l.Enabled)
						.OrderBy(l => l.SourceExitPointIndex)
						.FirstOrDefault();

					if (outputLink == null) {
						// No output at all — just remove the branch.
						CleanUpBranch(talking, branch, null, null);
						Logger.Log(LogLevel.Info, $"Removed initial dialog branch '{branch.Name ?? branch.Id.ToString()}' with no outputs from Talking '{talking.Name ?? talking.Id.ToString()}'");
						madeProgress = true;
						removed++;
						continue;
					}

					var dest = outputLink.Destination?.Element;

					switch (dest) {
						case Speech destSpeech:
							destSpeech.Initial = true;
							// Remove the link from the speech's InputLinks
							destSpeech.InputLinks?.Remove(outputLink);
							CleanUpBranch(talking, branch, outputLink, destSpeech);
							Logger.Log(LogLevel.Info, $"Removed initial dialog branch '{branch.Name ?? branch.Id.ToString()}' from Talking '{talking.Name ?? talking.Id.ToString()}' and marked Speech '{destSpeech.Text}' as initial");
							madeProgress = true;
							removed++;
							break;

						case Branch destBranch when IsSimpleBranch(destBranch):
							// Chained simple branches — also mark the inner one Initial
							// and let the next iteration handle the outer one.
							destBranch.Initial = true;
							// Remove the input link on the inner branch pointing from this branch
							destBranch.InputLinks?.Remove(outputLink);
							CleanUpBranch(talking, branch, outputLink, null);
							Logger.Log(LogLevel.Info, $"Removed initial dialog branch '{branch.Name ?? branch.Id.ToString()}' from Talking '{talking.Name ?? talking.Id.ToString()}' and marked next branch '{destBranch.Name ?? destBranch.Id.ToString()}' as initial");
							madeProgress = true;
							removed++;
							break;
					}
				}
			} while (madeProgress);
		}

		Logger.Log(LogLevel.Info, $"RemoveDialogInitialBranches: removed {removed} branch(es).");
	}

	/// <summary>
	/// A branch is "simple" (eligible for removal as an initial bypass) when it has:
	///   - no conditions (only the unconditional else arm), AND
	///   - exactly one output link, AND
	///   - it is itself marked Initial, or all its input links come from the Talking's
	///     entry points (i.e. it is the dialog entry node).
	/// </summary>
	private static bool IsSimpleBranch(Branch branch) {
		if (branch.BranchConditions is { Count: > 0 }) return false;

		var outputs = branch.OutputLinks?.Where(l => l.Enabled).ToList() ?? [];
		if (outputs.Count != 1) return false;

		// Must be an entry node: either flagged Initial itself, or has no input links
		// (the Talking.Initial flag lives on the Talking, not the branch, so zero
		// inputs or branch.Initial covers both cases the game uses).
		var inputs = branch.InputLinks?.Count ?? 0;
		return branch.Initial || inputs == 0;
	}

	private void CleanUpBranch(Talking talking, Branch branch, GraphLink? outputLink, Speech? destSpeech) {
		// 1. Remove the single output link
		if (outputLink != null) {
			branch.OutputLinks?.Remove(outputLink);
			Vm.RemoveElement(outputLink);
		}

		// 2. Remove all remaining input links from their sources.
		//    (OnDestroy will handle Vm.RemoveElement for the links themselves)
		foreach (var inLink in branch.InputLinks?.ToList() ?? []) {
			if (inLink.Source?.Element is Speech sourceSpeech)
				sourceSpeech.OutputLinks?.Remove(inLink);
			else if (inLink.Source?.Element is Branch sourceBranch)
				sourceBranch.OutputLinks?.Remove(inLink);
		}

		// 3. EntryPoints will be removed by Branch.OnDestroy automatically

		// 4. Remove the branch from the Talking.States list
		talking.States.RemoveAll(s => s.Element == branch);

		// 5. Remove the branch element itself (skip OnDestroy since we already
		//    handled links above to avoid double-removal)
		Vm.RemoveElement(branch);
	}
}
