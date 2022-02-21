using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using FluentAssertions;
using FluentAssertions.Execution;

using Microsoft.Reactive.Testing;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using ReactiveUIRoutingWithContracts;

using ReactiveUIUnoSample;
using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.ViewModels;
using ReactiveUIUnoSample.ViewModels.Testing;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

using Splat;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class Given_RoutingWithContractsState
    {
        private static readonly Lazy<INavigationViewProvider> _navigationViewProvider = new Lazy<INavigationViewProvider>(() => new TestNavigationViewProvider(), LazyThreadSafetyMode.PublicationOnly);
        private static readonly Lazy<TestSchedulerProvider> _schedulerProvider = new Lazy<TestSchedulerProvider>(() => new TestSchedulerProvider(), LazyThreadSafetyMode.PublicationOnly);

        protected static INavigationViewProvider TestNavigationViewProvider
        {
            get => _navigationViewProvider.Value;
        }

        protected static TestSchedulerProvider TestSchedulerProvider
        {
            get => _schedulerProvider.Value;
        }

        private IScreenForContracts _screenForContracts;
        private RoutingWithContractsState _routingWithContractsState;

        protected IScreenForContracts ScreenForContracts => _screenForContracts;

        protected void WaitForNavigation(int maxTimeToWait = 100, TestScheduler schedulerToWaitOn = null)
        {
            TestScheduler scheduler = schedulerToWaitOn ?? TestSchedulerProvider.MainThread;
            // If the Scheduler is enabled that means it thinks that it is processing things (and probably is). But we want to force it to through. We could try to rig up something to wait until it finishes since in theory when it's done processing items it switches off. But for now this works.
            bool schedulerWasRunning = scheduler.IsEnabled;
            scheduler.Stop();
            for (int i = 0; i < maxTimeToWait; i++)
            {
                try
                {
                    scheduler.AdvanceBy(1);
                    if (!ScreenForContracts.Router.IsNavigating)
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

        protected ViewModelAndContract GetEmptyVMViewModelAndContract()
        {
            return new EmptyViewModel(ScreenForContracts).ToViewModelAndContract();
        }

        protected ViewModelAndContract GetTemperatureConversionVMViewModelAndContract()
        {
            return new TemperatureConversionsViewModel(ScreenForContracts, TestSchedulerProvider).ToViewModelAndContract();
        }

        protected NavigateArgumentAndStatus<IViewModelAndContract> GetNavigateArgumentAndStatus(IViewModelAndContract vm = null)
        {
            if (vm == null)
            {
                vm = GetEmptyVMViewModelAndContract();
            }
            return vm.ToNavigateArgumentAndStatus();
        }

        protected void AdvanceScheduler(int time = 100, TestScheduler schedulerToWaitOn = null)
        {
            TestScheduler scheduler = schedulerToWaitOn ?? TestSchedulerProvider.MainThread;
            bool schedulerWasRunning = scheduler.IsEnabled;
            scheduler.Stop();
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
        }

        [SetUp]
        public void SetUp()
        {
            // Perform any pre-test actions here. This runs before each test in this class. This is a where you should set up the
            // preconditions that are assumed to exist for this particular Given: https://en.wikipedia.org/wiki/Behavior-driven_development
            // An exception here will prevent the test from running and the [TearDown] method in this class will not run (tear down
            // methods in base classes will still run though). This runs after the [SetUp] in ReactiveBase.
            _routingWithContractsState = new RoutingWithContractsState(TestSchedulerProvider.MainThread);
            _screenForContracts = new MockIScreenForContracts(_routingWithContractsState);
        }

        [TearDown]
        public void TearDown()
        {
            // Perform any post-test actions here. For example if there are any class members that need to be disposed, this is a good
            // place to do that. This will run even if the test failed so that that into account. Also, an exception here will not stop
            // a base class [TearDown] method from being called and other tests in this class, if any, will run.
            _routingWithContractsState.Dispose();
            _screenForContracts?.Dispose();
        }

        [Test(Description = "When navigate to view model, Then does not throw")]
        public void WhenNavigateToViewModel_ThenDoesNotThrow()
        {
            Assert.That(() => _routingWithContractsState.Navigate.Execute(new EmptyViewModel(ScreenForContracts).ToViewModelAndContract()), Throws.Nothing);
        }

        [Test(Description = "When Navigate.Execute is called with a valid view model, Then RoutingWithContractsState.IsNavigating is true")]
        public void WhenNavigateExecuteCalledWithValidViewModel_ThenRoutingWithContractsStateIsNavigatingIsTrue()
        {
            Assert.That(() => _routingWithContractsState.Navigate.Execute(new EmptyViewModel(ScreenForContracts).ToViewModelAndContract()), Throws.Nothing);
            Assert.That(_routingWithContractsState.IsNavigating, Is.True);
        }

        [Test(Description = "WhenNavigateExecuteCalledWithValidViewModel_ThenIViewModelAndContractObserverOnNextDelegateIsCalledAfterNoMoreThan10SchedulerAdvanceBy1Calls")]
        public void WhenNavigateExecuteCalledWithValidViewModel_ThenIViewModelAndContractObserverOnNextDelegateIsCalledAfterNoMoreThan10SchedulerAdvanceBy1Calls()
        {
            bool wasCalled = false;
            using IViewModelAndContractObserver viewModelAndContractObserver = new IViewModelAndContractObserver().Subscribe(_routingWithContractsState.CurrentViewModel, (val) => wasCalled = true, null, null, TestSchedulerProvider.MainThread);
            _routingWithContractsState.Navigate.Execute(new EmptyViewModel(ScreenForContracts).ToViewModelAndContract());
            AdvanceScheduler(10);
            Assert.That(wasCalled, Is.True);
        }

        // NavigateBack*

        [Test(Description = "When NavigateBackWithStatus.Execute with null argument, Then exception is thrown")]
        public void WhenNavigateBackWithStatusExecuteWithNullArgument_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateBackWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateBackWithStatus.Execute with valid argument, Then no exception is thrown")]
        public void WhenNavigateBackWithStatusExecuteWithValidArgument_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateBackWithStatus.Execute(NavigateArgumentAndStatus<System.Reactive.Unit>.GetInstanceForUnit()), Throws.Nothing);
        }

        [Test(Description = "When NavigateBack.Execute with empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateBackExecuteWithEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateBack.Execute(), Throws.Nothing);
        }

        // Navigate*

        [Test(Description = "When Navigate.Execute to null, Then exception is thrown")]
        public void WhenNavigateExecuteToNull_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.Navigate.Execute(null), Throws.Exception);
        }

        [Test(Description = "When Navigate.Execute to IViewModelContract with null ViewModel, Then exception is thrown")]
        public void WhenNavigateExecuteCalledWithNullIViewModelContractViewModel_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.Navigate.Execute(new ViewModelAndContract(null)), Throws.Exception);
        }

        [Test(Description = "When Navigate.Execute to valid IViewModelContract, Then no exception is thrown")]
        public void WhenNavigateExecuteCalledWithValidIViewModelContract_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract()), Throws.Nothing);
        }

        [Test(Description = "When NavigateWithStatus.Execute to null, Then exception is thrown")]
        public void WhenNavigateWithStatusExecuteToNull_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateWithStatus.Execute to null IViewModelContract, Then exception is thrown")]
        public void WhenNavigateWithStatusExecuteCalledWithNullIViewModelContract_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(null)), Throws.Exception);
        }

        [Test(Description = "When NavigateWithStatus.Execute to null IViewModelContract.ViewModel, Then exception is thrown")]
        public void WhenNavigateWithStatusExecuteCalledWithNullIViewModelContractViewModel_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateWithStatus.Execute(new ViewModelAndContract(null).ToNavigateArgumentAndStatus()), Throws.Exception);
        }

        [Test(Description = "When NavigateWithStatus.Execute to valid NavigateArgumentAndStatus, Then no exception is thrown")]
        public void WhenNavigateWithStatusExecuteCalledWithValidIViewModelContract_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(GetEmptyVMViewModelAndContract())), Throws.Nothing);
        }

        // Empty navigation stack NavigateAndReset and NavigateAndResetWithStatus

        [Test(Description = "When NavigateAndReset.Execute to null and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetExecuteToNullAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndReset.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndReset.Execute with IViewModelContract with null ViewModel and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetExecuteCalledWithNullIViewModelContractViewModelAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndReset.Execute(new ViewModelAndContract(null)), Throws.Exception);
        }

        [Test(Description = "When NavigateAndReset.Execute with valid IViewModelContract and empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndResetExecuteCalledWithValidIViewModelContractAndEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndReset.Execute(GetEmptyVMViewModelAndContract()), Throws.Nothing);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with null and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteToNullAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with null IViewModelContract and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteCalledWithNullIViewModelContractAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(null)), Throws.Exception);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with null IViewModelContract.ViewModel and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteCalledWithNullIViewModelContractViewModelAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(new ViewModelAndContract(null).ToNavigateArgumentAndStatus()), Throws.Exception);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with valid NavigateArgumentAndStatus and empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteCalledWithValidIViewModelContractAndEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(GetEmptyVMViewModelAndContract())), Throws.Nothing);
        }

        // Non-empty navigation stack NavigateAndReset and NavigateAndResetWithStatus

        [Test(Description = "When NavigateAndReset.Execute to null and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetExecuteToNullAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndReset.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndReset.Execute with IViewModelContract with null ViewModel and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetExecuteCalledWithNullIViewModelContractViewModelAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndReset.Execute(new ViewModelAndContract(null)), Throws.Exception);
        }

        [Test(Description = "When NavigateAndReset.Execute with valid IViewModelContract and non-empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndResetExecuteCalledWithValidIViewModelContractAndNonEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndReset.Execute(GetEmptyVMViewModelAndContract()), Throws.Nothing);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with null and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteToNullAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with null IViewModelContract and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteCalledWithNullIViewModelContractAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(null)), Throws.Exception);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with null IViewModelContract.ViewModel and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteCalledWithNullIViewModelContractViewModelAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(new ViewModelAndContract(null).ToNavigateArgumentAndStatus()), Throws.Exception);
        }

        [Test(Description = "When NavigateAndResetWithStatus.Execute with valid NavigateArgumentAndStatus and non-empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndResetWithStatusExecuteCalledWithValidIViewModelContractAndNonEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndResetWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(GetEmptyVMViewModelAndContract())), Throws.Nothing);
        }

        // Empty navigation stack NavigateAndRemoveCurrent and NavigateAndRemoveCurrentWithStatus

        [Test(Description = "When NavigateAndRemoveCurrent.Execute with null and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentExecuteToNullAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrent.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrent.Execute with IViewModelContract with null ViewModel and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentExecuteCalledWithNullIViewModelContractViewModelAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrent.Execute(new ViewModelAndContract(null)), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrent.Execute with valid IViewModelContract and empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndRemoveCurrentExecuteCalledWithValidIViewModelContractAndEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrent.Execute(GetEmptyVMViewModelAndContract()), Throws.Nothing);
        }

        [Test(Description = "When NavigateAndRemoveCurrentWithStatus.Execute with null and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentWithStatusExecuteToNullAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrentWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrentWithStatus.Execute with null IViewModelContract.ViewModel and empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentWithStatusExecuteCalledWithNullIViewModelContractViewModelAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrentWithStatus.Execute(new ViewModelAndContract(null).ToNavigateArgumentAndStatus()), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrentWithStatus.Execute with valid IViewModelContract and empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndRemoveCurrentWithStatusExecuteCalledWithValidNavigateArgumentAndStatusAndEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrentWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(GetEmptyVMViewModelAndContract())), Throws.Nothing);
        }

        // Non-empty navigation stack NavigateAndRemoveCurrent and NavigateAndRemoveCurrentWithStatus

        [Test(Description = "When NavigateAndRemoveCurrent.Execute with null and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentExecuteToNullAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrent.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrent.Execute with IViewModelContract with null ViewModel and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentExecuteCalledWithNullIViewModelContractViewModelAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrent.Execute(new ViewModelAndContract(null)), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrent.Execute with valid IViewModelContract and non-empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndRemoveCurrentExecuteCalledWithValidIViewModelContractAndNonEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrent.Execute(GetEmptyVMViewModelAndContract()), Throws.Nothing);
        }

        [Test(Description = "When NavigateAndRemoveCurrentWithStatus.Execute with null and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentWithStatusExecuteToNullAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrentWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrentWithStatus.Execute with null IViewModelContract.ViewModel and non-empty navigation stack, Then exception is thrown")]
        public void WhenNavigateAndRemoveCurrentWithStatusExecuteCalledWithNullIViewModelContractViewModelAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrentWithStatus.Execute(new ViewModelAndContract(null).ToNavigateArgumentAndStatus()), Throws.Exception);
        }

        [Test(Description = "When NavigateAndRemoveCurrentWithStatus.Execute with valid IViewModelContract and non-empty navigation stack, Then no exception is thrown")]
        public void WhenNavigateAndRemoveCurrentWithStatusExecuteCalledWithValidNavigateArgumentAndStatusAndNonEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigateAndRemoveCurrentWithStatus.Execute(new NavigateArgumentAndStatus<IViewModelAndContract>(GetEmptyVMViewModelAndContract())), Throws.Nothing);
        }

        // Empty navigation stack ModifyNavigationStack and ModifyNavigationStackWithStatus

        [Test(Description = "When ModifyNavigationStack.Execute called with null and empty navigation stack, Then exception is thrown")]
        public void WhenModifyNavigationStackExecuteToNullAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStack.Execute(null), Throws.Exception);
        }

        [Test(Description = "When ModifyNavigationStack.Execute with valid Action and empty navigation stack, Then no exception is thrown")]
        public void WhenModifyNavigationStackExecuteCalledWithValidActionAndEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStack.Execute((stack) => { }), Throws.Nothing);
        }

        [Test(Description = "When ModifyNavigationStackWithStatus.Execute with null and empty navigation stack, Then exception is thrown")]
        public void WhenModifyNavigationStackWithStatusExecuteCalledWithNullAndEmptyNavigationStack_ThenExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStackWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When ModifyNavigationStackWithStatus.Execute with valid Action and empty navigation stack, Then no exception is thrown")]
        public void WhenModifyNavigationStackWithStatusExecuteCalledWithValidIViewModelContractAndEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            Assert.That(() => _routingWithContractsState.NavigationStack.Count, Is.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStackWithStatus.Execute(new ModifyArgumentAndStatus((stack) => { })), Throws.Nothing);
        }

        // Non-empty navigation stack ModifyNavigationStack and ModifyNavigationStackWithStatus

        [Test(Description = "When ModifyNavigationStack.Execute called with null and non-empty navigation stack, Then exception is thrown")]
        public void WhenModifyNavigationStackExecuteToNullAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStack.Execute(null), Throws.Exception);
        }

        [Test(Description = "When ModifyNavigationStack.Execute with valid IViewModelContract and non-empty navigation stack, Then no exception is thrown")]
        public void WhenModifyNavigationStackExecuteCalledWithValidIViewModelContractAndNonEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStack.Execute((stack) => { }), Throws.Nothing);
        }

        [Test(Description = "When ModifyNavigationStackWithStatus.Execute with null and non-empty navigation stack, Then exception is thrown")]
        public void WhenModifyNavigationStackWithStatusExecuteCalledWithNullAndNonEmptyNavigationStack_ThenExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStackWithStatus.Execute(null), Throws.Exception);
        }

        [Test(Description = "When ModifyNavigationStackWithStatus.Execute with valid IViewModelContract and non-empty navigation stack, Then no exception is thrown")]
        public void WhenModifyNavigationStackWithStatusExecuteCalledWithValidIViewModelContractAndNonEmptyNavigationStack_ThenNoExceptionIsThrown()
        {
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(() => _routingWithContractsState.ModifyNavigationStackWithStatus.Execute(new ModifyArgumentAndStatus((stack) => { })), Throws.Nothing);
        }

        // 

        [Test(Description = "WhenModifyNavigationStackIsCalledToClearTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackIsClearedAndNavigationToTheNewItemOccurs")]
        public void WhenModifyNavigationStackIsCalledToClearTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackIsClearedAndNavigationToTheNewItemOccurs()
        {
            //Assert.Multiple(() =>
            //{
            var vm = GetEmptyVMViewModelAndContract();
            _routingWithContractsState.Navigate.Execute(vm);
            WaitForNavigation();
            vm = GetEmptyVMViewModelAndContract();
            _routingWithContractsState.Navigate.Execute(vm);
            WaitForNavigation();
            vm = GetEmptyVMViewModelAndContract();
            _routingWithContractsState.Navigate.Execute(vm);
            WaitForNavigation();
            vm = GetEmptyVMViewModelAndContract();
            _routingWithContractsState.ModifyNavigationStack.Execute((navstack) =>
                {
                    navstack.Clear();
                    navstack.Add(vm);
                });
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.EqualTo(1));
            Assert.That(_routingWithContractsState.NavigationStack[0], Is.SameAs(vm));
            //});
        }

        [Test(Description = "WhenModifyNavigationStackIsCalledToClearTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackIsClearedAndNavigationToTheNewItemOccurs")]
        public void WhenModifyNavigationStackIsCalledToRemoveAllItemsOfACertainTypeFromTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackContainsNoItemsOfThatTypeAndNavigationToTheNewItemOccurs()
        {
            //Assert.Multiple(() =>
            //{
            var emptyVMC = GetEmptyVMViewModelAndContract();
            _routingWithContractsState.Navigate.Execute(emptyVMC);
            WaitForNavigation();
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();

            var tempVMC = GetTemperatureConversionVMViewModelAndContract();
            _routingWithContractsState.Navigate.Execute(tempVMC);
            WaitForNavigation();

            Assert.That(emptyVMC.ViewModel, Is.Not.Null);
            Assert.That(tempVMC.ViewModel, Is.Not.Null);
            var emptyVMCViewModelType = emptyVMC.ViewModel.GetType();
            var tempVMCViewModelType = tempVMC.ViewModel.GetType();
            Assert.That(emptyVMCViewModelType, Is.Not.TypeOf(tempVMCViewModelType));

            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();
            var itemToAdd = GetTemperatureConversionVMViewModelAndContract();
            var itemToAddViewModelType = itemToAdd.ViewModel.GetType();
            var removeViewModelType = emptyVMCViewModelType;
            Assert.That(itemToAddViewModelType, Is.Not.TypeOf(removeViewModelType));
            _routingWithContractsState.ModifyNavigationStack.Execute((navstack) =>
                {
                    var removeList = navstack.Where((vmc) => vmc.ViewModel.GetType() == removeViewModelType).ToList();
                    foreach (var item in removeList)
                    {
                        navstack.Remove(item);
                    }
                    navstack.Add(itemToAdd);
                });
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count((vmc) => vmc.ViewModel.GetType() == removeViewModelType), Is.Zero);
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Not.Zero);
            Assert.That(_routingWithContractsState.NavigationStack[_routingWithContractsState.NavigationStack.Count - 1], Is.SameAs(itemToAdd));
            //});
        }

        [Test(Description = "WhenSecondNavigationWhileFirstNavigationIsOccurring_ThenFirstNavigationSucceedsAndSecondNavigationIsIgnored")]
        public void WhenSecondNavigationWhileFirstNavigationIsOccurring_ThenFirstNavigationSucceedsAndSecondNavigationIsIgnored()
        {
            var firstNavItem = GetEmptyVMViewModelAndContract().ToNavigateArgumentAndStatus();
            var secondNavItem = GetEmptyVMViewModelAndContract().ToNavigateArgumentAndStatus();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.Zero);
            _routingWithContractsState.NavigateWithStatus.Execute(firstNavItem);
            Assert.That(TestSchedulerProvider.MainThread.IsEnabled, Is.False);
            AdvanceScheduler(1);
            Assert.That(_routingWithContractsState.IsNavigating, Is.True);
            _routingWithContractsState.NavigateWithStatus.Execute(secondNavItem);
            WaitForNavigation();
            Assert.That(firstNavItem.AlreadyNavigating, Is.False);
            Assert.That(secondNavItem.AlreadyNavigating, Is.True);
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.EqualTo(1));
            Assert.That(_routingWithContractsState.NavigationStack[0], Is.SameAs(firstNavItem.Value));
        }


    }
}