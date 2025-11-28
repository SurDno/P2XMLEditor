using System;
using System.Reflection;
using MethodDecorator.Fody.Interfaces;
using P2XMLEditor.Logging;

[assembly: PerformanceLogHook] 
namespace P2XMLEditor.Logging;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Class |
                AttributeTargets.Assembly)]
public class PerformanceLogHookAttribute : Attribute, IMethodDecorator {
	private IDisposable? _performanceLogger;
	private string? _methodName;
	private string? _typeName;

	public void Init(object instance, MethodBase method, object[] args) {
		if (SettingsHolder.LogLevel < LogLevel.Performance) return;
		_methodName = method.Name;
		_typeName = method.DeclaringType?.Name ?? string.Empty;
	}

	public void OnEntry() {
		if (SettingsHolder.LogLevel < LogLevel.Performance) return;
		_performanceLogger = PerformanceLogger.Log(_methodName!, _typeName!);
	}

	public void OnExit() {
		_performanceLogger?.Dispose();
	}

	public void OnException(Exception exception) {
		_performanceLogger?.Dispose();
	}
}