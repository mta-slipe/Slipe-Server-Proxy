using Microsoft.Extensions.Logging;
using SlipeServer.Server.ElementCollections;
using SlipeServer.Server.Elements;
using SlipeServer.Server.Enums;
using SlipeServer.Server.Services;
using System.Collections.Concurrent;

namespace SlipeServerProxy.Loggers;

public class ProxyLogger : ILogger
{
    private class ConsoleLoggerScope : IDisposable
    {
        private readonly Action onComplete;

        public ConsoleLoggerScope(Action onComplete)
        {
            this.onComplete = onComplete;
        }

        public void Dispose()
        {
            onComplete();
        }
    }

    private readonly static Dictionary<LogLevel, Tuple<ConsoleColor, string>> prefixes = new()
    {
        [LogLevel.Trace] = new Tuple<ConsoleColor, string>(ConsoleColor.Gray, " [trace]   "),
        [LogLevel.Debug] = new Tuple<ConsoleColor, string>(ConsoleColor.Yellow, " [debug]   "),
        [LogLevel.Information] = new Tuple<ConsoleColor, string>(ConsoleColor.Blue, " [info]    "),
        [LogLevel.Warning] = new Tuple<ConsoleColor, string>(ConsoleColor.DarkYellow, " [warn]    "),
        [LogLevel.Error] = new Tuple<ConsoleColor, string>(ConsoleColor.Red, " [error]   "),
        [LogLevel.Critical] = new Tuple<ConsoleColor, string>(ConsoleColor.DarkRed, " [critical]"),
        [LogLevel.None] = new Tuple<ConsoleColor, string>(ConsoleColor.White, "        "),
    };
    private readonly IElementCollection elementCollection;
    private readonly DebugLog debugLog;
    private string prefix;

    private readonly ConcurrentQueue<Action> logActions;

    public ProxyLogger(IElementCollection elementCollection, DebugLog debugLog)
    {
        this.elementCollection = elementCollection;
        this.debugLog = debugLog;

        logActions = new();
        prefix = "";

        Task.Run(LogWorker);
    }

    private async Task LogWorker()
    {
        while (true)
        {
            while (logActions.TryDequeue(out var action))
                action();
            await Task.Delay(10);
        }
    }

    public IDisposable BeginScope<TState>(TState state)
    {
        prefix += "  ";
        Console.WriteLine($"{prefix}{state}:");

        return new ConsoleLoggerScope(() => prefix = prefix[^2..]);
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
#if !DEBUG
            if (logLevel == LogLevel.Trace)
                return;
#endif

        var prefix = this.prefix;
        logActions.Enqueue(() =>
        {
            Console.Write($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}]");

            Console.ForegroundColor = prefixes[logLevel].Item1;
            Console.Write($"{prefixes[logLevel].Item2}");
            Console.ResetColor();

            Console.WriteLine($" {prefix}{formatter(state, exception)}");

            if (exception != null)
            {
                Console.WriteLine($" {prefix}{exception.StackTrace}");
            }

            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    OutputDebug($"{prefixes[logLevel].Item2} {prefix}{formatter(state, exception)}", DebugLevel.Error);
                    break;
                case LogLevel.Warning:
                    OutputDebug($"{prefixes[logLevel].Item2} {prefix}{formatter(state, exception)}", DebugLevel.Warning);
                    break;
                case LogLevel.Information:
                    OutputDebug($"{prefixes[logLevel].Item2} {prefix}{formatter(state, exception)}", DebugLevel.Information);
                    break;
            }
        });
    }

    private void OutputDebug(string message, DebugLevel level)
    {
        var players = elementCollection.GetByType<Player>(ElementType.Player);
        foreach (var player in players)
        {
            debugLog.OutputTo(player, message, level);
        }
    }
}
