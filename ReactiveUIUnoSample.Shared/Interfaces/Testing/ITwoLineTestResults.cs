using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveUIUnoSample.Interfaces
{
    public interface ITwoLineTestResults
    {
        string Title { get; set; }

        IList<ITwoLineTestItem> UserWasCorrect { get; set; }
        IList<ITwoLineTestWrongAnswer> UserWasWrong { get; set; }
        IList<ITwoLineTestItem> TestItems { get; set; }

        bool HasRightAnswers { get; }
        bool AllWrong { get; }
        bool HasWrongAnswers { get; }

        string PercentCorrect { get; }
    }
}
