using ReactiveUIRoutingWithContracts;

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveUIUnoSample
{
    internal class IViewModelAndContractObserver : IObserver<IViewModelAndContract>, IDisposable
    {
        private IDisposable _unsubscriber;
        private CancellationToken _unsubscribeToken;
        private bool disposedValue;

        private readonly WeakReference<Action<IViewModelAndContract>> _onNextCallbackWeakRef = new WeakReference<Action<IViewModelAndContract>>(null);
        private readonly WeakReference<Func<Exception, bool>> _onErrorCallbackWeakRef = new WeakReference<Func<Exception, bool>>(null);
        private readonly WeakReference<Action> _onCompletedWeakRef = new WeakReference<Action>(null);

        /// <summary>
        /// Use this to subscribe by passing in the <see cref="IObservable{T}"/> rather than calling <see cref="IObservable{T}.Subscribe(IObserver{T})"/> 
        /// directly since this lets us properly handle disposing of resources associated with subscribing.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="onNextCallback">Called in <see cref="IObserver{T}.OnNext(T)"/>. Can be null. Is held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="onErrorCallback">Called in <see cref="IObserver{T}.OnError(Exception)"/>. Should return <c>true</c> if the exception should be rethrown, false if it was handled. Can be null. Is held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="onCompleted">Called in <see cref="IObserver{T}.OnCompleted"/>. Can be null. Is held using a <see cref="WeakReference{T}"/>.</param>
        public virtual IViewModelAndContractObserver Subscribe(IObservable<IViewModelAndContract> provider, Action<IViewModelAndContract> onNextCallback, Func<Exception, bool> onErrorCallback, Action onCompleted, IScheduler observeOn, CancellationToken unsubscribeToken = default)
        {
            if (provider != null)
            {
                _unsubscriber?.Dispose();
                _onNextCallbackWeakRef.SetTarget(null);
                _onErrorCallbackWeakRef.SetTarget(null);
                _unsubscriber = provider.ObserveOn(observeOn).Subscribe(this);
                _unsubscribeToken = unsubscribeToken;
                if (_unsubscribeToken.CanBeCanceled)
                {
                    _unsubscribeToken.Register(Unsubscribe);
                }
                _onNextCallbackWeakRef.SetTarget(onNextCallback);
                _onErrorCallbackWeakRef.SetTarget(onErrorCallback);
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
            if (_onCompletedWeakRef.TryGetTarget(out Action callback))
            {
                callback.Invoke();
            }
            _unsubscriber?.Dispose();
        }

        public void OnError(Exception error)
        {
            if (_onErrorCallbackWeakRef.TryGetTarget(out Func<Exception, bool> callback))
            {
                if (callback.Invoke(error))
                {
                    throw error;
                }
            }
        }

        public void OnNext(IViewModelAndContract value)
        {
            if (_onNextCallbackWeakRef.TryGetTarget(out Action<IViewModelAndContract> callback))
            {
                callback.Invoke(value);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _unsubscriber?.Dispose();
                    _onNextCallbackWeakRef.SetTarget(null);
                    _onErrorCallbackWeakRef.SetTarget(null);
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
