using System.Diagnostics.CodeAnalysis;
using P2XMLEditor.Logging;
using PostSharp.Aspects;
using PostSharp.Serialization;

[assembly: AutoPerformanceLogHook(AttributeTargetTypes = "P2XMLEditor.Logging.*", AttributeExclude = true)]
namespace P2XMLEditor.Logging;

[PSerializable]
public class AutoPerformanceLogHook : OnMethodBoundaryAspect {

	public override void OnEntry(MethodExecutionArgs args) {
		if (SettingsHolder.LogLevel < LogLevel.Performance) return;
			
		var actualMethod = args.Method;
		var typeName = actualMethod.DeclaringType?.Name ?? string.Empty;
		var methodName = actualMethod.Name;
		
		var performanceLogger = PerformanceLogger.Log(methodName, typeName);
		args.MethodExecutionTag = performanceLogger;
	}

	public override void OnExit(MethodExecutionArgs args) {
		if (args.MethodExecutionTag is IDisposable disposable)
			disposable.Dispose();
	}

	private static bool IsPropertyAccessor(string methodName) => methodName.StartsWith("get_") || 
	                                                             methodName.StartsWith("set_");
	
	private static bool IsConstructor(string methodName) => methodName.StartsWith(".ctor") ||
	                                                        methodName.EndsWith(".cctor");
}