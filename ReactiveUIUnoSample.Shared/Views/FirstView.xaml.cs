using ReactiveUI;

using ReactiveUIUnoSample.Converters;
using ReactiveUIUnoSample.ViewModels;

using System.Reactive.Disposables;
using System.Reactive.Linq;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReactiveUIUnoSample.Views
{
    // Note: As an alternative to implementing IViewFor each time you can do this instead: https://www.reactiveui.net/api/reactiveui/reactivepage_1/
    //  Both ways produce mostly the same outcome (ReactivePage<T> also implements IActivatableView for you - see: https://www.reactiveui.net/api/reactiveui/iactivatableview/ )
    //  Since we're using ViewModel-based navigation, IActivatableView isn't particularly useful for pages as a whole, but it is useful when a view
    // is meant to be embedded within another view such that it does not have its own view model or isn't part of the ViewModel-based navigation. You can
    // use it in those cases to take advantage of the benefits of ReactiveUI's various bindings via WhenActivated. There are other scenarios where it can
    // also be useful since WhenActivated executes only when a view is actually displayed, thus preventing any expenses associated with binding if
    // the user never actually ends up taking an action that causes the view to be shown.
    public sealed partial class FirstView : Page, IViewFor<FirstViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(FirstViewModel), typeof(FirstView), null);

        DecimalToStringBindingTypeConverter m_decimalToStringBindingTypeConverter = new DecimalToStringBindingTypeConverter();

        public FirstView()
        {
            InitializeComponent();
            // See: https://www.reactiveui.net/docs/handbook/when-activated/ and https://www.reactiveui.net/docs/handbook/data-binding/
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.EnteredAmount, view => view.EnteredAmountTextBox.Text, Observable.FromEventPattern(EnteredAmountTextBox, nameof(TextBox.LostFocus)), null, m_decimalToStringBindingTypeConverter, m_decimalToStringBindingTypeConverter).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.NextPageCommand, view => view.NextPageButton).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.AlternateNextPageCommand, view => view.AlternateNextPageButton).DisposeWith(disposables);
            });
        }

        public FirstViewModel ViewModel
        {
            get => (FirstViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (FirstViewModel)value;
        }
    }
}
