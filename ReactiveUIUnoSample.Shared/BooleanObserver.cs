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
    /// <summary>
    /// This is an implementation of <see cref="IObserver{T}"/> where T is <see cref="bool"/>. The <see cref="IObservable{T}"/>
    /// to be observed is passed in using the
    /// <see cref="Subscribe(IObservable{bool}, IScheduler, Action{bool}, Func{Exception, bool}, Action, CancellationToken)"/>
    /// method. It is subscribed to in that method and the <see cref="IDisposable"/> that results from subscribing to it
    /// is held by this class. Calling <see cref="Unsubscribe"/> will sever the connection between this and the observable
    /// by calling Dispose on the result of subscribing to it. Because this class holds the subscription's
    /// <see cref="IDisposable"/>, this class implements <see cref="IDisposable"/> and should be disposed of when you are
    /// finished with it.
    /// </summary>
    public class BooleanObserver : IObserver<bool>, IDisposable
    {
        private IDisposable _unsubscriber;
        private CancellationToken _unsubscribeToken;
        private bool disposedValue;
        private readonly WeakReference<Action<bool>> _onNextCallbackWeakRef = new WeakReference<Action<bool>>(null);
        private readonly WeakReference<Func<Exception, bool>> _onErrorCallbackWeakRef = new WeakReference<Func<Exception, bool>>(null);
        private readonly WeakReference<Action> _onCompletedWeakRef = new WeakReference<Action>(null);

        /// <summary>
        /// The last boolean value received by <see cref="OnNext(bool)"/>. If <c>null</c>, then no notification has been sent by the observable yet.
        /// Once this has a non-null value it will continue to have a non-null value, but if <see cref="Subscribe(IObservable{bool}, IScheduler, Action{bool}, Func{Exception, bool}, Action, CancellationToken)"/> is called, its value will be set to <c>null</c> until the new observable sends a notification.
        /// </summary>
        public bool? LastValue { get; protected set; }

        /// <summary>
        /// Use this to subscribe to the observable by passing in the <see cref="IObservable{T}"/>. This method handles subscribing to
        /// the observable via <see cref="IObservable{T}.Subscribe(IObserver{T})"/> and the class stores the <see cref="IDisposable"/>
        /// resulting from that, disposing of it at appropriate times such that all you need is a reference to the instance of this class
        /// and to make sure that you dispose of this class when you are done with it to properly manage the connection between the
        /// observable and observer. Calling this method with a new observable will properly dispose of the subscription to the previous
        /// disposable if there was one. Use <see cref="Unsubscribe"/> if you want to unsubscribe from the current observable without
        /// subscribing to a new one. Calling this method with a null value for <paramref name="provider"/> does not change the object.
        /// </summary>
        /// <param name="provider">The <see cref="IObservable{T}"/> to observe. If this is <c>null</c> then the state of this object does not change. Use <see cref="Unsubscribe"/> if you want to stop receiving notifications from the current observable.</param>
        /// <param name="observeOn">The scheduler to observe the <see cref="IObservable{T}"/> on.</param>
        /// <param name="onNextCallback">Called in <see cref="IObserver{T}.OnNext(T)"/>. Can be null. Is held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="onErrorCallback">Called in <see cref="IObserver{T}.OnError(Exception)"/>. Should return <c>true</c> if the exception should be thrown, false if it was handled. Can be null. If null, the exception will be thrown Is. held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="onCompleted">Called in <see cref="IObserver{T}.OnCompleted"/>. Can be null. Is held using a <see cref="WeakReference{T}"/>.</param>
        /// <param name="unsubscribeToken">If provided, cancellation of the token will result in <see cref="Unsubscribe"/> being called.</param>
        public virtual BooleanObserver Subscribe(IObservable<bool> provider, IScheduler observeOn, Action<bool> onNextCallback, Func<Exception, bool> onErrorCallback, Action onCompleted = null, CancellationToken unsubscribeToken = default)
        {
            if (provider != null)
            {
                _unsubscriber?.Dispose();
                _onNextCallbackWeakRef.SetTarget(null);
                _onErrorCallbackWeakRef.SetTarget(null);
                _onCompletedWeakRef.SetTarget(null);
                LastValue = null;
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
        /// <see cref="WeakReference{T}"/> objects that hold the callbacks for <see cref="OnNext(bool)"/>,
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
        /// <see cref="Subscribe(IObservable{bool}, IScheduler, Action{bool}, Func{Exception, bool}, Action, CancellationToken)"/>,
        /// if any, then calls <see cref="Unsubscribe"/>.
        /// </summary>
        public void OnCompleted()
        {
            if (_onCompletedWeakRef.TryGetTarget(out Action callback))
            {
                callback.Invoke();
            }
            Unsubscribe();
        }

        /// <summary>
        /// Invokes the onError callback passed to
        /// <see cref="Subscribe(IObservable{bool}, IScheduler, Action{bool}, Func{Exception, bool}, Action, CancellationToken)"/>,
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
        /// <see cref="Subscribe(IObservable{bool}, IScheduler, Action{bool}, Func{Exception, bool}, Action, CancellationToken)"/>,
        /// if any, passing it the new data from the observable. Then sets <see cref="LastValue"/> to the new value.
        /// </summary>
        /// <param name="value">The new data from the observable.</param>
        public void OnNext(bool value)
        {
            if (_onNextCallbackWeakRef.TryGetTarget(out Action<bool> callback))
            {
                callback.Invoke(value);
            }
            LastValue = value;
        }

        /// <summary>
        /// Changes the callback that is called in <see cref="OnNext(bool)"/>.
        /// </summary>
        /// <param name="callback">Call with <c>null</c> to clear any exising callback without setting a new one.</param>
        public void SetOnNextCallback(Action<bool> callback)
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
