using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;

using DynamicData;

using ReactiveUI.Fody.Helpers;
using ReactiveUI.UwpRouting.Views;

using Splat;

using Uno.Extensions;

using Windows.UI.Xaml.Controls;

namespace ReactiveUI.UwpRouting.ViewModels
{
    // This is the container through which all the other pages are managed. Its corresponding view contains the RoutedViewHost that is used to display our views.
    // As such, this is not really the first page of the app. It's just a wrapper that presents the views, along with any other things we want to be part of
    // all of the pages, such as navigation buttons, an app bar, etc. This sample uses a NavigationView that contains the RoutedViewHost, providing a basic
    // framework around which an app could be built.
    public class MainViewModel : ReactiveObject, IScreen//, IEnableLogger
    {
        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        [Reactive]
        public ContentControl CurrentHeader { get; set; }
        [Reactive]
        public bool IsBackEnabled { get; set; }

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
                    m_unsubscriber = provider.Subscribe(this);
                }
            }

            public virtual void Unsubscribe()
            {
                m_unsubscriber?.Dispose();
            }

            public void OnCompleted()
            {
                //throw new NotImplementedException();
            }

            public void OnError(Exception error)
            {
                //this.Log().
                //throw new NotImplementedException();
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
                if (m_mainViewModel.Router.CurrentViewModel is DisplayViewModelBase displayViewModelBase)
                {
                    if (displayViewModelBase.NoHeader)
                    {
                        m_mainViewModel.CurrentHeader = null;
                        return;
                    }
                    if (displayViewModelBase.HeaderContent is string headerString)
                    {
                        m_mainViewModel.CurrentHeader = new ContentControl() { Content = new TextBlock() { Text = headerString } };
                    }
                    else
                    {
                        m_mainViewModel.CurrentHeader = new ContentControl() { Content = displayViewModelBase.HeaderContent };
                    }
                }
                else
                {
                    m_mainViewModel.CurrentHeader = null;
                }
            }
        }

        private NavigationChangedObserver m_NavigationChangedObserver;

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
        public MainViewModel(NavigationView navigationView)
        {
            m_navigationView = navigationView;
            m_navigationView.BackRequested += NavigationView_BackRequested;
            m_navigationView.ItemInvoked += NavigationView_ItemInvoked;

#if __ANDROID__ || __IOS__
            // For small form-factor platforms we want the most compact pane display mode
            m_navigationView.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
#endif
            //
            // View resolution
            //
            // Router uses Splat.Locator to resolve views for
            // view models, so we need to register our views
            // using Locator.CurrentMutable.Register* methods.

            // You can register them individually like this:
            //Locator.CurrentMutable.Register(() => new FirstView(), typeof(IViewFor<FirstViewModel>));

            // Or you can register all of the views in this Assembly (Pages, UserControls, etc.) that implement IViewFor<T> like this:
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());

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
            //RoutingState

            //
            // Routing state management
            //

            // We want to know when navigation has occurred so that we can update the NavigationView's header and manage whether or not the
            // user can navigate back based on the number of items on the Router's NavigationStack.
            m_NavigationChangedObserver = new NavigationChangedObserver(this);
            m_NavigationChangedObserver.Subscribe(Router.NavigationChanged);

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
            manager.BackRequested += (s, e) =>
            {
                if (Router.NavigationStack.Count > 1)
                {
                    Router.NavigateBack.Execute();
                    e.Handled = true;
                }
            };
#endif
#if NETFX_CORE || __WASM__
            manager.AppViewBackButtonVisibility = Router.NavigationStack.Count > 1
            ? Windows.UI.Core.AppViewBackButtonVisibility.Visible
            : Windows.UI.Core.AppViewBackButtonVisibility.Collapsed;
#endif
        }

        private void NavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (Router.NavigationStack.Count > 1)
            {
                Router.NavigateBack.Execute();
            }
        }
    }
}
