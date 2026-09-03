using System;
using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes.Abstract;

namespace P2XMLEditor.Helper;

/// <summary>
/// "Where is this used?" — the elements that point at a given one.
///
/// A per-target search had to scan the whole VM for every element it was asked about (a holder
/// lookup alone walked every element there is), and the reference browser asks about hundreds of
/// elements while it builds a tree, so it hung. Instead the whole reverse map is built once from a
/// single forward pass — for each element, the elements it names — and cached on the VirtualMachine
/// (see <see cref="VirtualMachine.GetReferrers"/>). Every lookup after that is a dictionary hit.
///
/// The forward direction is enumerated once here, per element type, covering the same surfaces the
/// literal id search finds: the typed pointers (Parent, Owner, links, states…), the objects buried
/// in an action's or expression's target/source/param, and the ids packed into function argument
/// strings, hierarchy tables and the GameTimeContext/InheritanceInfo/PlayerRef fields.
/// </summary>
public static class DomainReferenceFinder {
    public static IEnumerable<VmElement> GetDirectReferences(VmElement target, VirtualMachine vm) =>
        vm.GetReferrers(target.Id);

    /// <summary>
    /// target id → the elements that reference it. One forward pass over every element; a source
    /// that names a target through several surfaces is listed once.
    /// </summary>
    public static Dictionary<ulong, List<VmElement>> BuildReferenceIndex(VirtualMachine vm) {
        var index = new Dictionary<ulong, List<VmElement>>();
        foreach (var source in vm.ElementsById.Values) {
            foreach (var target in EnumerateReferences(source, vm).Distinct()) {
                // A "non-null" pointer can still be null on a placeholder or a half-filled element.
                if (ReferenceEquals(target, null) || target.Id == source.Id) continue;
                if (!index.TryGetValue(target.Id, out var list))
                    index[target.Id] = list = [];
                list.Add(source);
            }
        }
        return index;
    }

    // --- Element-level forward references -------------------------------------------------------

