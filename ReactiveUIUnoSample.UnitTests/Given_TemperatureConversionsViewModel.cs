using System;
using System.Collections.Generic;
using System.Linq;

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
            if (GetCurrentViewModel()?.ViewModel is TemperatureConversionsViewModel vm)
            {
                _viewModel = vm;
            }
            Warn.If(_viewModel is null, () =>
            {
                _viewModel = new TemperatureConversionsViewModel(ScreenWithContract, TestSchedulerProvider);
                return $"Expected the Current View Model to be of type {nameof(TemperatureConversionsViewModel)}. Instead it is of type {GetCurrentViewModel()?.ViewModel?.GetType().FullName ?? "(null)"}";
            });
        }

        [TearDown]
        public void TearDown()
        {
            // Perform any post-test actions here. For example if there are any class members that need to be disposed, this is a good
            // place to do that. This will run even if the test failed so that that into account. Also, an exception here will not stop
            // a base class [TearDown] method from being called and other tests in this class, if any, will run.
        }

        [Test(Description = "When TempEntryOneText is 100 And Conversion is Celsius to Fahrenheit Then TempEntryTwoText is 212")]
        public void WhenTempEntryOneTextIs100AndConversionIsCelsiusToFahrenheit_ThenTempEntryTwoTextIs212()
        {
            _viewModel.TempEntryOneText = "100";
            _viewModel.SelectedTemperatureConversion = _viewModel.ConversionDirections.First((tcdvdp) => tcdvdp.Value == Helpers.TemperatureConversionDirection.CelsiusToFahrenheit);
            var sut = _viewModel.TempEntryTwoText;
            Assert.That(double.TryParse(sut, out double convertedTemperature), Is.True);
            Assert.That(convertedTemperature, Is.EqualTo(212));
        }

        [Test(Description = "When TempEntryOneText is 32 And Conversion is Fahrenheit to Celsius Then TempEntryTwoText is 0")]
        public void WhenTempEntryOneTextIs32AndConversionIsFahrenheitToCelsius_ThenTempEntryTwoTextIs0()
        {
            _viewModel.TempEntryOneText = "32";
            _viewModel.SelectedTemperatureConversion = _viewModel.ConversionDirections.First((tcdvdp) => tcdvdp.Value == Helpers.TemperatureConversionDirection.FahrenheitToCelsius);
            var sut = _viewModel.TempEntryTwoText;
            Assert.That(double.TryParse(sut, out double convertedTemperature), Is.True);
            Assert.That(convertedTemperature, Is.EqualTo(0));
        }

        [Test(Description = "When SelectedTestType and SelectedDifficulty are set, Then RunTestCommand.CanExecute is true")]
        public void WhenSelectedTestTypeAndSelectedDifficultyAreSet_ThenRunTestCommandCanExecuteIsTrue()
        {
            using var booleanObserver = new BooleanObserver();
            booleanObserver.Subscribe(_viewModel.RunTestCommand.CanExecute, TestSchedulerProvider.MainThread);
            _viewModel.SelectedTestType = _viewModel.TestTypes.First();
            _viewModel.SelectedDifficulty = _viewModel.TestDifficulties.First();
            // For reasons I do not currently understand, ticking by 1 or 2 does not cause CanExecute to change but ticking by 10 does (I didn't test 3-9).
            TestSchedulerProvider.MainThread.AdvanceBy(10);
            Assert.That(booleanObserver.LastValue is true, Is.True);
        }

        [Test(Description = "When SelectedTestType and SelectedDifficulty are set, Then RunTestCommand.Execute navigates to TwoLineTestViewModel")]
        public void WhenSelectedTestTypeAndSelectedDifficultyAreSet_ThenRunTestCommandExecuteNavigatesToTwoLineTestViewModel()
        {
            WhenSelectedTestTypeAndSelectedDifficultyAreSet_ThenRunTestCommandCanExecuteIsTrue();
            Assert.That(() => (_viewModel.RunTestCommand as System.Windows.Input.ICommand).Execute(null), Throws.Nothing);
            Assert.That(GetCurrentViewModel()?.ViewModel, Is.AssignableTo(typeof(TwoLineTestViewModel)));
        }
    }
}