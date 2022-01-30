using ReactiveUI;

using ReactiveUIUnoSample.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
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
    public sealed partial class AlternateSecondView : Page, IViewFor<SecondViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
                    DependencyProperty.Register(nameof(ViewModel), typeof(SecondViewModel), typeof(AlternateSecondView), new PropertyMetadata(default(SecondViewModel)));

        public SecondViewModel ViewModel
        {
            get => (SecondViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SecondViewModel)value;
        }
        public AlternateSecondView()
        {
            this.InitializeComponent();
            //this.WhenActivated(disposables =>
            //{
            //    // Note: Even though our AlternateSecondView doesn't actually care about stopping and checking navigation
            //    // back, because SecondViewModel implements ICallOnBackNavigation, if we wanted to avoid having the view model
            //    // check to see what View it is to configure its response in the CallOnBackNavigation method appropriately,
            //    // we could instead do this, which is just a pass through interaction that confirms it's ok to leave because
            //    // AlternateSecondView doesn't care about doing anything special during back navigation.
            //    this.BindInteraction(ViewModel, vm => vm.ConfirmLeavePage, async ic =>
            //    {
            //        try
            //        {
            //            await ic.Input.FinishInteraction(true);
            //            ic.SetOutput(null);
            //        }
            //        finally
            //        {
            //            ic.Input.IsNavigating.ForceToFalse();
            //        }
            //    }).DisposeWith(disposables);
            //});
        }
    }
}
