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
        /// and ensure that anything that needs to run in on the UI thread inside that method is scheduled to run on the UI thread's
        /// <see cref="CoreDispatcher"/>, which can be obtained from <see cref="MainThread.GetCoreDispatcherForMainThread"/>.
        /// 
        /// If you do use that method of displaying a popup/dialog, you should return false from this method and then have
        /// the background task initiate navigation back on the main thread if the user confirms that they would like to
        /// leave without finishing/saving whatever they were doing.
        /// 
        /// It's highly recommended that you use interactions for UI operations: <see cref="ReactiveUI.Interaction{TInput, TOutput}"/>
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
