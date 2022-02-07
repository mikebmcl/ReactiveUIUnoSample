using ReactiveUI;
using ReactiveUIUnoSample.ViewModels;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Microsoft.Toolkit.Extensions;
using Microsoft.Toolkit.Uwp;
using Microsoft.Extensions.Logging;
using Uno.Extensions;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ReactiveUIUnoSample.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SecondView : Page, IViewFor<SecondViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(SecondViewModel), typeof(SecondView), null);

        public SecondView()
        {
            InitializeComponent();

            this.WhenActivated(disposables =>
            {
                this.BindInteraction(ViewModel, x => x.ConfirmLeavePage, async ic =>
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

                this.Bind(ViewModel, x => x.SkipConfirmLeave, view => view.SkipConfirmLeaveCheckBox.IsChecked).DisposeWith(disposables);
            });
        }

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
    }
}
