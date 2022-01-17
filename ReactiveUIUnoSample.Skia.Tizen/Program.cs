using Tizen.Applications;

using Uno.UI.Runtime.Skia;

namespace ReactiveUIUnoSample.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new ReactiveUIUnoSample.App(), args);
            host.Run();
        }
    }
}
