
using ReactiveUI;

using ReactiveUIUnoSample.ViewModels;

using Splat;

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

        private NavigationViewProvider m_navigationViewProvider;

        public MainView()
        {
            this.InitializeComponent();
            m_navigationViewProvider = new NavigationViewProvider(RootNavigationView);
            ViewModel = new MainViewModel(m_navigationViewProvider, Locator.CurrentMutable, new ContentControl() { }, new SchedulerProvider());
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.Router, view => view.RoutedHostControl.Router).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.CurrentHeader, view => view.RootNavigationView.Header).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.IsBackEnabled, view => view.RootNavigationView.IsBackEnabled).DisposeWith(disposables);
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
