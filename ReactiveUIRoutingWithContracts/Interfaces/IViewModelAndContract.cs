namespace ReactiveUIRoutingWithContracts
{
    public interface IViewModelAndContract
    {
        /// <summary>
        /// Must not be null.
        /// </summary>
        IRoutableViewModelForContracts ViewModel { get; }

        /// <summary>
        /// Can be null if <see cref="ViewModel"/> was registered with a null
        /// or empty contract.
        /// </summary>
        string Contract { get; }
    }
}
