using System;
using System.Collections.Generic;
using System.Text;

using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.Interfaces
{
    public interface INavigationViewProvider
    {
        bool IsTest { get; }
        void SubscribeBackRequested(TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> handler);
        void UnsubscribeBackRequested(TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> handler);

        void SubscribeItemInvoked(TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> handler);
        void UnsubscribeItemInvoked(TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> handler);
    }
}
