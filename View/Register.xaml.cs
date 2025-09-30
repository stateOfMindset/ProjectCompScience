namespace ProjectCompScience.View;

public partial class Register : ContentPage
{
    bool isPasswordVisible = false;
    bool isConfirmPasswordVisible = false;

    public Register()
    {
        InitializeComponent();
    }

    private void OnTogglePasswordClicked(object sender, EventArgs e)
    {
        isPasswordVisible = !isPasswordVisible;
        PasswordEntry.IsPassword = !isPasswordVisible;
    }

    private void OnToggleConfirmPasswordClicked(object sender, EventArgs e)
    {
        isConfirmPasswordVisible = !isConfirmPasswordVisible;
        ConfirmPasswordEntry.IsPassword = !isConfirmPasswordVisible;
    }

    private void OnRegisterClicked(object sender, EventArgs e)
    {
        bool valid = ValidateForm();

        if (valid)
        {
            DisplayAlert("Success", "Registration successful!", "OK");
            // Do registration or navigation here
        }
    }

    private bool ValidateForm()
    {
        bool valid = true;

        if (string.IsNullOrWhiteSpace(NameEntry.Text))
        {
            NameErrorLabel.IsVisible = true;
            valid = false;
        }
        else
        {
            NameErrorLabel.IsVisible = false;
        }

        if (string.IsNullOrWhiteSpace(EmailEntry.Text) || !IsValidEmail(EmailEntry.Text))
        {
            EmailErrorLabel.IsVisible = true;
            valid = false;
        }
        else
        {
            EmailErrorLabel.IsVisible = false;
        }

        if (string.IsNullOrWhiteSpace(PasswordEntry.Text) || PasswordEntry.Text.Length < 6)
        {
            PasswordErrorLabel.IsVisible = true;
            valid = false;
        }
        else
        {
            PasswordErrorLabel.IsVisible = false;
        }

        if (ConfirmPasswordEntry.Text != PasswordEntry.Text)
        {
            ConfirmPasswordErrorLabel.IsVisible = true;
            valid = false;
        }
        else
        {
            ConfirmPasswordErrorLabel.IsVisible = false;
        }

        return valid;
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
