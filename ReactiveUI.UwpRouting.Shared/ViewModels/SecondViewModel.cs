using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ReactiveUI.UwpRouting.ViewModels
{
    public class SecondViewModel : DisplayViewModelBase
    {
        public SecondViewModel(IScreen hostScreen, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = new ContentControl() { Content = new TextBlock() { Text = "Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } };
        }

        public override object HeaderContent { get; set; }
    }
}
