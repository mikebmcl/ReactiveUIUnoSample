using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

using FluentAssertions;
using FluentAssertions.Execution;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using ReactiveUIUnoSample;
using ReactiveUIUnoSample.ViewModels;
using ReactiveUIUnoSample.ViewModels.Testing;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class Given_TemperatureConversionsViewModel : ReactiveTestBase
    {
        TemperatureConversionsViewModel _viewModel;
        [SetUp]
        public void SetUp()
        {
            // Perform any pre-test actions here. This runs before each test in this class. This is a where you should set up the
            // preconditions that are assumed to exist for this particular Given: https://en.wikipedia.org/wiki/Behavior-driven_development
            // An exception here will prevent the test from running and the [TearDown] method in this class will not run (tear down
            // methods in base classes will still run though). This runs after the [SetUp] in ReactiveBase.
            if (GetCurrentViewModel() is TemperatureConversionsViewModel vm)
            {
                _viewModel = vm;
            }
            Warn.If(_viewModel is null, () =>
            {
                _viewModel = new TemperatureConversionsViewModel(ScreenWithContract, TestSchedulerProvider);
                return $"Expected the Current View Model to be of type {nameof(TemperatureConversionsViewModel)}. Instead it is of type {GetCurrentViewModel()?.GetType().FullName ?? "(null)"}";
            });
        }

        [TearDown]
        public void TearDown()
        {
            // Perform any post-test actions here. For example if there are any class members that need to be disposed, this is a good
            // place to do that. This will run even if the test failed so that that into account. Also, an exception here will not stop
            // a base class [TearDown] method from being called and other tests in this class, if any, will run.
        }

        [Test(Description = "When TempInputText is 100 And Conversion is Celsius to Fahrenheit Then TempConversionResultText is 212")]
        public void WhenTempInputTextIs100AndConversionIsCelsiusToFahrenheit_ThenTempConversionResultTextIs212()
        {
            Assert.IsTrue(default(int) == 0);
            _viewModel.TempInputText = "100";
            _viewModel.SelectedTemperatureConversion = _viewModel.ConversionDirections.First((tcdvdp) => tcdvdp.Value == Helpers.TemperatureConversionDirection.CelsiusToFahrenheit);
            TestSchedulerProvider.AdvanceAllSchedulers();
            var sut = _viewModel.TempConversionResultText;
            Assert.That(double.TryParse(sut, out double convertedTemperature), Is.True);
            Assert.That(convertedTemperature, Is.EqualTo(212));
        }

        [Test(Description = "When TempInputText is 140 And Conversion is Fahrenheit to Celsius Then TempConversionResultText is 60")]
        public void WhenTempInputTextIs140AndConversionIsFahrenheitToCelsius_ThenTempConversionResultTextIs60()
        {
            _viewModel.TempInputText = "140";
            _viewModel.SelectedTemperatureConversion = _viewModel.ConversionDirections.First((tcdvdp) => tcdvdp.Value == Helpers.TemperatureConversionDirection.FahrenheitToCelsius);
            TestSchedulerProvider.AdvanceAllSchedulers();
            var sut = _viewModel.TempConversionResultText;
            Assert.That(double.TryParse(sut, out double convertedTemperature), Is.True);
            Assert.That(convertedTemperature, Is.EqualTo(60));
        }

        [Test(Description = "When SelectedTestType and SelectedDifficulty are set, Then RunTempTest.CanExecute is true")]
        public void WhenSelectedTestTypeAndSelectedDifficultyAreSet_ThenRunTempTestCanExecuteIsTrue()
        {
            using var booleanObserver = new BooleanObserver();
            booleanObserver.Subscribe(_viewModel.RunTempTest.CanExecute, TestSchedulerProvider.MainThread, null, null);
            _viewModel.SelectedTestType = _viewModel.TestTypes.First();
            _viewModel.SelectedDifficulty = _viewModel.TestDifficulties.First();
            TestSchedulerProvider.AdvanceAllSchedulers(10);
            Assert.That(booleanObserver.LastValue is true, Is.True);
        }

        [Test(Description = "When SelectedTestType and SelectedDifficulty are set, Then RunTempTest.Execute navigates to TwoLineTestViewModel")]
        public void WhenSelectedTestTypeAndSelectedDifficultyAreSet_ThenRunTempTestExecuteNavigatesToTwoLineTestViewModel()
        {
            WhenSelectedTestTypeAndSelectedDifficultyAreSet_ThenRunTempTestCanExecuteIsTrue();
            Assert.That(() => (_viewModel.RunTempTest as System.Windows.Input.ICommand).Execute(null), Throws.Nothing);
            Assert.That(GetCurrentViewModel(), Is.AssignableTo(typeof(TwoLineTestViewModel)));
        }

        [Test(Description = "When SelectedTestType and SelectedDifficulty are set and RunTempTest.Execute followed by NavigateToFirstView.Execute, Then navigates to TwoLineTestViewModel and not FirstView")]
        public void WhenSelectedTestTypeAndSelectedDifficultyAreSetAndRunRunTempTestExecuteFollowedByNavigateToFirstViewExecute_ThenNavigatesToTwoLineTestViewModelAndNotFirstView()
        {
            _viewModel.HostScreenWithContract.Router.CurrentViewModel.Subscribe();
            BooleanObserver runTestCanExecuteObserver = new BooleanObserver().Subscribe(_viewModel.RunTempTest.CanExecute, TestSchedulerProvider.MainThread, null, null);
            BooleanObserver navigateToFirstViewCanExecute = new BooleanObserver().Subscribe(_viewModel.NavigateToFirstView.CanExecute, TestSchedulerProvider.MainThread, null, null);
            Assert.That(GetCurrentViewModel(), Is.AssignableTo(typeof(TemperatureConversionsViewModel)));
            Assert.That(runTestCanExecuteObserver.LastValue, Is.Null);
            _viewModel.SelectedTestType = _viewModel.TestTypes.First();
            //AdvanceScheduler(10);
            TestSchedulerProvider.AdvanceAllSchedulers(10);
            Assert.That(runTestCanExecuteObserver.LastValue, Is.False);
            _viewModel.SelectedDifficulty = _viewModel.TestDifficulties.First();
            AdvanceScheduler(10);
            Assert.That(runTestCanExecuteObserver.LastValue, Is.True);
            using var runTestExecuteSchedulerDisposable = TestSchedulerProvider.MainThread.Schedule(string.Empty, (sch, state) => _viewModel.RunTempTest.Execute().Subscribe());
            AdvanceScheduler(4);
            Assert.That(runTestCanExecuteObserver.LastValue is false && (_viewModel.RunTempTest as System.Windows.Input.ICommand).CanExecute(null), Is.False);
            Assert.That(navigateToFirstViewCanExecute.LastValue is false && (_viewModel.NavigateToFirstView as System.Windows.Input.ICommand).CanExecute(null), Is.False);
            Assert.That(GetCurrentViewModel(), Is.AssignableTo(typeof(TwoLineTestViewModel)));
        }

        [Test(Description = "When neither SelectedTestType nor SelectedDifficulty are set, Then RunTempTest returns false for CanExecute")]
        public void WhenNeitherSelectedTestTypeNorSelectedDifficultyAreSet_ThenRunTempTestReturnsFalseForCanExecute()
        {
            _viewModel.HostScreenWithContract.Router.CurrentViewModel.Subscribe();
            Assert.That(GetCurrentViewModel(), Is.AssignableTo(typeof(TemperatureConversionsViewModel)));
            Assert.That(_viewModel.SelectedTestType, Is.Null);
            Assert.That(_viewModel.SelectedDifficulty, Is.Null);
            TestSchedulerProvider.AdvanceAllSchedulers();
            // Default to true in Observable.MostRecent because we want this to fail if CanExecute has not had any items since we expect that it will have had items and the most recent would be false.
            Assert.That(Observable.MostRecent(_viewModel.RunTempTest.CanExecute, true).FirstOrDefault(), Is.False);
            Assert.That((_viewModel.RunTempTest as System.Windows.Input.ICommand).CanExecute(null), Is.False);
        }

        [Test(Description = "When SelectedTestType is set and SelectedDifficulty is not set, Then RunTempTest returns false for CanExecute")]
        public void WhenSelectedTestTypeIsSetAndSelectedDifficultyIsNotSet_ThenRunTempTestReturnsFalseForCanExecute()
        {
            _viewModel.HostScreenWithContract.Router.CurrentViewModel.Subscribe();
            Assert.That(GetCurrentViewModel(), Is.AssignableTo(typeof(TemperatureConversionsViewModel)));
            _viewModel.SelectedTestType = _viewModel.TestTypes.First();
            TestSchedulerProvider.AdvanceAllSchedulers();
            Assert.That(_viewModel.SelectedTestType, Is.Not.Null);
            Assert.That(_viewModel.SelectedDifficulty, Is.Null);
            // Default to true in Observable.MostRecent because we want this to fail if CanExecute has not had any items since we expect that it will have had items and the most recent would be false.
            Assert.That(Observable.MostRecent(_viewModel.RunTempTest.CanExecute, true).FirstOrDefault(), Is.False);
            Assert.That((_viewModel.RunTempTest as System.Windows.Input.ICommand).CanExecute(null), Is.False);
        }

        [Test(Description = "When SelectedTestType is not set and SelectedDifficulty is set, Then RunTempTest returns false for CanExecute")]
        public void WhenSelectedTestTypeIsNotSetAndSelectedDifficultyIsSet_ThenRunTempTestReturnsFalseForCanExecute()
        {
            _viewModel.HostScreenWithContract.Router.CurrentViewModel.Subscribe();
            Assert.That(GetCurrentViewModel(), Is.AssignableTo(typeof(TemperatureConversionsViewModel)));
            _viewModel.SelectedDifficulty = _viewModel.TestDifficulties.First();
            TestSchedulerProvider.AdvanceAllSchedulers();
            Assert.That(_viewModel.SelectedTestType, Is.Null);
            Assert.That(_viewModel.SelectedDifficulty, Is.Not.Null);
            // Default to true in Observable.MostRecent because we want this to fail if CanExecute has not had any items since we expect that it will have had items and the most recent would be false.
            Assert.That(Observable.MostRecent(_viewModel.RunTempTest.CanExecute, true).FirstOrDefault(), Is.False);
            Assert.That((_viewModel.RunTempTest as System.Windows.Input.ICommand).CanExecute(null), Is.False);
        }
    }
}