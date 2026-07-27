using System;

namespace P2XMLEditor.GameData.VirtualMachineElements.InternalTypes;

public struct GameTime {
	private double totalValue;
	public static readonly GameTime Zero;
	private const int DaySeconds = 86400;
	private const int HourSeconds = 3600;
	private const int MinuteSeconds = 60;
	public ushort Days {
		get => (ushort)((uint)(int)Math.Floor(totalValue) / 86400u);
		set {
			var hours = Hours;
			var minutes = Minutes;
			var seconds = Seconds;
			var lastSecond = LastSecond;
			totalValue = value * 86400 + hours * 3600 + minutes * 60 + seconds + lastSecond;
		}
	}
	public byte Hours {
		get => (byte)((int)Math.Floor(totalValue) % 86400 / 3600);
		set {
			var days = Days;
			var minutes = Minutes;
			var seconds = Seconds;
			var lastSecond = LastSecond;
			totalValue = days * 86400 + value * 3600 + minutes * 60 + seconds + lastSecond;
		}
	}
	public byte Minutes {
		get => (byte)((int)Math.Floor(totalValue) % 3600 / 60);
		set {
			var days = Days;
			var hours = Hours;
			var seconds = Seconds;
			var lastSecond = LastSecond;
			totalValue = days * 86400 + hours * 3600 + value * 60 + seconds + lastSecond;
		}
	}
	public byte Seconds {
		get => (byte)((uint)(int)Math.Floor(totalValue) % 60u);
		set {
			var days = Days;
			var hours = Hours;
			var minutes = Minutes;
			var lastSecond = LastSecond;
			totalValue = days * 86400 + hours * 3600 + minutes * 60 + value + lastSecond;
		}
	}
	public ulong TotalSeconds {
		get => (ulong)Math.Floor(totalValue);
		set {
			var num = totalValue - Math.Floor(totalValue);
			totalValue = value + num;
		}
	}
	public double TotalValue => totalValue;
	public double LastSecond => totalValue - Math.Floor(totalValue);
	public GameTime(ushort days, byte hours, byte minutes, byte seconds) {
		totalValue = days * 86400 + hours * 3600 + minutes * 60 + seconds;
	}
	public GameTime(int days, int hours, int minutes, int seconds) {
		totalValue = days * 86400 + hours * 3600 + minutes * 60 + seconds;
	}
	public GameTime(ulong totalSeconds) {
		totalValue = totalSeconds;
	}
	public GameTime(ulong totalSeconds, double lastSecond) {
		totalValue = totalSeconds + lastSecond;
	}
	public static implicit operator TimeSpan(GameTime t) => TimeSpan.FromSeconds(t.totalValue);
	public static implicit operator GameTime(TimeSpan t) => new GameTime((ulong)t.TotalSeconds);
	public static bool operator ==(GameTime t1, GameTime t2) => t1.totalValue == t2.totalValue;
	public static bool operator !=(GameTime t1, GameTime t2) => t1.totalValue != t2.totalValue;
	public override bool Equals(object? obj) {
		if (obj is GameTime gameTime) {
			return gameTime.totalValue == totalValue;
		}
		return false;
	}
	public override int GetHashCode() => totalValue.GetHashCode();
	public override string ToString() => $"{Days}:{Hours}:{Minutes}:{Seconds}";
	public string ToString(string format) {
		if (format == "d\\.hh\\:mm\\:ss") {
			return $"{Days}.{Hours:D2}:{Minutes:D2}:{Seconds:D2}";
		}
		return ToString();
	}
	static GameTime() {
	}
}
