using System;
using System.Collections.Generic;
using System.Text;

using ReactiveUIUnoSample.Interfaces;

using Windows.Foundation;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample
{
    public class NavigationViewProvider : INavigationViewProvider
    {
        public NavigationViewProvider(NavigationView navigationView)
        {
            m_navigationView = navigationView;
        }

        public bool IsTest => false;

        private NavigationView m_navigationView;

        public void SubscribeBackRequested(TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> handler)
        {
            m_navigationView.BackRequested += handler;
        }

        public void SubscribeItemInvoked(TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> handler)
        {
            m_navigationView.ItemInvoked += handler;
        }

        public void UnsubscribeBackRequested(TypedEventHandler<NavigationView, NavigationViewBackRequestedEventArgs> handler)
        {
            m_navigationView.BackRequested -= handler;
        }

        public void UnsubscribeItemInvoked(TypedEventHandler<NavigationView, NavigationViewItemInvokedEventArgs> handler)
        {
            m_navigationView.ItemInvoked -= handler;
        }
    }
}
