using ProjectCompScience.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelRegister : ViewModelBase
    {
        #region Fields
        private string emailInput;
        private string passwordInput1;
        private string fullName;
        private string errorLabelNameText;
        private string errorLabelEmailField;
        private string errorLabelPasswordField;
        private bool isVisiblePasswordField = true;

        // --- THE LOCK FIELD ---
        private bool isBusy;
        #endregion

        #region Properties
        public string FullName
        {
            get => fullName;
            set { fullName = value; OnPropertyChanged(); ValidateName(); }
        }

        public string EmailInput
        {
            get => emailInput;
            set { emailInput = value; OnPropertyChanged(); ValidateEmail(); }
        }

        public string PasswordInput1
        {
            get => passwordInput1;
            set { passwordInput1 = value; OnPropertyChanged(); ValidatePassword(); }
        }

        public bool IsVisiblePasswordField
        {
            get => isVisiblePasswordField;
            set { isVisiblePasswordField = value; OnPropertyChanged(); }
        }

        public string ErrorLabelNameText
        {
            get => errorLabelNameText;
            set { errorLabelNameText = value; OnPropertyChanged(); }
        }

        public string ErrorLabelEmailField
        {
            get => errorLabelEmailField;
            set { errorLabelEmailField = value; OnPropertyChanged(); }
        }

        public string ErrorLabelPasswordField
        {
            get => errorLabelPasswordField;
            set { errorLabelPasswordField = value; OnPropertyChanged(); }
        }

        // --- THE LOCK PROPERTY ---
        public bool IsBusy
        {
            get => isBusy;
            set { isBusy = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public ICommand SubmitRegisterCommand { get; }
        public ICommand GoLoginCommand { get; }
        public ICommand ButtonShowPasswordCommand { get; }
        #endregion

        #region Constructor
        public ViewModelRegister()
        {
            SubmitRegisterCommand = new Command(async () => await Register());
            GoLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//Login"));
            ButtonShowPasswordCommand = new Command(() => IsVisiblePasswordField = !IsVisiblePasswordField);
        }
        #endregion

        #region Validation Logic
        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(FullName)) ErrorLabelNameText = "Name is required";
            else if (FullName.Length < 3) ErrorLabelNameText = "Name too short";
            else ErrorLabelNameText = "";
        }

        private void ValidateEmail()
        {
            if (string.IsNullOrWhiteSpace(EmailInput)) ErrorLabelEmailField = "Email is required";
            else if (!Regex.IsMatch(EmailInput, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) ErrorLabelEmailField = "Invalid email format";
            else ErrorLabelEmailField = "";
        }

        private void ValidatePassword()
        {
            if (string.IsNullOrWhiteSpace(PasswordInput1)) ErrorLabelPasswordField = "Password is required";
            else if (PasswordInput1.Length < 6) ErrorLabelPasswordField = "Minimum 6 characters"; // Firebase requires 6!
            else ErrorLabelPasswordField = "";
        }
        #endregion

        #region Registration Logic
        private async Task Register()
        {
            //  Force validations to run just in case the user clicked Register on an empty screen
            ValidateName();
            ValidateEmail();
            ValidatePassword();

            //  Check if any errors popped up from the validations
            if (!string.IsNullOrEmpty(ErrorLabelNameText) ||
                !string.IsNullOrEmpty(ErrorLabelEmailField) ||
                !string.IsNullOrEmpty(ErrorLabelPasswordField))
            {
                return;
            }

            //  PREVENT BUTTON SPAMMING
            if (IsBusy) return;
            IsBusy = true;

            try
            {

                var result = await LocalDataService.GetLocalDataService().TryRegister(EmailInput, PasswordInput1, FullName);

                if (result.isSuccess)
                {
                    ((App)Application.Current).SetAuthenticatedShell();
                    await Shell.Current.GoToAsync("//StockSharesPage");
                }
                else
                {
  
                    await App.Current.MainPage.DisplayAlert("Registration Failed", result.errorMessage, "Got it");
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
        #endregion
    }
}