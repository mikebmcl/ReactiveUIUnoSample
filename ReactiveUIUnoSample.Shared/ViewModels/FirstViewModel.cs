using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.ViewModels.UnitConversions;

using Splat;

using System.Windows.Input;

using Windows.System;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.ViewModels
{
    public class FirstViewModel : DisplayViewModelBase
    {
        public FirstViewModel(IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = "First Page";
            NextPageCommand = ReactiveCommand.CreateFromObservable(() =>
            {
                // Set the correct contract name to ensure we get SecondView. This must be done before navigation whenever a view is registered with a contract string such as the views that use SecondViewModel..
                HostScreenWithContract.Contract = SecondViewModel.SecondViewContractName;
                return HostScreen.Router.Navigate.Execute(new SecondViewModel(HostScreenWithContract, SchedulerProvider, () => new ContentControl() { Content = new TextBlock() { Text = "Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } }));
            });
            AlternateNextPageCommand = ReactiveCommand.CreateFromObservable(() =>
            {
                // Set the correct contract name to ensure we get AlternateSecondView. This must be done before navigation whenever a view is registered with a contract string such as the views that use SecondViewModel.
                HostScreenWithContract.Contract = SecondViewModel.AlternateSecondViewContractName;
                return HostScreen.Router.Navigate.Execute(new SecondViewModel(HostScreenWithContract, SchedulerProvider, () => new ContentControl() { Content = new TextBlock() { Text = "Alternate Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } }));
            });
            TemperatureConversionsMainViewCommand = ReactiveCommand.CreateFromObservable(() =>
                {
                    //HostScreenWithContract.Contract = UnitConversionsViewModel.TemperatureConversionsMainViewContract;
                    return HostScreen.Router.Navigate.Execute(new UnitConversionsViewModel(HostScreenWithContract, SchedulerProvider));
                });
        }
        public static string EnteredAmountTextBoxAutomationId => "EnteredAmountTextBox";

        // The ReactiveAttribute from Fody adds INotifyProperty, including a backing field, for us. See: https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code
        [Reactive]
        public decimal EnteredAmount { get; set; }

        public override object HeaderContent { get; set; }

        public ICommand NextPageCommand { get; set; }

        public ICommand AlternateNextPageCommand { get; set; }

        public ICommand TemperatureConversionsMainViewCommand { get; set; }
    }
}
