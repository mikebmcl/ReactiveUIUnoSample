using NUnit.Framework;

using ReactiveUIUnoSample.Interfaces;
using ReactiveUIUnoSample.ViewModels;

using Splat;

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ReactiveUIUnoSample.UITest
{
    /// <summary>
    /// Use this as the base class for all tests of view models and anything else not dependant of the UI actually existing. This
    /// does not create an instance of the app so nothing related to the UI actually exists. Therefore you can only test any portions of view
    /// models that will not attempt to do anything that relates to the UI directly, e.g. instantiating controls. Anything that relates to the UI
    /// should be tested against the view using a test class that derives from <see cref="TestBase"/>.
    /// </summary>
    public class ReactiveTestBase
    {
        private static Lazy<INavigationViewProvider> m_navigationViewProvider = new Lazy<INavigationViewProvider>(() => new TestNavigationViewProvider(), LazyThreadSafetyMode.PublicationOnly);
        private static Lazy<ISchedulerProvider> m_schedulerProvider = new Lazy<ISchedulerProvider>(() => new TestSchedulerProvider(), LazyThreadSafetyMode.PublicationOnly);
        private Lazy<IScreenWithContract> m_screenWithContract = new Lazy<IScreenWithContract>(InitScreenWithContract, LazyThreadSafetyMode.PublicationOnly);
        private static IScreenWithContract InitScreenWithContract()
        {
            return new MainViewModel(m_navigationViewProvider.Value, Locator.CurrentMutable, string.Empty, m_schedulerProvider.Value);
        }

        protected INavigationViewProvider TestNavigationViewProvider
        {
            get => m_navigationViewProvider.Value;
        }

        protected ISchedulerProvider TestSchedulerProvider
        {
            get => m_schedulerProvider.Value;
        }

        protected IScreenWithContract ScreenWithContract
        {
            get => m_screenWithContract.Value;
        }
    }
}
