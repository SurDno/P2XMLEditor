namespace P2XMLEditor.Helper;

public static class Logger {
    private static readonly string ExeLogFilePath;
    private static readonly object LockObject = new();

    static Logger() {
        var logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logDirectory);
        ExeLogFilePath = Path.Combine(logDirectory, "P2XMLEditor.log");
        File.Delete(ExeLogFilePath);
    }
    
    private static void WriteToLogs(string content, bool timestamped = true) {
        var logMsg = timestamped ? $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {content}" : content;
        Console.WriteLine(logMsg);

        try {
            lock (LockObject) {
                File.AppendAllText(ExeLogFilePath, logMsg + Environment.NewLine);
            }
        } catch (Exception ex) {
            ErrorHandler.Handle($"Error writing to log file: {ex.Message}", ex, skipLogging: true);
        }
    }

    public static void LogError(string msg) => WriteToLogs($"ERROR: {msg}");
    public static void LogWarning(string msg) => WriteToLogs($"WARNING: {msg}");
    public static void LogInfo(string msg) => WriteToLogs($"INFO: {msg}");
    public static void LogLineBreak() => WriteToLogs(string.Empty, timestamped: false);
}