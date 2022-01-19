﻿using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveUIUnoSample.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject, IRoutableViewModel
    {
        /// <summary>
        /// A string token representing the current ViewModel, such as 'login' or 'user'. Can be null
        /// </summary>
        public string UrlPathSegment { get; }
        /// <summary>
        /// Gets the IScreen that this ViewModel is currently being shown in. This is
        /// passed into the ViewModel in the Constructor and saved as a ReadOnly Property.
        /// </summary>
        public IScreen HostScreen { get; }

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

        /// <summary>
        /// </summary>
        /// <param name="hostScreen">The IScreen that this ViewModel is currently being shown in.</param>
        /// <param name="urlPathSegment">The string value for <see cref="UrlPathSegment"/>, which will be this value or, if this is null, the result of calling <see cref="GenerateStringForUrlPathSegment(int)"/> unless <paramref name="useNullUrlPathSegment"/> is true, in which case <see cref="UrlPathSegment"/> will be null.</param>
        /// <param name="useNullUrlPathSegment">Forces <see cref="UrlPathSegment"/> to be null if <paramref name="urlPathSegment"/> is null.</param>
        public ViewModelBase(IScreen hostScreen, string urlPathSegment = null, bool useNullUrlPathSegment = false)
        {
            HostScreen = hostScreen;
            UrlPathSegment = urlPathSegment ?? (useNullUrlPathSegment ? null : GenerateStringForUrlPathSegment());
        }
    }
}