using ReactiveUI;

using ReactiveUIRoutingWithContracts;

using Windows.System;
using Windows.UI.Xaml.Controls;

namespace ReactiveUIUnoSample.ViewModels
{
    public abstract class DisplayViewModelBase : ViewModelBase
    {
        /// <summary>
        /// The desired header content for the <see cref="NavigationView"/>. If it is a string, the string will be encapsulated in a <see cref="TextBlock"/>.
        /// </summary>
        public abstract object HeaderContent { get; set; }

        public bool NoHeader { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="hostScreen">The IScreen that this ViewModel is currently being shown in.</param>
        /// <param name="urlPathSegment">The string value for <see cref="ViewModelBase.UrlPathSegment"/>, which will be this value or, if this is null, the result of calling <see cref="ViewModelBase.GenerateStringForUrlPathSegment(int)"/> unless <paramref name="useNullUrlPathSegment"/> is true, in which case <see cref="ViewModelBase.UrlPathSegment"/> will be null.</param>
        /// <param name="useNullUrlPathSegment">Forces <see cref="ViewModelBase.UrlPathSegment"/> to be null if <paramref name="urlPathSegment"/> is null.</param>
        protected DisplayViewModelBase(IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment) { }
    }
}
