namespace ReactiveUI.UwpRouting.ViewModels
{
    public class SecondViewModel : ReactiveObject, IRoutableViewModel
    {
        public string UrlPathSegment { get; }
        public IScreen HostScreen { get; }

        public SecondViewModel(IScreen screen)
        {
            HostScreen = screen;
        }
    }
}
