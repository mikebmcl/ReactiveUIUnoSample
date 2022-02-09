using ReactiveUIUnoSample.Interfaces.Testing;

namespace ReactiveUIUnoSample.Helpers
{
    [Windows.UI.Xaml.Data.Bindable]
    public class TestDifficultyValueDisplayPair : ValueDisplayGenericPair<TestDifficulty>
    {
        public TestDifficultyValueDisplayPair(TestDifficulty value, string display) : base(value, display) { }
    }
}
