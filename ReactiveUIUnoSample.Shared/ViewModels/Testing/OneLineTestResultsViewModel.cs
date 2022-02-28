using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces.Testing;
using ReactiveUIRoutingWithContracts;

using System;
using System.Collections.Generic;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class OneLineTestResultsViewModel : DisplayViewModelBase, IOneLineTestResults
    {
        [Reactive]
        public string Title { get; set; }

        public IList<IOneLineTestItem> UserWasCorrect { get; set; }
        public IList<IOneLineTestWrongAnswer> UserWasWrong { get; set; }
        public IList<IOneLineTestItem> TestItems { get; set; }

        public bool HasRightAnswers => UserWasWrong?.Count != TestItems?.Count;
        public bool AllWrong => UserWasWrong?.Count == TestItems?.Count;
        public bool HasWrongAnswers => UserWasWrong?.Count > 0;

        public string PercentCorrect => UserWasCorrect.Count == 0
                    ? "0%"
                    : Math.Round((double)UserWasCorrect.Count / TestItems.Count * 100, 1, MidpointRounding.AwayFromZero).ToString("F0") + "%";

        public override object HeaderContent { get; set; }

        public OneLineTestResultsViewModel(string title, IList<IOneLineTestItem> userWasCorrect, IList<IOneLineTestWrongAnswer> userWasWrong, IList<IOneLineTestItem> testItems, IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            Title = title;
            HeaderContent = title;
            UserWasCorrect = userWasCorrect;
            UserWasWrong = userWasWrong;
            TestItems = testItems;
        }
    }
}
