
using ReactiveUI;

using ReactiveUIUnoSample.ViewModels;

using System.Reactive.Disposables;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ReactiveUIUnoSample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView : Page, IViewFor<MainViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(MainViewModel), typeof(MainView), null);

        public MainView()
        {
            this.InitializeComponent();
            ViewModel = new MainViewModel(RootNavigationView);

            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, x => x.CurrentHeader, view => view.RootNavigationView.Header).DisposeWith(disposables);
                this.OneWayBind(ViewModel, x => x.IsBackEnabled, view => view.RootNavigationView.IsBackEnabled).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.RoutedHostPadding, view => view.RoutedHostControl.Padding).DisposeWith(disposables);
            });
        }

        public MainViewModel ViewModel
        {
            get => (MainViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MainViewModel)value;
        }
    }
}
