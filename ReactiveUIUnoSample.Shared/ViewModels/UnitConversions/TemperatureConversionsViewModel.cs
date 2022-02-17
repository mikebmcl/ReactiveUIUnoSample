
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

namespace ReactiveUIUnoSample.ViewModels.UnitConversions
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TemperatureConversionsViewModel : TemperatureConversionsViewModelBase//, IUnitConversionsTesting
    {
        public TemperatureConversionsViewModel(IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            RunTestCommand = ReactiveCommand.CreateFromObservable(() =>
            RunTestCommandExecute()
            , this.WhenAnyValue(
                    x => x.SelectedTestType,
                    x => x.SelectedDifficulty,
                    (testType, difficulty) =>
                    testType != null &&
                    difficulty != null
                    ).ObserveOn(SchedulerProvider.MainThread)
                    , SchedulerProvider.MainThread
                    );
            TempEntryOneText = "0";
            TempPickerTitle = "Temperature";
            SelectedTemperatureConversion = ConversionDirections[0];
            NavigateToFirstViewCommand = ReactiveCommand.CreateFromObservable(() => HostScreenWithContract.Router.Navigate.Execute(new FirstViewModel(HostScreenWithContract, SchedulerProvider).ToViewModelAndContract()));
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
                            return m_errorValue;
                    }
                    if (convertedValue == double.PositiveInfinity)
                    {
                        return m_errorValue;
                    }
                    return convertedValue.ToString("F1", System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    return m_errorValue;
                }

            }).ToProperty(this, x => x.TempEntryTwoText, out _tempEntryTwoText);
        }

        // Number of Questions

        private const string m_numberOfQuestionsDefaultValue = "10";
        [Reactive]
        public string NumberOfQuestions { get; set; }

        // Temperature

        // These are arbitrary but are close to equal (-20F is -28.88889C)
        private const string m_minimumCelsiusTemperatureDefaultValue = "-30";
        private const string m_minimumFahrenheitTemperatureDefaultValue = "-20";
        [Reactive]
        public string MinimumTemperature { get; set; }

        // These are arbitrary but happen to be equal.
        private const string m_maximumCelsiusTemperatureDefaultValue = "60";
        private const string m_maximumFahrenheitTemperatureDefaultValue = "140";

        [Reactive]
        public string MaximumTemperature { get; set; }

        //private object m_selectedTestType;
        [Reactive]
        public object SelectedTestType { get; set; }

        public IList<TemperatureConversionDirectionValueDisplayPair> TestTypes => new List<TemperatureConversionDirectionValueDisplayPair>(new TemperatureConversionDirectionValueDisplayPair[]
        {
            new TemperatureConversionDirectionValueDisplayPair(TemperatureConversionDirection.CelsiusToFahrenheit, m_celsiusToFahrenheit)
            , new TemperatureConversionDirectionValueDisplayPair(TemperatureConversionDirection.FahrenheitToCelsius, m_fahrenheitToCelsius)
        });

        //private object m_selectedDifficulty;
        [Reactive]
        public object SelectedDifficulty { get; set; }

        private string m_errorValue = "(Error)";

        [Reactive]
        public object SelectedTemperatureConversion { get; set; }

        [Reactive]
        public string TempEntryOneText { get; set; }

        private readonly ObservableAsPropertyHelper<string> _tempEntryTwoText;
        public string TempEntryTwoText => _tempEntryTwoText.Value;

        [Reactive]
        public string TempPickerTitle { get; set; }

        private string m_title = "Temperature Conversions!";
        public string Title
        {
            get => m_title; set { if (m_title != value) { m_title = value; RaisePropertyChanged(); } }
        }

        public override object HeaderContent { get; set; } = "Temperature Conversions";

        public System.Windows.Input.ICommand NavigateToFirstViewCommand { get; set; }

        public ReactiveCommand<Unit, IViewModelAndContract> RunTestCommand { get; set; }
        private readonly AtomicBoolean m_runTestCommandIsExecuting = new AtomicBoolean();
        private IObservable<IViewModelAndContract> RunTestCommandExecute()
        {
            try
            {
                if (m_runTestCommandIsExecuting.Set(true))
                {
                    throw new InvalidOperationException($"Navigation is already occurring.");
                }
                var m_selectedTestType = SelectedTestType;
                if (!(m_selectedTestType is TemperatureConversionDirectionValueDisplayPair selectedTestType))
                {
                    throw new InvalidCastException($"Expected {nameof(m_selectedTestType)} to be of type {nameof(TemperatureConversionDirectionValueDisplayPair)} but instead it is of type '{m_selectedTestType?.GetType().FullName ?? "(null)"}'.");
                }
                var testType = selectedTestType.Value;
                if (testType == TemperatureConversionDirection.Invalid)
                {
                    throw new InvalidOperationException($"While trying to get the Value of {nameof(m_selectedTestType)}, it contained a {nameof(TemperatureConversionDirection)} value of '{testType}', which is invalid for test type.");
                }

                var m_selectedDifficulty = SelectedDifficulty;
                if (!(m_selectedDifficulty is TestDifficultyValueDisplayPair selectedDifficulty))
                {
                    throw new InvalidCastException($"Expected {nameof(m_selectedDifficulty)} to be of type {nameof(TestDifficultyValueDisplayPair)} but instead it is of type '{m_selectedDifficulty?.GetType().FullName ?? "(null)"}'.");
                }
                var difficulty = selectedDifficulty.Value;
                if (difficulty == TestDifficulty.Invalid)
                {
                    throw new InvalidOperationException($"While trying to get the Value of {nameof(m_selectedDifficulty)}, it contained a {nameof(TestDifficulty)} value of '{difficulty}', which is invalid for test difficulty.");
                }

                int numQuestions = int.Parse(m_numberOfQuestionsDefaultValue, NumberStyles.Integer, CultureInfo.InvariantCulture);
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
                    //minTemp = double.Parse(m_minimumCelsiusTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                    if (testType == TemperatureConversionDirection.FahrenheitToCelsius)
                    {
                        minTemp = double.Parse(m_minimumFahrenheitTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MinimumTemperature = m_minimumFahrenheitTemperatureDefaultValue;
                    }
                    else
                    {
                        minTemp = double.Parse(m_minimumCelsiusTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MinimumTemperature = m_minimumCelsiusTemperatureDefaultValue;
                    }
                }
                if (!double.TryParse(maximumTemperature, NumberStyles.Float, CultureInfo.InvariantCulture, out double maxTemp))
                {
                    if (testType == TemperatureConversionDirection.FahrenheitToCelsius)
                    {
                        maxTemp = double.Parse(m_maximumFahrenheitTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MaximumTemperature = m_maximumFahrenheitTemperatureDefaultValue;
                    }
                    else
                    {
                        maxTemp = double.Parse(m_maximumCelsiusTemperatureDefaultValue, NumberStyles.Float, CultureInfo.InvariantCulture);
                        MaximumTemperature = m_maximumCelsiusTemperatureDefaultValue;
                    }
                }
                const double spreadIfDefault = 30.0;
                if (minTemp >= maxTemp)
                {
                    if (minimumTemperature != m_minimumCelsiusTemperatureDefaultValue && maximumTemperature != m_maximumCelsiusTemperatureDefaultValue && Math.Abs(minTemp - maxTemp) >= 10)
                    {
                        double oldMinTemp = minTemp;
                        minTemp = maxTemp;
                        maxTemp = oldMinTemp;
                    }
                    else
                    {
                        if (minimumTemperature == m_minimumCelsiusTemperatureDefaultValue)
                        {
                            double oldMaxTemp = maxTemp;
                            maxTemp += spreadIfDefault;
                            minTemp = oldMaxTemp;
                        }
                        else
                        {
                            if (maximumTemperature == m_maximumCelsiusTemperatureDefaultValue)
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
            finally
            {
                m_runTestCommandIsExecuting.ForceToFalse();
            }
        }
    }
}
