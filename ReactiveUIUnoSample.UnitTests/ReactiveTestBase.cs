using NUnit.Framework;

using ReactiveUI;

using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.ViewModels;
using ReactiveUIRoutingWithContracts;

using Splat;

using System;
using System.Linq;
using System.Threading;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

namespace ReactiveUIUnoSample.UnitTests
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
    /// class that does not contain an implementation of <see cref="IScreen"/> or <see cref="IScreenForContracts"/> or in any way reference <see cref="Locator"/>, and 
    /// pass in null for the <see cref="IScreenForContracts"/> parameter that goes to <see cref="ViewModelBase.ViewModelBase(IScreenForContracts, ISchedulerProvider, string, bool)"/>, 
    /// which is the base class of all view models in the app. It does not do a null check for that argument to allow view models that don't need it to avoid 
    /// needing to have an instance of it to pass in. Doing that would ensure that those tests had nothing to do with navigation because if what they are testing 
    /// ever tries to access the routing stack in any way, it would result in a null reference exception. Whether or not they would run into other problems with
    /// determinacy and thread safety is something that would also need to be examined before running any such tests in parallel.
    /// </summary>
    public class ReactiveTestBase
    {
        private static readonly Lazy<INavigationViewProvider> _navigationViewProvider = new Lazy<INavigationViewProvider>(() => new TestNavigationViewProvider(), LazyThreadSafetyMode.PublicationOnly);
        private static readonly Lazy<TestSchedulerProvider> _schedulerProvider = new Lazy<TestSchedulerProvider>(() => new TestSchedulerProvider(), LazyThreadSafetyMode.PublicationOnly);
        private static readonly Lazy<IScreenForContracts> _screenWithContract = new Lazy<IScreenForContracts>(InitScreenWithContract, LazyThreadSafetyMode.PublicationOnly);
        private static IScreenForContracts InitScreenWithContract()
        {
            return new MainViewModel(_navigationViewProvider.Value, Locator.CurrentMutable, string.Empty, _schedulerProvider.Value);
        }

        /// <summary>
        /// Runs before every test. Clears the navigation stack and puts a new instance of <see cref="TemperatureConversionsViewModel"/> on it, which is
        /// the state of the navigation stack when the app has finished loading from a cold start. Existing view models are discarded so if
        /// they implement <see cref="IDisposable"/>, make sure you dispose of them before the test method you created them in exits. Classes that derive from this can have their own [Setup] method. This will run first followed by the setup method in the derived class. This is useful because it allows you to establish the Given conditions that the When methods expect without duplicating them or needing to remember to call them in every test method. Similarly, 
        /// </summary>
        [SetUp]
        public void SetUpTest()
        {
            ScreenWithContract.Router.NavigateAndReset.Execute(new TemperatureConversionsViewModel(ScreenWithContract, TestSchedulerProvider).ToViewModelAndContract());
        }

        [TearDown]
        public void TearDownTest()
        {
            // Add any code you want to run at the end of each test here. Because this should be the base class for virtually all test classes, keep in mind that this will run for every test method in every test class within this project. Think carefully before adding anything here. Each test class can have its own [TearDown] method. The tear down method, if any, in a derived class will run before the tear down, if any, in its base class. If an exception is thrown in a class's [SetUp] method then its [TearDown] will not be called.
        }

        /// <summary>
        /// Will call Dispose on each view model in the navigation stact that implements IDisposable starting from the highest index (current) 
        /// continuing in order to the first item. The arguments determine the behavior if an exception is thrown in the process. An exception,
        /// if any, will be transformed into a string by <see cref="DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(Exception, string, string)"/>
        /// and the resulting string will written to the test output.
        /// </summary>
        /// <param name="rethrowException">
        /// <code>true</code> If an exception occurs, it will be logged to the test output then rethrown.
        /// <code>false</code> If an exception occurs, it will be logged to the test output but will not be rethrown. Wbat happens next
        /// depends on the value of <paramref name="continueOnException"/>.
        /// </param>
        /// <param name="continueOnException">
        /// <code>true</code> If an exception occurs and <paramref name="rethrowException"/> is <c>false</c>, processing and disposal of
        /// remaining items on the navigation stack will continue.
        /// <code>false</code> If an exception occurs, processing and disposal of items on the navigation stack will cease. If
        /// <paramref name="rethrowException"/> is <c>true</c> then processing and disposal will automatically cease as a result of
        /// the exception being rethrown
        /// </param>
        protected void CallDisposeOnEntireNavigationStackHighestToLowestIndex(bool rethrowException = false, bool continueOnException = true)
        {
            var navigationStack = ScreenWithContract?.Router?.NavigationStack;
            if (navigationStack is null)
            {
                TestContext.WriteLine($"Unexpected null navigation stack in {nameof(CallDisposeOnEntireNavigationStackHighestToLowestIndex)}.");
                return;
            }
            for (int idx = navigationStack.Count - 1; idx >= 0; idx--)
            {
                var item = navigationStack[idx];
                try
                {
                    if (item.ViewModel is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception while disposing of {item.ViewModel.GetType().FullName} at navigation stack index {idx}."));
                    if (rethrowException)
                    {
                        throw;
                    }
                    if (!continueOnException)
                    {
                        break;
                    }
                }
            }
        }

        protected static INavigationViewProvider TestNavigationViewProvider
        {
            get => _navigationViewProvider.Value;
        }

        protected static TestSchedulerProvider TestSchedulerProvider
        {
            get => _schedulerProvider.Value;
        }

        /// <summary>
        /// An instance of <see cref="IScreenForContracts"/> that is either the view model of the main view (i.e. the view that contains the
        /// <see cref="ReactiveUI.Uno.RoutedViewHost"/>) or some type that mocks it by implementing that interface. Note that <see cref="SetUpTest"/>
        /// is called before each test, restoring the state of this object's navigation stack to the same state every time. Also, the 
        /// <see cref="ICallOnBackNavigation"/> interface is ignored entirely because implementations of it are likely to contain UI code. If you have 
        /// a view model that implements it without invoking any UI code and you want to test it, test the effects by directly invoking the relevant methods.
        /// You can test the effects of it regardless of whether it invokes UI code by testing it directly in a running instance of the app via a test
        /// in a class that derives from <see cref="AppTestBase"/>.
        /// </summary>
        protected static IScreenForContracts ScreenWithContract
        {
            get => _screenWithContract.Value;
        }

        /// <summary>
        /// This returns the most recently navigated to view model on the <see cref="ScreenWithContract"/>'s <see cref="IScreenForContracts.Router"/> navigation stack.
        /// </summary>
        /// <returns></returns>
        protected IViewModelAndContract GetCurrentViewModel()
        {
            return ScreenWithContract.Router.NavigationStack[ScreenWithContract.Router.NavigationStack.Count - 1];
        }
    }
}
