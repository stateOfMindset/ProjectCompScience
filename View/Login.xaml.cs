namespace ProjectCompScience.View;

using ProjectCompScience.Services;
using ProjectCompScience.ViewModels;

public partial class Login : ContentPage
{
    ViewModelLoginPage vm;
    public Login()
    {
        InitializeComponent();

        vm = new ViewModelLoginPage();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Now the UI draws instantly and ENV loads in the background
        await LocalDataService.GetLocalDataService().InitAsync();
    }
}