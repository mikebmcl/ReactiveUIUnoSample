using ReactiveUI;

using ReactiveUIRoutingWithContracts;

using System;
using System.Collections.Generic;
using System.Text;

namespace ReactiveUIUnoSample.UnitTests
{
    internal class EmptyViewModel : ReactiveObject, IRoutableViewModelForContracts
    {
        public IScreenForContracts HostScreenWithContract { get; }

        public EmptyViewModel(IScreenForContracts screen)
        {
            HostScreenWithContract = screen;
        }
    }
}
