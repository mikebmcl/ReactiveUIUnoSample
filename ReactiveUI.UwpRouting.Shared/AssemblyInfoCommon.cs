// Rather than having all of these properties in their own individual AssemblyInfo.cs files in each of the platform that support them, I've
// taken the ones that are most likely to be shared or to be changed at the same time and brought them here. Some properties do still
// remain in the platform-specific files, most notably the versioning properties. And as explained below, some platforms use other
// mechanisms for setting them. So this is not a one-stop solution, but it does reduce the amount of work needed to maintain this information.
using System.Reflection;

#if HAS_UNO_SKIA_TIZEN || HAS_UNO_SKIA_WPF || HAS_UNO_WASM || HAS_UNO_SKIA_GTK
// Note: 
//  Skia.Gtk and Skia.Tizen versions define these values in the Package.appxmanifest file and from their Project Properties.
//  IMPORTANT: Package.appxmanifest is shared across multiple platforms, including UWP, Skia.Gtk, and Skia.Tizen. Do not make platform-specific changes to that file.
//  Skia.Wpf obtains these values from the Skia.Wpf Project Properties (not the Skia.Wpf.Host project).
//  WASM defines these values in its AppManifest.js file and its Project Properties.
//  Except for Skia.Wpf, which generates values at build-time, they do not have [assembly:...] attributes. Also, they do not necessarily have equivalents for all of these.
// Note: A number of platforms that do not support these or do not support all of these will generate these values from their Project Properties in the Packaging section.
#else
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ReactiveUI with Uno")]
[assembly: AssemblyDescription("A sample showing how to use ReactiveUI with Uno Platform.")]
#if DEBUG
[assembly: AssemblyConfiguration("Debug Build")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif
[assembly: AssemblyCompany("")]
#if NETFX_CORE
[assembly: AssemblyProduct("ReactiveUI with Uno for Windows")]
#elif HAS_UNO_SKIA_GTK
[assembly: AssemblyProduct("ReactiveUI with Uno for Gtk")]
#elif __ANDROID__
[assembly: AssemblyProduct("ReactiveUI with Uno for Android")]
#elif __IOS__
[assembly: AssemblyProduct("ReactiveUI with Uno for iOS")]
#elif __MACOS__
[assembly: AssemblyProduct("ReactiveUI with Uno for macOS")]
#else
[assembly: AssemblyProduct("ReactiveUI with Uno")]
#endif
[assembly: AssemblyCopyright("Copyright © 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
#endif