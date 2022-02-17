using DynamicData;

using Uno.Extensions;
using Microsoft.Extensions.Logging;

using ReactiveUI;

using ReactiveUIUnoSample.ViewModels;

using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;


namespace ReactiveUIUnoSample
{
    public class ExceptionObserver : IObserver<Exception>, IDisposable
    {
        private IDisposable _unsubscriber;
        private CancellationToken _unsubscribeToken;
        private readonly string _identifier;
        private LogLevel _logLevel;
        private ILogger _logger;
        //private readonly WeakReference<Func<string>> _additionalLogInfoFuncWeakRef = new WeakReference<Func<string>>(null);
        private Func<string> _additionalLogInfoFunc;
        private bool disposedValue;

        public ExceptionObserver(string identifier)
        {
            _identifier = identifier ?? "";
        }

        /// <summary>
        /// Use this to subscribe by passing in the <see cref="IObservable{T}"/> rather than calling <see cref="IObservable{T}.Subscribe(IObserver{T})"/> 
        /// directly since this lets us properly handle disposing of resources associated with subscribing.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="additionalLogInfoFunc">A hard reference is held to this because the program might otherwise be destroying the object that would provide this info. If necessary, use <see cref="SetAdditionalLogInfoFunc(Func{string})"/> to set the internal reference to null to prevent memory leaks.</param>
        /// <param name="logLevel">This is very important because it is used by <see cref="DiagnosticsHelpers.ReportProblem(string, LogLevel, ILogger, Exception, Action{string}, bool, string, string, int)"/> to determine how it will handle the error. The default of <see cref="LogLevel.Error"/> is usually correct because it will rethrow after logging. <see cref="LogLevel.Critical"/> will cause the program to terminate immediately after it tries to log the error. The other <see cref="LogLevel"/> enumerators perform as described by <see cref="DiagnosticsHelpers.ReportProblem(string, LogLevel, ILogger, Exception, Action{string}, bool, string, string, int)"/>.</param>
        public virtual ExceptionObserver Subscribe(IObservable<Exception> provider, IScheduler observeOn, ILogger logger, Func<string> additionalLogInfoFunc = null, LogLevel logLevel = LogLevel.Error, CancellationToken unsubscribeToken = default)
        {
            if (provider != null)
            {
                _unsubscriber?.Dispose();
                _unsubscriber = provider.ObserveOn(observeOn).Subscribe(this);
                _unsubscribeToken = unsubscribeToken;
                if (_unsubscribeToken.CanBeCanceled)
                {
                    _unsubscribeToken.Register(Unsubscribe);
                }
                _logger = logger;
                _logLevel = logLevel;
                _additionalLogInfoFunc = additionalLogInfoFunc;
            }
            return this;
        }

        public void SetAdditionalLogInfoFunc(Func<string> additionalLogInfo)
        {
            _additionalLogInfoFunc = additionalLogInfo;
        }

        public virtual void Unsubscribe()
        {
            _unsubscriber?.Dispose();
        }

        public void OnCompleted()
        {
            _unsubscriber?.Dispose();
        }

        public void OnError(Exception error)
        {
            string additionalInfo = _additionalLogInfoFunc?.Invoke();
            DiagnosticsHelpers.ReportProblem($"Exception passed to {nameof(OnError)} in {nameof(ExceptionObserver)} with identifier '{_identifier}'. Additional info: '{additionalInfo}'. Details to follow.", _logLevel, _logger, error);
            if (_logLevel == LogLevel.Error || _logLevel == LogLevel.Critical)
            {
                throw error;
            }
        }

        public void OnNext(Exception value)
        {
            string additionalInfo = _additionalLogInfoFunc?.Invoke();
            DiagnosticsHelpers.ReportProblem($"Exception passed to {nameof(OnNext)} in {nameof(ExceptionObserver)} with identifier '{_identifier}'. Additional info: '{additionalInfo}'. Details to follow.", _logLevel, _logger, value);
            if (_logLevel == LogLevel.Error || _logLevel == LogLevel.Critical)
            {
                throw value;
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _unsubscriber?.Dispose();
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
