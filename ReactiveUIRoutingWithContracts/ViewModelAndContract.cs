namespace ReactiveUIRoutingWithContracts
{
    /// <summary>
    /// A very basic implementation of <see cref="IViewModelAndContract"/>. Feel free to create and use your own
    /// type or types that implement <see cref="IViewModelAndContract"/> if you need more functionality than this
    /// provides. If you are tempted to try to create an implementation that would let you mutate the contract or
    /// view model, you would be much better served by simply creating a new instance with the desired values and
    /// replacing the one you want to change. The navigation stack is already mutable so there is no reason to
    /// break the immutability that only providing getters for the <see cref="IViewModelAndContract"/> properties
    /// was intended to imply.
    /// </summary>
    public class ViewModelAndContract : IViewModelAndContract
    {
        public IRoutableViewModelForContracts ViewModel { get; }
        public string Contract { get; }
        internal static string DoNothingContract = "{8F2F167E-2707-48BD-AC7C-F1E818E760B1}";
        public static ViewModelAndContract DoNothing { get; } = new ViewModelAndContract(null, DoNothingContract);
        public ViewModelAndContract(IRoutableViewModelForContracts viewModel, string contract = null)
        {
            ViewModel = viewModel;
            Contract = contract;
        }
    }
}
