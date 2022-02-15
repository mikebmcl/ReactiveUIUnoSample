namespace ReactiveUIRoutingWithContracts
{
    public static class ViewModelAndContractMixins
    {

        /// <summary>
        /// Returns a <see cref="ViewModelAndContract"/> using this <see cref="IRoutableViewModelForContracts"/> as the 
        /// <see cref="ViewModelAndContract.ViewModel"/>. Its <see cref="ViewModelAndContract.Contract"/> will be null.
        /// </summary>
        /// <param name="viewModel">The <see cref="IRoutableViewModelForContracts"/> to use as the view model.</param>
        /// <returns>
        /// An instance of <see cref="ViewModelAndContract"/> that can be passed to any method requiring an 
        /// <see cref="IViewModelAndContract"/> argument.
        /// </returns>
        public static ViewModelAndContract ToViewModelAndContract(this IRoutableViewModelForContracts viewModel) => new ViewModelAndContract(viewModel);
        /// <summary>
        /// Returns a <see cref="ViewModelAndContract"/> using this <see cref="IRoutableViewModelForContracts"/> as the 
        /// <see cref="ViewModelAndContract.ViewModel"/>. Its <see cref="ViewModelAndContract.Contract"/> will be <paramref name="contract"/>.
        /// </summary>
        /// <param name="viewModel">The <see cref="IRoutableViewModelForContracts"/> to use as the view model.</param>
        /// <param name="contract">
        /// The contract string to use to resolve the correct view when there are multiple views registered for the same view model.
        /// </param>
        /// <returns>
        /// An instance of <see cref="ViewModelAndContract"/> that can be passed to any method requiring an 
        /// <see cref="IViewModelAndContract"/> argument.
        /// </returns>
        public static ViewModelAndContract ToViewModelAndContract(this IRoutableViewModelForContracts viewModel, string contract) => new ViewModelAndContract(viewModel, contract);
    }
}
