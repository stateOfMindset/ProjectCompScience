using ProjectCompScience.ViewModels;

namespace ProjectCompScience.View;

public partial class Register : ContentPage
{
    public Register()
    {
        InitializeComponent();
        BindingContext = new ViewModelRegister();


    }
}
