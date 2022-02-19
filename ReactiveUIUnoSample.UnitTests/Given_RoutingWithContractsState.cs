using System;
using System.Collections.Generic;
using System.Threading;

using FluentAssertions;
using FluentAssertions.Execution;

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
        private IScreenForContracts _screenWithContract;
        private RoutingWithContractsState _routingWithContractsState;

        [SetUp]
        public void SetUp()
        {
            // Perform any pre-test actions here. This runs before each test in this class. This is a where you should set up the
            // preconditions that are assumed to exist for this particular Given: https://en.wikipedia.org/wiki/Behavior-driven_development
            // An exception here will prevent the test from running and the [TearDown] method in this class will not run (tear down
            // methods in base classes will still run though). This runs after the [SetUp] in ReactiveBase.
            _routingWithContractsState = new RoutingWithContractsState(_schedulerProvider.Value.MainThread);
            _screenWithContract = new MockIScreenForContracts(_routingWithContractsState);
        }

        [TearDown]
        public void TearDown()
        {
            // Perform any post-test actions here. For example if there are any class members that need to be disposed, this is a good
            // place to do that. This will run even if the test failed so that that into account. Also, an exception here will not stop
            // a base class [TearDown] method from being called and other tests in this class, if any, will run.
            _routingWithContractsState.Dispose();
        }

        [Test(Description = "Given navigate to view model, Then does not throw")]
        public void GivenNavigateToViewModel_ThenDoesNotThrow()
        {
            Assert.That(() => _routingWithContractsState.Navigate.Execute(new EmptyViewModel(_screenWithContract).ToViewModelAndContract()), Throws.Nothing);
        }

        [Test(Description = "Given Navigate.Execute is called with a valid view model, Then RoutingWithContractsState.IsNavigating is true")]
        public void GivenNavigateExecuteCalledWithValidViewModel_ThenRoutingWithContractsStateIsNavigatingIsTrue()
        {
            Assert.That(() => _routingWithContractsState.Navigate.Execute(new EmptyViewModel(_screenWithContract).ToViewModelAndContract()), Throws.Nothing);
            Assert.That(_routingWithContractsState.IsNavigating, Is.True);
        }

        [Test(Description = "GivenNavigateExecuteCalledWithValidViewModel_ThenIViewModelAndContractObserverOnNextDelegateIsCalledAfterNoMoreThan10SchedulerAdvanceBy1Calls")]
        public void GivenNavigateExecuteCalledWithValidViewModel_ThenIViewModelAndContractObserverOnNextDelegateIsCalledAfterNoMoreThan10SchedulerAdvanceBy1Calls()
        {
            bool wasCalled = false;
            using IViewModelAndContractObserver viewModelAndContractObserver = new IViewModelAndContractObserver().Subscribe(_routingWithContractsState.CurrentViewModel, (val) => wasCalled = true, null, null, _schedulerProvider.Value.MainThread);
            _routingWithContractsState.Navigate.Execute(new EmptyViewModel(_screenWithContract).ToViewModelAndContract());
            _schedulerProvider.Value.MainThread.AdvanceBy(10);
            Assert.That(wasCalled, Is.True);
        }
    }
}