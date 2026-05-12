using ProjectCompScience.ViewModels;

namespace ProjectCompScience.View;

public partial class TransactionHistory : ContentPage
{
	public TransactionHistory()
	{
		InitializeComponent();
        var viewModel = new TransactionHistoryViewModel();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is TransactionHistoryViewModel viewModel)
        {
            await viewModel.LoadHistoryAsync();
        }
    }
}