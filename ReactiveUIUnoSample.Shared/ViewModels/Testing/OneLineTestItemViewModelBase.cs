using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces.Testing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Windows.Input;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ReactiveUIRoutingWithContracts;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public abstract class OneLineTestItemViewModelBase : ViewModelBase, IOneLineTestItem
    {
        [Reactive]
        public string FirstLine { get; set; }

        public string UserAnswer()
        {
            return !(SelectedItem is IThreeStateTestAnswer userAnswer) ? string.Empty : userAnswer.Text ?? string.Empty;
        }
        public bool UserAnswerIsCorrect()
        {
            return SelectedItem.Text == CorrectAnswer.Text;
        }

        [Reactive]
        public IThreeStateTestAnswer SelectedItem { get; set; }

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
                }
                else
                {
                    tsta.IsSelected = false;
                    if (!tsta.IsEnabled)
                    {
                        tsta.IsEnabled = true;
                    }
                }
            }
            if (SelectedItem != null)
            {
                SelectedItem.IsSelected = false;
            }
            if (selected != null)
            {
                selected.IsSelected = true;
            }
            SelectedItem = selected;
        }

        [Reactive]
        public IList<IThreeStateTestAnswer> Answers { get; set; }

        public IThreeStateTestAnswer CorrectAnswer { get; set; }

        public FrameworkElement CorrectAnswerFrameworkElement { get; }
        public ICommand ViewCorrectAnswerFrameworkElementCommand { get; set; }
        public bool HasCorrectAnswerFrameworkElement => CorrectAnswerFrameworkElement != null;
        public bool NoCorrectAnswerFrameworkElement => CorrectAnswerFrameworkElement == null;

        protected OneLineTestItemViewModelBase(string question, string correctAnswer, IEnumerable<string> answers, FrameworkElement correctAnswerContentView, IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            FirstLine = question;
            int enabledAnswersCount = 0;
            Answers = new List<IThreeStateTestAnswer>();
            foreach (string item in answers)
            {
                enabledAnswersCount++;
                Answers.Add(new ButtonViewModel(item, ReactiveCommand.Create<string>(SetAnswerStateExecute, this.WhenAny(x => x.Answers, (an) => an?.Value.FirstOrDefault(v => v.Text == item)?.IsEnabled is true).ObserveOn(SchedulerProvider.MainThread))));
            }
            CorrectAnswer = Answers.First(tsta => tsta.Text == correctAnswer);

            CorrectAnswerFrameworkElement = correctAnswerContentView;
            RaisePropertyChanged(nameof(HasCorrectAnswerFrameworkElement));
            RaisePropertyChanged(nameof(NoCorrectAnswerFrameworkElement));
        }
    }
}
