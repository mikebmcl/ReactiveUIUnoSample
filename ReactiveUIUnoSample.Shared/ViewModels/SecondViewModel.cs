﻿using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Toolkit.Uwp;

namespace ReactiveUIUnoSample.ViewModels
{
    public class SecondViewModel : DisplayViewModelBase, Interfaces.ICallOnBackNavigation
    {
        public SecondViewModel(IScreen hostScreen, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = new ContentControl() { Content = new TextBlock() { Text = "Second Page", FontStyle = Windows.UI.Text.FontStyle.Italic } };
            m_confirmLeavePage = new Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object>();
            SkipConfirmLeave = false;
        }
        public override object HeaderContent { get; set; }

        private readonly Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object> m_confirmLeavePage;

        public Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object> ConfirmLeavePage => m_confirmLeavePage;

        /// <summary>
        /// Note: This is bound to the IsChecked property of a <see cref="Windows.UI.Xaml.Controls.CheckBox"/> as an example of
        /// a task on a page being completed such that navigating back from it can be safely done without prompting the user.
        /// A more realistic scenario would be the user navigating to a new page that displays results or continues with data entry. 
        /// In that case you would want to make sure that when navigating back, they do not get stopped and prompted if they want to 
        /// leave when reaching this page and should also consider setting it up so that when navigating back to this page from the 
        /// page they moved to that they are automatically navigated back to the page the page they originally came from
        /// since they presumably completed everything related to this page. Essentially, make sure that when the user is finished
        /// with this page that they aren't stopped when coming back here, unless they are coming back via a navigation that is
        /// meant to allow them to correct some data they entered on this page or do something else here.
        /// </summary>
        [Reactive]
        public bool? SkipConfirmLeave { get; set; }

        private async Task FinishCallOnNavigateBack(bool navigateBack)
        {
            if (navigateBack)
            {
                SkipConfirmLeave = true;
                await HostScreen.Router.NavigateBack.Execute();
            }
        }
        private readonly AtomicBoolean m_isNavigating = new AtomicBoolean();
        public bool CallOnBackNavigation()
        {
            if (SkipConfirmLeave is true)
            {
                return true;
            }
            if (!m_isNavigating.Set(true))
            {
                // Note: Without the call to Subscribe at the end, this code will never execute.
                m_confirmLeavePage.Handle((Title: "Confirm Quit", Text: "Are you sure you want to leave before the test is finished?", Stay: "Stay", Leave: "Leave", FinishInteraction: FinishCallOnNavigateBack, IsNavigating: m_isNavigating)).ObserveOn(RxApp.MainThreadScheduler).Subscribe();
            }
            return false;
        }
    }
}
