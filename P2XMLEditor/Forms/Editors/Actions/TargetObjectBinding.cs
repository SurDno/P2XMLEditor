using P2XMLEditor.GameData.VirtualMachineElements.Abstract;

namespace P2XMLEditor.Forms.Editors.Actions;

/// <summary>
/// What an action's target object resolves to, for the controls that have to follow it.
/// </summary>
/// <param name="Holder">
/// The object whose parameters can be listed — the concrete target where there is one, or the
/// single blueprint an indirect target is pinned to by its declared type. Null when the object
/// is genuinely only known at runtime.
/// </param>
/// <param name="IsConcrete">
/// True when the target names an object outright rather than resolving to one. The distinction
/// matters because a concrete target always addresses its parameters by id, while a pinned one
/// is addressed both ways in the data.
/// </param>
public readonly record struct TargetObjectBinding(ParameterHolder? Holder, bool IsConcrete);
