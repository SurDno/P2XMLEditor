using System.Globalization;
using System.Xml.Linq;
using P2XMLEditor.GameData.Enums;
using P2XMLEditor.Helper;

namespace P2XMLEditor.GameData.Types;

public interface IParameter {
	ParameterName Name { get; set; }
	XElement ToXml();
	void LoadFromXml(XElement element);
}

public class Parameter<T> : IParameter {
	public ParameterName Name { get; set; }
	public T Value { get; set; }

	protected T ConvertValue(string v) {
		if (typeof(T).IsEnum) 
			return (T)(object)typeof(T).Deserialize(v);
		if (typeof(T) == typeof(TimeSpan))
			return (T)(object)XmlParsingHelper.ParseTimeSpan(v);
		return (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture);
	}

	public virtual void LoadFromXml(XElement element) {
		Name = element.Element("Name")!.Value.Deserialize<ParameterName>();
		Value = ConvertValue(element.Element("Value")!.Value);
	}

	public virtual XElement ToXml() {
		return new XElement("Item",
			new XAttribute("type", GetXmlType()),
			new XElement("Name", Name.Serialize()),
			new XElement("Value", typeof(T).IsEnum ? Value!.ToString() : Value)
		);
	}
	
	protected virtual string GetXmlType() {
		var t = typeof(T);
		return t switch {
			_ when t == typeof(bool) => "BoolParameter",
			_ when t == typeof(StammKind) => "StammKindParameter",
			_ when t == typeof(FractionEnum) => "FractionParameter",
			_ when t == typeof(ContainerOpenState) => "ContainerOpenStateEnumParameter",
			_ when t == typeof(LiquidType) => "LiquidTypeParameter",
			_ when t == typeof(WeaponKind) => "WeaponKindParameter",
			_ when t == typeof(BlockType) => "BlockTypeParameter",
			_ when t == typeof(BoundHealthState) => "BoundHealthStateParameter",
			_ when t == typeof(CombatStyle) => "CombatStyleParameter",
			_ when t == typeof(FastTravelPoint) => "FastTravelPointParameter",
			_ => throw new NotImplementedException($"Not implemented Parameter type {nameof(T)}")
		};
	}
}

public class ResetableParameter<T> : Parameter<T> {
	public T BaseValue { get; set; }

	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);
		BaseValue = ConvertValue(element.Element("BaseValue")!.Value);
	}

	public override XElement ToXml() {
		return new XElement("Item",
			new XAttribute("type", GetXmlType()),
			new XElement("Name", Name.Serialize()),
			new XElement("Value", Value),
			new XElement("BaseValue", BaseValue)
		);
	}
	
	protected override string GetXmlType() {
		var t = typeof(T);
		return t switch {
			_ when t == typeof(DetectType) => "ResetableDetectTypeParameter",
			_ when t == typeof(bool) => "ResetableBoolParameter",
			_ => throw new NotImplementedException($"Not implemented ResetableParameter type {nameof(T)}")
		};
	}
}

public class MinMaxParameter<T> : Parameter<T> {
	public T MinValue { get; set; }
	public T MaxValue { get; set; }

	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);
		MinValue = ConvertValue(element.Element("MinValue")!.Value);
		MaxValue = ConvertValue(element.Element("MaxValue")!.Value);
	}

	public override XElement ToXml() {
		var baseXml = base.ToXml(); 
		baseXml.Add(new XElement("MinValue", MinValue)); 
		baseXml.Add(new XElement("MaxValue", MaxValue));
		return baseXml;
	}
	
	protected override string GetXmlType() {
		var t = typeof(T);
		return t switch {
			_ when t == typeof(float) => "FloatParameter",
			_ when t == typeof(int) => "IntParameter",
			_ when t == typeof(TimeSpan) => "TimeSpanParameter",
			_ => throw new NotImplementedException($"Not implemented MinMaxParameter type {nameof(T)}")
		};
	}
}

public class ResetableMinMaxParameter<T> : MinMaxParameter<T> {
	public T BaseValue { get; set; }

	public override void LoadFromXml(XElement element) {
		base.LoadFromXml(element);
		BaseValue = ConvertValue(element.Element("BaseValue")!.Value);
	}

	public override XElement ToXml() {
		return new XElement("Item",
			new XAttribute("type", GetXmlType()),
			new XElement("Name", Name.Serialize()),
			new XElement("Value", Value),
			new XElement("BaseValue", BaseValue),
			new XElement("MinValue", MinValue),
			new XElement("MaxValue", MaxValue)
		);
	}
	
	protected override string GetXmlType() {
		var t = typeof(T);
		return t switch {
			_ when t == typeof(float) => "ResetableFloatParameter",
			_ when t == typeof(int) => "ResetableIntParameter",
			_ => throw new NotImplementedException($"Not implemented ResetableMinMaxParameter type {nameof(T)}")
		};
	}
}

public class PriorityParameter<T> : IParameter {
    public ParameterName Name { get; set; }
    
    public List<Entry> Items { get; } = new();

    public struct Entry {
        public Priority Priority;
        public T Value;
    }

    protected T ConvertValue(string v) {
	    var t = typeof(T);
	    return t.IsEnum ? (T)(object)t.Deserialize(v) : (T)Convert.ChangeType(v, t, CultureInfo.InvariantCulture);
    }

    public void LoadFromXml(XElement element) {
        Name = element.Element("Name")!.Value.Deserialize<ParameterName>();
        foreach (var item in element.Element("Container")!.Element("Items")!.Elements("Item")) {
            var priority = item.Element("Priority")!.Value.Deserialize<Priority>();
            var value = ConvertValue(item.Element("Value")!.Value);
            Items.Add(new Entry { Priority = priority, Value = value });
        }
    }

    public XElement ToXml() {
        var container = new XElement("Container");
        var list = new XElement("Items");

        foreach (var e in Items) {
            list.Add(new XElement("Item",
                new XElement("Priority", e.Priority.Serialize()),
                new XElement("Value", typeof(T).IsEnum ? e.Value!.ToString() : e.Value)
            ));
        }

        container.Add(list);

        return new XElement("Item",
            new XAttribute("type", GetXmlType()),
            new XElement("Name", Name.Serialize()),
            container
        );
    }

    protected string GetXmlType() {
        var t = typeof(T);
        return t switch {
            _ when t == typeof(bool) => "BoolPriorityParameter",
            _ when t == typeof(LockState) => "LockStatePriorityParameter",
            _ => throw new NotImplementedException($"Not implemented PriorityParameter type {nameof(T)}")
        };
    }
}
