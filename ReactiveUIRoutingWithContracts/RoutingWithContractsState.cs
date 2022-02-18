// Copyright (c) 2022 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

#pragma warning disable 8618

namespace ReactiveUIRoutingWithContracts
{
    //public class RoutableViewModelWithContract : ReactiveObject, IRoutableViewModelForContracts
    //{
    //    [IgnoreDataMember]
    //    public IRoutableViewModel ViewModel { get; private set; }

    //    public string Contract { get; private set; }

    //    /// <summary>
    //    /// Contructs a new instance of the type.
    //    /// </summary>
    //    /// <param name="vm">Must not be <c>null</c>.</param>
    //    /// <param name="Contract">Can be null if the ViewModel was registered without a Contract. If not null then the ViewModel must have been registered with this Contract. See <see cref="Splat.IMutableDependencyResolver.Register(Func{object?}, Type?, string?)" .</param>
    //    /// <exception cref="ArgumentNullException"></exception>
    //    public RoutableViewModelWithContract(IRoutableViewModel vm, string Contract = null)
    //    {
    //        if (vm is null)
    //        {
    //            throw new ArgumentNullException(nameof(vm), "Non-null IRoutableViewModel is required.");
    //        }
    //        ViewModel = vm;
    //        Contract = Contract;
    //    }
    //}

    //public static class RoutableViewModelWithContractMixins
    //{
    //    public static IRoutableViewModelForContracts FromIRoutableViewModel(this IRoutableViewModel value) => new RoutableViewModelWithContract(value);
    //    public static IRoutableViewModelForContracts FromIRoutableViewModel(this IRoutableViewModel value, string Contract) => new RoutableViewModelWithContract(value, Contract);
    //}

    [DataContract]
    public class RoutingWithContractsState : ReactiveObject
    {
        [IgnoreDataMember]
        private readonly IScheduler _scheduler;

        /// <summary>
        /// The goal here is to trigger the RxApp's static constructor if it hasn't already run. We don't care about this property and it should never actually be used.
        /// </summary>
        [IgnoreDataMember]
        protected static bool IgnoreMe { get; set; }

        /// <summary>
        /// Used to trigger the RxApp's static constructor if it hasn't already run. 
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoOptimization)]
        internal static bool GetValueForIgnoreMe() => RxApp.SuppressViewCommandBindingMessage;
        /// <summary>
        /// Initializes static members of the <see cref="RoutingWithContractsState"/> class.
        /// </summary>R
        static RoutingWithContractsState() => IgnoreMe = GetValueForIgnoreMe();

        /// <summary>
        /// Initializes a new instance of the <see cref="RoutingWithContractsState"/> class.
        /// </summary>
        /// <param name="scheduler">A scheduler for where to send navigation changes to.</param>
        public RoutingWithContractsState(IScheduler scheduler = null)
        {
            _scheduler = scheduler ?? RxApp.MainThreadScheduler;
            NavigationStack = new ObservableCollection<IViewModelAndContract>();
            SetupRx();
        }

        /// <summary>
        /// Gets the current navigation stack, the last element in the
        /// collection being the currently visible ViewModel.
        /// </summary>
        [IgnoreDataMember]
        public ObservableCollection<IViewModelAndContract> NavigationStack { get; }

        /// <summary>
        /// Gets or sets a command which will navigate back to the previous element in the stack.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<Unit, IViewModelAndContract> NavigateBack { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to the a new element in the stack - the Execute parameter
        /// must be a ViewModel that implements IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<IViewModelAndContract, IViewModelAndContract> Navigate { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to a new element and resets the navigation stack (i.e. the
        /// new ViewModel will now be the only element in the stack) - the
        /// Execute parameter must be a ViewModel that implements
        /// IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<IViewModelAndContract, IViewModelAndContract> NavigateAndReset { get; protected set; }

        /// <summary>
        /// Gets or sets the current view model which is to be shown for the Routing.
        /// </summary>
        [IgnoreDataMember]
        public IObservable<IViewModelAndContract> CurrentViewModel { get; protected set; }

        /// <summary>
        /// Gets or sets an observable which will signal when the Navigation changes.
        /// </summary>
        [IgnoreDataMember]
        public IObservable<IChangeSet<IViewModelAndContract>> NavigationChanged { get; protected set; } // TODO: Create Test

        [OnDeserialized]
        private void SetupRx(StreamingContext sc) => SetupRx();

        private void SetupRx()
        {
            var navigateScheduler = _scheduler;
            NavigationChanged = NavigationStack.ToObservableChangeSet();

            var countAsBehavior = Observable.Defer(() => Observable.Return(NavigationStack.Count)).Concat(NavigationChanged.CountChanged().Select(_ => NavigationStack.Count));
            NavigateBack =
                ReactiveCommand.CreateFromObservable<IViewModelAndContract>(
                                                                          () =>
                                                                          {
                                                                              NavigationStack.RemoveAt(NavigationStack.Count - 1);
                                                                              return Observable.Return(NavigationStack.Count > 0 ? NavigationStack[NavigationStack.Count - 1] : default).ObserveOn(navigateScheduler);
                                                                          },
                                                                          countAsBehavior.Select(x => x > 1));

            Navigate = ReactiveCommand.CreateFromObservable<IViewModelAndContract, IViewModelAndContract>(
             vm =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (vm is null)
                 {
                     return Observable.Return(ViewModelAndContract.DoNothing).ObserveOn(navigateScheduler);
                     //return Observable.Empty<IViewModelAndContract>().ObserveOn(navigateScheduler);
                 }
                 if (vm.ViewModel is null && vm != ViewModelAndContract.DoNothing)
                 {
                     throw new Exception("Navigate must be called with a non-null IViewModelAndContract.ViewModel");
                 }
                 if (vm != ViewModelAndContract.DoNothing)
                 {
                     NavigationStack.Add(vm);
                 }
                 return Observable.Return(vm).ObserveOn(navigateScheduler);
             });

            NavigateAndReset = ReactiveCommand.CreateFromObservable<IViewModelAndContract, IViewModelAndContract>(
             vm =>
             {
                 NavigationStack.Clear();
                 return Navigate.Execute(vm);
             });

            CurrentViewModel = Observable.Defer(() => Observable.Return(NavigationStack.LastOrDefault())).Concat(NavigationChanged.Select(_ => NavigationStack.LastOrDefault()));
        }
    }
}