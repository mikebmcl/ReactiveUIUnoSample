using System.Reactive;
using System.Reflection;

using ReactiveUI.UwpRouting.Views;

using Splat;

namespace ReactiveUI.UwpRouting.ViewModels
{
    // This is the container through which all the other pages are managed. Its corresponding view contains the RoutedViewHost that is used to display our views.
    // As such, this is not really the first page of the app. It's just a wrapper that presents the views, along with any other things we want to be part of
    // all of the pages, such as navigation buttons, an app bar, etc. This sample just contains the RoutedViewHost along with two buttons for navigation and
    // a diagnostic counter that tells you how many views are currently on the navigation stack. You would probably choose to use something such a NavigationView
    // that would contain the RoutedViewHost for your own app, though you can use something as basic as this two button approach if it fills all your needs.
    public class MainViewModel : ReactiveObject, IScreen
    {
        // The Router associated with this Screen.
        // Required by the IScreen interface.
        public RoutingState Router { get; } = new RoutingState();

        // The command that navigates a user to first view model.
        public ReactiveCommand<Unit, IRoutableViewModel> GoNext { get; }

        // The command that navigates a user back.
        public ReactiveCommand<Unit, IRoutableViewModel> GoBack => Router.NavigateBack;

        public MainViewModel()
        {
            //
            // View resolution
            //

            // Router uses Splat.Locator to resolve views for
            // view models, so we need to register our views
            // using Locator.CurrentMutable.Register* methods.

            // You can register them individually like this:
            Locator.CurrentMutable.Register(() => new FirstView(), typeof(IViewFor<FirstViewModel>));

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

            //
            // Routing state management
            //

            // Manage the routing state. Use the Router.Navigate.Execute
            // command to navigate to different view models.
            //
            // Note, that the Navigate.Execute method accepts an instance
            // of a view model, this allows you to pass parameters to
            // your view models, or to reuse existing view models.
            //
            GoNext = ReactiveCommand.CreateFromObservable(
                () => Router.Navigate.Execute(new SecondViewModel(this))
            );

            // As mentioned above, this page is just a container for presenting views. The next line of code loads the first view. Otherwise the user would just
            // see the navigation UI controls (two buttons in this sample) with no actual content. If you wanted a splash screen or some other introductory page(s)
            // you could use this to navigate to the splash screen/introductory pages (and perhaps wire things up so that the navigation UI is hidden until the
            // app is ready for use), and then use Router.NavigateAndReset.Execute(viewModelObject); when ready. NavigateAndReset clears the navigation stack in
            // addition to navigating. It can be used at any time, not just when the app starts running, but because it clears the whole stack the use cases for it
            // are limited (e.g. if your app should just swap between pages or if you want to give the user the option to go back to the "home" page without navigating
            // back through all the previous pages).
            Router.Navigate.Execute(new FirstViewModel(this));
        }
    }
}
