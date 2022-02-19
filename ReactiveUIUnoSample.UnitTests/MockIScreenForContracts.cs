using ReactiveUIRoutingWithContracts;

using System.Linq;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class MockIScreenForContracts : IScreenForContracts
    {
        public RoutingWithContractsState Router { get; }

        public string GetCurrentContract()
        {
            return Router?.NavigationStack.LastOrDefault().Contract;
        }

        public MockIScreenForContracts(RoutingWithContractsState router)
        {
            Router = router;
        }
    }
}
