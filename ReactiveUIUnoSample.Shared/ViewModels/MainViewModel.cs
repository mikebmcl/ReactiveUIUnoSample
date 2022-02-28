using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;

using DynamicData;

using Microsoft.Toolkit.Uwp;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIRoutingWithContracts;

using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.ViewModels.UnitConversions;
using ReactiveUIUnoSample.Views;

using Uno.Extensions;

using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using IMutableDependencyResolver = Splat.IMutableDependencyResolver;

namespace ReactiveUIUnoSample.ViewModels
{
    // This is the container through which all the other pages are managed. Its corresponding view contains the RoutedViewHost that is used to display our views.
    // As such, this is not really the first page of the app. It's just a wrapper that presents the views, along with any other things we want to be part of
    // all of the pages, such as navigation buttons, an app bar, etc. This sample uses a NavigationView that contains the RoutedViewHost, providing a basic
    // framework around which an app could be built.
    public class MainViewModel : ReactiveObject, IScreenForContracts//, IEnableLogger
    {
        public MainViewModel(INavigationViewProvider navigationView, IMutableDependencyResolver mutableDependencyResolver, object initialHeader, ISchedulerProvider schedulerProvider)
        {
            m_schedulerProvider = schedulerProvider;
            m_navigationView = navigationView;
            m_navigationView.SubscribeBackRequested(NavigationView_BackRequested);
            m_navigationView.SubscribeItemInvoked(NavigationView_ItemInvoked);
            IsBackEnabled = false;
            RoutedHostPadding = new Thickness(4);
            CurrentHeader = initialHeader;
            Router = new RoutingWithContractsState(schedulerProvider.MainThread);
            // Register all of our views and corresponding view models and contract strings
            RegisterAllViews(mutableDependencyResolver);

            //
            // Routing state management
            //

            // We want to know when navigation has occurred so that we can update the NavigationView's header and manage whether or not the
            // user can navigate back based on the number of items on the Router's NavigationStack.
            m_navigationChangedObserver = new NavigationChangedObserver(this).Subscribe(Router.NavigationChanged, schedulerProvider.MainThread);
            _navigateBackExceptionObserver = new ExceptionObserver("NavigateBack").Subscribe(Router.NavigateBack.ThrownExceptions, schedulerProvider.MainThread, this.Log(), GetCurrentContract);
            _navigateExceptionObserver = new ExceptionObserver("Navigate").Subscribe(Router.Navigate.ThrownExceptions, schedulerProvider.MainThread, this.Log(), GetCurrentContract);
            _navigateAndResetExceptionObserver = new ExceptionObserver("NavigateAndReset").Subscribe(Router.NavigateAndReset.ThrownExceptions, schedulerProvider.MainThread, this.Log(), GetCurrentContract);

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
            Router.Navigate.Execute(new TemperatureConversionsViewModel(this, m_schedulerProvider).ToViewModelAndContract());

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
        private void RegisterAllViews(IMutableDependencyResolver mutableDependencyResolver)
        {
            // Router uses Splat.Locator to resolve views for
            // view models, so we need to register our views
            // using Locator.CurrentMutable.Register* methods.

            // You can register them individually like this:
            //Locator.CurrentMutable.Register(() => new FirstView(), typeof(IViewFor<FirstViewModel>));

            // Or you can register all of the views in this Assembly (Pages, UserControls, etc.) that implement IViewFor<T> like this:
            //Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());

            // Note: RegisterViewsForViewModels will not register any views that you have in a library since that would be a different Assembly; you would
            // need to call RegisterViewsForViewModels with a reference to the assembly for that library to also register those. Additionally,
            // RegisterViewsForViewModels has a downside in that it will register ALL of the IViewFor<T> views. If you do not want to have them all registered for
            // some reason, e.g. you are testing different views that use the same view model, you should use individual registration to register only the views
            // you want instead.

            // Note: It's worthwhile to read through this to learn some of the intricacies and issues you might come up
            // against: https://www.reactiveui.net/docs/handbook/routing/ e.g. the bit about using Interactions rather than navigation for popups.

            // Lastly, we're using Splat for view resolving because it doesn't require any additional setup or additional packages, but you can use a number of
            // other dependency resolvers instead if their functionality better fits your needs. For a list of them along with details about how to set them up to be
            // used with ReactiveUI, see: https://www.reactiveui.net/api/splat/imutabledependencyresolver/ 

            // We're opting to register individually like this because we're making use of our IScreenForContracts interface to allow navigating
            // to multiple views that share the same view model.
            // Note that if you use contract names you should use them with all views you register for that view model. You can have one of those views be registered
            // with no contract (null and empty count as no contract, but whitespace is considered a distinct string rather than no contract), but that is not
            // recommended. Locator will let you register multiple views for the same view model with the same contract string (or no contract string). Whichever
            // was registered last is the one that will be resolved. So if you make a mistake during registering, you could end up with unexpected behavior.
            //
            // If Locator can't find a view associated with a specific contract for the view model it's trying to resolve then it will try to fall back to a view
            // that was registered with no contract. If it finds one it will go to that. If there is no view for that view model that was registered without a
            // contract, it will throw an exception. So by registering all of them with contracts, you will avoid the unintended side effect of having it "fallback"
            // to a view that isn't the one you intended. It's also fairly easy to write a UI test that verifies that navigation throws when trying to navigate to a
            // view model that should throw if no contract string or an invalid one is set compared to writing tests to try to check to see if you actually got the
            // view you wanted (e.g. by checking to see if certain elements are there that shouldn't be there or vice-versa).
            //
            // You can decorate your view classes (in their code behind file) with a ReactiveUI.ViewContractAttribute attribute to specify a contract string. If you
            // do that for all of the views that share a view model, you can use the RegisterViewsForViewModels(...) method of registration above and all of the
            // contracts will be properly registered. However, that attribute will have no effect if you register views manually with Register(...) as found below.
            // So even if you have that attribute, you still need to specify the contract as an argument to Register if you switch to using individual registration.
            // If you forget to add that attribute to a view then it will unintentionally become the fallback view. If you accidentally register multiple views using
            // the same contract string, then whichever one came last when Locator was iterating through all of the views will be the one that is associated with that
            // contract at runtime. This is also true for manual registration, though in that case you know the order of registration so it might be a bit easier to
            // figure out why you are getting a certain view by checking to see if that view is the last one that was registered for that view model.
            //
            // If you have a view model that is only associated with one view then it doesn't need a contract string and trying to navigate to it when the contract
            // string has a value will succeed (because of the fallback lookup described above). But you should strongly consider having all of your view-view models
            // use contract strings, even those that only have one view. It eases testing and will make it easier in the future if you decide you want a second view
            // for that view model. You will already have a contract string for the existing view with all of the code that navigates to it setup to use that so you
            // won't need to go through and add a contract string everywhere that navigates to it or otherwise tries to resolve a view for it. Also, adding a contract
            // string is a breaking change because you no longer have the fallback behavior and need to change all of the code that used to navigate without a contract
            // to navigate with a contract. If the code is used outside of your project in some way, either now or in the future, adding a contract string would be
            // significantly more problematic because everyone else would also need to make those changes and write tests, etc.
            //
            // However, you MUST use a view without a contract for the view model that is navigated to first (in the constructor of this class). If you try to use a
            // contract, you will get an exception saying that it could not find a view for the view model (despite registering everything correctly). Other than
            // that, you should strongly consider always using contracts for your views.
            //
            // You do not need to worry about restoring the value of Contract when navigating back to a view; the value is only used to find and create the correct
            // view when navigating fprwards. However you should store the contract strings in your saved state data if you are saving state. That way you can return
            // the user to the page they were on in the event that they navigated away from a page by accident on a platform where ICallOnBackNavigation either cannot
            // or might not be able to be used to confirm that the user meant to leave a page where they haven't finished doing something (creating a new account, going through the checkout process if they are buying something, completing a test they have started, and other activities that are only partially completed). You cannot prevent back navigation on iOS and if the user presses the browser back button multiple times in quick succession on WASM, they will navigate back (and possibly completely out of the app) before the ICallOnBackNavigation callback is called.
            mutableDependencyResolver.Register(() => new FirstView(), typeof(IViewFor<FirstViewModel>));
            mutableDependencyResolver.Register(() => new AboutView(), typeof(IViewFor<AboutViewModel>));
            mutableDependencyResolver.Register(() => new SILOpenFontLicense1_1View(), typeof(IViewFor<SILOpenFontLicense1_1ViewModel>));
            mutableDependencyResolver.Register(() => new SecondView(), typeof(IViewFor<SecondViewModel>), SecondViewModel.SecondViewContractName);
            mutableDependencyResolver.Register(() => new AlternateSecondView(), typeof(IViewFor<SecondViewModel>), SecondViewModel.AlternateSecondViewContractName);
            mutableDependencyResolver.Register(() => new TemperatureConversionsMainView(), typeof(IViewFor<ViewModels.UnitConversions.TemperatureConversionsViewModel>));
            mutableDependencyResolver.Register(() => new Views.Testing.OneLineTestView(), typeof(IViewFor<ViewModels.Testing.OneLineTestViewModel>));
            mutableDependencyResolver.Register(() => new Views.Testing.OneLineTestResultsView(), typeof(IViewFor<ViewModels.Testing.OneLineTestResultsViewModel>));
        }

        private ISchedulerProvider m_schedulerProvider;

        // The Router associated with this Screen.
        // Required by the IScreen interface.
        //public RoutingState Router { get; }
        public RoutingWithContractsState Router { get; }

        public string GetCurrentContract()
        {
            return Router?.NavigationStack.LastOrDefault().Contract;
        }

        //[Reactive]
        //public string Contract { get; set; }

        private readonly NavigationChangedObserver m_navigationChangedObserver;
        private readonly ExceptionObserver _navigateBackExceptionObserver;
        private readonly ExceptionObserver _navigateExceptionObserver;
        private readonly ExceptionObserver _navigateAndResetExceptionObserver;

        public void NavigationView_ItemInvoked(NavigationView view, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                //Router.Navigate.Execute(new SettingsViewModel(this, m_schedulerProvider));
            }
            else
            {
                // find NavigationViewItem with Content that equals InvokedItem
                NavigationViewItem item = view.MenuItems.OfType<NavigationViewItem>().FirstOrDefault(x => (string)x.Content == (string)args.InvokedItem);
                var vm = GetViewModelForNavigationViewItem(item);
                if (vm.ViewModel != null)
                {
                    Router.Navigate.Execute(vm);
                }
            }
        }

        private IViewModelAndContract GetViewModelForNavigationViewItem(NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "about":
                    return new AboutViewModel(this, m_schedulerProvider).ToViewModelAndContract();
                default:
                    return default;
            }
        }

        private INavigationViewProvider m_navigationView;

        public INavigationViewProvider NavigationView => m_navigationView;

        [Reactive]
        public object CurrentHeader { get; set; }

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
                    if (Router.NavigationStack.LastOrDefault()?.ViewModel is Interfaces.ICallOnBackNavigation callOnBackNavigation)
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
                    if (Router.NavigationStack.LastOrDefault()?.ViewModel is Interfaces.ICallOnBackNavigation callOnBackNavigation)
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

        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Router?.Dispose();
                }
                _disposedValue = true;
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
