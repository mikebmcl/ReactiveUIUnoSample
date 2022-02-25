using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Reactive.Testing;
using System.Reactive.Concurrency;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

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

        public const long IncrementalAdvanceByTime = 1;
        /// <summary>
        /// Advances all schedulers by the unit of time specified by <paramref name="time"/>, in the following order: <see cref="MainThread"/>,
        /// <see cref="CurrentThread"/>, and finally <see cref="TaskPool"/>. It loops processing in increments of
        /// <see cref="IncrementalAdvanceByTime"/> time units until <paramref name="time"/> is less than or equal to that value, at which point
        /// it advances each scheduler in the same order specified earlier by the remaining amount of time. The reason for incrementally advancing
        /// each scheduler in a loop is to provide for the possibility that advancement of one scheduler might result in a notification that an
        /// observer running on a different scheduler would get that would then generate a notification that an observer running on the earlier
        /// scheduler (or another scheduler) would get that would then generate a notification... etc. Not that they would be caught in an infinite
        /// loop, but simply that the intent of this method is to have the state of the program advance in a way that mimics runtime behavior as
        /// closely as possible while still ensuring that each scheduler advances at least <paramref name="time"/> units. See remarks for why the
        /// phrasing "at least" is used.
        /// </summary>
        /// <param name="time">Specifies the amount of time passed to <see cref="VirtualTimeSchedulerBase{TAbsolute, TRelative}.AdvanceBy(TRelative)"/>. If <c>default</c>, the value 100 will be used.</param>
        /// <remarks>Certain actions will cause schedulers to begin processing work. When this occurs, <see cref="VirtualTimeSchedulerBase{TAbsolute, TRelative}.IsEnabled"/> will be <c>true</c> and the scheduler will continue to process items until a certain time or until it has run out of items, depending on how the scheduler was enabled. Using AdvanceBy while a scheduler is running will cause an InvalidOperation exception. The intent of this method is to be a convenience helper that avoids throwing that exception for that reason (other exceptions or the same type of exception for other reasons will still percolate out). As a result, the schedulers must be stopped. Stopping a scheduler does not clear its work item queue nor even stop it from finishing processing an item it is processing (if any). It simply says do not start processing a new item. Unfortunately we have no way of determining if a currently running scheduler is supposed to end at a specific time or if it was told to run until its queue is empty. For this reason, we stop it then run the AdvanceBy call. We do not restart it, but it is possible that something that runs on one of the other schedulers might result in it restarting. There are a few other situations that might cause it to run (mostly if something running asynchronously started it; managing schedulers asynchronously during testing is a bad idea because you cannot have any confidence in the result of the test due to a loss of determinacy). All of this is to say that if a scheduler is already running when this method is called, it could run for more than <paramref name="time"/> ticks. Additionally, if an item on the scheduler or one of the other schedulers causes it to restart, then it would also likely run longer. Finally, if that happens during the final cycle of advancing the schedulers, a scheduler might be processing items even after this method returns. Design your tests with these facts in mind and design your application so that asynchronous tasks can be cancelled. That will give you the option to cancel any tasks and then wait until IsEnabled becomes false on all schedulers before calling this method, which should help eliminate at least some of the potential issues discussed here.</remarks>
        public void AdvanceAllSchedulers(long time = 100)
        {
            void AdvanceScheduler(long time, TestScheduler scheduler)
            {
                //bool schedulerWasRunning = scheduler.IsEnabled;
                scheduler.Stop();
                try
                {
                    scheduler.AdvanceBy(time);
                }
                catch (Exception ex)
                {
                    // TODO: Our actual exception observers can end up getting the intentional exceptions and throwing as a result, e.g. TemperatureConversionsViewModel. We don't want that. However, we can't bury the exception here because then expected exceptions in the tests never surface and tests fails because the exception they expected and should've received was eaten.
                    TestContext.WriteLine(DiagnosticsHelpers.GetDiagnosticStringWithExceptionData(ex, $"Exception in {nameof(AdvanceAllSchedulers)}. Details to follow."));
                    throw;
                }
                //if (schedulerWasRunning)
                //{
                //    scheduler.Start();
                //}
            }
            if (time is default(long))
            {
                time = 100;
            }
            while (time > IncrementalAdvanceByTime)
            {
                AdvanceScheduler(IncrementalAdvanceByTime, MainThread);
                AdvanceScheduler(IncrementalAdvanceByTime, CurrentThread);
                AdvanceScheduler(IncrementalAdvanceByTime, TaskPool);
                time -= IncrementalAdvanceByTime;
            }
            AdvanceScheduler(time, MainThread);
            AdvanceScheduler(time, CurrentThread);
            AdvanceScheduler(time, TaskPool);
        }
        IScheduler ISchedulerProvider.MainThread => _mainThreadScheduler;
        IScheduler ISchedulerProvider.CurrentThread => _currentThreadScheduler;
        IScheduler ISchedulerProvider.TaskPool => _taskPool;
    }
}
