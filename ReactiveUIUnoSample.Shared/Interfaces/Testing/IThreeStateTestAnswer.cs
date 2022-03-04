using ReactiveUI;

using System.Reactive;

namespace ReactiveUIUnoSample.Interfaces.Testing
{
    public interface IThreeStateTestAnswer
    {
        bool IsEnabled { get; set; }
        string Text { get; set; }
        bool IsSelected { get; set; }
        ReactiveCommand<Unit, Unit> PressCommand { get; set; }
    }
}
