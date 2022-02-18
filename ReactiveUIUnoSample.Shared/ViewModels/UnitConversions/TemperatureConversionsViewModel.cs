﻿
using ReactiveUIUnoSample.ViewModels.Testing;

using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Helpers;
using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.Interfaces.Testing;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Input;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

using Microsoft.Extensions.Logging;
using Uno.Extensions;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUIRoutingWithContracts;
using System.Linq;

namespace ReactiveUIUnoSample.ViewModels.UnitConversions
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TemperatureConversionsViewModel : DisplayViewModelBase//TemperatureConversionsViewModelBase//, IUnitConversionsTesting
    {
        public TemperatureConversionsViewModel(IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            // Note: It's safe to not unsubscribe from this event because it does not hold a hard reference to this object, it's subscribing to an event on an object
            // that is a non-static member of this class, and we have no reasonable way to 100% guarantee that our attempt to unsubscribe would always run (because certain
            // platforms cannot guarantee that custom back handlers will always run) barring creating some really horrible code that would involve other classes.
            _isNavigating.ValueChanged += IsNavigatingValueChangedHandler;

            RunTest = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!_isNavigating.Set(true))
                {
                    await HostScreenWithContract.Router.Navigate.Execute(null);
                }
                try
                {
                    await RunTestImpl();
                }
                finally
                {
                    _isNavigating.ForceToFalse();
                }
            },
                this.WhenAnyValue(
                    //x => x.CurrentViewModel,
                    x => x.SelectedTestType,
                    x => x.SelectedDifficulty,
                    x => x.IsNavigating,
                    (testType, difficulty, isnav) =>
                    // cvm is just a signal that does nothing becaue the problem is the delay between navigation beginning and the navigation stack actually changing
                    testType != null &&
                    difficulty != null &&
                    !isnav &&
                    HostScreenWithContract.Router.NavigationStack?.LastOrDefault()?.ViewModel?.GetType() == this.GetType())
                .ObserveOn(SchedulerProvider.MainThread),
                SchedulerProvider.MainThread
                );
            _runTestExceptionObserver = new ExceptionObserver(nameof(RunTest)).Subscribe(RunTest.ThrownExceptions, SchedulerProvider.MainThread, this.Log());
            TempEntryOneText = "0";
            TempPickerTitle = "Temperature";
            SelectedTemperatureConversion = ConversionDirections[0];
            NavigateToFirstView = ReactiveCommand.CreateFromTask(async () =>
            {
                if (!_isNavigating.Set(true))
                {
                    await HostScreenWithContract.Router.Navigate.Execute(null);
                }
                try
                {
                    await HostScreenWithContract.Router.Navigate.Execute(new FirstViewModel(HostScreenWithContract, SchedulerProvider).ToViewModelAndContract());
                }
                finally
                {
                    _isNavigating.ForceToFalse();
                }
            },
                this.WhenAnyValue(
                    x => x.CurrentViewModel,
                    x => x.IsNavigating,
                    (cvm, isnav) =>
                    !isnav &&
                    HostScreenWithContract.Router.NavigationStack?.LastOrDefault()?.ViewModel?.GetType() == this.GetType())
                .ObserveOn(SchedulerProvider.MainThread),
                SchedulerProvider.MainThread
                );
            //NavigateToFirstView.IsExecuting.ToProperty(this, nameof(NavigateToFirstViewIsRunning), out _navigateToFirstViewIsRunning, false, SchedulerProvider.MainThread);
            _navigateToFirstViewExceptionObserver = new ExceptionObserver(nameof(NavigateToFirstView)).Subscribe(NavigateToFirstView.ThrownExceptions, SchedulerProvider.MainThread, this.Log());
            this.WhenAnyValue(x => x.TempEntryOneText, x => x.SelectedTemperatureConversion, (value, directionAsObj) =>
            {
                string strToConvert = value;
                if (string.IsNullOrWhiteSpace(strToConvert) || directionAsObj == null || !(directionAsObj is TemperatureConversionDirectionValueDisplayPair direction))
                {
                    return "";
                }
                double convertedValue;
                if (double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double valueToConvert))
                {
                    switch (direction.Value)
                    {
                        case TemperatureConversionDirection.CelsiusToFahrenheit:
                            convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.CelsiusToFahrenheit, valueToConvert);
                            break;
                        case TemperatureConversionDirection.FahrenheitToCelsius:
                            convertedValue = MiscHelpers.ConvertTemperature(TemperatureConversionDirection.FahrenheitToCelsius, valueToConvert);
                            break;
                        case TemperatureConversionDirection.Invalid:
                            return "";
                        default:
                            DiagnosticsHelpers.ReportProblem($"Unknown temperature picker enumerator value '{direction.Value}'", LogLevel.Error, null);
                            return _errorValue;
                    }
                    if (convertedValue == double.PositiveInfinity)
                    {
                        return _errorValue;
                    }
                    return convertedValue.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return _errorValue;
                }

            }).ToProperty(this, nameof(TempEntryTwoText), out _tempEntryTwoText, false, SchedulerProvider.MainThread);
        }

        private readonly AtomicBoolean _isNavigating = new AtomicBoolean();
        /// <summary>
        /// This exists solely as a component of correctly determining the appropriate return value of RunTestCommand's CanExecute method. There is
        /// no guarantee that it is actually up-to-date and should never be modified except by the event handler for m_runTestCommandIsExecuting's
        /// ValueChanged event. It need to be marked with ReactiveAttribute or to otherwise implement INotifyPropertyChanged or one of the other mechanisms
        /// that ensures that ReactiveUI's WhenAny extension methods will be informed that its value changed.
        /// </summary>
        [Reactive]
        private bool IsNavigating { get; set; }
        private void IsNavigatingValueChangedHandler(object sender, EventArgs args)
        {
            // We're using this as part of calculating the proper value of the RunTestCommand's CanExecute method.
            IsNavigating = _isNavigating.Get();
        }

        // Number of Questions

        private const string _numberOfQuestionsDefaultValue = "10";
        [Reactive]
        public string NumberOfQuestions { get; set; }

        // Temperature

        // These are arbitrary but are close to equal (-20F is -28.88889C)
        private const string _minimumCelsiusTemperatureDefaultValue = "-30";
        private const string _minimumFahrenheitTemperatureDefaultValue = "-20";
        [Reactive]
        public string MinimumTemperature { get; set; }

        // These are arbitrary but happen to be equal.
        private const string _maximumCelsiusTemperatureDefaultValue = "60";
        private const string _maximumFahrenheitTemperatureDefaultValue = "140";

        [Reactive]
        public string MaximumTemperature { get; set; }

        //private object _selectedTestType;
        [Reactive]
        public object SelectedTestType { get; set; }

        public IList<TemperatureConversionDirectionValueDisplayPair> TestTypes => new List<TemperatureConversionDirectionValueDisplayPair>(new TemperatureConversionDirectionValueDisplayPair[]
        {
            new TemperatureConversionDirectionValueDisplayPair(TemperatureConversionDirection.CelsiusToFahrenheit, _celsiusToFahrenheit)
            , new TemperatureConversionDirectionValueDisplayPair(TemperatureConversionDirection.FahrenheitToCelsius, _fahrenheitToCelsius)
        });

        //private object _selectedDifficulty;
        [Reactive]
        public object SelectedDifficulty { get; set; }

        private string _errorValue = "(Error)";

        [Reactive]
        public object SelectedTemperatureConversion { get; set; }

        [Reactive]
        public string TempEntryOneText { get; set; }

        private readonly ObservableAsPropertyHelper<string> _tempEntryTwoText;
        public string TempEntryTwoText => _tempEntryTwoText.Value;

        [Reactive]
        public string TempPickerTitle { get; set; }

        private string _title = "Temperature Conversions!";
        public string Title
        {
            get => _title; set { if (_title != value) { _title = value; RaisePropertyChanged(); } }
        }

        public override object HeaderContent { get; set; } = "Temperature Conversions";
        protected const string _fahrenheitToCelsius = "F to C";
        protected const string _celsiusToFahrenheit = "C to F";
        private static readonly List<TestDifficultyValueDisplayPair> _testDifficulties = new List<TestDifficultyValueDisplayPair>(new TestDifficultyValueDisplayPair[] { new TestDifficultyValueDisplayPair(TestDifficulty.Easy, "Easy"), new TestDifficultyValueDisplayPair(TestDifficulty.Medium, "Medium"), new TestDifficultyValueDisplayPair(TestDifficulty.Hard, "Hard") });
        public List<TestDifficultyValueDisplayPair> TestDifficulties => _testDifficulties;

        private static readonly List<TemperatureConversionDirectionValueDisplayPair> _conversionDirections = new List<TemperatureConversionDirectionValueDisplayPair>(new TemperatureConversionDirectionValueDisplayPair[]
        {
            new TemperatureConversionDirectionValueDisplayPair(TemperatureConversionDirection.CelsiusToFahrenheit, _celsiusToFahrenheit)
            , new TemperatureConversionDirectionValueDisplayPair(TemperatureConversionDirection.FahrenheitToCelsius, _fahrenheitToCelsius)
        });

        public IReadOnlyList<TemperatureConversionDirectionValueDisplayPair> ConversionDirections => _conversionDirections;

        public static double GetUniqueOffset(Random random, int adjustedDifferencePlusOne, HashSet<int> existingQuestionOffsets, double testValueOffsetFromMinimum)
        {
            // We want to prevent duplicates since the number of possible values to test is greater than the number of questions. We do this before adjusting for half degree values (if we are doing half degrees) since those half degree values are part of the possible values.
            if (!existingQuestionOffsets.Add((int)testValueOffsetFromMinimum))
            {
                // If there isn't a large difference between possible values and number of questions, then we could run into issues with it bogging down trying to get a value from random that isn't already in the HashSet, especially towards the end of the question generation. So all of this code that follows exists to cut that off by capping the number of calls to random and if we hit the cap then incrementally checking values in the HashSet until we find an unused one.
                const int infiniteLoopPreventionMax = 100;
                bool foundValue = false;
                for (int infiniteLoopPreventionCounter = 0; infiniteLoopPreventionCounter < infiniteLoopPreventionMax; infiniteLoopPreventionCounter++)
                {
                    testValueOffsetFromMinimum = random.Next(adjustedDifferencePlusOne);
                    if (existingQuestionOffsets.Add((int)testValueOffsetFromMinimum))
                    {
                        foundValue = true;
                        break;
                    }
                    infiniteLoopPreventionCounter++;
                }
                if (!foundValue)
                {
                    double fallback = testValueOffsetFromMinimum;
                    for (int checkIfNotUsed = 0; checkIfNotUsed < adjustedDifferencePlusOne; checkIfNotUsed++)
                    {
                        if (existingQuestionOffsets.Add(checkIfNotUsed))
                        {
                            foundValue = true;
                            testValueOffsetFromMinimum = checkIfNotUsed;
                            break;
                        }
                    }
                    if (!foundValue)
                    {
                        // We should never get here because there should be more possible values than number of questions so we should've found a possible value.
                        testValueOffsetFromMinimum = fallback;
                    }
                }
            }
            return testValueOffsetFromMinimum;
        }

        public ReactiveCommand<Unit, Unit> NavigateToFirstView { get; set; }
        private readonly ExceptionObserver _navigateToFirstViewExceptionObserver;
        //private readonly ObservableAsPropertyHelper<bool> _navigateToFirstViewIsRunning;
        //public bool NavigateToFirstViewIsRunning => _navigateToFirstViewIsRunning?.Value ?? false;

        public ReactiveCommand<Unit, Unit> RunTest { get; set; }
        private readonly ExceptionObserver _runTestExceptionObserver;
        //private readonly ObservableAsPropertyHelper<bool> _runTestIsExecuting;
        //public bool RunTestIsExecuting => _runTestIsExecuting?.Value ?? false;

        private IObservable<IViewModelAndContract> RunTestImpl()
        {
            try
            {
                var _selectedTestType = SelectedTestType;
                if (!(_selectedTestType is TemperatureConversionDirectionValueDisplayPair selectedTestType))
                {
                    throw new InvalidCastException($"Expected {nameof(_selectedTestType)} to be of type {nameof(TemperatureConversionDirectionValueDisplayPair)} but instead it is of type '{_selectedTestType?.GetType().FullName ?? "(null)"}'.");
                }
                var testType = selectedTestType.Value;
                if (testType == TemperatureConversionDirection.Invalid)
                {
                    throw new InvalidOperationException($"While trying to get the Value of {nameof(_selectedTestType)}, it contained a {nameof(TemperatureConversionDirection)} value of '{testType}', which is invalid for test type.");
                }

                var _selectedDifficulty = SelectedDifficulty;
                if (!(_selectedDifficulty is TestDifficultyValueDisplayPair selectedDifficulty))
                {
                    throw new InvalidCastException($"Expected {nameof(_selectedDifficulty)} to be of type {nameof(TestDifficultyValueDisplayPair)} but instead it is of type '{_selectedDifficulty?.GetType().FullName ?? "(null)"}'.");
                }
                var difficulty = selectedDifficulty.Value;
                if (difficulty == TestDifficulty.Invalid)
                {
                    throw new InvalidOperationException($"While trying to get the Value of {nameof(_selectedDifficulty)}, it contained a {nameof(TestDifficulty)} value of '{difficulty}', which is invalid for test difficulty.");
                }

                int numQuestions = int.Parse(_numberOfQuestionsDefaultValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
#if DEBUG
                numQuestions = 2;
#endif
                if (numQuestions < 1)
                {
                    numQuestions = 1;
                }
                string minimumTemperature = MinimumTemperature;
                string maximumTemperature = MaximumTemperature;
                if (!double.TryParse(minimumTemperature, NumberStyles.Float, CultureInfo.InvariantCulture, out double minTemp))
                {
                    //minTemp = double.Parse(_minimumCelsiusTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                    if (testType == TemperatureConversionDirection.FahrenheitToCelsius)
                    {
                        minTemp = double.Parse(_minimumFahrenheitTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MinimumTemperature = _minimumFahrenheitTemperatureDefaultValue;
                    }
                    else
                    {
                        minTemp = double.Parse(_minimumCelsiusTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MinimumTemperature = _minimumCelsiusTemperatureDefaultValue;
                    }
                }
                if (!double.TryParse(maximumTemperature, NumberStyles.Float, CultureInfo.InvariantCulture, out double maxTemp))
                {
                    if (testType == TemperatureConversionDirection.FahrenheitToCelsius)
                    {
                        maxTemp = double.Parse(_maximumFahrenheitTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MaximumTemperature = _maximumFahrenheitTemperatureDefaultValue;
                    }
                    else
                    {
                        maxTemp = double.Parse(_maximumCelsiusTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MaximumTemperature = _maximumCelsiusTemperatureDefaultValue;
                    }
                }
                const double spreadIfDefault = 30.0;
                if (minTemp >= maxTemp)
                {
                    if (minimumTemperature != _minimumCelsiusTemperatureDefaultValue && maximumTemperature != _maximumCelsiusTemperatureDefaultValue && Math.Abs(minTemp - maxTemp) >= 10)
                    {
                        double oldMinTemp = minTemp;
                        minTemp = maxTemp;
                        maxTemp = oldMinTemp;
                    }
                    else
                    {
                        if (minimumTemperature == _minimumCelsiusTemperatureDefaultValue)
                        {
                            double oldMaxTemp = maxTemp;
                            maxTemp += spreadIfDefault;
                            minTemp = oldMaxTemp;
                        }
                        else
                        {
                            if (maximumTemperature == _maximumCelsiusTemperatureDefaultValue)
                            {
                                double oldMinTemp = minTemp;
                                minTemp -= spreadIfDefault;
                                maxTemp = oldMinTemp;
                            }
                            else
                            {
                                if (minTemp - maxTemp >= spreadIfDefault)
                                {
                                    double oldMinTemp = minTemp;
                                    minTemp = maxTemp;
                                    maxTemp = oldMinTemp;
                                }
                                else
                                {
                                    double oldMinTemp = minTemp;
                                    minTemp = maxTemp - (spreadIfDefault / 2);
                                    maxTemp = oldMinTemp + (spreadIfDefault / 2);
                                }
                            }
                        }
                    }
                }
                bool testHalfDegree = true;
                const int numAnswers = 5;
                // random.Next(int) gives us the minimum (0) but not the maximum and we want the maximum so we add 1 to ensure that the maximum is in our results.
                int adjustedDifferencePlusOne;
                HashSet<int> existingQuestionOffsets = null;

                adjustedDifferencePlusOne = testHalfDegree
                    ? (((int)Math.Round(maxTemp - minTemp, MidpointRounding.AwayFromZero)) * 2) + 1
                    : ((int)Math.Round(maxTemp - minTemp, MidpointRounding.AwayFromZero)) + 1;

                if (adjustedDifferencePlusOne > numQuestions)
                {
                    existingQuestionOffsets = new HashSet<int>();
                }
                Random random = new Random();
                List<ITwoLineTestItem> testItems = new List<ITwoLineTestItem>(numQuestions);
                switch (testType)
                {
                    case TemperatureConversionDirection.FahrenheitToCelsius:
                        {
                            for (int i = 0; i < numQuestions; i++)
                            {
                                double testValueOffsetFromMinimum = random.Next(adjustedDifferencePlusOne);
                                if (existingQuestionOffsets != null)
                                {
                                    testValueOffsetFromMinimum = GetUniqueOffset(random, adjustedDifferencePlusOne, existingQuestionOffsets, testValueOffsetFromMinimum);
                                }
                                if (testHalfDegree)
                                {
                                    testValueOffsetFromMinimum /= 2;
                                }
                                TemperatureConversionDirection conversionDirection = TemperatureConversionDirection.FahrenheitToCelsius;
                                string questionUnits = "F";
                                string answerUnits = "C";
                                double questionValue = minTemp + testValueOffsetFromMinimum;
                                string question = $"{questionValue:F1} {questionUnits}";
                                double correctAnswerValue = MiscHelpers.ConvertTemperature(conversionDirection, questionValue);
                                double answerValueIncrement;
                                switch (difficulty)
                                {
                                    case TestDifficulty.Invalid:
                                        throw new InvalidOperationException($"Value is {nameof(TestDifficulty)}.{nameof(TestDifficulty.Invalid)}. How did we even get here? We already checked {nameof(difficulty)} to ensure it was valid.");
                                    case TestDifficulty.Easy:
                                        answerValueIncrement = 7.5;
                                        break;
                                    case TestDifficulty.Medium:
                                        answerValueIncrement = 5;
                                        break;
                                    case TestDifficulty.Hard:
                                        answerValueIncrement = 2.5;
                                        break;
                                    default:
                                        throw new InvalidOperationException($"Unknown {nameof(TestDifficulty)} enumerator value '{difficulty}'.");
                                }
                                int correctAnswerIdx = random.Next(numAnswers);
                                double startingAnswerValue = correctAnswerValue - (answerValueIncrement * correctAnswerIdx);

                                List<string> answers = new List<string>(numAnswers);

                                for (int addAnswerCtr = 0; addAnswerCtr < numAnswers; addAnswerCtr++)
                                {
                                    answers.Add($"{startingAnswerValue + (addAnswerCtr * answerValueIncrement):F1} {answerUnits}");
                                }
                                string correctAnswer = answers[correctAnswerIdx];
                                testItems.Add(new TwoLineTestItemViewModel(question, correctAnswer, answers, null, null, HostScreenWithContract, SchedulerProvider));
                            }
                            return HostScreenWithContract.Router.Navigate.Execute(new TwoLineTestViewModel(PreferencesKeys.UnitConversionsTestShowSecondLinePreferencesKey, false, null, false, testItems, HostScreenWithContract, SchedulerProvider).ToViewModelAndContract());
                        }
                    case TemperatureConversionDirection.CelsiusToFahrenheit:
                        {
                            for (int i = 0; i < numQuestions; i++)
                            {
                                double testValueOffsetFromMinimum = random.Next(adjustedDifferencePlusOne);
                                if (existingQuestionOffsets != null)
                                {
                                    testValueOffsetFromMinimum = GetUniqueOffset(random, adjustedDifferencePlusOne, existingQuestionOffsets, testValueOffsetFromMinimum);
                                }
                                if (testHalfDegree)
                                {
                                    testValueOffsetFromMinimum /= 2;
                                }
                                TemperatureConversionDirection conversionDirection = TemperatureConversionDirection.CelsiusToFahrenheit;
                                string questionUnits = "C";
                                string answerUnits = "F";
                                double questionValue = minTemp + testValueOffsetFromMinimum;
                                string question = $"{questionValue:F1} {questionUnits}";
                                double correctAnswerValue = MiscHelpers.ConvertTemperature(conversionDirection, questionValue);
                                double answerValueIncrement;
                                switch (difficulty)
                                {
                                    case TestDifficulty.Invalid:
                                        throw new InvalidOperationException($"Value is {nameof(TestDifficulty)}.{nameof(TestDifficulty.Invalid)}. How did we even get here? We already checked {nameof(difficulty)} to ensure it was valid.");
                                    case TestDifficulty.Easy:
                                        answerValueIncrement = 10;
                                        break;
                                    case TestDifficulty.Medium:
                                        answerValueIncrement = 7.5;
                                        break;
                                    case TestDifficulty.Hard:
                                        answerValueIncrement = 5;
                                        break;
                                    default:
                                        throw new InvalidOperationException($"Unknown {nameof(TestDifficulty)} enumerator value '{difficulty}'.");
                                }
                                int correctAnswerIdx = random.Next(numAnswers);
                                double startingAnswerValue = correctAnswerValue - (answerValueIncrement * correctAnswerIdx);

                                List<string> answers = new List<string>(numAnswers);

                                for (int addAnswerCtr = 0; addAnswerCtr < numAnswers; addAnswerCtr++)
                                {
                                    answers.Add($"{startingAnswerValue + (addAnswerCtr * answerValueIncrement):F1} {answerUnits}");
                                }
                                string correctAnswer = answers[correctAnswerIdx];
                                testItems.Add(new TwoLineTestItemViewModel(question, correctAnswer, answers, null, null, HostScreenWithContract, SchedulerProvider));
                            }
                            return HostScreenWithContract.Router.Navigate.Execute(new TwoLineTestViewModel(PreferencesKeys.UnitConversionsTestShowSecondLinePreferencesKey, false, null, false, testItems, HostScreenWithContract, SchedulerProvider).ToViewModelAndContract());
                        }
                    case TemperatureConversionDirection.Invalid:
                        throw new InvalidOperationException($"Value is {nameof(TemperatureConversionDirection)}.{nameof(TemperatureConversionDirection.Invalid)}. How did we even get here? We already checked {nameof(testType)} to ensure it was valid.");
                    default:
                        throw new InvalidOperationException($"Unknown enumerator value '{testType}'. How did we even get here? We already checked {nameof(testType)} to ensure it was valid.");
                }
            }
            catch (Exception ex)
            {
                DiagnosticsHelpers.ReportProblem($"Exception when trying to create and run a temperature conversion test. Details to follow.", LogLevel.Error, this.Log(), ex);
                throw;
            }
        }
    }
}
