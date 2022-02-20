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
using System.Threading;

using DynamicData;
using DynamicData.Binding;

using ReactiveUI;

#pragma warning disable 8618

namespace ReactiveUIRoutingWithContracts
{
    /// <summary>
    /// This router is designed to guard against multiple, competing navigation requests. Internally it tracks whether a navigation is currently
    /// running. If so, it ignores any request for any form of navigation until the current navigation finishes (either successfully or by throwing
    /// an exception). Those requests are quietly discarded so make sure your code appropriately handles situations where navigation fails. Using
    /// <see cref="IsNavigating"/> as part of your CanExecute is probably the simplest since <see cref="ReactiveCommand{TParam, TResult}"/> will
    /// not execute if the can execute check fails when execute is called. As a consequence of this, it can be dangerous to modify
    /// <see cref="_navigationStack"/> directly because the guards against multiple navigation do not protect against direct modifications to the stack
    /// and direct modifications may put the router into an unusable state.
    /// </summary>
    [DataContract]
    public class RoutingWithContractsState : ReactiveObject, IDisposable
    {
        private class AtomicBoolean
        {
            private const int m_falseValue = 0;
            private const int m_trueValue = 1;
            private int m_boolValue;

            public AtomicBoolean(bool initialValue = false)
            {
                m_boolValue = initialValue ? m_trueValue : m_falseValue;
            }

            /// <summary>
            /// Gets the current value of this.
            /// </summary>
            /// <returns>The current value of this.</returns>
            /// <remarks>
            /// This only performs an immediate check on the current value. The value could change before control even returns to the method calling this.
            /// Because of this, the uses for this method are very limited. You should not use it to do something like:
            /// <code>
            /// // WARNING: DO NOT DO THIS
            /// if (!ab.Get()) { ab.Set(true); runMyCode(); ab.Set(false); }
            /// </code>
            /// That is not safe. Instead you should use something like this:
            /// <code>
            /// if (!ab.Set(true)) { try { runMyCode(); } finally { ab.ForceToFalse(); } }
            /// </code>
            /// The use of try-finally (or try-catch-finally) is critical when using the above pattern. If an exception is thrown during the execution of <c>runMyCode</c>, 
            /// without ensuring that the value of <c>ab</c> is set back to <c>false</c> in a finally block could result in the value of <c>ab</c> remaining <c>true</c>. 
            /// Any subsequent use of <c>ab</c> in the same way as above would result in your code never running since <c>ab</c> would always be <c>true</c>. And you would
            /// never be able to detect if <c>ab</c> was true because of a failure to set it to false somewhere else or because it was legitimately being used by some other
            /// code on another thread.
            /// </remarks>
            public bool Get()
            {
                return Interlocked.CompareExchange(ref m_boolValue, m_trueValue, m_trueValue) == m_trueValue;
            }

            /// <summary>
            /// Forces the value of this to be false regardless of its current value. It is functionally equivalent to
            /// <c>Set(false)</c> but skips a branch that <see cref="Set(bool)"/> requires in order to return the previous
            /// value.
            /// </summary>
            public void ForceToFalse()
            {
                int existingValue = Interlocked.Exchange(ref m_boolValue, m_falseValue);
            }

            /// <summary>
            /// Sets the current value of this to <paramref name="value"/> and returns the previous value of this. If the previous value
            /// and <paramref name="value"/> are the same, the value of this remains unchanged.
            /// </summary>
            /// <param name="value"></param>
            /// <returns>The previous value.</returns>
            public bool Set(bool value)
            {
                bool previousValue = Get();
                return value
                    ? Interlocked.CompareExchange(ref m_boolValue, m_trueValue, m_falseValue) == m_trueValue
                    : Interlocked.CompareExchange(ref m_boolValue, m_falseValue, m_trueValue) == m_trueValue;
            }
        }

        private class IViewModelAndContractObserver : IObserver<IViewModelAndContract>, IDisposable
        {
            private IDisposable _unsubscriber;
            private CancellationToken _unsubscribeToken;
            private bool disposedValue;
            private readonly RoutingWithContractsState _routingWithContractsState;

            public IViewModelAndContractObserver(RoutingWithContractsState routingWithContractsState)
            {
                _routingWithContractsState = routingWithContractsState;
            }

