namespace ProjectCompScience.View;
using ProjectCompScience.Models;
using ProjectCompScience.ViewModels;

public partial class StockSharesPage : ContentPage
{
	ViewModelShare vm;

    public StockSharesPage()
	{
		InitializeComponent();
        vm = new ViewModelShare();
		BindingContext = vm;
    }
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is ViewModelShare vm)
        {
            await vm.LoadDataAsync();
        }
    }
}