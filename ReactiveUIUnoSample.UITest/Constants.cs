using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Uno.UITest.Helpers.Queries;

namespace ReactiveUIUnoSample.UITest
{
    public class Constants
    {
        public readonly static string WebAssemblyDefaultUri = "http://localhost:59036/"; //"https://localhost:44352/"; //"http://localhost:59036/";
        public readonly static string iOSAppName = "com.example.app";
        public readonly static string AndroidAppName = "com.example.app";
        public readonly static string iOSDeviceNameOrId = "iPad Pro (12.9-inch) (3rd generation)";

        public readonly static Platform CurrentPlatform = Platform.Browser;
    }
}
