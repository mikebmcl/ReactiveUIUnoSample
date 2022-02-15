using FluentAssertions.Execution;

using NUnit.Framework;

using ReactiveUIUnoSample.ViewModels;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class Given_MainViewModel : ReactiveTestBase
    {
        [Test(Description = "When creating an instance of the MainViewModel, it does not throw an exception. This is critical because it is the IScreenForContracts provider that will be used for all tests of other view models.")]
        public void WhenCreateMainViewModel_ThenDoesNotThrow()
        {
            //using var _ = new AssertionScope();
            Assert.DoesNotThrow(() => new MainViewModel(TestNavigationViewProvider, Splat.Locator.CurrentMutable, "", TestSchedulerProvider));
            //Assert.NotNull(new MainViewModel(TestNavigationViewProvider, Splat.Locator.CurrentMutable, "", TestSchedulerProvider));
        }

        [Test(Description = "When creating an instance of the MainViewModel then it correctly navigates such that there is a view model on the navigation stack (which is the view model for the first view that the user sees when starting the app).")]
        public void WhenCreateMainViewModel_ThenNavigationStackNotEmpty()
        {
            var mainViewModel = new MainViewModel(TestNavigationViewProvider, Splat.Locator.CurrentMutable, "", TestSchedulerProvider);
            Assert.IsTrue(mainViewModel.Router.NavigationStack.Count > 0);
        }

        [Test(Description = "When creating an instance of the MainViewModel then the navigation stack contains an instance of FirstViewModel as its top most view model since that is the view model for the first view that the user sees after the app launches.")]
        public void WhenCreateMainViewModel_ThenNavigatedToFirstViewModel()
        {
            var mainViewModel = new MainViewModel(TestNavigationViewProvider, Splat.Locator.CurrentMutable, "", TestSchedulerProvider);
            Assert.IsInstanceOf(typeof(FirstViewModel), mainViewModel.Router.NavigationStack[mainViewModel.Router.NavigationStack.Count - 1]);
        }
    }
}
