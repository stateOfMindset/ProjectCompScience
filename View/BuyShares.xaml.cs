using ProjectCompScience.ViewModels;

namespace ProjectCompScience.View;

public partial class BuyShares : ContentPage
{
	public BuyShares()
	{
		InitializeComponent();
        BindingContext = new ViewModelBuyShares();
    }
}