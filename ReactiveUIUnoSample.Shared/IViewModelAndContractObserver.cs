using ReactiveUIRoutingWithContracts;

using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

namespace ReactiveUIUnoSample
{
    /// <summary>
    /// This is an implementation of <see cref="IObserver{T}"/> where T is <see cref="IViewModelAndContract"/>. Because
    /// it's observing an interface, the class name begins with an I despite this being a class rather than an interface.
    /// The <see cref="IObservable{T}"/> to be observed is passed in using the
    /// <see cref="Subscribe(IObservable{IViewModelAndContract}, IScheduler, Action{IViewModelAndContract}, Func{Exception, bool}, Action, CancellationToken)"/>
    /// method. It is subscribed to in that method and the <see cref="IDisposable"/> that results from subscribing to it
    /// is held by this class. Calling <see cref="Unsubscribe"/> will sever the connection between this and the observable
    /// by calling Dispose on the result of subscribing to it. Because this class holds the subscription's
    /// <see cref="IDisposable"/>, this class implements <see cref="IDisposable"/> and should be disposed of when you are
    /// finished with it.
    /// </summary>
    internal class IViewModelAndContractObserver : IObserver<IViewModelAndContract>, IDisposable
    {
        private IDisposable _unsubscriber;
        private CancellationToken _unsubscribeToken;
        private bool disposedValue;

        private readonly WeakReference<Action<IViewModelAndContract>> _onNextCallbackWeakRef = new WeakReference<Action<IViewModelAndContract>>(null);
        private readonly WeakReference<Func<Exception, bool>> _onErrorCallbackWeakRef = new WeakReference<Func<Exception, bool>>(null);
        private readonly WeakReference<Action> _onCompletedWeakRef = new WeakReference<Action>(null);

