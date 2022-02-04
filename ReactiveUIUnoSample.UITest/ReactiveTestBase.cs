using NUnit.Framework;

using ReactiveUI;

using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.ViewModels;

using Splat;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ReactiveUIUnoSample.UITest
{
    /// <summary>
    /// Use this as the base class for all tests of view models and anything else not dependant of the UI actually existing. This
    /// does not create an instance of the app so nothing related to the UI actually exists. Therefore you can only test any portions of view
    /// models that will not attempt to do anything that relates to the UI directly, e.g. instantiating controls. Anything that relates to the UI
    /// should be tested against the view using a test class that derives from <see cref="AppTestBase"/>.
    /// 
    /// NOTE: It is not safe to run any tests that derive from this in parallel because <see cref="ScreenWithContract"/> is a static instance to avoid
    /// multiple registrations of views and their view models with <see cref="Locator"/>, which maintains a static state. The app uses it for view/view model 
    /// resolution so it is being used here. This is because the app uses <see cref="RoutingState"/> for navigation, which uses <see cref="Locator"/>. To get 
    /// around this would require rewriting the app to use a custom system of view resolution and navigation. For determinacy purposes, the <see cref="IScreen.Router"/>
    /// that is accessible through <see cref="ScreenWithContract"/> is set to being in the state it is in when the app has just finished loading from a cold 
    /// start at the beginning of each test. For these reasons, tests must not be run in parallel. It is safe to run tests that derive from <see cref="AppTestBase"/> at the
    /// same time as tests that derive from this because those are directed at an instance of the app that is running in a separate process with its own instances of
    /// the types mentioned earlier.
    /// 
    /// If you want to run view model tests that in no way affect the routing stack nor examine its state in parallel with each other, start by creating a new base 
    /// class that does not contain an implementation of <see cref="IScreen"/> or <see cref="IScreenWithContract"/> or in any way reference <see cref="Locator"/>, and 
    /// pass in null for the <see cref="IScreenWithContract"/> parameter that goes to <see cref="ViewModelBase.ViewModelBase(IScreenWithContract, ISchedulerProvider, string, bool)"/>, 
    /// which is the base class of all view models in the app. It does not do a null check for that argument to allow view models that don't need it to avoid 
    /// needing to have an instance of it to pass in. Doing that would ensure that those tests had nothing to do with navigation because if what they are testing 
    /// ever tries to access the routing stack in any way, it would result in a null reference exception. Whether or not they would run into other problems with
    /// determinacy and thread safety is something that would also need to be examined before running any such tests in parallel.
    /// </summary>
    public class ReactiveTestBase
    {
        private static Lazy<INavigationViewProvider> m_navigationViewProvider = new Lazy<INavigationViewProvider>(() => new TestNavigationViewProvider(), LazyThreadSafetyMode.PublicationOnly);
        private static Lazy<ISchedulerProvider> m_schedulerProvider = new Lazy<ISchedulerProvider>(() => new TestSchedulerProvider(), LazyThreadSafetyMode.PublicationOnly);
        private static Lazy<IScreenWithContract> m_screenWithContract = new Lazy<IScreenWithContract>(InitScreenWithContract, LazyThreadSafetyMode.PublicationOnly);
        private static IScreenWithContract InitScreenWithContract()
        {
            return new MainViewModel(m_navigationViewProvider.Value, Locator.CurrentMutable, string.Empty, m_schedulerProvider.Value);
        }

        /// <summary>
        /// Runs before every test. Clears the navigation stack and puts a new instance of <see cref="FirstViewModel"/> on it, which is
        /// the state of the navigation stack when the app has finished loading from a cold start. Existing view models are discarded so if
        /// they implement <see cref="IDisposable"/>, 
        /// </summary>
        [SetUp]
        public void SetUpTest()
        {
            ScreenWithContract.Router.NavigateAndReset.Execute(new FirstViewModel(ScreenWithContract, TestSchedulerProvider));
        }

        protected static INavigationViewProvider TestNavigationViewProvider
        {
            get => m_navigationViewProvider.Value;
        }

        protected static ISchedulerProvider TestSchedulerProvider
        {
            get => m_schedulerProvider.Value;
        }

        /// <summary>
        /// An instance of <see cref="IScreenWithContract"/> that is either the view model of the main view (i.e. the view that contains the
        /// <see cref="ReactiveUI.Uno.RoutedViewHost"/>) or some type that mocks it by implementing that interface. Note that <see cref="SetUpTest"/>
        /// is called before each test, restoring the state of this object's navigation stack to the same state every time. Also, the 
        /// <see cref="ICallOnBackNavigation"/> interface is ignored entirely because implementations of it are likely to contain UI code. If you have 
        /// a view model that implements it without invoking any UI code and you want to test it, test the effects by directly invoking the relevant methods.
        /// You can test the effects of it regardless of whether it invokes UI code by testing it directly in a running instance of the app via a test
        /// in a class that derives from <see cref="AppTestBase"/>.
        /// </summary>
        protected static IScreenWithContract ScreenWithContract
        {
            get => m_screenWithContract.Value;
        }

        /// <summary>
        /// This returns the most recently navigated to view model on the <see cref="ScreenWithContract"/>'s <see cref="IScreen.Router"/> navigation stack.
        /// </summary>
        /// <returns></returns>
        protected IRoutableViewModel GetCurrentViewModel()
        {
            return ScreenWithContract.Router.NavigationStack[ScreenWithContract.Router.NavigationStack.Count - 1];
        }
    }
}
