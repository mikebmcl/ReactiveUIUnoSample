#define WRITE_DIAGNOSTIC_TO_TRACE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using Microsoft.Extensions.Logging;

using Uno.Extensions;

namespace ReactiveUIUnoSample
{
    public static class DiagnosticsHelpers
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Any type (the only operation that occurs on the objects is that <see cref="object.ToString"/> is called on them</typeparam>
        /// <param name="enumerable"></param>
        /// <param name="addIfSemicolonInString">If <c>null</c> or empty, nothing will be added. Otherwise any item in <paramref name="enumerable"/> that contains a semicolon will be surrounded by this string, e.g. (with default value) (king; queen; or regent) would become '(king; queen; or regent)'.</param>
        /// <returns></returns>
        public static string CombineToStringResultsWithSemicolonSeparator<T>(this IEnumerable<T> enumerable, string addIfSemicolonInString = null)
        {
            return CombineToStringResultsWithCustomSeparator(enumerable, "; ", addIfSemicolonInString);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T">Any type (the only operation that occurs on the objects is that <see cref="object.ToString"/> is called on them</typeparam>
        /// <param name="enumerable"></param>
        /// <param name="addIfCommaInString">If <c>null</c> or empty, nothing will be added. Otherwise any item in <paramref name="enumerable"/> that contains a comma will be surrounded by this string, e.g. (with default value) (king, queen, or regent) would become '(king, queen, or regent)'.</param>
        /// <returns></returns>
        public static string CombineToStringResultsWithCommaSeparator<T>(this IEnumerable<T> enumerable, string addIfCommaInString = "'")
        {
            return CombineToStringResultsWithCustomSeparator(enumerable, ", ", addIfCommaInString);
        }

        /// <summary>
        /// Concatenates objects using a specified separator string, with various other adjustments based on argument values for the other parameters.
        /// </summary>
        /// <typeparam name="T">Any type (the only operation that occurs on the objects is that <see cref="object.ToString"/> is called on them</typeparam>
        /// <param name="enumerable"></param>
        /// <param name="separator">Must not be <c>null</c> or an empty string.</param>
        /// <param name="addIfSeparatorInString">If <c>null</c> or empty, nothing will be added. Otherwise any item in <paramref name="enumerable"/> that contains <paramref name="separator"/> will be surrounded by this string; for example, with the default value "'" "(king, queen, or regent)" would become "'(king, queen, or regent)'".</param>
        /// <returns><code>enumerable == null</code>null<code>enumerable != null</code>the result of concatenating each non-null item in <paramref name="enumerable"/> separated by <paramref name="separator"/>, with <paramref name="addIfSeparatorInString"/> added if and where applicable. The result will not begin or end with <paramref name="separator"/> unless the first or last value of <paramref name="enumerable"/> returns the value of <paramref name="separator"/> when <see cref="object.ToString"/> is called on them.</returns>
        public static string CombineToStringResultsWithCustomSeparator<T>(this IEnumerable<T> enumerable, string separator, string addIfSeparatorInString = "'")
        {
            if (enumerable == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(separator))
            {
                throw new ArgumentException($"{nameof(separator)} must have a non-null, non-empty value.", nameof(separator));
            }

            StringBuilder result = new StringBuilder();
            bool addSeparator = false;
            bool addIfSeparatorInStringNotNullOrEmpty = !string.IsNullOrEmpty(addIfSeparatorInString);
            foreach (T item in enumerable)
            {
                //string stringToAdd;
                if (!(item is string stringToAdd))
                {
                    stringToAdd = item?.ToString();
                    if (stringToAdd == null)
                    {
                        continue;
                    }
                }
                if (addSeparator)
                {
                    _ = result.Append(separator);
                }
                addSeparator = true;
                _ = addIfSeparatorInStringNotNullOrEmpty
                    ? stringToAdd.Contains(separator)
                        ? result.Append(addIfSeparatorInString + stringToAdd + addIfSeparatorInString)
                        : result.Append(stringToAdd)
                    : result.Append(stringToAdd);
            }
            return result.ToString();
        }

        /// <summary>
        /// Just a straight-up concatenation. Use <see cref="CombineToStringResultsWithCommaSeparator{T}(IEnumerable{T}, string)"/>, <see cref="CombineToStringResultsWithSemicolonSeparator{T}(IEnumerable{T}, string)"/>, or <see cref="CombineToStringResultsWithCustomSeparator{T}(IEnumerable{T}, string, string)"/> if you want any bells or whistles.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        public static string CombineToString<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return null;
            }

            StringBuilder result = new StringBuilder();
            foreach (T item in enumerable)
            {
                //string stringToAdd;
                if (!(item is string stringToAdd))
                {
                    stringToAdd = item?.ToString();
                    if (stringToAdd == null)
                    {
                        continue;
                    }
                }
                _ = result.Append(stringToAdd);
            }
            return result.ToString();
        }

