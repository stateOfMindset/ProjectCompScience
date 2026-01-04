namespace ProjectCompScience.View;
using ProjectCompScience.ViewModels;

public partial class addNewShares : ContentPage
{
    ViewModelAddNewShares vm;
    public addNewShares()
    {
        InitializeComponent();
        vm = new ViewModelAddNewShares();
        BindingContext = vm;
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        vm.ClearFields();
    }
}
