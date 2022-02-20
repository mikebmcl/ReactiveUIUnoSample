using System;

namespace ReactiveUIRoutingWithContracts
{
    public interface IScreenForContracts : IDisposable
    {
        RoutingWithContractsState Router { get; }
        string GetCurrentContract();
    }
}
