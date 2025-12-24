namespace ProjectCompScience.View;
using ProjectCompScience.ViewModels;

public partial class addNewShares : ContentPage
{
	public addNewShares()
	{
        InitializeComponent();
		BindingContext = new ViewModelAddNewShares();
	}
}