using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Reactive.Testing;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveUIUnoSample.UnitTests
{
    public class TestSchedulerProvider : ISchedulerProvider
    {
        private readonly TestScheduler _mainThreadScheduler = new TestScheduler();
        public TestScheduler MainThread => _mainThreadScheduler;
        private readonly TestScheduler _currentThreadScheduler = new TestScheduler();
        public TestScheduler CurrentThread => _currentThreadScheduler;
        private readonly TestScheduler _taskPool = new TestScheduler();
        public TestScheduler TaskPool => _taskPool;

        IScheduler ISchedulerProvider.MainThread => _mainThreadScheduler;
        IScheduler ISchedulerProvider.CurrentThread => _currentThreadScheduler;
        IScheduler ISchedulerProvider.TaskPool => _taskPool;
    }
}
