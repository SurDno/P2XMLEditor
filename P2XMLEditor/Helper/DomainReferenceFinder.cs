using System.Collections.Generic;
using System.Linq;
using P2XMLEditor.Core;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Helper;

public static class DomainReferenceFinder {
    public static IEnumerable<VmElement> GetDirectReferences(VmElement target, VirtualMachine vm) {
        return target switch {
            Sample s => GetDirectReferences(s, vm),
            Action a => GetDirectReferences(a, vm),
            Parameter p => GetDirectReferences(p, vm),
            Expression e => GetDirectReferences(e, vm),
            State st => GetDirectReferences(st, vm),
            Graph g => GetDirectReferences(g, vm),
            FunctionalComponent fc => GetDirectReferences(fc, vm),
            GameMode gm => GetDirectReferences(gm, vm),
            GameRoot gr => GetDirectReferences(gr, vm),
            ParameterHolder ph => GetDirectReferences(ph, vm),
            Branch b => GetDirectReferences(b, vm),
            Event ev => GetDirectReferences(ev, vm),
            Speech sp => GetDirectReferences(sp, vm),
            Talking t => GetDirectReferences(t, vm),
            Reply r => GetDirectReferences(r, vm),
            ActionLine al => GetDirectReferences(al, vm),
            EntryPoint ep => GetDirectReferences(ep, vm),
            GraphLink gl => GetDirectReferences(gl, vm),
            MindMap mm => GetDirectReferences(mm, vm),
            MindMapNode mmn => GetDirectReferences(mmn, vm),
            MindMapLink mml => GetDirectReferences(mml, vm),
            MindMapNodeContent mmnc => GetDirectReferences(mmnc, vm),
            Condition c => GetDirectReferences(c, vm),
            PartCondition pc => GetDirectReferences(pc, vm),
            // Fallback for types not explicitly overloaded yet
            _ => Enumerable.Empty<VmElement>()
        };
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(Branch target, VirtualMachine vm) {
        if (target.Parent.Element != null) yield return target.Parent.Element;
        foreach (var e in vm.GetElementsByType<Expression>()) {
            if (e.LocalContext.Element == target) yield return e;
        }
        foreach (var g in vm.GetElementsByType<Graph>()) {
            if (g.States != null && g.States.Any(s => s.Element == target)) yield return g;
        }
        foreach (var gl in vm.GetElementsByType<GraphLink>()) {
            if (gl.Source?.Element == target || gl.Destination?.Element == target) yield return gl;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(Event target, VirtualMachine vm) {
        if (target.Parent.Element != null) yield return target.Parent.Element;
        foreach (var fc in vm.GetElementsByType<FunctionalComponent>()) {
            if (fc.Events != null && fc.Events.Contains(target)) yield return fc;
        }
        foreach (var gl in vm.GetElementsByType<GraphLink>()) {
            if (gl.Source?.Element == target || gl.Destination?.Element == target) yield return gl;
        }
        foreach (var q in vm.GetElementsByType<Quest>()) {
            if (q.StartEvent == target) yield return q;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(Speech target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var gs in vm.GetElementsByType<GameString>()) {
            if (gs.Parent.Element == target) yield return gs;
        }
        foreach (var gl in vm.GetElementsByType<GraphLink>()) {
            if (gl.Source?.Element == target || gl.Destination?.Element == target) yield return gl;
        }
        foreach (var r in vm.GetElementsByType<Reply>()) {
            if (r.Parent == target) yield return r;
        }
        foreach (var t in vm.GetElementsByType<Talking>()) {
            if (t.States != null && t.States.Any(s => s.Element == target)) yield return t;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(Talking target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var b in vm.GetElementsByType<Branch>()) {
            if (b.Parent.Element == target) yield return b;
        }
        foreach (var gl in vm.GetElementsByType<GraphLink>()) {
            if (gl.Source?.Element == target || gl.Destination?.Element == target) yield return gl;
        }
        foreach (var p in vm.GetElementsByType<Parameter>()) {
            if (ParameterReferencesElement(p, target)) yield return p;
        }
        foreach (var sp in vm.GetElementsByType<Speech>()) {
            if (sp.Parent == target) yield return sp;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(Reply target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var gs in vm.GetElementsByType<GameString>()) {
            if (gs.Parent.Element == target) yield return gs;
        }
        foreach (var sp in vm.GetElementsByType<Speech>()) {
            if (sp.Replies != null && sp.Replies.Contains(target)) yield return sp;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(ActionLine target, VirtualMachine vm) {
        foreach (var ep in vm.GetElementsByType<EntryPoint>()) {
            if (ep.ActionLine == target) yield return ep;
        }
        foreach (var al in vm.GetElementsByType<ActionLine>()) {
            if (al.Actions != null && al.Actions.Any(a => a.Element == target)) yield return al;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(EntryPoint target, VirtualMachine vm) {
        foreach (var sp in vm.GetElementsByType<Speech>()) {
            if (sp.EntryPoints != null && sp.EntryPoints.Contains(target)) yield return sp;
        }
        foreach (var t in vm.GetElementsByType<Talking>()) {
            if (t.EntryPoints != null && t.EntryPoints.Contains(target)) yield return t;
        }
        foreach (var b in vm.GetElementsByType<Branch>()) {
            if (b.EntryPoints != null && b.EntryPoints.Contains(target)) yield return b;
        }
        foreach (var g in vm.GetElementsByType<Graph>()) {
            if (g.EntryPoints != null && g.EntryPoints.Contains(target)) yield return g;
        }
        foreach (var st in vm.GetElementsByType<State>()) {
            if (st.EntryPoints != null && st.EntryPoints.Contains(target)) yield return st;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(GraphLink target, VirtualMachine vm) {
        foreach (var g in vm.GetElementsByType<Graph>()) {
            if (g.EventLinks != null && g.EventLinks.Contains(target)) yield return g;
            if (g.InputLinks != null && g.InputLinks.Contains(target)) yield return g;
            if (g.OutputLinks != null && g.OutputLinks.Contains(target)) yield return g;
        }
        foreach (var st in vm.GetElementsByType<State>()) {
            if (st.InputLinks != null && st.InputLinks.Contains(target)) yield return st;
            if (st.OutputLinks != null && st.OutputLinks.Contains(target)) yield return st;
        }
        foreach (var b in vm.GetElementsByType<Branch>()) {
            if (b.InputLinks != null && b.InputLinks.Contains(target)) yield return b;
            if (b.OutputLinks != null && b.OutputLinks.Contains(target)) yield return b;
        }
        foreach (var t in vm.GetElementsByType<Talking>()) {
            if (t.InputLinks != null && t.InputLinks.Contains(target)) yield return t;
            if (t.EventLinks != null && t.EventLinks.Contains(target)) yield return t;
        }
        foreach (var sp in vm.GetElementsByType<Speech>()) {
            if (sp.InputLinks != null && sp.InputLinks.Contains(target)) yield return sp;
            if (sp.OutputLinks != null && sp.OutputLinks.Contains(target)) yield return sp;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(MindMap target, VirtualMachine vm) {
        foreach (var a in vm.GetElementsByType<Action>()) {
            if (a.EventParams != null) {
                foreach (var p in a.EventParams) {
                    if (ParameterSourceReferencesElement(p, target)) yield return a;
                }
            }
        }
        foreach (var gs in vm.GetElementsByType<GameString>()) {
            if (gs.Parent.Element == target) yield return gs;
        }
        foreach (var gr in vm.GetElementsByType<GameRoot>()) {
            if (gr.LogicMaps != null && gr.LogicMaps.Contains(target)) yield return gr;
        }
        foreach (var mml in vm.GetElementsByType<MindMapLink>()) {
            if (mml.Parent == target) yield return mml;
        }
        foreach (var mmn in vm.GetElementsByType<MindMapNode>()) {
            if (mmn.Parent == target) yield return mmn;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(MindMapNode target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var a in vm.GetElementsByType<Action>()) {
            if (a.EventParams != null) {
                foreach (var p in a.EventParams) {
                    if (ParameterSourceReferencesElement(p, target)) yield return a;
                }
            }
        }
        foreach (var e in vm.GetElementsByType<Expression>()) {
            if (e.TargetParam != null && e.TargetParam.Value.ObjectLiteral == target) yield return e;
        }
        foreach (var mm in vm.GetElementsByType<MindMap>()) {
            if (mm.Nodes != null && mm.Nodes.Contains(target)) yield return mm;
        }
        foreach (var mml in vm.GetElementsByType<MindMapLink>()) {
            if (mml.Source == target || mml.Destination == target) yield return mml;
        }
        foreach (var mmnc in vm.GetElementsByType<MindMapNodeContent>()) {
            if (mmnc.Parent == target) yield return mmnc;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(MindMapLink target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var mm in vm.GetElementsByType<MindMap>()) {
            if (mm.Links != null && mm.Links.Contains(target)) yield return mm;
        }
        foreach (var mmn in vm.GetElementsByType<MindMapNode>()) {
            if (mmn.InputLinks != null && mmn.InputLinks.Contains(target)) yield return mmn;
            if (mmn.OutputLinks != null && mmn.OutputLinks.Contains(target)) yield return mmn;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(MindMapNodeContent target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var mmn in vm.GetElementsByType<MindMapNode>()) {
            if (mmn.Content != null && mmn.Content.Contains(target)) yield return mmn;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(Condition target, VirtualMachine vm) {
        foreach (var b in vm.GetElementsByType<Branch>()) {
            if (b.BranchConditions != null && b.BranchConditions.Any(c => c.Element == target)) yield return b;
        }
        foreach (var ev in vm.GetElementsByType<Event>()) {
            if (ev.Condition == target) yield return ev;
        }
        foreach (var r in vm.GetElementsByType<Reply>()) {
            if (r.EnableCondition == target) yield return r;
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(PartCondition target, VirtualMachine vm) {
        foreach (var c in vm.GetElementsByType<Condition>()) {
            if (c.Predicates != null && c.Predicates.Any(p => p.Element == target)) yield return c;
        }
    }

    private static bool ParameterReferencesElement(Parameter p, VmElement target) {
        if (p.Value is IElementValue ev && ev.Element == target) return true;
        if (p.Value is IHierarchyValue hv && hv.Hierarchy != null) {
            foreach (var el in hv.Hierarchy.Elements) {
                if (el.Element == target) return true;
            }
        }
        return false;
    }

    private static bool ParameterSourceReferencesElement(ParameterSource ps, VmElement target) {
        if (ps.ElementReference == target) return true;
        if (ps.ParameterReference == target) return true;
        if (ps.DynamicObjectReference == target) return true;
        if (ps.PrefixHolder == target) return true;
        if (ps.HierarchyReference != null) {
            foreach (var el in ps.HierarchyReference.Elements) {
                if (el.Element == target) return true;
            }
        }
        if (ps.PrefixHierarchy != null) {
            foreach (var el in ps.PrefixHierarchy.Elements) {
                if (el.Element == target) return true;
            }
        }
        if (ps.LiteralValue is IElementValue ev && ev.Element == target) return true;
        if (ps.LiteralValue is IHierarchyValue hv && hv.Hierarchy != null) {
            foreach (var el in hv.Hierarchy.Elements) {
                if (el.Element == target) return true;
            }
        }
        return false;
    }

    public static IEnumerable<VmElement> GetDirectReferences(Sample target, VirtualMachine vm) {
        foreach (var p in vm.GetElementsByType<Parameter>()) {
            if (ParameterReferencesElement(p, target)) yield return p;
        }
        
        foreach (var a in vm.GetElementsByType<Action>()) {
            if (a.EventParams != null) {
                foreach (var ep in a.EventParams) {
                    if (ParameterSourceReferencesElement(ep, target)) yield return a;
                }
            }
        }
    }

    public static IEnumerable<VmElement> GetDirectReferences(Action target, VirtualMachine vm) {
        foreach (var p in vm.GetElementsByType<Parameter>()) {
            if (ParameterReferencesElement(p, target)) yield return p;
        }
        foreach (var al in vm.GetElementsByType<ActionLine>()) {
            if (al.Actions != null && al.Actions.Any(x => x.Element == target)) yield return al;
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(Parameter target, VirtualMachine vm) {
        if (target.Parent.HasValue) yield return target.Parent.Value.Element;

        foreach (var p in vm.GetElementsByType<Parameter>()) {
            if (ParameterReferencesElement(p, target)) yield return p;
        }
        foreach (var a in vm.GetElementsByType<Action>()) {
            if (a.TargetObject.ParameterRef == target) yield return a;
            if (a.EventParams != null) {
                foreach (var ep in a.EventParams) {
                    if (ParameterSourceReferencesElement(ep, target)) yield return a;
                }
            }
        }
        foreach (var e in vm.GetElementsByType<Expression>()) {
            if (e.Const == target) yield return e;
            if (e.TargetParam?.Param?.Parameter?.Element == target) yield return e;
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(Expression target, VirtualMachine vm) {
        if (target.LocalContext.Element != null) yield return target.LocalContext.Element;

        foreach (var p in vm.GetElementsByType<Parameter>()) {
            if (ParameterReferencesElement(p, target)) yield return p;
        }
        foreach (var pc in vm.GetElementsByType<PartCondition>()) {
            if (pc.FirstExpression == target) yield return pc;
            if (pc.SecondExpression == target) yield return pc;
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(State target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        if (target.Owner != null) yield return target.Owner;

        foreach (var p in vm.GetElementsByType<Parameter>()) {
            if (p.Parent?.Element == target) yield return p;
        }
        foreach (var al in vm.GetElementsByType<ActionLine>()) {
            if (al.LocalContext.Element == target) yield return al;
        }
        foreach (var ep in vm.GetElementsByType<EntryPoint>()) {
            if (ep.Parent?.Element == target) yield return ep;
        }
        foreach (var g in vm.GetElementsByType<Graph>()) {
            if (g.States != null && g.States.Any(s => s.Element == target)) yield return g;
        }
        foreach (var gl in vm.GetElementsByType<GraphLink>()) {
            if (gl.Source?.Element == target || gl.Destination?.Element == target) yield return gl;
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(Graph target, VirtualMachine vm) {
        if (target.Parent.Element != null) yield return target.Parent.Element;

        foreach (var ep in vm.GetElementsByType<EntryPoint>()) {
            if (ep.Parent?.Element == target) yield return ep;
        }
        foreach (var gl in vm.GetElementsByType<GraphLink>()) {
            if (gl.Source?.Element == target || gl.Destination?.Element == target) yield return gl;
        }
        foreach (var gr in vm.GetElementsByType<GameRoot>()) {
            if (gr.EventGraph == target) yield return gr;
        }
        foreach (var q in vm.GetElementsByType<Quest>()) {
            if (q.EventGraph == target) yield return q;
        }
        foreach (var b in vm.GetElementsByType<Blueprint>()) {
            if (b.EventGraph == target) yield return b;
        }
        foreach (var g in vm.GetElementsByType<Graph>()) {
            if (g.SubstituteGraph?.Element == target) yield return g;
            if (g.States != null && g.States.Any(s => s.Element == target)) yield return g;
        }
        foreach (var st in vm.GetElementsByType<State>()) {
            if (st.Parent == target) yield return st;
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(ParameterHolder target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        
        foreach (var element in vm.ElementsById.Values) {
            switch (element) {
                case ParameterHolder ph:
                    if (ph.Parent == target) yield return ph;
                    if (ph.ChildObjects != null && ph.ChildObjects.Contains(target)) yield return ph;
                    if (ph is GameObject go && go.InheritanceInfo != null && go.InheritanceInfo.Contains(target.Id.ToString())) yield return ph;
                    break;
                case Parameter p:
                    if (p.Parent?.Element == target) yield return p;
                    if (ParameterReferencesElement(p, target)) yield return p;
                    break;
                case State st:
                    if (st.Owner == target) yield return st;
                    break;
                case Branch b:
                    if (b.Owner == target) yield return b;
                    break;
                case FunctionalComponent fc:
                    if (fc.Parent == target) yield return fc;
                    break;
                case Action a:
                    if (a.TargetObject.Holder == target) yield return a;
                    if (a.TargetObject.ObjectLiteral == target) yield return a;
                    if (a.TargetObject.Hierarchy != null) {
                        foreach (var el in a.TargetObject.Hierarchy.Elements) {
                            if (el.Element == target) yield return a;
                        }
                    }
                    if (a.EventParams != null) {
                        foreach (var ep in a.EventParams) {
                            if (ParameterSourceReferencesElement(ep, target)) yield return a;
                        }
                    }
                    if (a.Function != null) {
                        var strings = a.Function.GetParamStrings();
                        if (strings != null && strings.Any(s => s.Contains(target.Id.ToString()))) yield return a;
                    }
                    break;
                case Expression e:
                    if (e.TargetObject.Holder == target) yield return e;
                    if (e.TargetObject.ObjectLiteral == target) yield return e;
                    if (e.TargetObject.Hierarchy != null) {
                        foreach (var el in e.TargetObject.Hierarchy.Elements) {
                            if (el.Element == target) yield return e;
                        }
                    }
                    if (e.TargetParam != null) {
                        var tp = e.TargetParam.Value;
                        if (tp.ObjectLiteral == target) yield return e;
                        if (tp.LiteralHierarchy != null) {
                            foreach (var el in tp.LiteralHierarchy.Elements) {
                                if (el.Element == target) {
                                    yield return e;
                                    break;
                                }
                            }
                        }
                    }
                    if (e.Function != null) {
                        var strings = e.Function.GetParamStrings();
                        if (strings != null && strings.Any(s => s.Contains(target.Id.ToString()))) yield return e;
                    }
                    break;
                case Graph g:
                    if (g.Parent.Element == target) yield return g;
                    if (g.Owner == target) yield return g;
                    break;
                case GraphLink gl:
                    if (gl.EventObject != null) {
                        var eo = gl.EventObject.Value;
                        if (eo.Holder == target) yield return gl;
                        if (eo.Hierarchy != null) {
                            foreach (var el in eo.Hierarchy.Elements) {
                                if (el.Element == target) yield return gl;
                            }
                        }
                    }
                    if (gl.SourceParams != null && gl.SourceParams.Any(p => p.Contains(target.Id.ToString()))) {
                        yield return gl;
                    }
                    break;
                case ActionLine al:
                    if (al.Name != null && al.Name.Contains(target.Id.ToString())) yield return al;
                    break;
                case GameMode gm:
                    if (gm.Parent == target) yield return gm;
                    break;
                case MindMap mm:
                    if (mm.Parent == target) yield return mm;
                    break;
                case GameString gs:
                    if (gs.Parent.Element == target) yield return gs;
                    break;
                case Event ev:
                    if (ev.Parent.Element == target) yield return ev;
                    break;
                case GameMode gm:
                    if (gm.PlayerRef == target.Id.ToString()) yield return gm;
                    break;
            }
        }
        foreach (var gr in vm.GetElementsByType<GameRoot>()) {
            if (gr.HierarchyScenesStructure != null) {
                if (gr.HierarchyScenesStructure.ContainsKey(target.Id)) {
                    yield return gr;
                    continue;
                }
                bool found = false;
                foreach (var innerDict in gr.HierarchyScenesStructure.Values) {
                    foreach (var arr in innerDict.Values) {
                        if (arr.Contains(target.Id)) {
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
                if (found) yield return gr;
            }
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(FunctionalComponent target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var ev in vm.GetElementsByType<Event>()) {
            if (ev.Parent.Element == target) yield return ev;
        }
        foreach (var ph in vm.ElementsById.Values.OfType<ParameterHolder>()) {
            if (ph.FunctionalComponents != null && ph.FunctionalComponents.Contains(target)) yield return ph;
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(GameMode target, VirtualMachine vm) {
        if (target.Parent != null) yield return target.Parent;
        foreach (var gr in vm.GetElementsByType<GameRoot>()) {
            if (gr.GameModes != null && gr.GameModes.Contains(target)) yield return gr;
        }
        foreach (var p in vm.GetElementsByType<Parameter>()) {
            if (ParameterReferencesElement(p, target)) yield return p;
        }
        var targetId = target.Id.ToString();
        foreach (var a in vm.GetElementsByType<Action>()) {
            if (a.TargetObject.Hierarchy != null) {
                foreach (var el in a.TargetObject.Hierarchy.Elements) {
                    if (el.Element == target) {
                        yield return a;
                        break;
                    }
                }
            }
            if (a.EventParams != null) {
                foreach (var ep in a.EventParams) {
                    if (ParameterSourceReferencesElement(ep, target)) yield return a;
                }
            }
            if (a.Function != null) {
                var strings = a.Function.GetParamStrings();
                if (strings != null && strings.Any(s => s.Contains(targetId))) {
                    yield return a;
                }
            }
        }
        foreach (var e in vm.GetElementsByType<Expression>()) {
            if (e.TargetObject.Hierarchy != null) {
                foreach (var el in e.TargetObject.Hierarchy.Elements) {
                    if (el.Element == target) {
                        yield return e;
                        break;
                    }
                }
            }
            if (e.TargetParam != null) {
                var tp = e.TargetParam.Value;
                if (tp.ObjectLiteral == target) yield return e;
                if (tp.LiteralHierarchy != null) {
                    foreach (var el in tp.LiteralHierarchy.Elements) {
                        if (el.Element == target) {
                            yield return e;
                            break;
                        }
                    }
                }
            }
            if (e.Function != null) {
                var strings = e.Function.GetParamStrings();
                if (strings != null && strings.Any(s => s.Contains(targetId))) {
                    yield return e;
                }
            }
        }
        foreach (var q in vm.GetElementsByType<Quest>()) {
            if (q.GameTimeContext == targetId) yield return q;
        }
        foreach (var b in vm.GetElementsByType<Blueprint>()) {
            if (b.GameTimeContext == targetId) yield return b;
        }
        foreach (var c in vm.GetElementsByType<Character>()) {
            if (c.GameTimeContext == targetId) yield return c;
        }
        foreach (var i in vm.GetElementsByType<Item>()) {
            if (i.GameTimeContext == targetId) yield return i;
        }
        foreach (var o in vm.GetElementsByType<Other>()) {
            if (o.GameTimeContext == targetId) yield return o;
        }
        foreach (var gm in vm.GetElementsByType<Geom>()) {
            if (gm.GameTimeContext == targetId) yield return gm;
        }
        foreach (var sc in vm.GetElementsByType<P2XMLEditor.GameData.VirtualMachineElements.Scene>()) {
            if (sc.GameTimeContext == targetId) yield return sc;
        }
        foreach (var gr in vm.GetElementsByType<GameRoot>()) {
            if (gr.GameTimeContext == targetId) yield return gr;
        }
        foreach (var cp in vm.GetElementsByType<P2XMLEditor.GameData.VirtualMachineElements.Placeholders.CharacterPlaceholder>()) {
            if (cp.GameTimeContext == targetId) yield return cp;
        }
        foreach (var ip in vm.GetElementsByType<P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ItemPlaceholder>()) {
            if (ip.GameTimeContext == targetId) yield return ip;
        }
        foreach (var sp in vm.GetElementsByType<P2XMLEditor.GameData.VirtualMachineElements.Placeholders.ScenePlaceholder>()) {
            if (sp.GameTimeContext == targetId) yield return sp;
        }
    }
    
    public static IEnumerable<VmElement> GetDirectReferences(GameRoot target, VirtualMachine vm) {
        foreach (var element in GetDirectReferences((ParameterHolder)target, vm)) {
            yield return element;
        }
        var targetId = target.Id.ToString();
        foreach (var gr in vm.GetElementsByType<GameRoot>()) {
            if (gr.BaseToEngineGuidsTable != null && (gr.BaseToEngineGuidsTable.Keys.Any(k => k.Contains(targetId)) || gr.BaseToEngineGuidsTable.Values.Any(v => v.Contains(targetId)))) yield return gr;
            if (gr.HierarchyEngineGuidsTable != null && gr.HierarchyEngineGuidsTable.Any(s => s.Contains(targetId))) yield return gr;
        }
    }
}
