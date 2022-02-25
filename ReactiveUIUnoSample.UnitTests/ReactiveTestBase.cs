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
using Microsoft.Reactive.Testing;
using System.Reactive.Linq;

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
            // Note: This works. For some reason if the scheduler is running then calling Sleep on it doesn't cause it to actually advance?
            TestSchedulerProvider.CurrentThread.Stop();
            TestSchedulerProvider.MainThread.Stop();
            TestSchedulerProvider.TaskPool.Stop();
            if (_screenWithContract.Value.Router.IsNavigating)
            {
                try
                {
                    WaitForNavigation();
                }
                catch (Exception ex)
                {
                    // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. We can bury exceptions here because this is just clean up code.
                    TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(TearDownTest)}. Details to follow."));
                    if (_screenWithContract.Value.Router.IsNavigating)
                    {
                        throw;
                    }
                }
            }
            ScreenWithContract.Router.NavigateAndReset.Execute(new TemperatureConversionsViewModel(ScreenWithContract, TestSchedulerProvider).ToViewModelAndContract());
            try
            {
                WaitForNavigation();
            }
            catch (Exception ex)
            {
                // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. We can bury exceptions here because this is just clean up code.
                TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(TearDownTest)}. Details to follow."));
                if (_screenWithContract.Value.Router.IsNavigating)
                {
                    throw;
                }
            }
        }

        [TearDown]
        public void TearDownTest()
        {
            // Note: This works. For some reason if the scheduler is running then calling Sleep on it doesn't cause it to actually advance?
            TestSchedulerProvider.CurrentThread.Stop();
            TestSchedulerProvider.MainThread.Stop();
            TestSchedulerProvider.TaskPool.Stop();
            // Add any code you want to run at the end of each test here. Because this should be the base class for virtually all test classes, keep in mind that this will run for every test method in every test class within this project. Think carefully before adding anything here. Each test class can have its own [TearDown] method. The tear down method, if any, in a derived class will run before the tear down, if any, in its base class. If an exception is thrown in a class's [SetUp] method then its [TearDown] will not be called.
            try
            {
                AdvanceScheduler();
            }
            catch (Exception ex)
            {
                // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. We can bury exceptions here because this is just clean up code.
                TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(TearDownTest)}. Details to follow."));
            }
            try
            {
                AdvanceScheduler(schedulerToWaitOn: TestSchedulerProvider.CurrentThread);
            }
            catch (Exception ex)
            {
                // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. We can bury exceptions here because this is just clean up code.
                TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(TearDownTest)}. Details to follow."));
            }
            try
            {
                AdvanceScheduler(schedulerToWaitOn: TestSchedulerProvider.TaskPool);
            }
            catch (Exception ex)
            {
                // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. We can bury exceptions here because this is just clean up code.
                TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(TearDownTest)}. Details to follow."));
            }
            try
            {
                WaitForNavigation();
            }
            catch (Exception ex)
            {
                // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. We can bury exceptions here because this is just clean up code.
                TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(TearDownTest)}. Details to follow."));
            }
        }

        /// <summary>
        /// Call with an <see cref="Action"/> that performs navigation. If navigation is already occurring and has not completed when this is called, then <see cref="TimeoutException"/> will always be thrown no matter what value you pass for <paramref name="maxTimeToWait"/> because your navigation will be ignored. This is by design. <see cref="RoutingWithContractsState"/> intentionally ignores other attempts to navigate while a navigation is running.
        /// </summary>
        /// <param name="navigate">The delegate that performs navigation</param>
        /// <param name="maxTimeToWait">The maximum amount of time to wait for in units passed to either <see cref="System.Reactive.Concurrency.VirtualTimeSchedulerBase{TAbsolute, TRelative}.AdvanceBy(TRelative)"/> or <see cref="System.Reactive.Concurrency.VirtualTimeSchedulerBase{TAbsolute, TRelative}.Sleep(TRelative)"/> depemding on whether or not the scheduler is running, as determined by checking <see cref="System.Reactive.Concurrency.VirtualTimeSchedulerBase{TAbsolute, TRelative}.IsEnabled"/>.</param>
        /// <param name="schedulerToWaitOn">The <see cref="TestScheduler"/> to use to observe whether or not navigation has occurred. Normally this will be <see cref="TestSchedulerProvider.MainThread"/>, which is what will be used if this is null.</param>
        /// <exception cref="TimeoutException">Thrown if navigation still has not occurred after waiting <paramref name="maxTimeToWait"/> units of time. If navigation is already occurring and has not completed when this is called, then this exception will always be thrown no matter what value you pass for <paramref name="maxTimeToWait"/> because your navigation will be ignored. This is by design. If the scheduler has a lot of work going on you should increase that value and run the test again. If the scheduler should not be particularly busy or you advanced the scheduler by a much larger amount and still get this exception, then navigation is either not occurring or is not being signaled as complete. This could indicate a problem in <see cref="RoutingWithContractsState"/>. Set a breakpoint in the approriate navigation method (defined in <see cref="RoutingWithContractsState.SetupRx"/> to verify that navigation is occurring.</exception>
        public static void WaitForNavigation(int maxTimeToWait = 100, TestScheduler schedulerToWaitOn = null)
        {
            TestScheduler scheduler = schedulerToWaitOn ?? TestSchedulerProvider.MainThread;
            // Note: scheduler.Sleep(...) just skips time, it doesn't cause events to be processed.
            bool schedulerWasRunning = scheduler.IsEnabled;
            scheduler.Stop();
            for (int i = 0; i < maxTimeToWait; i++)
            {
                try
                {
                    scheduler.AdvanceBy(1);
                    if (!ScreenWithContract.Router.IsNavigating)
                    {
                        if (schedulerWasRunning)
                        {
                            scheduler.Start();
                        }
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. However, we can't bury the exception here because then expected exceptions in the tests never surface and tests fails because the exception they expected and should've received was eaten. Then again this might not be an actual problem since it's only causing failure where failure was expected and doesn't seem to be causing further problems for now. I wonder what it would do in the event of a double throw though...
                    TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(WaitForNavigation)} at iteration {i + 1}. Details to follow."));
                    throw;
                }
            }
            throw new TimeoutException($"In {nameof(WaitForNavigation)}, navigation still had not occurred after {maxTimeToWait} passed.");
        }

        /// <summary>
        /// Use this to advance a scheduler. Reactive things do not happen instantly so they need a bit of time to process. Additionally,
        /// you can schedule things to run at specific times on the various schedulers for testing purposes. This method is a convenience
        /// method that handles checking to see if the scheduler is already running and then calling Sleep or AdvanceBy (for running or
        /// not running); whichever is appropriate.
        /// </summary>
        /// <param name="time">The amount of time to advance the scheduler by in units. If you specify 0 or <c>default</c>, you will get the default value of 100. The default should be plenty, however depending on how many observables and observers were affected and how many times, it may not be enough. You may want to design the test so that each time you change something, you call this with a small value in a loop that checks to see if the observable effects of the change have occurred and if they do not after a reasonable number of loops, throw a meaningful exception that will let you examine the problem at the point it is occurring rather than having to piece it together from a single exception at the end of the test.</param>
        /// <param name="schedulerToWaitOn">The <see cref="TestScheduler"/> to advance. The default value of <c>null</c> will advance <see cref="TestSchedulerProvider.MainThread"/>.</param>
        public static void AdvanceScheduler(int time = 100, TestScheduler schedulerToWaitOn = null)
        {
            if (time is default(int))
            {
                time = 100;
            }
            TestScheduler scheduler = schedulerToWaitOn ?? TestSchedulerProvider.MainThread;
            bool schedulerWasRunning = scheduler.IsEnabled;
            scheduler.Stop();
            //if (scheduler.IsEnabled)
            //{
            //    scheduler.Sleep(time);
            //}
            //else
            //{
            try
            {
                scheduler.AdvanceBy(time);
            }
            catch (Exception ex)
            {
                // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. However, we can't bury the exception here because then expected exceptions in the tests never surface and tests fails because the exception they expected and should've received was eaten.
                TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(AdvanceScheduler)}. Details to follow."));
                throw;
            }
            if (schedulerWasRunning)
            {
                scheduler.Start();
            }
            //}
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
        protected IRoutableViewModelForContracts GetCurrentViewModel()
        {
            return ScreenWithContract.Router.NavigationStack[ScreenWithContract.Router.NavigationStack.Count - 1]?.ViewModel;
        }
    }
}
