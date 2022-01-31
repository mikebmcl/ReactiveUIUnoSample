//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.Reactive.Testing;
//using System.Reactive.Concurrency;
//using System.Text;
//using System.Threading.Tasks;

//namespace ReactiveUIUnoSample.UITest
//{
//    public class TestSchedulerProvider : ISchedulerProvider
//    {
//        private IScheduler m_mainThreadScheduler = new TestScheduler();
//        public IScheduler MainThread => m_mainThreadScheduler;
//        private IScheduler m_currentThreadScheduler = new TestScheduler();
//        public IScheduler CurrentThread => m_currentThreadScheduler;
//        private IScheduler m_taskPool = new TestScheduler();
//        public IScheduler TaskPool => m_taskPool;
//    }
//}
