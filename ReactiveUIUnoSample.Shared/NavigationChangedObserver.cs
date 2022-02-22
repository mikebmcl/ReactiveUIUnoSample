using DynamicData;

using ReactiveUI;

using ReactiveUIRoutingWithContracts;

using ReactiveUIUnoSample.ViewModels;

using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;

using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample
{
    /// <summary>
    /// Lets us keep track of when navigation has occurred. For more info about <see cref="IObservable{T}"/>, 
    /// see https://docs.microsoft.com/en-us/dotnet/api/system.iobserver-1?view=netstandard-2.0
    /// </summary>
    public class NavigationChangedObserver : IObserver<IChangeSet<IViewModelAndContract>>, IDisposable
    {
        private MainViewModel _mainViewModel;
        private IDisposable _unsubscriber;
        private CancellationToken _unsubscribeToken;
        private bool disposedValue;

        public NavigationChangedObserver(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel;
        }
        /// <summary>
        /// Use this to subscribe by passing in the <see cref="IObservable{T}"/> rather than calling <see cref="IObservable{T}.Subscribe(IObserver{T})"/> 
        /// directly since this lets us properly handle disposing of resources associated with subscribing.
        /// </summary>
        /// <param name="provider"></param>
        public virtual NavigationChangedObserver Subscribe(IObservable<IChangeSet<IViewModelAndContract>> provider, IScheduler observeOn, CancellationToken unsubscribeToken = default)
        {
            if (provider != null)
            {
                _unsubscriber?.Dispose();
                _unsubscriber = provider.ObserveOn(observeOn/*_mainViewModel._schedulerProvider.MainThread*/).Subscribe(this);
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
        }

        public void OnError(Exception error)
        {
            //this.Log().
            throw error;
        }

        public void OnNext(IChangeSet<IViewModelAndContract> value)
        {
            // Ignore any changes the user is making; we will get a notification when this is false if the current view model actually changed.
            if (_mainViewModel.Router.IsInModifyNavigationStack)
            {
                return;
            }
            _mainViewModel.IsBackEnabled = _mainViewModel.Router.NavigationStack.Count > 1;
            var manager = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();
#if NETFX_CORE || __WASM__
            manager.AppViewBackButtonVisibility = _mainViewModel.Router.NavigationStack.Count > 1
            ? Windows.UI.Core.AppViewBackButtonVisibility.Visible
            : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
#endif
            if (_mainViewModel.Router.NavigationStack.LastOrDefault()?.ViewModel is DisplayViewModelBase displayViewModelBase)
            {
                if (_mainViewModel.CurrentHeader is null && !_mainViewModel.NavigationView.IsTest)
                {
                    _mainViewModel.CurrentHeader = new ContentControl();
                }
                if (_mainViewModel.CurrentHeader != null)
                {
                    if (_mainViewModel.CurrentHeader is ContentControl currentHeader)
                    {
                        if (displayViewModelBase.NoHeader)
                        {
                            currentHeader.Content = null;
                            return;
                        }
                        if (displayViewModelBase.HeaderContent is string headerString)
                        {
                            currentHeader.Content = new TextBlock() { Text = headerString };
                        }
                        else
                        {
                            currentHeader.Content = displayViewModelBase.HeaderContent;
                        }
                    }
                    else
                    {
                        _mainViewModel.CurrentHeader = displayViewModelBase.HeaderContent;
                    }
                }
            }
            else
            {
                if (_mainViewModel.CurrentHeader is ContentControl currentHeader)
                {
                    currentHeader.Content = null;
                }
                else
                {
                    _mainViewModel.CurrentHeader = null;
                }
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
