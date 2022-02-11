using ReactiveUI;

using ReactiveUIUnoSample.ViewModels.Testing;

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
    public sealed partial class TwoLineTestView : Page, IViewFor<ViewModels.Testing.TwoLineTestViewModel>
    {
        // Add the following interface to the class
        // , IViewFor<TwoLineTestViewModel>

        public static readonly DependencyProperty ViewModelProperty =
                DependencyProperty.Register(nameof(ViewModel), typeof(TwoLineTestViewModel),
            typeof(TwoLineTestView), new PropertyMetadata(default(TwoLineTestViewModel)));

        public TwoLineTestViewModel ViewModel
        {
            get => (TwoLineTestViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (TwoLineTestViewModel)value;
        }
        private ReactiveUI.Uno.BooleanToVisibilityTypeConverter _booleanToVisibilityTypeConverter =
            new ReactiveUI.Uno.BooleanToVisibilityTypeConverter();

        public TwoLineTestView()
        {
            this.InitializeComponent();
            // This goes in the constructor. It should be used for various types of bindings that need to be disposed of
            // See: https://www.reactiveui.net/docs/handbook/when-activated/ and https://www.reactiveui.net/docs/handbook/data-binding/
            this.WhenActivated(disposables =>
            {
                this.OneWayBind(ViewModel, vm => vm.CurrentTestItem.FirstLine, view => view.CurrentTestItemFirstLineTextBlock.Text).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.CurrentTestItem.Answers, view => view.AnswersListBox.ItemsSource).DisposeWith(disposables);
                this.Bind(ViewModel, vm => vm.CurrentTestItem.SelectedItem, view => view.AnswersListBox.SelectedItem).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.CheckAnswerCommand, view => view.CheckAnswerButton).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.DisableOneWrongAnswerCommand, view => view.DisableOneWrongAnswerButton).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.DisableOneWrongAnswerText, view => view.DisableOneWrongAnswerButton.Content).DisposeWith(disposables);
                this.BindCommand(ViewModel, vm => vm.NextFinishCommand, view => view.NextFinishButton).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.ResultText, view => view.ResultTextBlock.Text).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.CheckAnswerButtonText, view => view.CheckAnswerButton.Content).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.NextFinishButtonTest, view => view.NextFinishButton.Content).DisposeWith(disposables);
                this.BindInteraction(ViewModel, vm => vm.ConfirmLeavePage, async ic =>
                {
                    try
                    {
                        var dialog = new ContentDialog()
                        {
                            Title = ic.Input.Title,
                            Content = ic.Input.Text,
                            PrimaryButtonText = ic.Input.Stay,
                            SecondaryButtonText = ic.Input.Leave,
                            DefaultButton = ContentDialogButton.Primary
                        };
                        Exception exception = null;
                        ContentDialogResult result = ContentDialogResult.None;
                        await dialog.ShowAsync().AsTask().ContinueWith(res =>
                        {
                            if (res.IsFaulted)
                            {
                                exception = res.Exception;
                                return;
                            }
                            result = res.Result;
                        });
                        if (exception != null)
                        {
                            throw exception;
                        }
                        await ic.Input.FinishInteraction(result == ContentDialogResult.Secondary);
                        ic.SetOutput(null);
                    }
                    finally
                    {
                        ic.Input.IsNavigating.ForceToFalse();
                    }
                }).DisposeWith(disposables);
            });

            //Loaded += TwoLineTestView_Loaded;
        }

        //private void TwoLineTestView_Loaded(object sender, RoutedEventArgs e)
        //{
        //    Loaded -= TwoLineTestView_Loaded;
        //    ViewModel.AnswersListViewWeakRef.SetTarget(AnswersListBox);
        //}
    }
}
