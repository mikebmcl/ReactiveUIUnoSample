using ReactiveUIRoutingWithContracts;

using System;
using System.Linq;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class MockIScreenForContracts : IScreenForContracts
    {
        private bool _disposedValue;

        public RoutingWithContractsState Router { get; }

        public string GetCurrentContract()
        {
            return Router?.NavigationStack.LastOrDefault().Contract;
        }

        public MockIScreenForContracts(RoutingWithContractsState router)
        {
            Router = router;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    Router?.Dispose();
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
