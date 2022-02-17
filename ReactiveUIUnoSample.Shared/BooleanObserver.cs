using DynamicData;

using ReactiveUI;

using ReactiveUIRoutingWithContracts;

using ReactiveUIUnoSample.ViewModels;

using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveUIUnoSample
{
    public class BooleanObserver : IObserver<bool>, IDisposable
    {
        private IDisposable _unsubscriber;
        private CancellationToken _unsubscribeToken;
        private bool disposedValue;

        public bool? LastValue { get; set; }

        /// <summary>
        /// Use this to subscribe by passing in the <see cref="IObservable{T}"/> rather than calling <see cref="IObservable{T}.Subscribe(IObserver{T})"/> 
        /// directly since this lets us properly handle disposing of resources associated with subscribing.
        /// </summary>
        /// <param name="provider"></param>
        public virtual BooleanObserver Subscribe(IObservable<bool> provider, IScheduler observeOn, CancellationToken unsubscribeToken = default)
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
            }
            return this;
        }

        public virtual void Unsubscribe()
        {
            _unsubscriber?.Dispose();
        }

        public void OnCompleted()
        {
            // Note: This is not called after every navigation but instead when the IObservable<T> itself is finished its business completely, i.e.
            //  it won't be sending any more notifications (presumably because the app is shutting down). See:
            //  https://docs.microsoft.com/en-us/dotnet/api/system.iobserver-1.oncompleted
            _unsubscriber?.Dispose();
        }

        public void OnError(Exception error)
        {
            //this.Log().
            throw error;
        }

        public void OnNext(bool value)
        {
            LastValue = value;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    _unsubscriber?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~BooleanObserver()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
