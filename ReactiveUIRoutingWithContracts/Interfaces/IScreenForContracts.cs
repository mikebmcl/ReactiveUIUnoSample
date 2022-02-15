namespace ReactiveUIRoutingWithContracts
{
    public interface IScreenForContracts
    {
        RoutingWithContractsState Router { get; }
        string GetCurrentContract();
    }
}
