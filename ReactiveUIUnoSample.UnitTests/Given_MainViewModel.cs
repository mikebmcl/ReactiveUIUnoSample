using FluentAssertions;
using FluentAssertions.Execution;

using NUnit.Framework;

using ReactiveUIUnoSample.ViewModels;
using ReactiveUIUnoSample.ViewModels.UnitConversions;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class Given_MainViewModel : ReactiveTestBase
    {
        [Test(Description = "When creating an instance of the MainViewModel, it does not throw an exception. This is critical because it is the IScreenForContracts provider that will be used for all tests of other view models.")]
        public void WhenCreateMainViewModel_ThenDoesNotThrow()
        {
            Assert.That(() => new MainViewModel(TestNavigationViewProvider, Splat.Locator.CurrentMutable, "", TestSchedulerProvider), Throws.Nothing);
        }

        [Test(Description = "When creating an instance of the MainViewModel then it correctly navigates such that there is a view model on the navigation stack (which is the view model for the first view that the user sees when starting the app).")]
        public void WhenCreateMainViewModel_ThenNavigationStackNotEmpty()
        {
            var mainViewModel = new MainViewModel(TestNavigationViewProvider, Splat.Locator.CurrentMutable, "", TestSchedulerProvider);
            Assert.That(mainViewModel.Router.NavigationStack.Count > 0, Is.True);
        }

        [Test(Description = "When creating an instance of the MainViewModel then the navigation stack contains an instance of TemperatureConversionsViewModel as its top most view model since that is the view model for the first view that the user sees after the app launches.")]
        public void WhenCreateMainViewModel_ThenNavigatedToTemperatureConversionsViewModel()
        {
            var mainViewModel = new MainViewModel(TestNavigationViewProvider, Splat.Locator.CurrentMutable, "", TestSchedulerProvider);
            Assert.That(mainViewModel.Router.NavigationStack.Count > 0, Is.True);
            Assert.That(mainViewModel.Router.NavigationStack[mainViewModel.Router.NavigationStack.Count - 1].ViewModel, Is.AssignableTo(typeof(TemperatureConversionsViewModel)));
        }
    }
}
