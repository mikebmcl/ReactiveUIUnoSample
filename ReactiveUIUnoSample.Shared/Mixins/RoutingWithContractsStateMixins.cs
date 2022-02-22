using ReactiveUI;

using ReactiveUIRoutingWithContracts;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveUIUnoSample
{
    public static class RoutingWithContractsStateMixins
    {
        /// <summary>
        /// Removes the last item (i.e. the current ViewModel) from the <see cref="RoutingWithContractsStateUnguarded.NavigationStack"/> and returns 
        /// <see cref="RoutingWithContractsStateUnguarded.Navigate"/>. When you call <see cref="ReactiveCommand{TParam, TResult}.Execute(TParam)"/> on
        /// the returned value, the parameter must be a ViewModel that implements <see cref="IRoutableViewModelForContracts"/>, because that
        /// is the requirement imposed by <see cref="RoutingWithContractsStateUnguarded.Navigate"/>, which is what this returns.
        /// </summary>.
        /// <param name="routingState">The routing state this extension method is being called for.</param>
        /// <param name="runAfterRemoval">
        /// An optional action that will be passed the removed item after it has been removed. If there were no items on the navigation stack 
        /// or if this is null, it will not be called. Because this method does not trigger any back navigation, any custom operations you
        /// might run during back navigation will not run in this case. So if you need to dispose of your view model, for example, this gives you
        /// a chance to do so.
        /// </param>
        /// <returns><see cref="RoutingWithContractsStateUnguarded.Navigate"/></returns>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <remarks>
        /// If there is only one item on the navigation stack then this method will temporarily leave the navigation stack empty.
        /// If you are observing <see cref="RoutingWithContractsStateUnguarded.NavigationChanged"/> you need to make sure you handle that possibility
        /// appropriately. You should already be doing so since <see cref="RoutingWithContractsStateUnguarded.NavigateAndReset"/> also clears the
        /// navigation stack before navigating, but if you never use that then your observer might not be written to handle an 
        /// empty navigation stack and your unit tests might not contain any tests to ensure that your observers properly handle
        /// an empty navigation stack.
        /// </remarks>
        internal static ReactiveCommand<IViewModelAndContract, IViewModelAndContract> NavigateAndRemoveCurrent(this RoutingWithContractsStateUnguarded routingState, Action<IViewModelAndContract> runAfterRemoval = null)
        {
            if (routingState.NavigationStack.Count > 0)
            {
                IViewModelAndContract item = routingState.NavigationStack.Last();
                routingState.NavigationStack.RemoveAt(routingState.NavigationStack.Count - 1);
                runAfterRemoval?.Invoke(item);
            }
            return routingState.Navigate;
        }

        /// <summary>
        /// Removes all items from the navigation stack and returns <see cref="RoutingWithContractsStateUnguarded.Navigate"/>. This exists
        /// to give you a chance to perform any custom actions on the removed view models since they do not pass through the normal
        /// back navigation handlers. If you do not need to run any custom actions, use the built in 
        /// <see cref="RoutingWithContractsStateUnguarded.NavigateAndReset"/> property instead.
        /// </summary>
        /// <param name="routingState">The routing state this extension method is being called for.</param>
        /// <param name="runOnEachHighestToLowestIndexOrdered">
        /// An optional action that will be called on the removed items after they have all been removed. If there were no items on the 
        /// navigation stack or if this is null, it will not be called. Because this method does not trigger any back navigation, any 
        /// custom operations you might run during back navigation will not run in this case. So if you need to dispose of your view 
        /// model, for example, this gives you a chance to do so. This is called on each item in the order they existed on the stack,
        /// with the most recent item (the current view model before calling this) called first and so on until the earliest view 
        /// model (i.e. the one that was at index 0).
        /// </param>
        /// <returns></returns>
        internal static ReactiveCommand<IViewModelAndContract, IViewModelAndContract> NavigateAndReset(this RoutingWithContractsStateUnguarded routingState, Action<IViewModelAndContract> runOnEachHighestToLowestIndexOrdered)
        {
            List<IViewModelAndContract> reversedList = routingState.NavigationStack.Reverse().ToList();
            routingState.NavigationStack.Clear();
            if (runOnEachHighestToLowestIndexOrdered != null)
            {
                foreach (var item in reversedList)
                {
                    runOnEachHighestToLowestIndexOrdered.Invoke(item);
                }
            }
            return routingState.Navigate;
        }
    }
}