    private static IEnumerable<VmElement> EnumerateReferences(VmElement e, VirtualMachine vm) {
        switch (e) {
            case Action a:
                if (a.EventToRaise != null) yield return a.EventToRaise;
                if (a.SourceExpression != null) yield return a.SourceExpression;
                foreach (var x in TargetObjectTargets(a.TargetObject)) yield return x;
                foreach (var x in ParamTargetTargets(a.TargetParam)) yield return x;
                if (a.Source.HasValue)
                    foreach (var x in SourceTargets(a.Source.Value)) yield return x;
                if (a.EventParams != null)
                    foreach (var ps in a.EventParams)
                        foreach (var x in SourceTargets(ps)) yield return x;
                foreach (var x in IdTargets(a.Function?.GetParamStrings(), vm)) yield return x;
                if (a.LocalContext.Element != null) yield return a.LocalContext.Element;
                break;

            case Expression ex:
                if (ex.Const != null) yield return ex.Const;
                foreach (var x in TargetObjectTargets(ex.TargetObject)) yield return x;
                if (ex.TargetParam.HasValue)
                    foreach (var x in ExprParamTargetTargets(ex.TargetParam.Value)) yield return x;
                foreach (var x in IdTargets(ex.Function?.GetParamStrings(), vm)) yield return x;
                if (ex.FormulaChilds != null)
                    foreach (var c in ex.FormulaChilds) yield return c;
                if (ex.LocalContext.Element != null) yield return ex.LocalContext.Element;
                break;

            case Parameter p:
                if (p.Parent?.Element != null) yield return p.Parent.Value.Element;
                if (p.OwnerComponent != null) yield return p.OwnerComponent;
                // The serialized value carries every id the value holds — a plain reference, a
                // hierarchy path, or the ids packed into a list or combination — and the type name
                // carries a custom type's id, so both are scanned wholesale.
                foreach (var x in IdTargets(new[] { p.SerializedValue, p.Type }, vm)) yield return x;
                break;

            case GameRoot gr:
                foreach (var x in HolderTargets(gr, vm)) yield return x;
                foreach (var s in gr.Samples ?? []) yield return s;
                foreach (var m in gr.LogicMaps ?? []) yield return m;
                foreach (var g in gr.GameModes ?? []) yield return g;
                foreach (var x in IdTargets(gr.HierarchyEngineGuidsTable, vm)) yield return x;
                if (gr.BaseToEngineGuidsTable != null) {
                    foreach (var x in IdTargets(gr.BaseToEngineGuidsTable.Keys, vm)) yield return x;
                    foreach (var x in IdTargets(gr.BaseToEngineGuidsTable.Values, vm)) yield return x;
                }
                if (gr.HierarchyScenesStructure != null)
                    foreach (var kv in gr.HierarchyScenesStructure) {
                        if (vm.GetNullableElement(kv.Key) is { } k) yield return k;
                        foreach (var arr in kv.Value.Values)
                            foreach (var id in arr)
                                if (vm.GetNullableElement(id) is { } el) yield return el;
                    }
                break;

            case Quest q:
                foreach (var x in HolderTargets(q, vm)) yield return x;
                if (q.StartEvent != null) yield return q.StartEvent;
                break;

            case ParameterHolder ph: // Character, Blueprint, Item, Other, Geom and their placeholders
                foreach (var x in HolderTargets(ph, vm)) yield return x;
                break;

            case FunctionalComponent fc:
                if (fc.Parent != null) yield return fc.Parent;
                foreach (var ev in fc.Events ?? []) yield return ev;
                break;

            case State st:
                yield return st.Parent;
                if (st.Owner != null) yield return st.Owner;
                foreach (var x in EntryLinkTargets(st.EntryPoints, st.InputLinks, st.OutputLinks)) yield return x;
                break;

            case Branch b:
                if (b.Parent.Element != null) yield return b.Parent.Element;
                if (b.Owner != null) yield return b.Owner;
                foreach (var c in b.BranchConditions ?? []) if (c.Element != null) yield return c.Element;
                foreach (var x in EntryLinkTargets(b.EntryPoints, b.InputLinks, b.OutputLinks)) yield return x;
                break;

            case Graph g:
                if (g.Parent.Element != null) yield return g.Parent.Element;
                if (g.Owner != null) yield return g.Owner;
                if (g.SubstituteGraph?.Element != null) yield return g.SubstituteGraph.Value.Element;
                foreach (var s in g.States ?? []) if (s.Element != null) yield return s.Element;
                foreach (var l in g.EventLinks ?? []) yield return l;
                foreach (var x in EntryLinkTargets(g.EntryPoints, g.InputLinks, g.OutputLinks)) yield return x;
                break;

            case GraphLink gl:
                if (gl.Parent.Element != null) yield return gl.Parent.Element;
                if (gl.Source?.Element != null) yield return gl.Source.Value.Element;
                if (gl.Destination?.Element != null) yield return gl.Destination.Value.Element;
                if (gl.Event != null) yield return gl.Event;
                foreach (var x in EventOwnerTargets(gl.EventObject)) yield return x;
                foreach (var x in IdTargets(gl.SourceParams, vm)) yield return x;
                break;

            case Speech sp:
                yield return sp.Parent;
                if (sp.Owner.Element != null) yield return sp.Owner.Element;
                if (sp.AuthorGuid.Element != null) yield return sp.AuthorGuid.Element;
                if (sp.Text != null) yield return sp.Text;
                foreach (var r in sp.Replies ?? []) yield return r;
                foreach (var x in EntryLinkTargets(sp.EntryPoints, sp.InputLinks, sp.OutputLinks)) yield return x;
                break;

            case Talking tk:
                yield return tk.Parent;
                if (tk.Owner.Element != null) yield return tk.Owner.Element;
                foreach (var s in tk.States ?? []) if (s.Element != null) yield return s.Element;
                foreach (var l in tk.EventLinks ?? []) yield return l;
                foreach (var x in EntryLinkTargets(tk.EntryPoints, tk.InputLinks, null)) yield return x;
                break;

            case Reply rp:
                yield return rp.Parent;
                if (rp.EnableCondition != null) yield return rp.EnableCondition;
                if (rp.ActionLine != null) yield return rp.ActionLine;
                if (rp.Text != null) yield return rp.Text;
                break;

            case Event evt:
                if (evt.Parent.Element != null) yield return evt.Parent.Element;
                if (evt.EventParameter != null) yield return evt.EventParameter;
                if (evt.Condition != null) yield return evt.Condition;
                break;

            case Condition cond:
                foreach (var pr in cond.Predicates ?? []) if (pr.Element != null) yield return pr.Element;
                break;

            case PartCondition pc:
                if (pc.FirstExpression != null) yield return pc.FirstExpression;
                if (pc.SecondExpression != null) yield return pc.SecondExpression;
                break;

            case ActionLine al:
                foreach (var act in al.Actions ?? []) if (act.Element != null) yield return act.Element;
                if (al.LocalContext.Element != null) yield return al.LocalContext.Element;
                foreach (var x in IdTargets(new[] { al.Name }, vm)) yield return x;
                if (al.LoopInfo != null) {
                    foreach (var x in SourceTargets(al.LoopInfo.Name)) yield return x;
                    foreach (var x in SourceTargets(al.LoopInfo.Start)) yield return x;
                    foreach (var x in SourceTargets(al.LoopInfo.End)) yield return x;
                }
                break;

            case EntryPoint ep:
                if (ep.Parent?.Element != null) yield return ep.Parent.Value.Element;
                if (ep.ActionLine != null) yield return ep.ActionLine;
                break;

            case GameMode gm:
                if (gm.Parent != null) yield return gm.Parent;
                foreach (var x in IdTargets(new[] { gm.PlayerRef }, vm)) yield return x;
                break;

            case MindMap mm:
                if (mm.Parent != null) yield return mm.Parent;
                if (mm.Title != null) yield return mm.Title;
                foreach (var n in mm.Nodes ?? []) yield return n;
                foreach (var l in mm.Links ?? []) yield return l;
                break;

            case MindMapNode mmn:
                if (mmn.Parent != null) yield return mmn.Parent;
                foreach (var c in mmn.Content ?? []) yield return c;
                foreach (var l in mmn.InputLinks ?? []) yield return l;
                foreach (var l in mmn.OutputLinks ?? []) yield return l;
                if (mmn.NodeNameText != null) yield return mmn.NodeNameText;
                if (mmn.NodeDescriptionText != null) yield return mmn.NodeDescriptionText;
                break;

            case MindMapLink mml:
                if (mml.Parent != null) yield return mml.Parent;
                if (mml.Source != null) yield return mml.Source;
                if (mml.Destination != null) yield return mml.Destination;
                break;

            case MindMapNodeContent mmc:
                if (mmc.Parent != null) yield return mmc.Parent;
                if (mmc.ContentDescriptionText != null) yield return mmc.ContentDescriptionText;
                if (mmc.ContentPicture != null) yield return mmc.ContentPicture;
                if (mmc.ContentCondition != null) yield return mmc.ContentCondition;
                break;

            case GameString gs:
                if (gs.Parent.Element != null) yield return gs.Parent.Element;
                break;

            case CustomType ct:
                if (ct.Parent != null) yield return ct.Parent;
                break;
        }
    }

