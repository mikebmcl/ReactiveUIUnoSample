using ReactiveUI.Fody.Helpers;

using System;

namespace ReactiveUI.UwpRouting.ViewModels
{
    public class FirstViewModel : ReactiveObject, IRoutableViewModel
    {
        // The ReactiveAttribute from Fody adds INotifyProperty, including a backing field, for us. See: https://www.reactiveui.net/docs/handbook/view-models/boilerplate-code
        [Reactive]
        public decimal EnteredAmount { get; set; }

        public FirstViewModel(IScreen screen)
        {
            HostScreen = screen;
            //this.RaisePropertyChanged(nameof(EnteredAmount));
        }

        // Reference to IScreen that owns the routable view model.
        public IScreen HostScreen { get; }

        // Unique identifier for the routable view model.
        public string UrlPathSegment { get; } = Guid.NewGuid().ToString().Substring(0, 5);
    }
}
