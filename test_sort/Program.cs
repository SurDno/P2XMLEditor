using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class Program {
    public static void Main() {
        var items = new List<string> {
            "D1_Theater_Marker",
            "D10_Oyun_Here",
            "D1-2_Bull_Salesman_Default_Marker",
            "D11_Aglaya_Escape",
            "Dora_ChildServiceLady_state",
            "DoraPoint",
            "CreepyObjects",
            "CreepyObjectSirieZastroiki"
        };

        var tests = new Dictionary<string, StringComparer> {
            { "Ordinal", StringComparer.Ordinal },
            { "OrdinalIgnoreCase", StringComparer.OrdinalIgnoreCase },
            { "InvariantCulture", StringComparer.InvariantCulture },
            { "InvariantCulture (StringSort)", StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.StringSort) },
            { "InvariantCultureIgnoreCase", StringComparer.InvariantCultureIgnoreCase },
            { "InvariantCultureIgnoreCase (StringSort)", StringComparer.Create(CultureInfo.InvariantCulture, CompareOptions.StringSort | CompareOptions.IgnoreCase) },
            { "Current (RU)", StringComparer.Create(new CultureInfo("ru-RU"), false) },
            { "Current (RU) StringSort", StringComparer.Create(new CultureInfo("ru-RU"), CompareOptions.StringSort) }
        };

        foreach (var test in tests) {
            Console.WriteLine($"--- {test.Key} ---");
            var sorted = items.OrderBy(x => x, test.Value).ToList();
            foreach (var s in sorted) {
                Console.WriteLine(s);
            }
        }
    }
}
