using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReactiveUIUnoSample.WPF.Host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // This code is designed to get the AssemblyProductAttribute, which is contained in ReactiveUIUnoSample.App.Shared\AssemblyInfoCommon.cs
            // We used that attribute to specify the name of the product, which we are now going to use as the Title for this Window, with fallback
            // values in case we have a problem retrieving it for some reason.
            var assembly = Assembly.GetExecutingAssembly();
            AssemblyProductAttribute productAttribute = (AssemblyProductAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyProductAttribute));
            Title = productAttribute?.Product ?? assembly.GetName()?.Name ?? "ReactiveUI with Uno";

            // The file was added to the project's resources in Resources.resx. This turned it into an embedded resource
            // which we access using pack-uri syntax: https://docs.microsoft.com/en-us/dotnet/desktop/wpf/app-development/pack-uris-in-wpf
            try
            {
                Icon = new BitmapImage(new Uri("pack://application:,,,/Assets/Square150x150Logo.scale-200.png"));
            }
            catch
            {
                // You could handle this if you have some way to do so, but in general, failing to set the window icon should not bring down the
                // application in the event that something goes wrong. The icon is just decoration, not some vital component.
            }

            // This is the code that actually puts the Uno App inside the WPF window. The WPF window's only job is to act as a container for the Uno App.
            // You should generally avoid using any features of WPF beyond setting the window title and icon like we are doing above and changing other
            // properties of the window such as its default size. That said, you can use various Windows features if you wish to provide access to features
            // on platforms that support them by making use of partial classes and partial methods:
            // https://platform.uno/docs/articles/platform-specific-csharp.html#partial-class-definitions
            // But make sure you do thorough testing to avoid problems and make sure that whatever you are doing is the result of Uno calling the WPF app using
            // a partial method in a partial class that is implemented in the WPF project. This WPF app should not be calling Uno on its own (beyond the initial
            // boilerplate setup code that set's the Window's content below). One last note. Because of technical reasons, all of the Uno shared projects (the
            // app, your views, view models, etc.) are included as Project References in the ReactiveUIUnoSample.Skia.Wpf project, and that Skia.Wpf project is
            // then included here. So if you do end up using partial classes and partial methods, you will need to implement them in that project and have that
            // project act as a go-between using callbacks. Static methods here implement the functionality and are registered with the Skia.Wpf project when
            // this (or the constructor in this project's App.xaml.cs file) runs so that the implementation of the partial methods in the Skia.Wpf project can
            // invoke those methods when needed. You might be able to use existing delegates such as Action and Func but you may need to write your own. The
            // static callbacks here will need to be registered with Skia.Wpf in some way, most likely by having it provide a static method to do so or by having
            // it expose static properties directly. I've done that in the past and may try to provide an example here eventually.
            root.Content = new global::Uno.UI.Skia.Platform.WpfHost(Dispatcher, () => new ReactiveUIUnoSample.App());
        }
    }
}
