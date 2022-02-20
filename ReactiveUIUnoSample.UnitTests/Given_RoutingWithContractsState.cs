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
            for (int i = 0; i < maxTimeToWait; i++)
            {
                if (scheduler.IsEnabled)
                {
                    scheduler.Sleep(1);
                    if (!ScreenForContracts.Router.IsNavigating)
                    {
                        return;
                    }
                }
                else
                {
                    scheduler.AdvanceBy(1);
                    if (!ScreenForContracts.Router.IsNavigating)
                    {
                        return;
                    }
                }
            }
            throw new TimeoutException($"In {nameof(WaitForNavigation)}, navigation still had not occurred after {maxTimeToWait} passed.");
        }

        protected ViewModelAndContract GetEmptyVMViewModelAndContract()
        {
            return new EmptyViewModel(ScreenForContracts).ToViewModelAndContract();
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
            if (scheduler.IsEnabled)
            {
                scheduler.Sleep(time);
            }
            else
            {
                scheduler.AdvanceBy(time);
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
            TestSchedulerProvider.MainThread.AdvanceBy(10);
            Assert.That(wasCalled, Is.True);
        }

        [Test(Description = "WhenNavigateAndApplyFuncIsCalledToClearTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackIsClearedAndNavigationToTheNewItemOccurs")]
        public void WhenNavigateAndApplyFuncIsCalledToClearTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackIsClearedAndNavigationToTheNewItemOccurs()
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
            _routingWithContractsState.NavigateAndApplyFunc.Execute(new RoutingWithContractsStateApplyFuncData(
                (navstack) =>
                {
                    navstack.Clear();
                    return true;
                }, vm));
            WaitForNavigation();
            Assert.That(_routingWithContractsState.NavigationStack.Count, Is.EqualTo(1));
            Assert.That(_routingWithContractsState.NavigationStack[0], Is.SameAs(vm));
            //});
        }

        [Test(Description = "WhenNavigateAndApplyFuncIsCalledToClearTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackIsClearedAndNavigationToTheNewItemOccurs")]
        public void WhenNavigateAndApplyFuncIsCalledToRemoveAllItemsOfACertainTypeFromTheNavigationStackAndNavigateToANewItem_ThenTheNavigationStackContainsNoItemsOfThatTypeAndNavigationToTheNewItemOccurs()
        {
            //Assert.Multiple(() =>
            //{
            TemperatureConversionsViewModel GenerateTemperatureConversionViewModel()
            {
                return new TemperatureConversionsViewModel(ScreenForContracts, TestSchedulerProvider);
            }
            var emptyVMC = GetEmptyVMViewModelAndContract();
            _routingWithContractsState.Navigate.Execute(emptyVMC);
            WaitForNavigation();
            _routingWithContractsState.Navigate.Execute(GetEmptyVMViewModelAndContract());
            WaitForNavigation();

            var tempVMC = GenerateTemperatureConversionViewModel().ToViewModelAndContract();
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
            var itemToAdd = GenerateTemperatureConversionViewModel().ToViewModelAndContract();
            var itemToAddViewModelType = itemToAdd.ViewModel.GetType();
            var removeViewModelType = emptyVMCViewModelType;
            Assert.That(itemToAddViewModelType, Is.Not.TypeOf(removeViewModelType));
            _routingWithContractsState.NavigateAndApplyFunc.Execute(new RoutingWithContractsStateApplyFuncData(
                (navstack) =>
                {
                    var removeList = navstack.Where((vmc) => vmc.ViewModel.GetType() == removeViewModelType).ToList();
                    foreach (var item in removeList)
                    {
                        navstack.Remove(item);
                    }
                    return true;
                }, itemToAdd));
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
            TestSchedulerProvider.MainThread.AdvanceBy(1);
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