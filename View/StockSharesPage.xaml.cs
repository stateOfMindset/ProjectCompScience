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
    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        vm.InitAsync();
    }
}