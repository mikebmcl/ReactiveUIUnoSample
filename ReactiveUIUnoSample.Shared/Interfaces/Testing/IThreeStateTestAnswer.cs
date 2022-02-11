namespace ReactiveUIUnoSample.Interfaces.Testing
{
    public interface IThreeStateTestAnswer
    {
        bool IsEnabled { get; set; }
        string Text { get; set; }
        bool IsSelected { get; set; }
        System.Windows.Input.ICommand PressCommand { get; set; }
    }
}
