using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces.Testing;

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

        public ICommand PressCommand { get; set; }

        public ButtonViewModel(string text, ICommand pressCommand)
        {
            IsEnabled = true;
            Text = text ?? "";
            PressCommand = pressCommand;
        }
    }
}
