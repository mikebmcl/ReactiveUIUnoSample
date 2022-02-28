using ReactiveUI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReactiveUIUnoSample.Views.Testing
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OneLineTestResultsView : Page, IViewFor<ViewModels.Testing.OneLineTestResultsViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
                    DependencyProperty.Register(nameof(ViewModel), typeof(ViewModels.Testing.OneLineTestResultsViewModel),
                typeof(OneLineTestResultsView), new PropertyMetadata(default(ViewModels.Testing.OneLineTestResultsViewModel)));

        public ViewModels.Testing.OneLineTestResultsViewModel ViewModel
        {
            get => (ViewModels.Testing.OneLineTestResultsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ViewModels.Testing.OneLineTestResultsViewModel)value;
        }

        public OneLineTestResultsView()
        {
            this.InitializeComponent();
            // This goes in the constructor. It should be used for various types of bindings that need to be disposed of
            // See: https://www.reactiveui.net/docs/handbook/when-activated/ and https://www.reactiveui.net/docs/handbook/data-binding/
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.UserWasWrong, view => view.WrongAnswersItemsControl.ItemsSource).DisposeWith(disposables);
                //this.Bind(ViewModel, vm => vm.EnteredAmount, view => view.EnteredAmountTextBox.Text, Observable.FromEventPattern(EnteredAmountTextBox, nameof(TextBox.LostFocus)), null, m_decimalToStringBindingTypeConverter, m_decimalToStringBindingTypeConverter).DisposeWith(disposables);

                //this.BindCommand(ViewModel, vm => vm.NextPageCommand, view => view.NextPageButton).DisposeWith(disposables);
            });
        }
    }
}