    private static IEnumerable<VmElement> HolderTargets(ParameterHolder ph, VirtualMachine vm) {
        if (ph.Parent != null) yield return ph.Parent;
        foreach (var c in ph.ChildObjects ?? []) yield return c;
        foreach (var kv in ph.StandartParams ?? []) yield return kv.Value;
        foreach (var kv in ph.CustomParams ?? []) yield return kv.Value;
        foreach (var fc in ph.FunctionalComponents ?? []) yield return fc;
        foreach (var ev in ph.Events ?? []) yield return ev;
        if (ph.EventGraph != null) yield return ph.EventGraph;
        foreach (var x in IdTargets(ph.InheritanceInfo, vm)) yield return x;
        foreach (var x in IdTargets(new[] { ph.GameTimeContext }, vm)) yield return x;
        // A game object's world position names a scene placeholder by id.
        if (ph is GameObject go) foreach (var x in IdTargets(new[] { go.WorldPositionGuid }, vm)) yield return x;
    }

    // --- Sub-structure forward references (shared by actions, expressions, links, params) --------

    private static IEnumerable<VmElement> HierarchyTargets(HierarchyGuid? hierarchy) {
        if (hierarchy == null) yield break;
        foreach (var el in hierarchy.Elements)
            if (el.Element != null) yield return el.Element;
    }

