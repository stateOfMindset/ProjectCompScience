using ProjectCompScience.ViewModels;

namespace ProjectCompScience.View;

public partial class Register : ContentPage
{
    ViewModelRegister vm;
    public Register()
    {
        InitializeComponent();
        vm = new ViewModelRegister();
        BindingContext = vm;
    }
}
