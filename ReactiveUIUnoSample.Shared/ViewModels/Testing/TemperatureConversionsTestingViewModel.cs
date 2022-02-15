
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
using System.Threading.Tasks;
using ReactiveUIRoutingWithContracts;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TemperatureConversionsTestingViewModel : UnitConversionsViewModelBase//, IUnitConversionsTesting
    {

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

        public override object HeaderContent { get; set; }

        public ICommand RunTestCommand { get; set; }
        private readonly AtomicBoolean m_runTestCommandIsExecuting = new AtomicBoolean();
        /// <summary>
        /// This exists solely as a component of correctly determining the appropriate return value of RunTestCommand's CanExecute method. There is
        /// no guarantee that it is actually up-to-date and should never be modified except by the event handler for m_runTestCommandIsExecuting's
        /// ValueChanged event. It need to be marked with ReactiveAttribute or to otherwise implement INotifyPropertyChanged or one of the other mechanisms
        /// that ensures that ReactiveUI's WhenAny extension methods will be informed that its value changed.
        /// </summary>
        [Reactive]
        private bool RunTestCommandIsExecutingValue { get; set; }
        private void RunTestCommandIsExecutingValueChangedHandler(object sender, EventArgs args)
        {
            // We're using this as part of calculating the proper value of the RunTestCommand's CanExecute method.
            RunTestCommandIsExecutingValue = m_runTestCommandIsExecuting.Get();
        }
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

        private bool RunTestCommandCanExecuteSelector(IObservedChange<TemperatureConversionsTestingViewModel, object> testType, IObservedChange<TemperatureConversionsTestingViewModel, object> difficultyObj, IObservedChange<TemperatureConversionsTestingViewModel, bool> runTestCommandIsExecutingValue)
        {
            var isRunningValue = runTestCommandIsExecutingValue?.GetValue();
            return isRunningValue is false &&
                    testType?.GetValue() != null &&
                    difficultyObj?.GetValue() != null;
        }
        private IObservable<bool> RunTestCommandCanExecute()
        {
            return this.WhenAny(
                    x => x.SelectedTestType,
                    x => x.SelectedDifficulty,
                    x => x.RunTestCommandIsExecutingValue,
                    RunTestCommandCanExecuteSelector
                    //(testType, difficultyObj, runTestCommandIsExecutingValue) =>
                    //runTestCommandIsExecutingValue?.Value is false &&
                    //testType?.Value != null &&
                    //difficultyObj?.Value != null
                    /*&& difficultyObj.Value is ValueDisplayGenericPair<TestDifficulty> difficulty && difficulty.Value != TestDifficulty.Invalid*/
                    );
        }
        public TemperatureConversionsTestingViewModel(IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            // Note: It's safe to not unsubscribe from this event because it does not hold a hard reference to this object, it's subscribing to an event on an object
            // that is a non-static member of this class, and we have no reasonable way to 100% guarantee that our attempt to unsubscribe would always run (because certain
            // platforms cannot guarantee that custom back handlers will always run) barring creating some really horrible code that would involve other classes.
            m_runTestCommandIsExecuting.ValueChanged += RunTestCommandIsExecutingValueChangedHandler;

            RunTestCommand = ReactiveCommand.CreateFromObservable(() =>
            RunTestCommandExecute()
            , RunTestCommandCanExecute().ObserveOn(SchedulerProvider.MainThread)
                    , SchedulerProvider.MainThread
                    );
        }
    }
}
