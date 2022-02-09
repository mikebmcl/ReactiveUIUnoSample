using ReactiveUI.Fody.Helpers;

using ReactiveUIUnoSample.Interfaces;

using System.Windows.Input;

namespace ReactiveUIUnoSample.ViewModels
{
    [Windows.UI.Xaml.Data.Bindable]
    public class ButtonViewModel : IThreeStateTestAnswer
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
