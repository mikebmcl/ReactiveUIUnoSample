using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.Interfaces.Testing;
using ReactiveUIUnoSample.ViewModels;
using ReactiveUIRoutingWithContracts;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Microsoft.Extensions.Logging;
using Uno.Extensions;
using Windows.Storage;
using System.Reactive.Linq;

//using Windows.UI.Xaml.Controls;
//using Windows.UI.Xaml.Media;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TwoLineTestViewModel : DisplayViewModelBase, ITwoLineTest, ICallOnBackNavigation
    {
        //private string m_title = "";
        //public override string Title { get => m_title; set { if (m_title != value) { m_title = value; RaisePropertyChanged(); } } }

        //private void TestItemChangedEventHandler(object sender, PropertyChangedEventArgs args)
        //{
        //    if (args.PropertyName == nameof(ITwoLineTestItem.SelectedItem))
        //    {
        //        ChangeCommandCanExecute(CheckAndNextButtonCommand);
        //    }
        //}

        private readonly Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object> m_confirmLeavePage;
        public Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object> ConfirmLeavePage => m_confirmLeavePage;
        /// <summary>
        /// Note: This is bound to the IsChecked property of a <see cref="Windows.UI.Xaml.Controls.CheckBox"/> as an example of
        /// a task on a page being completed such that navigating back from it can be safely done without prompting the user.
        /// A more realistic scenario would be the user navigating to a new page that displays results or continues with data entry. 
        /// In that case you would want to make sure that when navigating back, they do not get stopped and prompted if they want to 
        /// leave when reaching this page and should also consider setting it up so that when navigating back to this page from the 
        /// page they moved to that they are automatically navigated back to the page the page they originally came from
        /// since they presumably completed everything related to this page. Essentially, make sure that when the user is finished
        /// with this page that they aren't stopped when coming back here, unless they are coming back via a navigation that is
        /// meant to allow them to correct some data they entered on this page or do something else here.
        /// </summary>
        [Reactive]
        public bool? SkipConfirmLeave { get; set; }

        private async Task FinishCallOnNavigateBack(bool navigateBack)
        {
            if (navigateBack)
            {
                SkipConfirmLeave = true;
                await HostScreenWithContract.Router.NavigateBack.Execute();
            }
        }
        private readonly AtomicBoolean m_isNavigating = new AtomicBoolean();
        public bool CallOnBackNavigation()
        {
            if (SkipConfirmLeave is true || m_confirmLeavePage is null)
            {
                return true;
            }
            if (!m_isNavigating.Set(true))
            {
                // Note: Without the call to Subscribe at the end, this code will never execute.
                // Note: You don't need to worry about this leaking despite Subscribe() returning an IDisposable. The WhenActivated
                //  in the SecondView ctor ensures that everything is disconnected so that the GC will take care of it. Also, Android
                //  will not dismiss the dialog properly if you try to get a reference to this and dispose it from here.
                m_confirmLeavePage.Handle((Title: "Confirm Quit", Text: "Are you sure you want to leave before the test is finished?", Stay: "Stay", Leave: "Leave", FinishInteraction: FinishCallOnNavigateBack, IsNavigating: m_isNavigating)).ObserveOn(SchedulerProvider.MainThread).Subscribe();
            }
            return false;
        }

        public static string UpdateVocabReadingsProgressMessage { get; } = nameof(UpdateVocabReadingsProgressMessage);
        private const int m_numberOfCorrectnessTrackingDataItemsToKeep = 10;
        //public async void UpdateProgress()
        //{
        //    bool userIsCorrect = m_currentTestItem.UserAnswerIsCorrect();
        //    if (CurrentTestItem.TestingProgress == null)
        //    {
        //        return;
        //    }
        //    CurrentTestItem.TestingProgress.NumberOfTimesCorrect += userIsCorrect ? 1 : 0;
        //    CurrentTestItem.TestingProgress.NumberOfTimesTested += 1;
        //    Queue<bool> updateCorrectnessTrackingData = new Queue<bool>(CurrentTestItem.TestingProgress.CorrectnessTrackingData);
        //    updateCorrectnessTrackingData.Enqueue(userIsCorrect);
        //    if (updateCorrectnessTrackingData.Count > m_numberOfCorrectnessTrackingDataItemsToKeep)
        //    {
        //        _ = updateCorrectnessTrackingData.Dequeue();
        //    }
        //    CurrentTestItem.TestingProgress.CorrectnessTrackingData = updateCorrectnessTrackingData;
        //    CurrentTestItem.TestingProgress.LastTestedDate = DateTime.UtcNow;
        //    await m_currentTestItem.TestingProgress.UpdateDBEntry();
        //}

        private bool m_hasSecondLine;
        public bool HasSecondLine
        {
            get => m_hasSecondLine;
            set
            {
                if (m_hasSecondLine != value)
                {
                    m_hasSecondLine = value;
                    RaisePropertyChanged();
                }
            }
        }

        [Reactive]
        public bool ShowSecondLine { get; set; }
        public string ShowSecondLinePrompt { get; set; }

        //private ITwoLineTestItem m_currentTestItem = default;
        [Reactive]
        public ITwoLineTestItem CurrentTestItem { get; set; }
        //{
        //    get => m_currentTestItem;
        //    set
        //    {
        //        if (m_currentTestItem != value)
        //        {
        //            //if (m_currentTestItem != null)
        //            //{
        //            //    m_currentTestItem.PropertyChanged -= TestItemChangedEventHandler;
        //            //}
        //            //m_currentTestItem = value;
        //            //if (m_currentTestItem != null)
        //            //{
        //            //    m_currentTestItem.PropertyChanged += TestItemChangedEventHandler;
        //            //}
        //            RaisePropertyChanged();
        //            RaisePropertyChanged(nameof(ShowSecondLine));
        //        }
        //    }
        //}

        [Reactive]
        public List<ITwoLineTestItem> TestItems { get; set; }
        public List<ITwoLineTestItem> UserWasCorrect { get; set; } = new List<ITwoLineTestItem>();
        public List<ITwoLineTestWrongAnswer> UserWasWrong { get; set; } = new List<ITwoLineTestWrongAnswer>();

        private int m_currentTestItemIndex = -1;
        public int CurrentTestItemIndex
        {
            get => m_currentTestItemIndex;
            set
            {
                if (m_currentTestItemIndex != value)
                {
                    m_currentTestItemIndex = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(CurrentTestItemAsTitleString));
                }
            }
        }

        public string CurrentTestItemAsTitleString => "Item " + (m_currentTestItemIndex + 1).ToString() + " out of " + TestItemCount.ToString();

        public int TestItemCount => TestItems.Count;

        // This is used to let us know that the test is ready for the user since we'll have some async stuff that needs to complete before the test is ready.

        [Reactive]
        public bool TestIsReady { get; set; }

        public static string CheckText { get; } = "Check";
        public static string NextText { get; } = "Next";
        public static string FinishText { get; } = "Finish";

        ////[Reactive]
        //public string CheckAnswerText => CheckText;
        [Reactive]
        public string ResultText { get; set; }

        [Reactive]
        public string CheckAnswerButtonText { get; set; }
        [Reactive]
        public string NextFinishButtonTest { get; set; }

        public ICommand CheckAnswerCommand { get; set; }
        public ICommand NextFinishCommand { get; set; }

        private const string m_resultTextEmpty = " ";
        private const string m_resultTextCorrect = "Correct!";
        private const string m_resultTextWrongBeginning = "Sorry";
        private const string m_resultTextWrongEnd = "was the wrong answer. See above for the correct answer.";

        private string m_disableOneWrongAnswerText = "Remove a Wrong Answer"; // Give Me A Hint
        public string DisableOneWrongAnswerText
        {
            get => m_disableOneWrongAnswerText;
            set
            {
                if (m_disableOneWrongAnswerText != value)
                {
                    m_disableOneWrongAnswerText = value;
                    RaisePropertyChanged();
                }
            }
        }

        //private bool m_canDisableOneWrongAnswer = true;
        //public bool CanDisableOneWrongAnswer
        //{
        //    get => m_canDisableOneWrongAnswer;
        //    set
        //    {
        //        if (m_canDisableOneWrongAnswer != value)
        //        {
        //            m_canDisableOneWrongAnswer = value;
        //            RaisePropertyChanged();
        //        }
        //    }
        //}

        [Reactive]
        public bool? CheckedAnswerIsCorrect { get; set; }

        private bool CheckCommandCanExecute()
        {
            return CurrentTestItem != null && CurrentTestItem.Answers.Any(tsta => tsta.IsSelected);
        }
        private void CheckCommandExecute()
        {
            //CanDisableOneWrongAnswer = false;
            // Perform validation, show user correct answer, update Progress entry to note if they were right or wrong and update its last tested date.

            bool userIsCorrect = CurrentTestItem.UserAnswerIsCorrect();

            // Need to update progress before checking if the user is correct otherwise the answer is changed to the correct answer so it marks the user correct internally.
            //UpdateProgress();

            if (userIsCorrect)
            {
                UserWasCorrect.Add(CurrentTestItem);
                ResultText = m_resultTextCorrect;
                CheckedAnswerIsCorrect = true;
                //CheckResultBGColor = m_resultCorrectBGColor;
                //CheckResultTextColor = m_resultCorrectTextColor;
            }
            else
            {
                string userAnswer = CurrentTestItem.UserAnswer();
                UserWasWrong.Add(new TwoLineTestWrongAnswer(CurrentTestItem, userAnswer));
                ResultText = $"{m_resultTextWrongBeginning} '{userAnswer}' {m_resultTextWrongEnd}";
                CheckedAnswerIsCorrect = false;
                //IThreeStateTestAnswer correctItem = CurrentTestItem.Answers.First((item) => item.Text == CurrentTestItem.CorrectAnswer.Text);
                //CurrentTestItem.SelectedItem = correctItem;
                CurrentTestItem.CorrectAnswer.PressCommand.Execute(null);
                //CheckResultBGColor = m_resultWrongBGColor;
                //CheckResultTextColor = m_resultWrongTextColor;
            }

            if (CurrentTestItemIndex + 1 == TestItems.Count)
            {
                NextFinishButtonTest = FinishText;
                //CheckAndNextButtonCommand = new CommandHandler(FinishCommandExecute);
            }
            //else
            //{
            //    CheckAndNextButtonText = NextText;
            //    //CheckAndNextButtonCommand = new CommandHandler(NextCommandExecute);
            //}
        }

        private async Task NextCommandExecute()
        {
            ResultText = m_resultTextEmpty;
            CheckedAnswerIsCorrect = null;
            //CheckResultBGColor = m_resultEmptyBGColor;
            //CheckResultTextColor = m_resultEmptyTextColor;

            if (CurrentTestItemIndex + 1 == TestItems.Count)
            {
                await FinishCommandExecuteInternal();
                return;
            }
            CurrentTestItemIndex++;
            CurrentTestItem = TestItems[CurrentTestItemIndex];
            EnabledAnswersCount = CurrentTestItem.Answers.Count;
            //NextFinishButtonTest = FinishText;
            //CheckAndNextButtonText = CheckText;
            //CanDisableOneWrongAnswer = true;
        }

        //public readonly WeakReference<Windows.UI.Xaml.Controls.ListView> AnswersListViewWeakRef = new WeakReference<Windows.UI.Xaml.Controls.ListView>(null);

        private async Task FinishCommandExecuteInternal()
        {
            SkipConfirmLeave = true;
            if (UserWasCorrect.Count == 0 && UserWasWrong.Count == 0)
            {
                await HostScreenWithContract.Router.NavigateBack.Execute();
                return;
            }
            // We're done and the user clicked to go to the results so zero this out.
            // Navigate to test results page
            int takeCount;
            if (string.IsNullOrEmpty(CurrentTestItem.UserAnswer()))
            {
                takeCount = CurrentTestItemIndex;
            }
            else
            {
                takeCount = CurrentTestItemIndex + 1;
            }
            await HostScreenWithContract.Router.NavigateAndRemoveCurrent().Execute(new TwoLineTestResultsViewModel("Test Results", UserWasCorrect, UserWasWrong, new List<ITwoLineTestItem>(TestItems.Take(takeCount)), HostScreenWithContract, SchedulerProvider).ToViewModelAndContract());
        }

        [Reactive]
        private int EnabledAnswersCount { get; set; }

        /// <summary>
        /// If there are at more than this many enabled answers, <see cref="DisableOneWrongAnswerCommand"/> can execute.
        /// </summary>
        private const int _canDisableAnswersAboveCount = 2;
        public ICommand DisableOneWrongAnswerCommand { get; set; }
        protected void DisableOneWrongAnswerCommandExecute()
        {
            if (CurrentTestItem == null)
            {
                DiagnosticsHelpers.ReportProblem($"Unexpected null {nameof(CurrentTestItem)}.", LogLevel.Debug, this.Log());
                return;
            }
            Random random = new Random();
            int count = CurrentTestItem.Answers.Count(tsta => tsta.IsEnabled) - 1; // - 1 because we don't want to disable the correct one
            count = random.Next(count);
            for (int i = 0; i < count + 1; i++)
            {
                while (!CurrentTestItem.Answers[i].IsEnabled || CurrentTestItem.Answers[i] == CurrentTestItem.CorrectAnswer)
                {
                    // Skip the correct answer and the already disabled answers; reflect that we skipped by incrementing i and count since i and count were a correct or disabled answer
                    i++;
                    count++;
                }
                if (i == count)
                {
                    IThreeStateTestAnswer answer = CurrentTestItem.Answers[i];
                    answer.IsEnabled = false;
                    EnabledAnswersCount -= 1;
                    //answer.ButtonState = ThreeStateButtonState.Disabled;
                    if (answer.IsSelected)
                    {
                        answer.IsSelected = false;
                        CurrentTestItem.SelectedItem = null;
                    }
                    //if (AnswersListViewWeakRef.TryGetTarget(out Windows.UI.Xaml.Controls.ListView listView))
                    //{
                    //    Windows.UI.Xaml.DependencyObject container = listView.ContainerFromIndex(i);
                    //    if (container is Windows.UI.Xaml.Controls.ListViewItem viewItem)
                    //    {
                    //        viewItem.IsHitTestVisible = false;
                    //        viewItem.IsEnabled = false;
                    //    }
                    //}
                    break;
                }
            }
        }

        public string Title { get; set; }
        public override object HeaderContent { get; set; }

        private readonly string _secondLineSettingsKey;

        public TwoLineTestViewModel(string showSecondLinePreferencesKey, bool showSecondLineDefault, string showSecondLinePrompt, bool hasSecondLine, IList<ITwoLineTestItem> testItems, IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false, [System.Runtime.CompilerServices.CallerMemberName] string callerMemberName = null, [System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = null, [System.Runtime.CompilerServices.CallerLineNumber] int callerLineNumber = 0) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            m_confirmLeavePage = new Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object>(schedulerProvider.CurrentThread);
            HeaderContent = "First Page";
            //NextPageCommand = ReactiveCommand.CreateFromObservable(() =>
            //{
            //    // Set the correct contract name to ensure we get SecondView. This must be done before navigation whenever a view is registered with a contract string such as the views that use SecondViewModel..
            //    HostScreenWithContract.Contract = SecondViewModel.SecondViewContractName;
            //    return HostScreen.Router.Navigate.Execute(new SecondViewModel(HostScreenWithContract, SchedulerProvider, () => new ContentControl() { Content = new TextBlock() { Text = "Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } }));
            //});

            TestItems = new List<ITwoLineTestItem>(testItems);
            //CheckAndNextButtonText = CheckText;
            ResultText = m_resultTextEmpty;
            CheckAnswerButtonText = CheckText;
            NextFinishButtonTest = NextText;
            CurrentTestItemIndex = 0;
            CheckAnswerCommand = ReactiveCommand.Create(CheckCommandExecute, this.WhenAnyValue(x => x.CurrentTestItem, x => x.CurrentTestItem.SelectedItem, x => x.CheckedAnswerIsCorrect, (item, selectedAnswer, checkedIsCorrect) => item != null && selectedAnswer != null && selectedAnswer?.IsEnabled is true && checkedIsCorrect == null).ObserveOn(SchedulerProvider.MainThread));
            // Need to explicitly specify the generics for WhenAnyValue here because of an ambiguity issue between overloads
            NextFinishCommand = ReactiveCommand.Create(NextCommandExecute, this.WhenAnyValue<TwoLineTestViewModel, bool, bool?>(x => x.CheckedAnswerIsCorrect, (checkedIsCorrect) => checkedIsCorrect != null).ObserveOn(SchedulerProvider.MainThread));

            HasSecondLine = hasSecondLine;
            if (hasSecondLine)
            {
                if (!string.IsNullOrWhiteSpace(showSecondLinePreferencesKey))
                {
                    _secondLineSettingsKey = showSecondLinePreferencesKey;
                    var values = ApplicationData.Current.LocalSettings.Values;
                    if (!values.ContainsKey(showSecondLinePreferencesKey))
                    {
                        values.Add(showSecondLinePreferencesKey, showSecondLineDefault);
                        ShowSecondLine = showSecondLineDefault && HasSecondLine;
                    }
                    else
                    {
                        if (values[showSecondLinePreferencesKey] is bool showSecondLineValue)
                        {
                            ShowSecondLine = showSecondLineValue && HasSecondLine;
                        }
                        else
                        {
                            if (callerMemberName == null)
                            {
                                callerMemberName = string.Empty;
                            }
                            if (callerFilePath == null)
                            {
                                callerMemberName = string.Empty;
                            }
                            // Because this is a generalized interface for a lot of potential different test types, we should show the called of the constructor in order to
                            // give a better idea of where this bad key string was sent from.
                            DiagnosticsHelpers.ReportProblem($"Expected local setting with key '{showSecondLinePreferencesKey}' to be of type bool but instead it is of type '{values[showSecondLinePreferencesKey].GetType().FullName}'. Using default.", LogLevel.Warning, this.Log(), callerMemberName: callerMemberName, callerFilePath: callerFilePath, callerLineNumber: callerLineNumber);
                            ShowSecondLine = showSecondLineDefault && HasSecondLine;
                        }
                    }
                    if (string.IsNullOrWhiteSpace(showSecondLinePrompt))
                    {
                        showSecondLinePrompt = "Show additional question information?";
                    }
                    ShowSecondLinePrompt = showSecondLinePrompt;
                }
            }

            CurrentTestItem = TestItems[CurrentTestItemIndex];
            EnabledAnswersCount = CurrentTestItem.Answers.Count;
            DisableOneWrongAnswerCommand = ReactiveCommand.Create(DisableOneWrongAnswerCommandExecute, this.WhenAnyValue(x => x.EnabledAnswersCount, (currentlyEnabledCount) => currentlyEnabledCount > _canDisableAnswersAboveCount).ObserveOn(SchedulerProvider.MainThread));

            TestIsReady = true;
        }
    }
}
