namespace ReactiveUIRoutingWithContracts
{
    public static class ViewModelAndContractMixins
    {
        public static ViewModelAndContract ToViewModelAndContract(this IRoutableViewModelForContracts viewModel) => new ViewModelAndContract(viewModel);
        public static ViewModelAndContract ToViewModelAndContract(this IRoutableViewModelForContracts viewModel, string contract) => new ViewModelAndContract(viewModel, contract);
    }
}
