using FluentAssertions.Execution;

using NUnit.Framework;

using ReactiveUIRoutingWithContracts;

using ReactiveUIUnoSample.ViewModels;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

namespace ReactiveUIUnoSample.UnitTests
{
    public class Given_ReactiveTestBase : ReactiveTestBase
    {
        [Test(Description = "When SetUpTest is called then the navigation stack should return to it original state.")]
        public void WhenSetUpTestIsCalled_ThenTheNavigationStackIsResetToItsOriginalState()
        {
            Assert.IsTrue(ScreenWithContract.Router.NavigationStack.Count == 1);
            var typeOfFirstViewModel = GetCurrentViewModel().GetType();
            //WaitForNavigation(() => ScreenWithContract.Router.NavigateBack.Execute());
            ScreenWithContract.Router.NavigateBack.Execute();
            WaitForNavigation();
            Assert.IsTrue(ScreenWithContract.Router.NavigationStack.Count == 0);
            SetUpTest();
            Assert.IsTrue(ScreenWithContract.Router.NavigationStack.Count == 1);
            Assert.IsTrue(typeOfFirstViewModel == GetCurrentViewModel().GetType());
        }

        [Test(Description = "When test runs, Then navigation stack will contain one item of type TemperatureConversionsViewModel")]
        public void WhenTestRuns_ThenNavigationStackWIllContainOneItem()
        {
            Assert.That(ScreenWithContract.Router.NavigationStack.Count, Is.EqualTo(1));
        }

        [Test(Description = "When test runs, Then navigation stack will contain one item of type TemperatureConversionsViewModel")]
        public void WhenTestRuns_ThenNavigationStackWIllContainOneItemOfTypeTemperatureConversionsViewModel()
        {
            Assert.That(GetCurrentViewModel(), Is.TypeOf(typeof(TemperatureConversionsViewModel)));
        }

        [Test(Description = "When navigating to null, Then the navigation stack is not changed and exception is thrown")]
        public void WhenNavigatingToNull_ThenTheNavigationStackIsNotChangedAndExceptionIsThrown()
        {
            Assert.That(ScreenWithContract.Router.NavigationStack.Count == 1, Is.True);
            var initialViewModel = GetCurrentViewModel();
            Assert.That(() => { ScreenWithContract.Router.Navigate.Execute(null); AdvanceScheduler(); }, Throws.Exception);
            Assert.That(ScreenWithContract.Router.IsNavigating, Is.False);
            Assert.That(ScreenWithContract.Router.NavigationStack.Count == 1, Is.True);
            Assert.That(GetCurrentViewModel(), Is.SameAs(initialViewModel));
        }
    }
}