            /// <summary>
            /// Use this to subscribe by passing in the <see cref="IObservable{T}"/> rather than calling <see cref="IObservable{T}.Subscribe(IObserver{T})"/> 
            /// directly since this lets us properly handle disposing of resources associated with subscribing.
            /// </summary>
            /// <param name="provider"></param>
            public virtual IViewModelAndContractObserver Subscribe(IObservable<IViewModelAndContract> provider, IScheduler observeOn, CancellationToken unsubscribeToken = default)
            {
                if (provider != null)
                {
                    _unsubscriber?.Dispose();
                    _unsubscriber = provider.ObserveOn(observeOn).Subscribe(this);
                    _unsubscribeToken = unsubscribeToken;
                    if (_unsubscribeToken.CanBeCanceled)
                    {
                        _unsubscribeToken.Register(Unsubscribe);
                    }
                }
                return this;
            }

            public virtual void Unsubscribe()
            {
                _unsubscriber?.Dispose();
            }

            public void OnCompleted()
            {
                // Note: This is not called after every navigation but instead when the IObservable<T> itself is finished its business completely, i.e.
                //  it won't be sending any more notifications (presumably because the app is shutting down). See:
                //  https://docs.microsoft.com/en-us/dotnet/api/system.iobserver-1.oncompleted
                _unsubscriber?.Dispose();
            }

            public void OnError(Exception error)
            {
                _routingWithContractsState._userManipulatingStack = false;
                _routingWithContractsState._isNavigating.ForceToFalse();
                _routingWithContractsState.IsNavigating = false;
                throw error;
            }

            public void OnNext(IViewModelAndContract value)
            {
                if (_routingWithContractsState._userManipulatingStack)
                {
                    return;
                }
                if (_routingWithContractsState._navigatingToIViewModelAndContractWeakRef.TryGetTarget(out IViewModelAndContract vmc))
                {
                    if (vmc == value)
                    {
                        _routingWithContractsState._navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                        _routingWithContractsState._isNavigating.ForceToFalse();
                        _routingWithContractsState.IsNavigating = false;
                    }
                }
                else
                {
                    if (value == null && _routingWithContractsState._stackWasCleared)
                    {
                        _routingWithContractsState._stackWasCleared = false;
                        _routingWithContractsState._isNavigating.ForceToFalse();
                        _routingWithContractsState.IsNavigating = false;
                    }
                    else
                    {
                        if (_routingWithContractsState._navigationStack.Count == 0)
                        {
                            // We can end up here when the router is first created and this is wired up to it
                            _routingWithContractsState._isNavigating.ForceToFalse();
                            _routingWithContractsState.IsNavigating = false;
                        }
                        else
                        {
                            // Huh?
                            _routingWithContractsState._isNavigating.ForceToFalse();
                            _routingWithContractsState.IsNavigating = false;
                            throw new WarningException("Expecting to find the view model being navigated to in order to match against the observed change, but the WeakReference was empty.");
                        }
                    }
                }
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        _unsubscriber?.Dispose();
                    }

                    disposedValue = true;
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

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
            _navigationStack = new ObservableCollection<IViewModelAndContract>();
            NavigationStack = new ReadOnlyObservableCollection<IViewModelAndContract>(_navigationStack);
            _iViewModelAndContractObserver = new IViewModelAndContractObserver(this);
            SetupRx();
        }

        /// <summary>
        /// Gets the current navigation stack, the last element in the
        /// collection being the currently visible ViewModel. It is recommended that you do not
        /// modify this directly. Bad things might happen if you do. 
        /// </summary>
        [IgnoreDataMember]
        private ObservableCollection<IViewModelAndContract> _navigationStack { get; }

        /// <summary>
        /// Gets the current navigation stack, the last element in the
        /// collection being the currently visible ViewModel. 
        /// </summary>
        //Note: The functionality that prevents multiple navigations depends on the stack not being modified externally hence the ReadOnly version being publicly exposed.
        [IgnoreDataMember]
        public ReadOnlyObservableCollection<IViewModelAndContract> NavigationStack { get; }

        /// <summary>
        /// Gets or sets a command which will navigate back to the previous element in the stack.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<Unit, IViewModelAndContract> NavigateBack { get; protected set; }

