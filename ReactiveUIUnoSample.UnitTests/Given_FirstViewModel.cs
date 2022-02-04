using FluentAssertions.Execution;

using NUnit.Framework;

using ReactiveUIUnoSample.ViewModels;

namespace ReactiveUIUnoSample.UnitTests
{
    public class Given_FirstViewModel : ReactiveTestBase
    {
        [Test(Description = "When executing the FirstViewModel.NextPageCommand then it should not throw.")]
        public void WhenNavigateNextPageCommand_ThenDoesNotThrow()
        {
            Assert.IsTrue(ScreenWithContract.Router.NavigationStack.Count == 1);
            var firstViewModel = GetCurrentViewModel() as FirstViewModel;
            Assert.IsNotNull(firstViewModel);
            Assert.DoesNotThrow(() => firstViewModel.NextPageCommand.Execute(null));
        }

        [Test(Description = "When executing the FirstViewModel.NextPageCommand then the navigation stack should be an instance of SecondViewModel.")]
        public void WhenNavigateNextPageCommand_ThenTopOfNavigationStackIsSecondViewModelInstance()
        {
            (GetCurrentViewModel() as FirstViewModel).NextPageCommand.Execute(null);
            Assert.IsTrue(GetCurrentViewModel().GetType() == typeof(SecondViewModel));
        }
    }
}
