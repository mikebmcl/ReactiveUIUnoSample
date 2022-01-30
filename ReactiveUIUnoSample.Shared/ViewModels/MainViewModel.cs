using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;

using DynamicData;

using Microsoft.Toolkit.Uwp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Views;

using Splat;

using Uno.Extensions;

using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.ViewModels
{
    // This is the container through which all the other pages are managed. Its corresponding view contains the RoutedViewHost that is used to display our views.
    // As such, this is not really the first page of the app. It's just a wrapper that presents the views, along with any other things we want to be part of
    // all of the pages, such as navigation buttons, an app bar, etc. This sample uses a NavigationView that contains the RoutedViewHost, providing a basic
    // framework around which an app could be built.
    public class MainViewModel : ReactiveObject, IScreenWithContract//, IEnableLogger
    {
        /// <summary>
        /// Lets us keep track of when navigation has occurred. For more info about <see cref="IObservable{T}"/>, 
        /// see https://docs.microsoft.com/en-us/dotnet/api/system.iobserver-1?view=netstandard-2.0
        /// </summary>
        public class NavigationChangedObserver : IObserver<IChangeSet<IRoutableViewModel>>
        {
            private IDisposable m_unsubscriber;
            private MainViewModel m_mainViewModel;
            public NavigationChangedObserver(MainViewModel mainViewModel)
            {
                m_mainViewModel = mainViewModel;
            }
            /// <summary>
            /// Use this to subscribe by passing in the <see cref="IObservable{T}"/> rather than calling <see cref="IObservable{T}.Subscribe(IObserver{T})"/> 
            /// directly since this lets us properly handle disposing of resources associated with subscribing.
            /// </summary>
            /// <param name="provider"></param>
            public virtual void Subscribe(IObservable<IChangeSet<IRoutableViewModel>> provider)
            {
                if (provider != null)
                {
                    m_unsubscriber?.Dispose();
                    m_unsubscriber = provider.ObserveOn(RxApp.MainThreadScheduler).Subscribe(this);
                }
            }

            public virtual void Unsubscribe()
            {
                m_unsubscriber?.Dispose();
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

            public void OnNext(IChangeSet<IRoutableViewModel> value)
            {
                m_mainViewModel.IsBackEnabled = m_mainViewModel.Router.NavigationStack.Count > 1;
                var manager = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();
#if NETFX_CORE || __WASM__
                manager.AppViewBackButtonVisibility = m_mainViewModel.Router.NavigationStack.Count > 1
                ? Windows.UI.Core.AppViewBackButtonVisibility.Visible
                : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
#endif
                if (m_mainViewModel.Router.NavigationStack.Last() is DisplayViewModelBase displayViewModelBase)
                {
                    if (m_mainViewModel.CurrentHeader == null)
                    {
                        m_mainViewModel.CurrentHeader = new ContentControl();
                    }
                    if (displayViewModelBase.NoHeader)
                    {
                        m_mainViewModel.CurrentHeader.Content = null;
                        return;
                    }
                    if (displayViewModelBase.HeaderContent is string headerString)
                    {
                        m_mainViewModel.CurrentHeader.Content = new TextBlock() { Text = headerString };
                    }
                    else
                    {
                        m_mainViewModel.CurrentHeader.Content = displayViewModelBase.HeaderContent;
                    }
                }
                else
                {
                    m_mainViewModel.CurrentHeader.Content = null;
                }
            }
        }

        public MainViewModel(NavigationView navigationView)
        {
            m_navigationView = navigationView;
            m_navigationView.BackRequested += NavigationView_BackRequested;
            m_navigationView.ItemInvoked += NavigationView_ItemInvoked;
            IsBackEnabled = false;
            RoutedHostPadding = new Thickness(4);
            CurrentHeader = new ContentControl() { };

            // Register all of our views and corresponding view models and contract strings
            RegisterAllViews();

            //
            // Routing state management
            //

            // We want to know when navigation has occurred so that we can update the NavigationView's header and manage whether or not the
            // user can navigate back based on the number of items on the Router's NavigationStack.
            m_navigationChangedObserver = new NavigationChangedObserver(this);
            m_navigationChangedObserver.Subscribe(Router.NavigationChanged);

            // Manage the routing state. Use the Router.Navigate.Execute
            // command to navigate to different view models.
            //
            // Note, that the Navigate.Execute method accepts an instance
            // of a view model, this allows you to pass parameters to
            // your view models, or to reuse existing view models.
            //

            // As mentioned above, this page is just a container for presenting views. The next line of code loads the first view. Otherwise the user would just
            // see the navigation UI controls (two buttons in this sample) with no actual content. If you wanted a splash screen or some other introductory page(s)
            // you could use this to navigate to the splash screen/introductory pages (and perhaps wire things up so that the navigation UI is hidden until the
            // app is ready for use), and then use Router.NavigateAndReset.Execute(viewModelObject); when ready. NavigateAndReset clears the navigation stack in
            // addition to navigating. It can be used at any time, not just when the app starts running, but because it clears the whole stack the use cases for it
            // are limited (e.g. if your app should just swap between pages or if you want to give the user the option to go back to the "home" page without navigating
            // back through all the previous pages).

            Router.Navigate.Execute(new FirstViewModel(this));

            // The following is some special code needed to let us handle things like pressing the browser back button in WASM or the system back button in Android
            // This is based off of https://platform.uno/docs/articles/features/native-frame-nav.html with modifications due to our use of ReactiveUI rather than
            // the Frame-based navigation that is depicted in that guide.
#if NETFX_CORE || __ANDROID__ || __WASM__
            var manager = Windows.UI.Core.SystemNavigationManager.GetForCurrentView();
            // On some platforms, the back navigation request needs to be hooked up to the back navigation of the SystemNavigationManager.
            // These requests can come from:
            // - uwp: title bar back button
            // - droid: os back button/gesture
            // - wasm: browser back button
            manager.BackRequested += SystemNavigationManager_BackRequested;
#endif
#if NETFX_CORE || __WASM__
            manager.AppViewBackButtonVisibility = Router.NavigationStack.Count > 1
            ? Windows.UI.Core.AppViewBackButtonVisibility.Visible
            : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
#endif
        }

        /// <summary>
        /// All views and corresponding view model registrations should be done in this method. This method is called before any navigation takes place. Once 
        /// navigation has begun, bad things might happen if you try to register additional views since you would need to do so using 
        /// <see cref="Locator.CurrentMutable"/>, which says to register the views at startup if you are using the default locator. See
        /// https://stackoverflow.com/a/64127425 and https://www.reactiveui.net/docs/handbook/view-location/#registering-new-views .
        /// If you really need to register views after navigation has begun, it seems like it can be done provided that you write and use a
        /// custom <see cref="IViewLocator"/> instead of the default.
        /// </summary>
        private void RegisterAllViews()
        {
            // Router uses Splat.Locator to resolve views for
            // view models, so we need to register our views
            // using Locator.CurrentMutable.Register* methods.

            // You can register them individually like this:
            //Locator.CurrentMutable.Register(() => new FirstView(), typeof(IViewFor<FirstViewModel>));

            // Or you can register all of the views in this Assembly (Pages, UserControls, etc.) that implement IViewFor<T> like this:
            // Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());

            // Note: RegisterViewsForViewModels will not register any views that you have in a library since that would be a different Assembly; you would
            // need to call RegisterViewsForViewModels with a reference to the assembly for that library to also register those. Additionally,
            // RegisterViewsForViewModels has a downside in that it will register ALL of the IViewFor<T> views. If you do not want to have them all registered for
            // some reason, e.g. you are testing different views that use the same view model, you should use individual registration to register only the views
            // you want instead.

            // Note: It's worthwhile to read through this to learn some of the intricacies and issues you might come up
            // against: https://www.reactiveui.net/docs/handbook/routing/ e.g. the bit about using Interactions rather than navigation for popups.

            // Lastly, we're using Splat for dependency resolving because it doesn't require any additional setup or additional packages, but you can use a number of
            // other dependency resolvers instead if their functionality better fits your needs. For a list of them along with details about how to set them up to be
            // used with ReactiveUI, see: https://www.reactiveui.net/api/splat/imutabledependencyresolver/ 

            // We're opting to register individually like this because we're making use of our IScreenWithContract interface to allow navigating
            // to multiple views that share the same view model. Note that if you use contract names you must use them with all views you register for the
            // view model and must ensure that you set the IScreenWithContract.Contract property to a valid contract name for the view model before navigation.
            // It is safe to navigte to a view model that has no contract name even if the Contract property has a value. You do not need to worry about restoring
            // the value of Contract when navigating back to a view; the value is only used to find and create the correct view. However you should store the values
            // in your saved state data if you are saving state in the event that the user navigated away from a page by accident on a platform where 
            // ICallOnBackNavigation might not be able to be used to successfully prevent back navigation where the user accidentally navigates back (iOS and WASM).
            Locator.CurrentMutable.Register(() => new FirstView(), typeof(IViewFor<FirstViewModel>));
            Locator.CurrentMutable.Register(() => new AboutView(), typeof(IViewFor<AboutViewModel>));
            Locator.CurrentMutable.Register(() => new SecondView(), typeof(IViewFor<SecondViewModel>), SecondViewModel.SecondViewContractName);
            Locator.CurrentMutable.Register(() => new AlternateSecondView(), typeof(IViewFor<SecondViewModel>), SecondViewModel.AlternateSecondViewContractName);
        }

        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        [Reactive]
        public string Contract { get; set; }

        private readonly NavigationChangedObserver m_navigationChangedObserver;

        public void NavigationView_ItemInvoked(NavigationView view, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                //ContentFrame.Navigate(typeof(SettingsPage));
            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                NavigationViewItem item = view.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => (string)x.Content == (string)args.InvokedItem);
                var vm = GetViewModelForNavigationViewItem(item);
                if (vm != null)
                {
                    Router.Navigate.Execute(vm);
                }
            }
        }

        private DisplayViewModelBase GetViewModelForNavigationViewItem(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "about":
                    return new AboutViewModel(this);
                default:
                    return null;
            }
        }

        private NavigationView m_navigationView;

        [Reactive]
        public ContentControl CurrentHeader { get; set; }

        [Reactive]
        public bool IsBackEnabled { get; set; }

        [Reactive]
        public Thickness RoutedHostPadding { get; set; }

        private readonly AtomicBoolean m_navigationViewBackRequestedIsRunning = new AtomicBoolean();
        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (!m_navigationViewBackRequestedIsRunning.Set(true))
            {
                try
                {
                    if (Router.NavigationStack.Last() is Interfaces.ICallOnBackNavigation callOnBackNavigation)
                    {
                        if (callOnBackNavigation.CallOnBackNavigation())
                        {
                            Router.NavigateBack.Execute();
                        }
                    }
                    else
                    {
                        if (Router.NavigationStack.Count > 1)
                        {
                            Router.NavigateBack.Execute();
                        }
                    }
                }
                finally
                {
                    m_navigationViewBackRequestedIsRunning.ForceToFalse();
                }
            }
        }

        private readonly AtomicBoolean m_systemNavigationManagerBackRequestedIsRunning = new AtomicBoolean();
        private void SystemNavigationManager_BackRequested(object sender, Windows.UI.Core.BackRequestedEventArgs args)
        {
            // Note: On WASM, if the user repeatedly clicks the browser back button before this is called, backwards navigation will occur
            if (!m_systemNavigationManagerBackRequestedIsRunning.Set(true))
            {
                try
                {
                    if (Router.NavigationStack.Last() is Interfaces.ICallOnBackNavigation callOnBackNavigation)
                    {
                        if (callOnBackNavigation.CallOnBackNavigation())
                        {
                            Router.NavigateBack.Execute();
                        }
                        args.Handled = true;
                    }
                    else
                    {
                        if (Router.NavigationStack.Count > 1)
                        {
                            Router.NavigateBack.Execute();
                            args.Handled = true;
                        }
                    }
                }
                finally
                {
                    m_systemNavigationManagerBackRequestedIsRunning.ForceToFalse();
                }
            }
            else
            {
                // Already running so mark as handled to prevent navigation from occurring while waiting for navigation to occur.
                args.Handled = true;
            }
        }
    }
}
