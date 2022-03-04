using ReactiveUI;

using System.Reactive;
using System.Windows.Input;

namespace ReactiveUIUnoSample.Interfaces.Testing
{
    public interface IOneLineTest
    {
        string Title { get; set; }
        string CurrentTestItemAsTitleString { get; }
        bool TestIsReady { get; set; }
        IOneLineTestItem CurrentTestItem { get; set; }
        string ResultText { get; set; }
        //Brush CheckResultBackgroundBrush { get; set; }
        //Brush CheckResultForegroundBrush { get; set; }
        string CheckAnswerButtonText { get; set; }
        string DisableOneWrongAnswerText { get; set; }
        ReactiveCommand<Unit, Unit> DisableOneWrongAnswerCommand { get; set; }
        ReactiveCommand<Unit, Unit> CheckAnswerCommand { get; set; }
    }
}