    private static IEnumerable<VmElement> TargetObjectTargets(TargetObject to) {
        if (to.Holder != null) yield return to.Holder;
        if (to.ParameterRef != null) yield return to.ParameterRef;
        foreach (var x in HierarchyTargets(to.Hierarchy)) yield return x;
    }

    private static IEnumerable<VmElement> ParamTargetTargets(ParamTarget tp) {
        if (tp.ContextHolder != null) yield return tp.ContextHolder;
        if (tp.Parameter?.Element != null) yield return tp.Parameter.Value.Element;
        foreach (var x in HierarchyTargets(tp.ContextHierarchy)) yield return x;
    }

    private static IEnumerable<VmElement> ExprParamTargetTargets(ExpressionParamTarget tp) {
        if (tp.ObjectLiteral != null) yield return tp.ObjectLiteral;
        foreach (var x in HierarchyTargets(tp.LiteralHierarchy)) yield return x;
        if (tp.Param.HasValue)
            foreach (var x in ParamTargetTargets(tp.Param.Value)) yield return x;
    }

    private static IEnumerable<VmElement> SourceTargets(ParameterSource ps) {
        if (ps.ElementReference != null) yield return ps.ElementReference;
        if (ps.ParameterReference != null) yield return ps.ParameterReference;
        if (ps.DynamicObjectReference != null) yield return ps.DynamicObjectReference;
        if (ps.PrefixHolder != null) yield return ps.PrefixHolder;
        foreach (var x in HierarchyTargets(ps.HierarchyReference)) yield return x;
        foreach (var x in HierarchyTargets(ps.PrefixHierarchy)) yield return x;
        if (ps.LiteralValue is IElementValue ev && ev.Element != null) yield return ev.Element;
        if (ps.LiteralValue is IHierarchyValue hv)
            foreach (var x in HierarchyTargets(hv.Hierarchy)) yield return x;
    }

    private static IEnumerable<VmElement> EventOwnerTargets(EventOwner? owner) {
        if (owner == null) yield break;
        var o = owner.Value;
        if (o.Holder != null) yield return o.Holder;
        if (o.ParameterRef != null) yield return o.ParameterRef;
        foreach (var x in HierarchyTargets(o.Hierarchy)) yield return x;
    }

    private static IEnumerable<VmElement> EntryLinkTargets(IEnumerable<EntryPoint>? entryPoints,
        IEnumerable<GraphLink>? inputLinks, IEnumerable<GraphLink>? outputLinks) {
        foreach (var ep in entryPoints ?? []) yield return ep;
        foreach (var l in inputLinks ?? []) yield return l;
        foreach (var l in outputLinks ?? []) yield return l;
    }

    // --- Ids packed into strings (function args, hierarchy tables, GameTimeContext, …) -----------

    private static IEnumerable<VmElement> IdTargets(IEnumerable<string?>? strings, VirtualMachine vm) {
        if (strings == null) yield break;
        foreach (var s in strings)
            foreach (var id in ExtractIds(s))
                if (vm.GetNullableElement(id) is { } el) yield return el;
    }

    private static IEnumerable<ulong> ExtractIds(string? s) {
        if (string.IsNullOrEmpty(s)) yield break;
        var i = 0;
        while (i < s.Length) {
            if (!char.IsDigit(s[i])) { i++; continue; }
            var j = i;
            while (j < s.Length && char.IsDigit(s[j])) j++;
            // Element ids are long; a short run is a count or index and only costs a failed lookup.
            if (ulong.TryParse(s.AsSpan(i, j - i), out var id)) yield return id;
            i = j;
        }
    }
}