        public static T OnlyOrDefault<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() == 1 ? enumerable.First() : default;
        }

        /// <summary>
        /// Should be set to an appropriate method to terminate the application because of an unrecoverable error. This is used by
        /// <see cref="ReportProblem(string, LogLevel, Exception, Action{string}, bool, string, string, int)"/> when called with
        /// <see cref="LogLevel.Critical"/> unless this remains null.
        /// Typically this will be <see cref="Windows.UI.Xaml.Application.Exit" 
        /// </summary>
        public static Action TerminateApplication { get; set; }

        /// <summary>
        /// Appends data about an exception and its inner exception recursively, including handling <see cref="AggregateException"/>, to a diagnostic string.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="diagnosticString"></param>
        /// <param name="tabs">Used to progressively indent inner exceptions; if null it will be set to "\t". Typically you want the default value.</param>
        /// <returns></returns>
        public static string GetDiagnosticStringWithExceptionData(Exception ex, string diagnosticString, string tabs = null)
        {
            if (tabs == null)
            {
                tabs = "\t";
            }
            if (diagnosticString == null)
            {
                diagnosticString = "";
            }
            if (ex == null)
            {
                return diagnosticString + $"\n{tabs}{ex} exception argument was null\n\n{tabs}********************\n{tabs}********************\n{tabs}********************\n";
            }
            string dataString = $"{tabs}Type: {ex.GetType().FullName}\n{tabs}HRESULT: {ex.HResult:X8}\n{tabs}Message: {ex.Message ?? "(no message)"}\n{tabs}Stack Trace: {ex.StackTrace ?? "(no stack trace)"}\n\n{tabs}********************\n{tabs}********************\n{tabs}********************\n";
            if (ex is AggregateException aggregate)
            {
                diagnosticString += $"{tabs}Aggregate Inner Exception exists. Data follows.\n{dataString}{tabs}Begin Aggregate Inner Exceptions\n";
                foreach (Exception item in aggregate.InnerExceptions)
                {
                    string aggregateTabs = tabs + "\t";
                    diagnosticString = GetDiagnosticStringWithExceptionData(item, diagnosticString, aggregateTabs);
                }
                return diagnosticString + $"{tabs}End Aggregate Inner Exceptions\n\n{tabs}********************\n{tabs}********************\n{tabs}********************\n";
            }
            else
            {
                diagnosticString += $"{tabs}Inner Exception exists. Data follows.\n{dataString}";
                return ex?.InnerException != null
                    ? GetDiagnosticStringWithExceptionData(ex.InnerException, diagnosticString, tabs + "\t")
                    : diagnosticString + $"{tabs}End Inner Exception\n\n{tabs}********************\n{tabs}********************\n{tabs}********************\n";
            }
        }
        public const string BeginDiagnosticStringValue = "\n\nBegin Diagnostic";

        public const string DebugDefaultConditionForConditionalAttributeString = "DEBUG";

        /// <summary>
        /// Reports an issue via a unified mechanism. Will throw an <see cref="Exception"/> with the diagnostic message string it creates as its message and with <paramref name="innerException"/> as its inner exception when either <paramref name="severity"/> is <see cref="LogLevel.Error"/> and <c>DEBUG</c> is defined or when <paramref name="severity"/> is <see cref="LogLevel.Critical"/> regardless of other conditions. These are thrown in a finally block in case an exception occurs internally. So it is recommended that you catch any exception, and pass it to this using <paramref name="innerException"/> and then deal with it appropriately after calling this method (though you won't get there when this method (re)throws, as specified above). If <paramref name="severity"/> is <see cref="LogLevel.Warning"/> and <c>DEBUG</c> is defined, <see cref="Debugger.Break"/> is called after the diagnostic message is called to enable examination of the stack, etc.
        /// </summary>
        /// <param name="message">Some description of what went wrong, possibly a stack trace, etc.</param>
        /// <param name="severity">The severity level of the problem.</param>
        /// <param name="logger">The logger to be used. Can be null. Typically you get this by adding:
        /// <code>
        /// using Uno.Extensions;
        /// using Microsoft.Extensions.Logging;
        /// </code>
        /// to the file that you want to get the Uno ILogger from and then caling <c>this.Log()</c> to get it. 
        /// See <see href="https://platform.uno/docs/articles/logging.html"/> for more details.
        /// </param>
        /// <param name="innerException">If this is being called within a catch block or there is otherwise an exception available (e.g. through <see cref="System.Threading.Tasks.Task.Exception"/>), pass the exception so that it can be sent to any TraceListeners as a message and can be passed as an inner exception in situations where this method will throw. Can be <c>null</c>.</param>
        /// <param name="callBeforeFinalActions">An <see cref="Action{T}"/> to run, regardless of whether the method will throw, but if it will throw then this will be called before any exceptions are thrown. The action will be passed the diagnostic string constructed by this method. One possible use would be to display error information to the user. You are responsible for ensuring that any parts of this action that must be run on the UI thread are run on it. Be wary of running async actions that are allowed to continue running after the action returns since if this method does throw those async actions may not complete or could introduce race conditions.</param>
        /// <param name="addPaddingLines"><code>true</code> Adds all the **** lines before and after the diagnostic.<code>false</code> No **** lines, just the diagnostic.</param>
        /// <param name="callerMemberName">Only pass a value if you want to hide the name of the caller or if you want to assign blame to some method that called the method that this is being called from.</param>
        /// <param name="callerFilePath">Only pass a value if you want to hide the file name or otherwise want to assign blame to some method that called the method that this is being called from.</param>
        /// <param name="callerLineNumber">Only pass a value if you want to hide the line number of the caller or if you want to assign blame to some method that called the method that this is being called from.</param>
        public static void ReportProblem(string message, LogLevel severity, ILogger logger, Exception innerException = null, Action<string> callBeforeFinalActions = null, bool addPaddingLines = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0)
        {
            if (severity == LogLevel.None)
            {
                return;
            }
            if (callerMemberName == null)
            {
                callerMemberName = "";
            }
            if (callerFilePath == null)
            {
                callerFilePath = "";
            }
            string diagnosticString = addPaddingLines
                ? $"{BeginDiagnosticStringValue}\n********************\n********************\n********************\n\n\nFile Path: {callerFilePath}\nMethod Name: {callerMemberName}\nLine Number: {callerLineNumber}\nSeverity: {severity}\nMessage: {message}\n\n********************\n********************\n********************\n"
                : $"n\nFile Path: {callerFilePath}\nMethod Name: {callerMemberName}\nLine Number: {callerLineNumber}\nSeverity: {severity}\nMessage: {message}\n\n";
            if (innerException != null)
            {
                diagnosticString = GetDiagnosticStringWithExceptionData(innerException, diagnosticString, "\t");
            }
            diagnosticString += "End Diagnostic\n";
            Exception exception = null;
            try
            {
                Debug.WriteLine(diagnosticString);
                switch (severity)
                {
                    case LogLevel.Trace:
#if WRITE_DIAGNOSTIC_TO_TRACE
                        Trace.TraceInformation($"{nameof(LogLevel)}.{nameof(LogLevel.Trace)}: {diagnosticString}");
#endif
                        logger?.LogTrace(diagnosticString);
                        break;
                    case LogLevel.Debug:
#if WRITE_DIAGNOSTIC_TO_TRACE
#if DEBUG
                        Trace.TraceInformation($"{nameof(LogLevel)}.{nameof(LogLevel.Debug)}: {diagnosticString}");
#endif
#endif
                        logger?.LogDebug(diagnosticString);
                        break;
                    case LogLevel.Information:
#if WRITE_DIAGNOSTIC_TO_TRACE
                        Trace.TraceInformation($"{nameof(LogLevel)}.{nameof(LogLevel.Information)}: {diagnosticString}");
#endif
                        logger?.LogInformation(diagnosticString);
                        break;
                    case LogLevel.Warning:
#if WRITE_DIAGNOSTIC_TO_TRACE
                        Trace.TraceWarning(diagnosticString);
#endif
                        logger?.LogWarning(diagnosticString);
                        break;
                    case LogLevel.Error:
#if WRITE_DIAGNOSTIC_TO_TRACE
                        Trace.TraceError($"{nameof(LogLevel)}.{nameof(LogLevel.Error)}: {diagnosticString}");
#endif
                        logger?.LogError(diagnosticString);
                        break;
                    case LogLevel.Critical:
#if WRITE_DIAGNOSTIC_TO_TRACE
                        Trace.TraceError($"{nameof(LogLevel)}.{nameof(LogLevel.Critical)}: {diagnosticString}");
#endif
                        logger?.LogCritical(diagnosticString);
                        break;
                    case LogLevel.None:
                        break;
                    default:
                        {
                            var unknownEnumString = $"Unknown {nameof(LogLevel)} enum value {severity} in call from File Path: {callerFilePath}\nMethod Name: {callerMemberName}\nLine Number: {callerLineNumber}";
#if WRITE_DIAGNOSTIC_TO_TRACE
                            Trace.TraceWarning(unknownEnumString);
#endif
                            logger?.LogWarning(unknownEnumString);
                            break;
                        }
                }

                if (severity == LogLevel.Critical)
                {
                    Debugger.Break();
                    // Stupid .NET swallows many exceptions so we use Application.Quit instead.
                    //TerminateApplication?.Invoke();
                    exception = innerException == null
                        ? new Exception(diagnosticString)
                        : innerException is AggregateException aggregate
                            ? new AggregateException(diagnosticString, aggregate.InnerExceptions)
                            : new Exception(diagnosticString, innerException);
                }
            }
            finally
            {
                callBeforeFinalActions?.Invoke(diagnosticString);
                DebugOnlySeverityConditionalBehavior(severity, innerException, diagnosticString);
            }
            if (exception != null)
            {
                Debugger.Break();
                // Stupid .NET swallows many exceptions so we use Application.Quit instead.
                TerminateApplication?.Invoke();
                throw exception;
            }
        }

        /// <summary>
        /// This is used in a ConditionalAttribute marked method that is ignored unless DEBUG is defined so in a Release build this is normally ignored because
        /// the method that uses will not be called (unless you define DEBUG). If <c>true</c> and DEBUG is defined, <see cref="Debugger.Break"/> will be called
        /// whenever <see cref="ReportProblem(string, LogLevel, Exception, Action{string}, bool, string, string, int)"/> is called with <see cref="LogLevel.Warning"/>.
        /// </summary>
        public static bool DebuggerBreakOnLogLevelWarning { get; set; } = true;

        /// <summary>
        /// This is used in a ConditionalAttribute marked method that is ignored unless DEBUG is defined so in a Release build this is normally ignored because
        /// the method that uses will not be called (unless you define DEBUG). If <c>true</c> and DEBUG is defined, <see cref="Debugger.Break"/> will be called
        /// whenever <see cref="ReportProblem(string, LogLevel, Exception, Action{string}, bool, string, string, int)"/> is called with <see cref="LogLevel.Debug"/>.
        /// </summary>
        public static bool DebuggerBreakOnLogLevelDebug { get; set; } = true;

        [Conditional(DebugDefaultConditionForConditionalAttributeString)]
        private static void DebugOnlySeverityConditionalBehavior(LogLevel severity, Exception innerException, string diagnosticString)
        {
            if (severity == LogLevel.Error)
            {
                Debugger.Break();
                if (innerException == null)
                {
                    throw new Exception(diagnosticString);
                }
                else
                {
                    if (innerException is AggregateException aggregate)
                    {
                        throw new AggregateException(diagnosticString, aggregate.InnerExceptions);
                    }
                    else
                    {
                        throw new Exception(diagnosticString, innerException);
                    }
                }
            }
            if (severity == LogLevel.Warning && DebuggerBreakOnLogLevelWarning)
            {
                Debugger.Break();
            }
            if (severity == LogLevel.Debug && DebuggerBreakOnLogLevelDebug)
            {
                Debugger.Break();
            }
        }

        /// <summary>
        /// Reports a diagnostic message only in debug configurations.
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="callAtEnd">An optional <see cref="Action{T}"/> that will be called at the end of the method. The action will be passed the diagnostic string constructed by this method. One possible use would be to display error information to the user. You are responsible for ensuring that any parts of this action that must be run on the UI thread are run on it.</param>
        /// <param name="addPaddingLines"><code>true</code> Adds all the **** lines before and after the diagnostic.<code>false</code> No **** lines, just the diagnostic with two empty lines before and after it (to aid in locating the messages).</param>
        /// <param name="addEmptySpaceLines">Spacer lines, if any, are added contained within the lines added when <paramref name="addPaddingLines"/> is <c>true</c>.
        /// <code>true</code>Adds two empty lines before and after the diagnostic (to aid in locating the diagnostic messages).
        /// <code>false</code>No lines are added.</param>
        /// <param name="callerMemberName">Only pass a value if you want to hide the name of the caller or if you want to assign blame to some method that called the method that this is being called from.</param>
        /// <param name="callerFilePath">Only pass a value if you want to hide the file name or otherwise want to assign blame to some method that called the method that this is being called from.</param>
        /// <param name="callerLineNumber">Only pass a value if you want to hide the line number of the caller or if you want to assign blame to some method that called the method that this is being called from.</param>
        /// <param name="ex">The exception, if any, that should have its info included in the diagnostic string by use of <see cref="GetDiagnosticStringWithExceptionData(Exception, string, string)"/>.</param>
        [Conditional(DebugDefaultConditionForConditionalAttributeString)]
        public static void DebugDiagnostic(string message, ILogger logger, Action<string> callAtEnd = null, bool addPaddingLines = true, bool addEmptySpaceLines = true, [CallerMemberName] string callerMemberName = null, [CallerFilePath] string callerFilePath = null, [CallerLineNumber] int callerLineNumber = 0, Exception ex = null)
        {
            if (callerMemberName == null)
            {
                callerMemberName = "";
            }
            if (callerFilePath == null)
            {
                callerFilePath = "";
            }
            string spacerLines = addEmptySpaceLines ? "\n\n" : "";
            string diagnosticString = $"{spacerLines}File Path: {callerFilePath}\nMethod Name: {callerMemberName}\nLine Number: {callerLineNumber}\nSeverity: DEBUG DIAGNOSTIC\nMessage: {message}{spacerLines}";
            if (addPaddingLines)
            {
                diagnosticString = $"{BeginDiagnosticStringValue}\n********************\n********************\n********************\n" + diagnosticString;
            }
            if (ex != null)
            {
                diagnosticString = GetDiagnosticStringWithExceptionData(ex, diagnosticString);
            }
            diagnosticString += "********************\n********************\n********************\nEnd Diagnostic\n";
            Debug.WriteLine(diagnosticString);
#if WRITE_DIAGNOSTIC_TO_TRACE
            Trace.TraceInformation(diagnosticString);
#endif
            logger?.LogInformation(diagnosticString);
            callAtEnd?.Invoke(diagnosticString);
        }
    }
}
