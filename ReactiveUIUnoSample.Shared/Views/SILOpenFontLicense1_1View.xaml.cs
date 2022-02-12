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

namespace ReactiveUIUnoSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SILOpenFontLicense1_1View : Page, IViewFor<ViewModels.SILOpenFontLicense1_1ViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty =
                    DependencyProperty.Register(nameof(ViewModel), typeof(ViewModels.SILOpenFontLicense1_1ViewModel),
                typeof(SILOpenFontLicense1_1View), new PropertyMetadata(default(ViewModels.SILOpenFontLicense1_1ViewModel)));

        public ViewModels.SILOpenFontLicense1_1ViewModel ViewModel
        {
            get => (ViewModels.SILOpenFontLicense1_1ViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (ViewModels.SILOpenFontLicense1_1ViewModel)value;
        }

        // An instance of a class that implements IBindingTypeConverter
        //private ReactiveUI.Uno.BooleanToVisibilityTypeConverter _booleanToVisibilityTypeConverter =
        //    new ReactiveUI.Uno.BooleanToVisibilityTypeConverter();

        //// This goes in the constructor. It should be used for various types of bindings that need to be disposed of
        //// See: https://www.reactiveui.net/docs/handbook/when-activated/ and https://www.reactiveui.net/docs/handbook/data-binding/
        //this.WhenActivated(disposables =>
        //{
        //    this.Bind(ViewModel, vm => vm.EnteredAmount, view => view.EnteredAmountTextBox.Text, Observable.FromEventPattern(EnteredAmountTextBox, nameof(TextBox.LostFocus)), null, m_decimalToStringBindingTypeConverter, m_decimalToStringBindingTypeConverter).DisposeWith(disposables);
        //
        //    this.BindCommand(ViewModel, vm => vm.NextPageCommand, view => view.NextPageButton).DisposeWith(disposables);
        //
        ////    // The code from the corresponding view model for the interaction below:
        ////    // In the ctor
        ////    m_confirmLeavePage = new Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object>(schedulerProvider.CurrentThread);
        ////    In the class
        ////    private readonly Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object> m_confirmLeavePage;
        ////public Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object> ConfirmLeavePage => m_confirmLeavePage;
        /////// <summary>
        /////// Note: This is bound to the IsChecked property of a <see cref="Windows.UI.Xaml.Controls.CheckBox"/> as an example of
        /////// a task on a page being completed such that navigating back from it can be safely done without prompting the user.
        /////// A more realistic scenario would be the user navigating to a new page that displays results or continues with data entry. 
        /////// In that case you would want to make sure that when navigating back, they do not get stopped and prompted if they want to 
        /////// leave when reaching this page and should also consider setting it up so that when navigating back to this page from the 
        /////// page they moved to that they are automatically navigated back to the page the page they originally came from
        /////// since they presumably completed everything related to this page. Essentially, make sure that when the user is finished
        /////// with this page that they aren't stopped when coming back here, unless they are coming back via a navigation that is
        /////// meant to allow them to correct some data they entered on this page or do something else here.
        /////// </summary>
        ////[Reactive]
        ////public bool? SkipConfirmLeave { get; set; }

        ////private async Task FinishCallOnNavigateBack(bool navigateBack)
        ////{
        ////    if (navigateBack)
        ////    {
        ////        SkipConfirmLeave = true;
        ////        await HostScreen.Router.NavigateBack.Execute();
        ////    }
        ////}
        ////private readonly AtomicBoolean m_isNavigating = new AtomicBoolean();
        ////public bool CallOnBackNavigation()
        ////{
        ////    if (SkipConfirmLeave is true || m_confirmLeavePage is null)
        ////    {
        ////        return true;
        ////    }
        ////    if (!m_isNavigating.Set(true))
        ////    {
        ////        // Note: Without the call to Subscribe at the end, this code will never execute.
        ////        // Note: You don't need to worry about this leaking despite Subscribe() returning an IDisposable. The WhenActivated
        ////        //  in the SecondView ctor ensures that everything is disconnected so that the GC will take care of it. Also, Android
        ////        //  will not dismiss the dialog properly if you try to get a reference to this and dispose it from here.
        ////        m_confirmLeavePage.Handle((Title: "Confirm Quit", Text: "Are you sure you want to leave before the test is finished?", Stay: "Stay", Leave: "Leave", FinishInteraction: FinishCallOnNavigateBack, IsNavigating: m_isNavigating)).ObserveOn(SchedulerProvider.MainThread).Subscribe();
        ////    }
        ////    return false;
        ////}
        //    this.BindInteraction(ViewModel, vm => vm.ConfirmLeavePage, async ic =>
        //    {
        //        try
        //        {
        //            var dialog = new ContentDialog()
        //            {
        //                Title = ic.Input.Title,
        //                Content = ic.Input.Text,
        //                PrimaryButtonText = ic.Input.Stay,
        //                SecondaryButtonText = ic.Input.Leave,
        //                DefaultButton = ContentDialogButton.Primary
        //            };
        //            Exception exception = null;
        //            ContentDialogResult result = ContentDialogResult.None;
        //            await dialog.ShowAsync().AsTask().ContinueWith(res =>
        //            {
        //                if (res.IsFaulted)
        //                {
        //                    exception = res.Exception;
        //                    return;
        //                }
        //                result = res.Result;
        //            });
        //            if (exception != null)
        //            {
        //                throw exception;
        //            }
        //            await ic.Input.FinishInteraction(result == ContentDialogResult.Secondary);
        //            ic.SetOutput(null);
        //        }
        //        finally
        //        {
        //            ic.Input.IsNavigating.ForceToFalse();
        //        }
        //    }).DisposeWith(disposables);
        //});

        public SILOpenFontLicense1_1View()
        {
            this.InitializeComponent();
        }
    }
}
