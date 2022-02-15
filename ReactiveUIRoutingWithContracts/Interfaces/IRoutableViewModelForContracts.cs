using ReactiveUI;

using System.ComponentModel;

namespace ReactiveUIRoutingWithContracts
{
    public interface IRoutableViewModelForContracts : IReactiveObject, INotifyPropertyChanged, INotifyPropertyChanging, Splat.IEnableLogger
    {
        IScreenForContracts HostScreenWithContract { get; }
    }
}
