//#nullable disable
using ReactiveUI;

// Typically this would be in Interfaces but because it's sole use is for view models, it makes sense to have it in their namespace
namespace ReactiveUIUnoSample.ViewModels
{
    /// <summary>
    /// The RoutedViewHost control on MainView provides bindings for both the Router AND the ViewContract, which is 
    /// that custom string you can assign when registering your views in MainViewModel's ctor. Unfortunately, IScreen
    /// only exposes the Router as a property, which kind of negates the whole purpose of passing around IScreen to
    /// do routing since you can't do any routing where you want multiple views for the same view model.
    /// 
    /// This interface resolves that issue.
    /// </summary>
    public interface IScreenWithContract : IScreen
    {
        /// <summary>
        /// It is not safe to try to change this value, e.g. to null, in the view model constructor or the view constructor
        /// because the routing system still requires the correct value after both of those constructors have run. Once the
        /// view is loaded it is safe to modify. It is safe to navigate to a view where this
        /// Note: The actual <see cref="ReactiveUI.Uno.RoutedViewHost.ViewContract"/> property is a nullable 
        /// string, however .NET Stancard 2.0 limits us to C# 7.3 and nullable reference types were introduced in 
        /// C# 8, so we are stuck with this slight mismatch
        /// </summary>
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
        [System.Diagnostics.CodeAnalysis.AllowNull]
#endif
        string Contract { get; set; }
    }
}
