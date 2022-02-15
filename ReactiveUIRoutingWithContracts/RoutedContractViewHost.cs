// Copyright (c) 2022 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Linq;
using Splat;

#if HAS_WINUI
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#elif NETFX_CORE || HAS_UNO
using System.Windows;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else

using System.Windows;
using System.Windows.Controls;

#endif
#if HAS_UNO
using ReactiveUI.Uno;
using ReactiveUI;
#else
using ReactiveUI;
#endif

namespace ReactiveUIRoutingWithContracts
{
    /// <summary>
    /// This control hosts the View associated with a Router, and will display
    /// the View and wire up the ViewModel whenever a new ViewModel is
    /// navigated to. Put this control as the only control in your Window.
    /// </summary>
    public
#if HAS_UNO
        partial
#endif
        class RoutedContractViewHost : ContentControl, IActivatableView, IEnableLogger
    {
        /// <summary>
        /// The router dependency property.
        /// </summary>
        public static readonly DependencyProperty RouterProperty =
            DependencyProperty.Register("Router", typeof(RoutingWithContractsState), typeof(RoutedContractViewHost), new PropertyMetadata(null));

        /// <summary>
        /// The default content property.
        /// </summary>
        public static readonly DependencyProperty DefaultContentProperty =
            DependencyProperty.Register("DefaultContent", typeof(object), typeof(RoutedContractViewHost), new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutedViewHost"/> class.
        /// </summary>
        public RoutedContractViewHost()
        {
#if NETFX_CORE
            DefaultStyleKey = typeof(RoutedContractViewHost);
#endif
            HorizontalContentAlignment = HorizontalAlignment.Stretch;
            VerticalContentAlignment = VerticalAlignment.Stretch;

            var platform = Locator.Current.GetService<IPlatformOperations>();
            Func<string> platformGetter = () => default;

            if (platform is null)
            {
                // NB: This used to be an error but WPF design mode can't read
                // good or do other stuff good.
                this.Log().Error("Couldn't find an IPlatformOperations implementation. Please make sure you have installed the latest version of the ReactiveUI packages for your platform. See https://reactiveui.net/docs/getting-started/installation for guidance.");
            }
            else
            {
                platformGetter = () => platform.GetOrientation();
            }

            IViewModelAndContract currentViewModel = default;
            var vmAndContract = this.WhenAnyObservable(x => x.Router.CurrentViewModel).Do(x => currentViewModel = x).StartWith(currentViewModel);

            this.WhenActivated(d =>
            {
                // NB: The DistinctUntilChanged is useful because most views in
                // WinRT will end up getting here twice - once for configuring
                // the RoutedViewHost's ViewModel, and once on load via SizeChanged
                d(vmAndContract.DistinctUntilChanged().Subscribe(
                    ResolveViewForViewModel,
                    ex => RxApp.DefaultExceptionHandler.OnNext(ex)));
            });
        }

        /// <summary>
        /// Gets or sets the <see cref="RoutingWithContractsState"/> of the view model stack.
        /// </summary>
        public RoutingWithContractsState Router
        {
            get => (RoutingWithContractsState)GetValue(RouterProperty);
            set => SetValue(RouterProperty, value);
        }

        /// <summary>
        /// Gets or sets the content displayed whenever there is no page currently
        /// routed.
        /// </summary>
        public object DefaultContent
        {
            get => GetValue(DefaultContentProperty);
            set => SetValue(DefaultContentProperty, value);
        }

        /// <summary>
        /// Gets or sets the view locator.
        /// </summary>
        /// <value>
        /// The view locator.
        /// </value>
        public IViewLocator ViewLocator { get; set; }

        private void ResolveViewForViewModel(IViewModelAndContract viewModel)
        {
            if (viewModel?.ViewModel is null)
            {
                Content = DefaultContent;
                return;
            }

            var viewLocator = ViewLocator ?? ReactiveUI.ViewLocator.Current;
            var view = viewLocator.ResolveView(viewModel.ViewModel, viewModel.Contract) ?? viewLocator.ResolveView(viewModel.ViewModel);

            if (view is null)
            {
                throw new Exception($"Couldn't find view for '{viewModel.ViewModel}' with contract '{viewModel.Contract}'.");
            }

            view.ViewModel = viewModel.ViewModel;
            Content = view;
        }
    }
}
