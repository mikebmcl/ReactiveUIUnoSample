using System;
using System.Collections.Generic;
using System.Text;

using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.UITest
{
    internal class TestNavigationViewProvider : Interfaces.INavigationViewProvider
    {
        public bool IsTest => true;

        public void SubscribeBackRequested(TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> handler)
        {
        }

        public void SubscribeItemInvoked(TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> handler)
        {
        }

        public void UnsubscribeBackRequested(TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> handler)
        {
        }

        public void UnsubscribeItemInvoked(TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> handler)
        {
        }
    }
}
