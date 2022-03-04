using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces.Testing;

using System.Reactive;
using System.Windows.Input;

namespace ReactiveUIUnoSample.ViewModels
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ButtonViewModel : ReactiveObject, IThreeStateTestAnswer
    {
        [Reactive]
        public bool IsEnabled { get; set; }

        [Reactive]
        public string Text { get; set; }

        [Reactive]
        public bool IsSelected { get; set; }

        public ReactiveCommand<Unit, Unit> PressCommand { get; set; }

        public ButtonViewModel(string text, ReactiveCommand<Unit, Unit> pressCommand)
        {
            IsEnabled = true;
            Text = text ?? "";
            PressCommand = pressCommand;
        }
    }
}
