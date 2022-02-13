using ReactiveUI;

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace ReactiveUIUnoSample
{
    public static class RoutingStateExtensions
    {
        // TODO: Remove the text at the end of the remarks about my incorrect assertions elsewhere regarding back navigation and contract strings once I have fixed those assertions.
        /// <summary>
        /// Removes the last item (i.e. the current ViewModel) from the <see cref="RoutingState.NavigationStack"/> and returns 
        /// <see cref="RoutingState.Navigate"/>. When you call <see cref="ReactiveCommand{TParam, TResult}.Execute(TParam)"/> on
        /// the returned value, the parameter must be a ViewModel that implements <see cref="IRoutableViewModel"/>, because that
        /// is the requirement imposed by <see cref="RoutingState.Navigate"/>, which is what this returns.
        /// </summary>.
        /// <param name="routingState">The routing state this extension method is being called for.</param>
        /// <returns><see cref="RoutingState.Navigate"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <remarks>
        /// If there is only one item on the navigation stack then this method will temporarily leave the navigation stack empty.
        /// If you are observing <see cref="RoutingState.NavigationChanged"/> you need to make sure you handle that possibility
        /// appropriately. You should already be doing so since <see cref="RoutingState.NavigateAndReset"/> also clears the
        /// navigation stack before navigating, but if you never use that then your observer might not be written to handle an 
        /// empty navigation stack and your unit tests might not contain any tests to ensure that your observers properly handle
        /// an empty navigation stack. The removal of the current ViewModel from the navigation stack will not cause any problems
        /// such as attempting navigation for the same reason that NavigateAndReset does not cause any such problems when it clears
        /// the navigation stack. However when the actual navigation (forward or backward) occurs, the correct contract string (if 
        /// any) needs to be set, despite my assertions to the contrary about back navigation elsewhere (which I will clean up shortly).
        /// </remarks>
        public static ReactiveCommand<IRoutableViewModel, IRoutableViewModel> NavigateAndRemoveCurrent(this RoutingState routingState)
        {
            routingState.NavigationStack.RemoveAt(routingState.NavigationStack.Count - 1);
            return routingState.Navigate;
        }


    }
}
