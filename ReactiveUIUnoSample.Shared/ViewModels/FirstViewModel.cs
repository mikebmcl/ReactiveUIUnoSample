using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System.Windows.Input;

using Windows.System;

namespace ReactiveUIUnoSample.ViewModels
{
    public class FirstViewModel : DisplayViewModelBase
    {
        public FirstViewModel(IScreen hostScreen, DispatcherQueue uiThreadDispatcherQueue, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, uiThreadDispatcherQueue, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = "First Page";
            NextPageCommand = ReactiveCommand.CreateFromObservable(() => HostScreen.Router.Navigate.Execute(new SecondViewModel(HostScreen, UIThreadDispatcherQueue)));
        }

        // The ReactiveAttribute from Fody adds INotifyProperty, including a backing field, for us. See: https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code
        [Reactive]
        public decimal EnteredAmount { get; set; }

        public override object HeaderContent { get; set; }

        public ICommand NextPageCommand { get; set; }
    }
}