        /// <summary>
        /// Gets or sets a command which will navigate back to the previous element in the stack.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<NavigateArgumentAndStatus<Unit>, IViewModelAndContract> NavigateBackWithStatus { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to the a new element in the stack - the Execute parameter
        /// must be a ViewModel that implements IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<IViewModelAndContract, IViewModelAndContract> Navigate { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to the a new element in the stack - the Execute parameter
        /// must be a ViewModel that implements IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<NavigateArgumentAndStatus<IViewModelAndContract>, IViewModelAndContract> NavigateWithStatus { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to a new element and resets the navigation stack (i.e. the
        /// new ViewModel will now be the only element in the stack) - the
        /// Execute parameter must be a ViewModel that implements
        /// IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<IViewModelAndContract, IViewModelAndContract> NavigateAndReset { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to a new element and resets the navigation stack (i.e. the
        /// new ViewModel will now be the only element in the stack) - the
        /// Execute parameter must be a ViewModel that implements
        /// IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<NavigateArgumentAndStatus<IViewModelAndContract>, IViewModelAndContract> NavigateAndResetWithStatus { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to a new element and removes the last element from the navigation stack (i.e.
        /// the new ViewModel essentially replaces the view model that is the current view model but it is done by removal of
        /// the current view model, if there is one, followed by adding the new view model to the stack) - the Execute parameter
        /// must be a ViewModel that implements IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<IViewModelAndContract, IViewModelAndContract> NavigateAndRemoveCurrent { get; protected set; }

        /// <summary>
        /// Gets or sets a command that navigates to a new element and removes the last element from the navigation stack (i.e.
        /// the new ViewModel essentially replaces the view model that is the current view model but it is done by removal of
        /// the current view model, if there is one, followed by adding the new view model to the stack) - the Execute parameter
        /// must be a ViewModel that implements IRoutableViewModelForContracts.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<NavigateArgumentAndStatus<IViewModelAndContract>, IViewModelAndContract> NavigateAndRemoveCurrentWithStatus { get; protected set; }

        /// <summary>
        /// Gets or sets a command that invokes an <see cref="Func{T, TResult}"/> that is passed the navigation stack to examine
        /// and manipulate it and then report whether or not navigation should occur. The Execute parameter has the following
        /// requirements: If the <see cref="IViewModelAndContract"/> of the <see cref="Func{T, TResult}"/> is null then Func will
        /// not run and nothing will change on the stack. If that <see cref="IViewModelAndContract"/> has a null
        /// <see cref="IRoutableViewModelForContracts"/> and the <see cref="IViewModelAndContract"/> is not
        /// <see cref="ViewModelAndContract.DoNothing"/>, an exception will be thrown. If no exception was thrown, the Func will
        /// run. If the Func returns true and <see cref="IViewModelAndContract"/> is not <see cref="ViewModelAndContract.DoNothing"/>,
        /// navigation to the supplied <see cref="IViewModelAndContract"/> will proceed. If false, no navigation will occur.
        /// Regardless of whether or not navigation will occur, before the Func runs, <see cref="IsNavigating"/> will be set to
        /// <c>true</c>. If no navigation will occur after the Func runs, if the Func throws an exception, or if Func is null, then
        /// <see cref="IsNavigating"/> will be set to <c>false</c> and no navigation will occur. If navigation does occur, then it
        /// will be set to false when the navigation has completed or errored due to an exception. 
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<RoutingWithContractsStateApplyFuncData, IViewModelAndContract> NavigateAndApplyFunc { get; protected set; }

