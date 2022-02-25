using ReactiveUI;
using ReactiveUI.Uno;
using ReactiveUI.Fody.Helpers;
using ReactiveUIRoutingWithContracts;

using System;
using System.ComponentModel;

namespace ReactiveUIUnoSample.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IRoutableViewModelForContracts, INotifyPropertyChanged
    {
        /// <summary>
        /// A string token representing the current ViewModel, such as 'login' or 'user'. Can be null
        /// </summary>
        public string UrlPathSegment { get; }

        public IScreenForContracts HostScreenWithContract { get; }
        ///// <summary>
        ///// Gets the IScreen that this ViewModel is currently being shown in. This is
        ///// passed into the ViewModel in the Constructor and saved as a ReadOnly Property.
        ///// </summary>
        //public IScreen HostScreen { get; }

        /// <summary>
        /// Use the appropriate thread scheduler from here rather than directly from, e.g., RxApp. This lets us do unit testing with ReactiveUI.UnitTesting - see:
        /// http://introtorx.com/Content/v1.0.10621.0/16_TestingRx.html
        /// </summary>
        public ISchedulerProvider SchedulerProvider { get; }

        /// <summary>
        /// Creates a string from a <see cref="Guid"/>. 
        /// </summary>
        /// <param name="desiredLength">The desired length of the returned string. Pass in 0 to get the maximum length (32)</param>
        /// <returns>The string that results from creating a new <see cref="Guid"/>, converting it to a string, removing all '-' characters, and returning the result of calling <see cref="string.Substring(int, int)"/> with a start index of 0 and a length of <paramref name="desiredLength"/> (constrained to the range [5, 32]).</returns>
        protected static string GenerateStringForUrlPathSegment(int desiredLength = 8)
        {
            desiredLength = desiredLength == 0 ? 32 : desiredLength;
            desiredLength = Math.Min(32, Math.Max(5, desiredLength));
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, desiredLength);
        }

        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            (this as IReactiveObject).RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// </summary>
        /// <param name="hostScreen">The IScreen that this ViewModel is currently being shown in.</param>
        /// <param name="schedulerProvider">The collection of schedulers to be used for any actions that need to be run on the UI thread, the current thread, or the thread pool.</param>
        /// <param name="urlPathSegment">The string value for <see cref="UrlPathSegment"/>, which will be this value or, if this is null, the result of calling <see cref="GenerateStringForUrlPathSegment(int)"/> unless <paramref name="useNullUrlPathSegment"/> is true, in which case <see cref="UrlPathSegment"/> will be null.</param>
        /// <param name="useNullUrlPathSegment">Forces <see cref="UrlPathSegment"/> to be null if <paramref name="urlPathSegment"/> is null.</param>
        public ViewModelBase(IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false)
        {
            HostScreenWithContract = hostScreen;
            SchedulerProvider = schedulerProvider;
            UrlPathSegment = urlPathSegment ?? (useNullUrlPathSegment ? null : GenerateStringForUrlPathSegment());
        }
    }
}
