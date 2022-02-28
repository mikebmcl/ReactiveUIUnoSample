using ReactiveUIUnoSample.Interfaces.Testing;

namespace ReactiveUIUnoSample.ViewModels.Testing
{
    [Windows.UI.Xaml.Data.Bindable]
    public class OneLineTestWrongAnswer : IOneLineTestWrongAnswer
    {
        public OneLineTestWrongAnswer(IOneLineTestItem testItem, string wasWrong)
        {
            TestItem = testItem;
            WrongAnswer = wasWrong;
        }

        public IOneLineTestItem TestItem { get; set; }
        public string WrongAnswer { get; set; }
    }
}
