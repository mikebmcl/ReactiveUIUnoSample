using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces;

using System;
using System.Collections.Generic;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TwoLineTestResultsViewModel : DisplayViewModelBase, ITwoLineTestResults
    {
        [Reactive]
        public string Title { get; set; }

        public IList<ITwoLineTestItem> UserWasCorrect { get; set; }
        public IList<ITwoLineTestWrongAnswer> UserWasWrong { get; set; }
        public IList<ITwoLineTestItem> TestItems { get; set; }

        public bool HasRightAnswers => UserWasWrong?.Count != TestItems?.Count;
        public bool AllWrong => UserWasWrong?.Count == TestItems?.Count;
        public bool HasWrongAnswers => UserWasWrong?.Count > 0;

        public string PercentCorrect => UserWasCorrect.Count == 0
                    ? "0%"
                    : Math.Round((double)UserWasCorrect.Count / TestItems.Count * 100, 1, MidpointRounding.AwayFromZero).ToString("F0") + "%";

        public override object HeaderContent { get; set; }

        public TwoLineTestResultsViewModel(string title, IList<ITwoLineTestItem> userWasCorrect, IList<ITwoLineTestWrongAnswer> userWasWrong, IList<ITwoLineTestItem> testItems, IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            Title = title;
            HeaderContent = title;
            UserWasCorrect = userWasCorrect;
            UserWasWrong = userWasWrong;
            TestItems = testItems;
        }
    }
}
