using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading;

using ReactiveUI;

using ReactiveUIUnoSample.Interfaces;

namespace ReactiveUIUnoSample
{
    public class SchedulerProvider : ISchedulerProvider
    {
        public IScheduler MainThread => RxApp.MainThreadScheduler;
        public IScheduler CurrentThread => Scheduler.CurrentThread;
        public IScheduler TaskPool => RxApp.TaskpoolScheduler;
    }
}