        /// <summary>
        /// Gets or sets a command that invokes <see cref="RoutingWithContractsStateApplyFuncData.Fn"/> an <see cref="Func{T, TResult}"/> that . The Execute parameter has the following
        /// requirements: If the <see cref="IViewModelAndContract"/> of the <see cref="Func{T, TResult}"/> is null then Func will
        /// not run and nothing will change on the stack. If that <see cref="IViewModelAndContract"/> has a null
        /// <see cref="IRoutableViewModelForContracts"/> and the <see cref="IViewModelAndContract"/> is not
        /// <see cref="ViewModelAndContract.DoNothing"/>, an exception will be thrown. If no exception was thrown, the Func will
        /// run. If the Func returns true and <see cref="IViewModelAndContract"/> is not <see cref="ViewModelAndContract.DoNothing"/>,
        /// navigation to the supplied <see cref="IViewModelAndContract"/> will proceed. If false, no navigation will occur.
        /// Regardless of whether or not navigation will occur, before the Func runs, <see cref="IsNavigating"/> will be set to
        /// <c>true</c>. If no navigation will occur after the Func runs, if the Func throws an exception, or if Func is null, then
        /// <see cref="IsNavigating"/> will be set to <c>false</c> and no navigation will occur. If navigation does occur, then it
        /// will be set to false when the navigation has completed or errored due to an exception.
        /// </summary>
        [IgnoreDataMember]
        public ReactiveCommand<NavigateArgumentAndStatus<RoutingWithContractsStateApplyFuncData>, IViewModelAndContract> NavigateAndApplyFuncWithStatus { get; protected set; }

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

        /// <summary>
        /// This is a notification property and so can be used in your can execute checks to disable navigation buttons when there is already a navigation in progress.
        /// </summary>
        [IgnoreDataMember]
        [ReactiveUI.Fody.Helpers.Reactive]
        public bool IsNavigating { get; set; }

        [IgnoreDataMember]
        private readonly AtomicBoolean _isNavigating = new AtomicBoolean();

        [IgnoreDataMember]
        private readonly IViewModelAndContractObserver _iViewModelAndContractObserver;

        [IgnoreDataMember]
        private readonly WeakReference<IViewModelAndContract> _navigatingToIViewModelAndContractWeakRef = new WeakReference<IViewModelAndContract>(null);
        [IgnoreDataMember]
        private bool _stackWasCleared;

        [IgnoreDataMember]
        private bool _userManipulatingStack;

        [IgnoreDataMember]
        private bool _disposedValue;

        [IgnoreDataMember]
        private const string DoNothingContract = "{8F2F167E-2707-48BD-AC7C-F1E818E760B1}";
        [IgnoreDataMember]
        private static ViewModelAndContract DoNothing { get; } = new ViewModelAndContract(null, DoNothingContract);

        [OnDeserialized]
        private void SetupRx(StreamingContext sc) => SetupRx();

