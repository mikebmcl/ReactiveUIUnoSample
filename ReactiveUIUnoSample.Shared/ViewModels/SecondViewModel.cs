using ReactiveUI;
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
using System.Reactive.Concurrency;

using Microsoft.Extensions.Logging;
using Uno.Extensions;

namespace ReactiveUIUnoSample.ViewModels
{
    public class SecondViewModel : DisplayViewModelBase, Interfaces.ICallOnBackNavigation
    {
        public SecondViewModel(IScreenWithContract hostScreen, ISchedulerProvider schedulerProvider, Func<object> createHeaderContent, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = createHeaderContent();
            if (hostScreen.Contract == SecondViewContractName)
            {
                // Note: We only want the confirm leave if we are being attached to SecondView; AlternateSecondView's constructor does not have the BindInteraction
                //  that SecondView's ctor has so if we try to navigate back from this we'd get a ReactiveUI.UnhandledInteractionException'2  thrown that would
                //  take down the whole program.
                // Note: That it's not recommended to have multiple views for a view model that implements ICallOnBackNavigation. Nonetheless there might be scenarios
                //  where it makes sense for you to do it and so we handle this here by only creating an Interaction<...> for m_confirmLeavePage when this is being
                //  used for SecondView and we're doing a null check on this in the CallBackOnNavigation method below to allow pass through navigation instead
                //  of implementing a pass-through interaction binding in AlternateSecondView's ctor. 
                m_confirmLeavePage = new Interaction<(string Title, string Text, string Stay, string Leave, Func<bool, Task> FinishInteraction, AtomicBoolean IsNavigating), object>(schedulerProvider.CurrentThread);
            }
            SkipConfirmLeave = false;
        }
        public const string SecondViewContractName = nameof(Views.SecondView);
        public const string AlternateSecondViewContractName = nameof(Views.AlternateSecondView);

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
            if (SkipConfirmLeave is true || m_confirmLeavePage is null)
            {
                return true;
            }
            if (!m_isNavigating.Set(true))
            {
                // Note: Without the call to Subscribe at the end, this code will never execute.
                // Note: You don't need to worry about this leaking despite Subscribe() returning an IDisposable. The WhenActivated
                //  in the SecondView ctor ensures that everything is disconnected so that the GC will take care of it. Also, Android
                //  will not dismiss the dialog properly if you try to get a reference to this and dispose it from here.
                m_confirmLeavePage.Handle((Title: "Confirm Quit", Text: "Are you sure you want to leave before the test is finished?", Stay: "Stay", Leave: "Leave", FinishInteraction: FinishCallOnNavigateBack, IsNavigating: m_isNavigating)).ObserveOn(SchedulerProvider.MainThread).Subscribe();
            }
            return false;
        }
    }
}
