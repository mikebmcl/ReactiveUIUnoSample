using System;
using System.Collections.ObjectModel;

namespace ReactiveUIRoutingWithContracts
{
    public class ModifyArgumentAndStatus : NavigateArgumentAndStatus<Action<ObservableCollection<IViewModelAndContract>>>
    {
        public ModifyArgumentAndStatus(Action<ObservableCollection<IViewModelAndContract>> fn) : base(fn) { }
    }

    //public class UnitArgumentAndStatus : NavigateArgumentAndStatus<System.Reactive.Unit>
    /// <summary>
    /// Used by <see cref="RoutingWithContractState"/> to receive the navigation argument and report back the status of
    /// whether the call was discarded because navigation was already in progress.
    /// </summary>
    /// <typeparam name="T">The argument for navigation. Back navigation uses <see cref="System.Reactive.Unit"/>. All other navigations use <see cref="IViewModelAndContract"/></typeparam>
    public class NavigateArgumentAndStatus<T>
    {
        /// <summary>
        /// The argument for navigation. Back navigation uses <see cref="System.Reactive.Unit"/> and the value <see cref="System.Reactive.Unit.Default"/> can be used for convenience.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// If true, navigation was already occurring so the navigation attempt was ignored. If false navigation was attempted. This is not an indication that navigation succeeded. Observe <see cref="ReactiveUI.ReactiveCommandBase{TParam, TResult}.ThrownExceptions"/> to check for exceptions.
        /// </summary>
        public bool AlreadyNavigating { get; private set; }

        public NavigateArgumentAndStatus(T value)
        {
            Value = value;
        }

        /// <summary>
        /// Intended for use by <see cref="RoutingWithContractsState"/>. Do not call.
        /// </summary>
        /// <param name="alreadyNavigating"></param>
        public void SetAlreadyNavigating(bool alreadyNavigating)
        {
            AlreadyNavigating = alreadyNavigating;
        }

        /// <summary>
        /// Returns a new instance of this class meant to be used with back navigation.
        /// </summary>
        /// <returns>A new instance of this class with <see cref="System.Reactive.Unit.Default"/> as its <see cref="Value"/>.</returns>
        public static NavigateArgumentAndStatus<System.Reactive.Unit> GetInstanceForUnit()
        {
            return new NavigateArgumentAndStatus<System.Reactive.Unit>(System.Reactive.Unit.Default);
        }
    }
}
