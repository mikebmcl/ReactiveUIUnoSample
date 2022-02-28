using ReactiveUIUnoSample.Interfaces.Testing;
using ReactiveUIRoutingWithContracts;

using System.Collections.Generic;

using Windows.UI.Xaml;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class OneLineTestItemViewModel : OneLineTestItemViewModelBase, IOneLineTestItem
    {
        public OneLineTestItemViewModel(string question, string correctAnswer, IEnumerable<string> answers, FrameworkElement correctAnswerFrameworkElement, IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(question, correctAnswer, answers, correctAnswerFrameworkElement, hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment) { }
    }
}
