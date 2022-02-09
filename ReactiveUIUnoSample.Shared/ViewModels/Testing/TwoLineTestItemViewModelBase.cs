using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public abstract class TwoLineTestItemViewModelBase : ViewModelBase, ITwoLineTestItem
    {
        [Reactive]
        public string FirstLine { get; set; }

        [Reactive]
        public string SecondLine { get; set; }

        public string UserAnswer()
        {
            return !(SelectedItem is IThreeStateTestAnswer userAnswer) ? string.Empty : userAnswer.Text ?? string.Empty;
        }
        public bool UserAnswerIsCorrect()
        {
            return SelectedItem.Text == CorrectAnswer.Text;
        }

        private IThreeStateTestAnswer m_selectedItem = null;
        public IThreeStateTestAnswer SelectedItem
        {
            get => m_selectedItem;
            set
            {
                if (m_selectedItem != value)
                {
                    if (m_selectedItem != null)
                    {
                        m_selectedItem.IsSelected = false;
                    }
                    if (value != null)
                    {
                        value.IsSelected = true;
                    }
                    m_selectedItem = value;
                    RaisePropertyChanged();
                }
            }
        }

        protected bool SetAnswerStateCanExecute(string answer)
        {
            return Answers?.FirstOrDefault(item => item.Text == answer).IsEnabled ?? false;
        }
        protected void SetAnswerStateExecute(string answer)
        {
            IList<IThreeStateTestAnswer> localAnswers = Answers;
            if (localAnswers == null)
            {
                return;
            }
            IThreeStateTestAnswer selected = null;
            foreach (IThreeStateTestAnswer tsta in localAnswers)
            {
                if (tsta.Text == answer && tsta.IsEnabled)
                {
                    selected = tsta;
                    tsta.IsSelected = true;
                    //tsta.ButtonState = ThreeStateButtonState.Selected;
                    //ChangeCommandCanExecute(tsta.PressCommand);
                }
                else
                {
                    tsta.IsSelected = false;
                    if (!tsta.IsEnabled)
                    {
                        //tsta.ButtonState = ThreeStateButtonState.Enabled;
                        tsta.IsEnabled = true;
                    }
                    //ChangeCommandCanExecute(tsta.PressCommand);
                }
            }
            SelectedItem = selected;
        }

        private IList<IThreeStateTestAnswer> m_answers = new List<IThreeStateTestAnswer>();
        public IList<IThreeStateTestAnswer> Answers
        {
            get => m_answers;
            set
            {
                if (m_answers != value)
                {
                    m_answers = value;
                    RaisePropertyChanged();
                }
            }
        }

        public IThreeStateTestAnswer CorrectAnswer { get; set; }

        public ICommand DisableOneWrongAnswerCommand { get; set; }
        protected bool DisableOneWrongAnswerCommandCanExecute(object obj)
        {
            return Answers.Count(tsta => tsta.IsEnabled) > 2;
        }
        protected void DisableOneWrongAnswerCommandExecute(object obj)
        {
            Random random = new Random();
            int count = Answers.Count(tsta => tsta.IsEnabled) - 1; // - 1 because we don't want to disable the correct one
            count = random.Next(count);
            for (int i = 0; i < count + 1; i++)
            {
                while (!Answers[i].IsEnabled || Answers[i] == CorrectAnswer)
                {
                    // Skip the correct answer and the already disabled answers; reflect that we skipped by incrementing i and count since i and count were a correct or disabled answer
                    i++;
                    count++;
                }
                if (i == count)
                {
                    IThreeStateTestAnswer answer = Answers[i];
                    answer.IsEnabled = false;
                    //answer.ButtonState = ThreeStateButtonState.Disabled;
                    if (answer.IsSelected)
                    {
                        answer.IsSelected = false;
                        SelectedItem = null;
                    }
                    if (obj is ListView listView)
                    {
                        DependencyObject container = listView.ContainerFromIndex(i);
                        if (container is ListViewItem viewItem)
                        {
                            viewItem.IsHitTestVisible = false;
                            viewItem.IsEnabled = false;
                        }
                    }
                    break;
                }
            }
            //ChangeCommandCanExecute(DisableOneWrongAnswerCommand);
        }

        public FrameworkElement CorrectAnswerFrameworkElement { get; }
        public ICommand ViewCorrectAnswerFrameworkElementCommand { get; set; }
        public bool HasCorrectAnswerFrameworkElement => CorrectAnswerFrameworkElement != null;
        public bool NoCorrectAnswerFrameworkElement => CorrectAnswerFrameworkElement == null;

        private bool ViewCorrectAnswerFrameworkElementCommandCanExecute()
        {
            return CorrectAnswerFrameworkElement != null;
        }
        private async void ViewCorrectAnswerFrameworkElementCommandExecute()
        {
            //await RootPage.Current.PushContent(CorrectAnswerFrameworkElement);
            //if (CorrectAnswerFrameworkElement is ScrollViewer scrollViewer)
            //{
            //    Views.ContentViewPresenterContentPage page = new Views.ContentViewPresenterContentPage(CorrectAnswerFrameworkElement, FirstLine);
            //    await App.Navigation.PushAsync(page);
            //}
            //else
            //{
            //    Views.ContentViewPresenterContentPage page = new Views.ContentViewPresenterContentPage(new Views.EmbedContentViewInScrollViewContentView(CorrectAnswerFrameworkElement), FirstLine);
            //    await App.Navigation.PushAsync(page);
            //}
        }

        protected TwoLineTestItemViewModelBase(string question, string correctAnswer, IEnumerable<string> answers, FrameworkElement correctAnswerContentView, string secondLine, IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            FirstLine = question;
            SecondLine = secondLine;
            //foreach (string item in answers)
            //{
            //    Answers.Add(new ButtonViewModel(item, new CommandHandler(() => SetAnswerStateExecute(item), () => SetAnswerStateCanExecute(item))));
            //}
            CorrectAnswer = Answers.First(tsta => tsta.Text == correctAnswer);
            //DisableOneWrongAnswerCommand = new CommandHandler(DisableOneWrongAnswerCommandExecute, DisableOneWrongAnswerCommandCanExecute);
            CorrectAnswerFrameworkElement = correctAnswerContentView;
            //TestingProgress = testingProgress;
            RaisePropertyChanged(nameof(HasCorrectAnswerFrameworkElement));
            RaisePropertyChanged(nameof(NoCorrectAnswerFrameworkElement));
            //IsKana = isKana;
            //ViewCorrectAnswerFrameworkElementCommand = new CommandHandler(ViewCorrectAnswerFrameworkElementCommandExecute, ViewCorrectAnswerFrameworkElementCommandCanExecute);
        }
    }
}
