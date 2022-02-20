using System;
using System.Collections.ObjectModel;

namespace ReactiveUIRoutingWithContracts
{
    /// <summary>
    /// Holds the data that is used with <see cref="RoutingWithContractsState.NavigateAndApplyFunc"/> and <see cref="RoutingWithContractsState.NavigateAndApplyFuncWithStatus"/>.
    /// </summary>
    public class RoutingWithContractsStateApplyFuncData
    {
        public RoutingWithContractsStateApplyFuncData(Func<ObservableCollection<IViewModelAndContract>, bool> fn, IViewModelAndContract vmc)
        {
            Fn = fn;
            ViewModelAndContract = vmc;
        }

        /// <summary>
        /// This is passed the navigation stack of a <see cref="RoutingStateWithContracts"/> object to examine and manipulate. It should return
        /// <code>true</code>
        /// If navigation to <see cref="ViewModelContract"/> should proceed.
        /// <code>false</code>
        /// If navigation should not proceed.
        /// </summary>
        public Func<ObservableCollection<IViewModelAndContract>, bool> Fn { get; }

        /// <summary>
        /// The view model to navigate to if navigation should proceed.
        /// </summary>
        public IViewModelAndContract ViewModelAndContract { get; }
    }
}
