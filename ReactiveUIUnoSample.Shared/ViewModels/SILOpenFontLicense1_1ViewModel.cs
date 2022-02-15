using System;
using System.Collections.Generic;
using System.Text;

using ReactiveUIRoutingWithContracts;

namespace ReactiveUIUnoSample.ViewModels
{
    public class SILOpenFontLicense1_1ViewModel : DisplayViewModelBase
    {
        public SILOpenFontLicense1_1ViewModel(IScreenForContracts hostScreen, ISchedulerProvider schedulerProvider, string urlPathSegment = null, bool useNullUrlPathSegment = false) : base(hostScreen, schedulerProvider, urlPathSegment, useNullUrlPathSegment)
        {
            HeaderContent = "SIL Open Font License Version 1.1";
        }

        public override object HeaderContent { get; set; }
    }
}