        /// <summary>
        /// Use this to subscribe to the observable by passing in the <see cref="IObservable{T}"/> rather than calling
        /// <see cref="IObservable{T}.Subscribe(IObserver{T})"/> directly since this can be disposed of to properly handle unsubscribing.
        /// </summary>
        /// <param name="provider">The <see cref="IObservable{T}"/> to observe.</param>
        /// <param name="observeOn">The scheduler to observe the <see cref="IObservable{T}"/> on.</param>
        /// <param name="onNextCallback">Called in <see cref="IObserver{T}.OnNext(T)"/>. Can be null. Is held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="onErrorCallback">Called in <see cref="IObserver{T}.OnError(Exception)"/>. Should return <c>true</c> if the exception should be thrown, false if it was handled. Can be null. If null, the exception will be thrown Is. held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="onCompleted">Called in <see cref="IObserver{T}.OnCompleted"/>. Can be null. Is held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="unsubscribeToken">If provided, cancellation of the token will result in <see cref="Unsubscribe"/> being called.</param>
        public virtual IViewModelAndContractObserver Subscribe(IObservable<IViewModelAndContract> provider, IScheduler observeOn, Action<IViewModelAndContract> onNextCallback, Func<Exception, bool> onErrorCallback, Action onCompleted = null, CancellationToken unsubscribeToken = default)
        {
            if (provider != null)
            {
                _unsubscriber?.Dispose();
                _onNextCallbackWeakRef.SetTarget(null);
                _onErrorCallbackWeakRef.SetTarget(null);
                _onCompletedWeakRef.SetTarget(null);
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

        /// <summary>
        /// Unsubscribes from the <see cref="IObservable{T}"/> that this is observing and sets each of the references in the
        /// <see cref="WeakReference{T}"/> objects that hold the callbacks for <see cref="OnNext(IViewModelAndContract)"/>,
        /// <see cref="OnError(Exception)"/>, and <see cref="OnCompleted"/> to <c>null</c>.
        /// </summary>
        public virtual void Unsubscribe()
        {
            _unsubscriber?.Dispose();
            _onNextCallbackWeakRef.SetTarget(null);
            _onErrorCallbackWeakRef.SetTarget(null);
            _onCompletedWeakRef.SetTarget(null);
        }

        /// <summary>
        /// Invokes the onCompleted callback passed to
        /// <see cref="Subscribe(IObservable{IViewModelAndContract}, IScheduler, Action{IViewModelAndContract}, Func{Exception, bool}, Action, CancellationToken)"/>,
        /// if any, then calls <see cref="Unsubscribe"/>.
        /// </summary>
        /// <remarks>
        /// This is not called after every navigation but instead when the IObservable<T> itself is finished its business completely,
        /// i.e. it won't be sending any more notifications (presumably because the app is shutting down). See:
        /// https://docs.microsoft.com/en-us/dotnet/api/system.iobserver-1.oncompleted
        /// </remarks>
        public void OnCompleted()
        {
            // Note: This is not called after every navigation but instead when the IObservable<T> itself is finished its business completely, i.e.
            //  it won't be sending any more notifications (presumably because the app is shutting down). See:
            //  https://docs.microsoft.com/en-us/dotnet/api/system.iobserver-1.oncompleted
            if (_onCompletedWeakRef.TryGetTarget(out Action callback))
            {
                callback.Invoke();
            }
            Unsubscribe();
        }

        /// <summary>
        /// Invokes the onError callback passed to
        /// <see cref="Subscribe(IObservable{IViewModelAndContract}, IScheduler, Action{IViewModelAndContract}, Func{Exception, bool}, Action, CancellationToken)"/>,
        /// if any. If one was provided and it returns false, the exception is assumed to have been handled and <paramref name="error"/>
        /// will not be thrown. If one was provided and it returns true or if none was provided, <paramref name="error"/> will be thrown.
        /// </summary>
        /// <param name="error">Information about the error condition that the observable experienced.</param>
        public void OnError(Exception error)
        {
            if (_onErrorCallbackWeakRef.TryGetTarget(out Func<Exception, bool> callback))
            {
                if (callback.Invoke(error))
                {
                    throw error;
                }
            }
            else
            {
                throw error;
            }
        }

        /// <summary>
        /// Invokes the onNext callback passed to
        /// <see cref="Subscribe(IObservable{IViewModelAndContract}, IScheduler, Action{IViewModelAndContract}, Func{Exception, bool}, Action, CancellationToken)"/>,
        /// if any, passing it the new data from the observable. The data might be null.
        /// </summary>
        /// <param name="value">The new data from the observable. Might be null.</param>
        public void OnNext(IViewModelAndContract value)
        {
            if (_onNextCallbackWeakRef.TryGetTarget(out Action<IViewModelAndContract> callback))
            {
                callback.Invoke(value);
            }
        }

        /// <summary>
        /// Changes the callback that is called in <see cref="OnNext(IViewModelAndContract)"/>.
        /// </summary>
        /// <param name="callback">Call with <c>null</c> to clear any exising callback without setting a new one.</param>
        public void SetOnNextCallback(Action<IViewModelAndContract> callback)
        {
            _onNextCallbackWeakRef.SetTarget(callback);
        }

        /// <summary>
        /// Changes the callback that is called in <see cref="OnError(Exception)"/>.
        /// </summary>
        /// <param name="callback">Call with <c>null</c> to clear any exising callback without setting a new one.</param>
        public void SetOnErrorCallback(Func<Exception, bool> callback)
        {
            _onErrorCallbackWeakRef.SetTarget(callback);
        }

        /// <summary>
        /// Changes the callback that is called in <see cref="OnCompleted"/>.
        /// </summary>
        /// <param name="callback">Call with <c>null</c> to clear any exising callback without setting a new one.</param>
        public void SetOnCompletedCallback(Action callback)
        {
            _onCompletedWeakRef.SetTarget(callback);
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
                    _onCompletedWeakRef.SetTarget(null);
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
