namespace ProjectCompScience.View;
using ProjectCompScience.Models;
using ProjectCompScience.ViewModels;

public partial class StockSharesPage : ContentPage
{
	public StockSharesPage()
	{
		InitializeComponent();
        BindingContext = new ViewModelAddNewShares();
    }
}