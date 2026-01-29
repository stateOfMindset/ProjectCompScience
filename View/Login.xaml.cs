namespace ProjectCompScience.View;

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
}