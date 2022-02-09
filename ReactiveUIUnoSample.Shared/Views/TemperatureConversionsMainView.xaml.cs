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
    //[ViewContract(UnitConversionsViewModel.TemperatureConversionsMainViewContract)]
    public sealed partial class TemperatureConversionsMainView : Page, IViewFor<UnitConversionsViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
                    DependencyProperty.Register(nameof(ViewModel), typeof(UnitConversionsViewModel),
                typeof(TemperatureConversionsMainView), null);//  new PropertyMetadata(default(UnitConversionsViewModel)));

        public UnitConversionsViewModel ViewModel
        {
            get => (UnitConversionsViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (UnitConversionsViewModel)value;
        }

        public TemperatureConversionsMainView()
        {
            this.InitializeComponent();
            // This goes in the constructor. It should be used for various types of bindings that need to be disposed of
            // See: https://www.reactiveui.net/docs/handbook/when-activated/ and https://www.reactiveui.net/docs/handbook/data-binding/
            this.WhenActivated(disposables =>
            {
                this.Bind(ViewModel, vm => vm.TempEntryOneText, view => view.TempEntryOneTextBox.Text, Observable.FromEventPattern(TempEntryOneTextBox, nameof(TextBox.TextChanged))).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.TempEntryTwoText, view => view.TempEntryTwoTextBox.Text).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.TemperatureConversions, view => view.TemperaturePickerItemsComboBox.ItemsSource);
                this.Bind(ViewModel, vm => vm.SelectedTemperatureConversion, view => view.TemperaturePickerItemsComboBox.SelectedItem).DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.TemperatureTestingVM.TestTypes, view => view.TestTypeComboBox.ItemsSource).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.TemperatureTestingVM.SelectedTestType, view => view.TestTypeComboBox.SelectedItem).DisposeWith(disposables);

                this.OneWayBind(ViewModel, vm => vm.TestDifficulties, view => view.TestDifficultyComboBox.ItemsSource).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.TemperatureTestingVM.SelectedDifficulty, view => view.TestDifficultyComboBox.SelectedItem).DisposeWith(disposables);

                //this.BindCommand(ViewModel, vm => vm.TemperatureTestingVM.RunTestCommand, view => view.TestTemperatureConversionButton).DisposeWith(disposables);
            });
        }
    }
}
