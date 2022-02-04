using FluentAssertions.Execution;

using NUnit.Framework;

using ReactiveUIUnoSample.ViewModels;

namespace ReactiveUIUnoSample.UITest
{
    public class Given_ReactiveTestBase : ReactiveTestBase
    {
        [Test(Description = "When SetUpTest is called then the navigation stack should return to it original state.")]
        public void WhenSetUpTestIsCalled_ThenTheNavigationStackIsResetToItsOriginalState()
        {
            Assert.IsTrue(ScreenWithContract.Router.NavigationStack.Count == 1);
            var typeOfFirstViewModel = GetCurrentViewModel().GetType();
            ScreenWithContract.Router.NavigateBack.Execute();
            Assert.IsTrue(ScreenWithContract.Router.NavigationStack.Count == 0);
            SetUpTest();
            Assert.IsTrue(ScreenWithContract.Router.NavigationStack.Count == 1);
            Assert.IsTrue(typeOfFirstViewModel == GetCurrentViewModel().GetType());
        }
    }
}
