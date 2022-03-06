using ReactiveUI;

using ReactiveUIUnoSample.ViewModels.UnitConversions;

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

namespace ReactiveUIUnoSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    [Bindable]
    //[ViewContract(TemperatureConversionsViewModel.TemperatureConversionsMainViewContract)]
    public sealed partial class TemperatureConversionsMainView : Page, IViewFor<TemperatureConversionsViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
                    DependencyProperty.Register(nameof(ViewModel), typeof(TemperatureConversionsViewModel),
                typeof(TemperatureConversionsMainView), null);//  new PropertyMetadata(default(TemperatureConversionsViewModel)));

        public TemperatureConversionsViewModel ViewModel
        {
            get => (TemperatureConversionsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TemperatureConversionsViewModel)value;
        }

        public TemperatureConversionsViewModel BindingRoot => ViewModel;

        public TemperatureConversionsMainView()
        {
            this.InitializeComponent();

            // This goes in the constructor. It should be used for various types of bindings that need to be disposed of
            // See: https://www.reactiveui.net/docs/handbook/when-activated/ and https://www.reactiveui.net/docs/handbook/data-binding/
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.TempInputText, view => view.TempInputTextBox.Text, Observable.FromEventPattern(TempInputTextBox, nameof(TextBox.TextChanged))).DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.TempConversionResultText, view => view.TempConversionResultTextBox.Text).DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.ConversionDirections, view => view.TemperaturePickerItemsComboBox.ItemsSource);
                this.Bind(ViewModel, vm => vm.SelectedTemperatureConversion, view => view.TemperaturePickerItemsComboBox.SelectedItem).DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.TestTypes, view => view.TempTestTypeComboBox.ItemsSource).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedTestType, view => view.TempTestTypeComboBox.SelectedItem).DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.TestDifficulties, view => view.TempTestDifficultyComboBox.ItemsSource).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.SelectedDifficulty, view => view.TempTestDifficultyComboBox.SelectedItem).DisposeWith(disposables);
            });
        }

        public static string TempInputAutomationId => "TempInputTextId";
        public static string TempConversionResultAutomationId => "TempConversionResultId";
        public static string TempConversionTypeAutomationId => "TempConversionTypeId";
        public static string TempTestTypeAutomationId => "TempTestTypeId";
        public static string TempTestDifficultyAutomationId => "TempTestDifficultyId";
        public static string RunTempTestAutomationId => "RunTempTestId";
    }
}