        private void SetupRx()
        {
            var navigateScheduler = _scheduler;
            NavigationChanged = _navigationStack.ToObservableChangeSet();

            var countAsBehavior = Observable.Defer(() => Observable.Return(_navigationStack.Count)).Concat(NavigationChanged.CountChanged().Select(_ => _navigationStack.Count));
            NavigateBack =
                ReactiveCommand.CreateFromObservable<IViewModelAndContract>(
                    () =>
                    {
                        if (_isNavigating.Set(true))
                        {
                            return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                        }
                        try
                        {
                            IsNavigating = true;
                            _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                            IViewModelAndContract navigatingTo = _navigationStack.Count > 1 ? _navigationStack[_navigationStack.Count - 2] : default;
                            _navigatingToIViewModelAndContractWeakRef.SetTarget(navigatingTo);
                            _stackWasCleared = navigatingTo == null;
                            _navigationStack.RemoveAt(_navigationStack.Count - 1);
                            return Observable.Return(navigatingTo).ObserveOn(navigateScheduler);
                        }
                        catch
                        {
                            _stackWasCleared = false;
                            _isNavigating.ForceToFalse();
                            IsNavigating = false;
                            throw;
                        }
                    },
                    countAsBehavior.Select(x => x > 1));

            NavigateBackWithStatus =
                ReactiveCommand.CreateFromObservable<NavigateArgumentAndStatus<Unit>, IViewModelAndContract>(
                    status =>
                    {
                        if (status is null)
                        {
                            throw new Exception($"NavigateBackWithStatus must be called with a non-null value.");
                        }
                        if (_isNavigating.Set(true))
                        {
                            status.SetAlreadyNavigating(true);
                            return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                        }
                        try
                        {
                            IsNavigating = true;
                            status.SetAlreadyNavigating(false);
                            _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                            IViewModelAndContract navigatingTo = _navigationStack.Count > 1 ? _navigationStack[_navigationStack.Count - 2] : default;
                            _navigatingToIViewModelAndContractWeakRef.SetTarget(navigatingTo);
                            _stackWasCleared = navigatingTo == null;
                            _navigationStack.RemoveAt(_navigationStack.Count - 1);
                            return Observable.Return(navigatingTo).ObserveOn(navigateScheduler);
                        }
                        catch
                        {
                            _stackWasCleared = false;
                            _isNavigating.ForceToFalse();
                            IsNavigating = false;
                            throw;
                        }
                    },
                    countAsBehavior.Select(x => x > 1));

            Navigate = ReactiveCommand.CreateFromObservable<IViewModelAndContract, IViewModelAndContract>(
             vm =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (vm is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 if (vm.ViewModel is null && vm != DoNothing)
                 {
                     throw new Exception("Navigate must be called with a non-null IViewModelAndContract.ViewModel");
                 }
                 if (_isNavigating.Set(true))
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (vm != DoNothing)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         vm = _navigationStack.Count > 0 ? _navigationStack[_navigationStack.Count - 1] : default;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _stackWasCleared = vm == null;
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            NavigateWithStatus = ReactiveCommand.CreateFromObservable<NavigateArgumentAndStatus<IViewModelAndContract>, IViewModelAndContract>(
             status =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (status is null || status.Value is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }

                 var vm = status.Value;

                 if (vm.ViewModel is null && vm != DoNothing)
                 {
                     throw new Exception("NavigateWithStatus must be called with a non-null IViewModelAndContract.ViewModel");
                 }
                 if (_isNavigating.Set(true))
                 {
                     status.SetAlreadyNavigating(true);
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     status.SetAlreadyNavigating(false);
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (vm != DoNothing)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         vm = _navigationStack.Count > 0 ? _navigationStack[_navigationStack.Count - 1] : default;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _stackWasCleared = vm == null;
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            NavigateAndReset = ReactiveCommand.CreateFromObservable<IViewModelAndContract, IViewModelAndContract>(
             vm =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (vm is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 if (vm.ViewModel is null && vm != DoNothing)
                 {
                     throw new Exception("NavigateAndReset must be called with a non-null IViewModelAndContract.ViewModel");
                 }
                 if (_isNavigating.Set(true))
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (vm != DoNothing)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _navigationStack.Clear();
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         vm = null;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                         _stackWasCleared = true;
                         _navigationStack.Clear();
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            NavigateAndResetWithStatus = ReactiveCommand.CreateFromObservable<NavigateArgumentAndStatus<IViewModelAndContract>, IViewModelAndContract>(
             status =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (status is null || status.Value is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }

                 var vm = status.Value;

                 if (vm.ViewModel is null && vm != DoNothing)
                 {
                     throw new Exception("NavigateAndReset must be called with a non-null IViewModelAndContract.ViewModel");
                 }
                 if (_isNavigating.Set(true))
                 {
                     status.SetAlreadyNavigating(true);
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     status.SetAlreadyNavigating(false);
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (vm != DoNothing)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _navigationStack.Clear();
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         vm = null;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _stackWasCleared = true;
                         _navigationStack.Clear();
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            NavigateAndRemoveCurrent = ReactiveCommand.CreateFromObservable<IViewModelAndContract, IViewModelAndContract>(
             vm =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (vm is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 if (vm.ViewModel is null && vm != DoNothing)
                 {
                     throw new Exception("NavigateAndRemoveCurrent must be called with a non-null IViewModelAndContract.ViewModel");
                 }
                 if (_isNavigating.Set(true))
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (vm != DoNothing)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         if (_navigationStack.Count > 0)
                         {
                             _navigationStack.RemoveAt(_navigationStack.Count - 1);
                         }
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         vm = _navigationStack.Count > 0 ? _navigationStack[_navigationStack.Count - 1] : default;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _stackWasCleared = vm == null;
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            NavigateAndRemoveCurrentWithStatus = ReactiveCommand.CreateFromObservable<NavigateArgumentAndStatus<IViewModelAndContract>, IViewModelAndContract>(
             status =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (status is null || status.Value is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }

                 var vm = status.Value;

                 if (vm.ViewModel is null && vm != DoNothing)
                 {
                     throw new Exception("NavigateAndRemoveCurrent must be called with a non-null IViewModelAndContract.ViewModel");
                 }
                 if (_isNavigating.Set(true))
                 {
                     status.SetAlreadyNavigating(true);
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     status.SetAlreadyNavigating(false);
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (vm != DoNothing)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         if (_navigationStack.Count > 0)
                         {
                             _navigationStack.RemoveAt(_navigationStack.Count - 1);
                         }
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         vm = _navigationStack.Count > 0 ? _navigationStack[_navigationStack.Count - 1] : default;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _stackWasCleared = vm == null;
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            NavigateAndApplyFunc = ReactiveCommand.CreateFromObservable<RoutingWithContractsStateApplyFuncData, IViewModelAndContract>(
             tup =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (tup.ViewModelAndContract is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 if (_isNavigating.Set(true))
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (tup.ViewModelAndContract?.ViewModel is null && tup.ViewModelAndContract != DoNothing)
                     {
                         throw new Exception("NavigateAndApplyFunc must receive a non-null IViewModelAndContract with a non-null IViewModelAndContract.ViewModel");
                     }
                     var vm = tup.ViewModelAndContract;
                     // This is how we avoid having the observer accidentally switch state to navigation is over in case the user's action causes its OnNext to get the same view model that is the one that will actually be navigated to at the end.
                     _userManipulatingStack = true;
                     var navigate = tup.Fn?.Invoke(_navigationStack) ?? false;
                     _userManipulatingStack = false;
                     if (vm != DoNothing && navigate)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         // If the user kept the current view model the same then this shouldn't do anything in terms of changing the view. If they did change it then this will cause the view to update to the view model the user's changes left as the most recent view model.
                         vm = _navigationStack.Count > 0 ? _navigationStack[_navigationStack.Count - 1] : default;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _stackWasCleared = vm == null;
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _userManipulatingStack = false;
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            NavigateAndApplyFuncWithStatus = ReactiveCommand.CreateFromObservable<NavigateArgumentAndStatus<RoutingWithContractsStateApplyFuncData>, IViewModelAndContract>(
             status =>
             {
                 // As a workaround for blocking navigation when the framework doesn't because the observables didn't update before new user input came in to a different control that triggered navigation.
                 if (status is null || status.Value.ViewModelAndContract is null)
                 {
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }

                 var tup = status.Value;

                 if (_isNavigating.Set(true))
                 {
                     status.SetAlreadyNavigating(true);
                     return Observable.Return(DoNothing).ObserveOn(navigateScheduler);
                 }
                 try
                 {
                     IsNavigating = true;
                     status.SetAlreadyNavigating(false);
                     _navigatingToIViewModelAndContractWeakRef.SetTarget(null);
                     if (tup.ViewModelAndContract?.ViewModel is null && tup.ViewModelAndContract != DoNothing)
                     {
                         throw new Exception("NavigateAndApplyFunc must receive a non-null IViewModelAndContract with a non-null IViewModelAndContract.ViewModel");
                     }
                     var vm = tup.ViewModelAndContract;
                     // This is how we avoid havig the observer accidentally switch state to navigation is over in case the user's action causes its OnNext to get the same view model that is the one that will actually be navigated to at the end.
                     _userManipulatingStack = true;
                     var navigate = tup.Fn?.Invoke(_navigationStack) ?? false;
                     _userManipulatingStack = false;

                     if (vm != DoNothing && navigate)
                     {
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _navigationStack.Add(vm);
                     }
                     else
                     {
                         vm = _navigationStack.Count > 0 ? _navigationStack[_navigationStack.Count - 1] : default;
                         _navigatingToIViewModelAndContractWeakRef.SetTarget(vm);
                         _stackWasCleared = vm == null;
                     }
                     return Observable.Return(vm).ObserveOn(navigateScheduler);
                 }
                 catch
                 {
                     _userManipulatingStack = false;
                     _stackWasCleared = false;
                     _isNavigating.ForceToFalse();
                     IsNavigating = false;
                     throw;
                 }
             });

            CurrentViewModel = Observable.Defer(() => Observable.Return(_navigationStack.LastOrDefault())).Concat(NavigationChanged.Select(_ => _navigationStack.LastOrDefault()));
            _iViewModelAndContractObserver.Subscribe(CurrentViewModel, navigateScheduler);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _iViewModelAndContractObserver?.Dispose();
                }
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}