namespace ProjectCompScience.View;

public partial class Login : ContentPage
{
    bool isPasswordVisible = false;

    public Login()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        PasswordEntry.IsPassword = !isPasswordVisible;
        TogglePasswordBtn.Text = isPasswordVisible ? "Hide" : "Show";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var email = EmailEntry.Text;
        var password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Error", "Please fill in both fields.", "OK");
            return;
        }

        await DisplayAlert("Success", "Login successful!", "OK");

    }

    private async void OnForgotPasswordClicked(object sender, EventArgs e)
    {
        await DisplayAlert("Reset", "Password reset link has been sent.", "OK");
    }

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        Application.Current.MainPage = new Register();
    }
}