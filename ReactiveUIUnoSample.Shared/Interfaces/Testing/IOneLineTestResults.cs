using System.Collections.Generic;

namespace ReactiveUIUnoSample.Interfaces.Testing
{
    public interface IOneLineTestResults
    {
        string Title { get; set; }

        IList<IOneLineTestItem> UserWasCorrect { get; set; }
        IList<IOneLineTestWrongAnswer> UserWasWrong { get; set; }
        IList<IOneLineTestItem> TestItems { get; set; }

        bool HasRightAnswers { get; }
        bool AllWrong { get; }
        bool HasWrongAnswers { get; }

        string PercentCorrect { get; }
    }
}
