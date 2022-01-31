using System.Reactive.Concurrency;

namespace ReactiveUIUnoSample
{
    public interface ISchedulerProvider
    {
        IScheduler MainThread { get; }
        IScheduler CurrentThread { get; }
        IScheduler TaskPool { get; }
    }
}
