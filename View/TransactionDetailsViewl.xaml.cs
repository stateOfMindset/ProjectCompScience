using ProjectCompScience.ViewModels;

namespace ProjectCompScience.View;

public partial class TransactionDetailsViewl : ContentPage
{
	public TransactionDetailsViewl()
	{
		InitializeComponent();
        var viewModel = new TransactionDetailsViewModel();
        BindingContext = viewModel;
    }
}