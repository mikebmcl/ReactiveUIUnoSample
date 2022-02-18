using FluentAssertions.Execution;

using NUnit.Framework;

using ReactiveUIRoutingWithContracts;

using ReactiveUIUnoSample.ViewModels;

namespace ReactiveUIUnoSample.UnitTests
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

        [Test(Description = "When navigating to null, Then the navigation stack is not changed and no exception is thrown")]
        public void WhenNavigatingToNull_ThenTheNavigationStackIsNotChangedAndNoExceptionIsThrown()
        {
            Assert.That(ScreenWithContract.Router.NavigationStack.Count == 1, Is.True);
            var initialViewModel = GetCurrentViewModel();
            Assert.That(() => { ScreenWithContract.Router.Navigate.Execute(null); }, Throws.Nothing);
            Assert.That(ScreenWithContract.Router.NavigationStack.Count == 1, Is.True);
            Assert.That(GetCurrentViewModel(), Is.SameAs(initialViewModel));
        }
    }
}
