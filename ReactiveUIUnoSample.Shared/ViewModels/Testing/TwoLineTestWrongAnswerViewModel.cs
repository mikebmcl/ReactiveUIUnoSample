using ReactiveUIUnoSample.Interfaces;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TwoLineTestWrongAnswer : ITwoLineTestWrongAnswer
    {
        public TwoLineTestWrongAnswer(ITwoLineTestItem testItem, string wasWrong)
        {
            TestItem = testItem;
            WrongAnswer = wasWrong;
        }

        public ITwoLineTestItem TestItem { get; set; }
        public string WrongAnswer { get; set; }
    }
}
