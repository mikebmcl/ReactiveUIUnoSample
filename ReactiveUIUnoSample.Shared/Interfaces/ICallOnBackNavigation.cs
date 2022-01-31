using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace ReactiveUIUnoSample.Interfaces
{
    /// <summary>
    /// Implement this interface on your <see cref="ViewModels.DisplayViewModelBase"/>-derived view model if you want
    /// to execute custom logic, such as a dialog to confirm leaving the page or to save state before leaving the page,
    /// when the user attempts to navigate away from the associated view using a system back button (web browser back, 
    /// Android back button, etc.), which is handled in <see cref="ViewModels.MainViewModel"/> for cross-platform reasons.
    /// For more information, see: https://platform.uno/docs/articles/features/native-frame-nav.html .
    /// NOTE: On WASM, if the user repeatedly presses the browser back button, this may not be called, especially if the 
    /// current page does not implement this but a previous page in the navigation stack does.
    /// NOTE: On iOS, this cannot be used to prevent backward navigation. See the summary section of <see cref="CallOnBackNavigation"/>
    /// for suggested workarounds. NOTE: It's generally a bad idea to have multiple views for a view model that implements
    /// this because either every view must be configured to work properly, including setting bindings for things it doesn't 
    /// use, for everything <see cref="ICallOnBackNavigation.CallOnBackNavigation"/> does, or the view model needs to be tailored
    /// to deal with each view that uses it in order to avoid trying to invoke a binding that doesn't exist on one or more of
    /// the views. That said, you can use multiple views for a view model that implements this; you just need to be careful to
    /// ensure that either the views or the view model handle the custom back navigation behavior in <see cref="ICallOnBackNavigation.CallOnBackNavigation"/>
    /// appropriately.
    /// </summary>
    internal interface ICallOnBackNavigation
    {
        /// <summary>
        /// This is called if the user uses a system back button (browser back, Android back button, etc.) to navigate back.
        /// The primary use case for it is to confirm that the user really wants to navigate away. For example, they might
        /// be part way through a test or they might have unsaved changes.
        /// 
        /// Another use case for this is to save the current state before exiting the page. Interaction with the user is not
        /// required. This method could also simply refuse to let the user navigate away via the system buttons, but this is
        /// not recommended as it is both poor design practice and is likely to make the user force-quit your application and
        /// uninstall it.
        /// 
        /// This method should not attempt call anything that needs to run on the UI thread as this will cause the application
        /// to silently crash on certain platforms. If you wish to display a <see cref="Popup"/>, a <see cref="ContentDialog"/>,
        /// etc., you should schedule them to run on a background thread, e.g. by using a non-awaited <see cref="Task.Run(System.Action)"/>
        /// and ensure that anything that needs to run in on the UI thread inside that method is scheduled to run on the UI thread. This 
        /// should be done using the <see cref="ISchedulerProvider.MainThread"/> from the view model's 
        /// <see cref="ViewModels.ViewModelBase.SchedulerProvider"/> together with the 
        /// <see cref="Observable.ObserveOn{TSource}(IObservable{TSource}, System.Reactive.Concurrency.IScheduler)"/> extension
        /// method for <see cref="IObservable{T}"/>. An example can be seen in <see cref="ViewModels.SecondViewModel.CallOnBackNavigation"/>.
        /// 
        /// If you do use that method of displaying a popup/dialog, you should return false from this method and then have
        /// the background task initiate navigation back on the main thread if the user confirms that they would like to
        /// leave without finishing/saving whatever they were doing.
        /// 
        /// It's highly recommended that you use interactions for UI operations: <see cref="ReactiveUI.Interaction{TInput, TOutput}"/>
        /// 
        /// IMPORTANT: On WASM, if the user repeatedly presses the browser back button, this may not be called, especially if the 
        /// current page does not implement this but a previous page in the navigation stack does. If important data might be lost 
        /// or other problems might occur, you should consider mitigation strategies such as saving state at appropriate times, and
        /// giving the user the option to return to where they last left off the next time they start the application or on whatever
        /// page they stop at if they do not competely exit the application by multiple, rapid browser back button presses. This only 
        /// applies to browser back button; the application's NavigationView back button properly ignores multiple clicks/taps and 
        /// will run this method.
        /// 
        /// IMPORTANT: You cannot stop back button navigation from proceeding on iOS. For that platform you should consider
        /// doing something such as using this to save state and then on the page they navigated back to showing a dialog asking
        /// if they would like to resume/complete whatever they were doing on the page they navigated away from. If they
        /// would, then navigate back to the page they left and restore the saved state so that they can proceed as if they had 
        /// never navigated away.
        /// 
        /// You can see a demonstration of how to use a ContentDialog to confirm that the user would like to leave (not applicable 
        /// to iOS) in <see cref="Views.SecondView"/> and its view model <see cref="ViewModels.SecondViewModel"/>.
        /// </summary>
        /// <returns>
        /// <code>true</code> if navigation back should proceed
        /// <code>false</code> if navigation back should be cancelled
        /// </returns>
        bool CallOnBackNavigation();
    }
}
