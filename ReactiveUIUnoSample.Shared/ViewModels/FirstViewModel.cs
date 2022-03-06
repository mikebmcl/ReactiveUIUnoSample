using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.ViewModels.UnitConversions;
using ReactiveUIRoutingWithContracts;

using Splat;

using System.Windows.Input;

using Windows.System;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.ViewModels
{
    public class FirstViewModel : DisplayViewModelBase
    {
        public FirstViewModel(IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = "First Page";
            NextPageCommand = ReactiveCommand.CreateFromObservable(() =>
            {
                return HostScreenWithContract.Router.Navigate.Execute(new SecondViewModel(HostScreenWithContract, SchedulerProvider, () => new ContentControl() { Content = new TextBlock() { Text = "Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } }).ToViewModelAndContract(SecondViewModel.SecondViewContractName));
            });
            AlternateNextPageCommand = ReactiveCommand.CreateFromObservable(() =>
            {
                return HostScreenWithContract.Router.Navigate.Execute(new SecondViewModel(HostScreenWithContract, SchedulerProvider, () => new ContentControl() { Content = new TextBlock() { Text = "Alternate Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } }).ToViewModelAndContract(SecondViewModel.AlternateSecondViewContractName));
            });
            TemperatureConversionsMainViewCommand = ReactiveCommand.CreateFromObservable(() =>
                {
                    return HostScreenWithContract.Router.Navigate.Execute(new TemperatureConversionsViewModel(HostScreenWithContract, SchedulerProvider).ToViewModelAndContract());
                });
        }

        // The ReactiveAttribute from Fody adds INotifyProperty, including a backing field, for us. See: https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code
        [Reactive]
        public decimal EnteredAmount { get; set; }

        public override object HeaderContent { get; set; }

        public ICommand NextPageCommand { get; set; }

        public ICommand AlternateNextPageCommand { get; set; }

        public ICommand TemperatureConversionsMainViewCommand { get; set; }
    }
}
