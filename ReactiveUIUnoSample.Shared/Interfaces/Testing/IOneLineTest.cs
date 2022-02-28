
using System.Windows.Input;

namespace ReactiveUIUnoSample.Interfaces.Testing
{
    public interface ITwoLineTest
    {
        string Title { get; set; }
        string CurrentTestItemAsTitleString { get; }
        bool HasSecondLine { get; set; }
        bool ShowSecondLine { get; set; }
        /// <summary>
        /// A string that can be used to ask the user if they want to hide the data associated with the second line? (e.g. for
        /// a vocab meaning test the second line currently is the kana reading (unless it's a kana-only items in which case the
        /// first line is the kana and the second line is an empty string) so the prompt asks if they want to hide that kana
        /// reading and just see the kanji (it's moot for kana-only because they'll only see that regardless).
        /// </summary>
        string ShowSecondLinePrompt { get; set; }
        bool TestIsReady { get; set; }
        ITwoLineTestItem CurrentTestItem { get; set; }
        string ResultText { get; set; }
        //Brush CheckResultBackgroundBrush { get; set; }
        //Brush CheckResultForegroundBrush { get; set; }
        string CheckAnswerButtonText { get; set; }
        string DisableOneWrongAnswerText { get; set; }
        ICommand DisableOneWrongAnswerCommand { get; set; }
        ICommand CheckAnswerCommand { get; set; }
    }
}
