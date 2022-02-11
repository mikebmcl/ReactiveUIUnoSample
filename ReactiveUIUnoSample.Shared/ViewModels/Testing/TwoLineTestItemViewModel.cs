using ReactiveUIUnoSample.Interfaces.Testing;

using System.Collections.Generic;

using Windows.UI.Xaml;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TwoLineTestItemViewModel : TwoLineTestItemViewModelBase, ITwoLineTestItem
    {
        public TwoLineTestItemViewModel(string question, string correctAnswer, IEnumerable<string> answers, FrameworkElement correctAnswerFrameworkElement, string secondLine, IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(question, correctAnswer, answers, correctAnswerFrameworkElement, secondLine, hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment) { }
    }
}
