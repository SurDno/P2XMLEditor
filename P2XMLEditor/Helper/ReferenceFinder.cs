using System.Collections.Generic;
using P2XMLEditor.Core;
using P2XMLEditor.GameData;
using P2XMLEditor.GameData.VirtualMachineElements;
using P2XMLEditor.GameData.VirtualMachineElements.Abstract;
using P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

namespace P2XMLEditor.Helper;

public class ReferenceFinder {
    private readonly VirtualMachine _vm;

    public Dictionary<ulong, HashSet<VmElement>> ReferenceIndex { get; } = new();
    public Dictionary<string, HashSet<VmElement>> NameReferenceIndex { get; } = new();

    public ReferenceFinder(VirtualMachine vm) {
        _vm = vm;
        BuildIndex();
    }

    private void BuildIndex() {
        foreach (var typeList in _vm.ElementsByType.Values) {
            foreach (var el in typeList) {
                var references = ManualReferenceFinder.ExtractReferences(el);

                foreach (var refObj in references) {
                    if (refObj == null) continue;

                    if (refObj is VmElement targetEl) {
                        if (!ReferenceIndex.TryGetValue(targetEl.Id, out var set)) {
                            set = new HashSet<VmElement>();
                            ReferenceIndex[targetEl.Id] = set;
                        }
                        set.Add(el);
                    } 
                    else if (refObj is ParameterSource ps) {
                        if (ps.ParameterReference != null) {
                            if (!ReferenceIndex.TryGetValue(ps.ParameterReference.Id, out var set)) {
                                set = new HashSet<VmElement>();
                                ReferenceIndex[ps.ParameterReference.Id] = set;
                            }
                            set.Add(el);
                        }
                        if (ps.ElementReference != null) {
                            if (!ReferenceIndex.TryGetValue(ps.ElementReference.Id, out var set)) {
                                set = new HashSet<VmElement>();
                                ReferenceIndex[ps.ElementReference.Id] = set;
                            }
                            set.Add(el);
                        }
                        if (ps.PrefixHolder != null) {
                            if (!ReferenceIndex.TryGetValue(ps.PrefixHolder.Id, out var set)) {
                                set = new HashSet<VmElement>();
                                ReferenceIndex[ps.PrefixHolder.Id] = set;
                            }
                            set.Add(el);
                        }
                        if (ps.DynamicObjectReference != null) {
                            if (!ReferenceIndex.TryGetValue(ps.DynamicObjectReference.Id, out var set)) {
                                set = new HashSet<VmElement>();
                                ReferenceIndex[ps.DynamicObjectReference.Id] = set;
                            }
                            set.Add(el);
                        }
                        if (ps.DynamicParameterName != null) {
                            if (!NameReferenceIndex.TryGetValue(ps.DynamicParameterName, out var strSet)) {
                                strSet = new HashSet<VmElement>();
                                NameReferenceIndex[ps.DynamicParameterName] = strSet;
                            }
                            strSet.Add(el);
                        }
                        if (ps.MessageReference != null && ps.MessageReference.Name != null) {
                            if (!NameReferenceIndex.TryGetValue(ps.MessageReference.Name, out var strSet)) {
                                strSet = new HashSet<VmElement>();
                                NameReferenceIndex[ps.MessageReference.Name] = strSet;
                            }
                            strSet.Add(el);
                        }
                        if (ps.LiteralValue?.Serialize() is string lit) {
                            if (!NameReferenceIndex.TryGetValue(lit, out var strSet)) {
                                strSet = new HashSet<VmElement>();
                                NameReferenceIndex[lit] = strSet;
                            }
                            strSet.Add(el);
                        }
                        if (ps.HierarchyReference != null) {
                            foreach (var hEl in ps.HierarchyReference.Elements) {
                                if (hEl.Element != null) {
                                    if (!ReferenceIndex.TryGetValue(hEl.Element.Id, out var hSet)) {
                                        hSet = new HashSet<VmElement>();
                                        ReferenceIndex[hEl.Element.Id] = hSet;
                                    }
                                    hSet.Add(el);
                                }
                            }
                        }
                        if (ps.PrefixHierarchy != null) {
                            foreach (var hEl in ps.PrefixHierarchy.Elements) {
                                if (hEl.Element != null) {
                                    if (!ReferenceIndex.TryGetValue(hEl.Element.Id, out var hSet)) {
                                        hSet = new HashSet<VmElement>();
                                        ReferenceIndex[hEl.Element.Id] = hSet;
                                    }
                                    hSet.Add(el);
                                }
                            }
                        }
                    }
                    else if (refObj is string str) {
                        if (!NameReferenceIndex.TryGetValue(str, out var strSet)) {
                            strSet = new HashSet<VmElement>();
                            NameReferenceIndex[str] = strSet;
                        }
                        strSet.Add(el);
                    }
                    else if (refObj is HierarchyGuid hGuid) {
                        foreach (var hEl in hGuid.Elements) {
                            if (hEl.Element != null) {
                                if (!ReferenceIndex.TryGetValue(hEl.Element.Id, out var hSet)) {
                                    hSet = new HashSet<VmElement>();
                                    ReferenceIndex[hEl.Element.Id] = hSet;
                                }
                                hSet.Add(el);
                            }
                        }
                    }
                }
            }
        }
    }
}
