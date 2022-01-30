using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Splat;

using System.Windows.Input;

using Windows.System;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.ViewModels
{
    public class FirstViewModel : DisplayViewModelBase
    {
        public FirstViewModel(IScreenWithContract hostScreen, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = "First Page";
            NextPageCommand = ReactiveCommand.CreateFromObservable(() =>
            {
                HostScreenWithContract.Contract = SecondViewModel.SecondViewContractName;
                return HostScreen.Router.Navigate.Execute(new SecondViewModel(HostScreenWithContract, () => new ContentControl() { Content = new TextBlock() { Text = "Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } }));
            });
            AlternateNextPageCommand = ReactiveCommand.CreateFromObservable(() =>
            {
                HostScreenWithContract.Contract = SecondViewModel.AlternateSecondViewContractName;
                return HostScreen.Router.Navigate.Execute(new SecondViewModel(HostScreenWithContract, () => new ContentControl() { Content = new TextBlock() { Text = "Alternate Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } }));
            });
            //AlternateNextPageCommand = ReactiveCommand.CreateFromObservable(() => HostScreen.Router.Navigate.Execute();
        }

        // The ReactiveAttribute from Fody adds INotifyProperty, including a backing field, for us. See: https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code
        [Reactive]
        public decimal EnteredAmount { get; set; }

        public override object HeaderContent { get; set; }

        public ICommand NextPageCommand { get; set; }

        public ICommand AlternateNextPageCommand { get; set; }
    }
}
