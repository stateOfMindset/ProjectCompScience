using ProjectCompScience.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelLoginPage : ViewModelBase
    {
        #region Getters & Setters
        private string emailInput;
        public string EmailInput
        {
            get { return emailInput; }
            set { emailInput = value; }
        }

        private string passwordInput;
        public string PasswordInput
        {
            get { return passwordInput; }
            set { passwordInput = value; }
        }

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set { isBusy = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands Declaration
        public ICommand SubmitLoginCommand { get; set; }
        public ICommand GoRegisterCommand { get; set; }
        #endregion

        #region Constructor
        public ViewModelLoginPage()
        {
            EmailInput = "supermike@gmail.com";
            SubmitLoginCommand = new Command(async () => await Login());
            GoRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("//Register"));
        }
        #endregion

        #region Methods / Functions
        private async Task Login()
        {
            if (string.IsNullOrWhiteSpace(EmailInput) || string.IsNullOrWhiteSpace(PasswordInput))
            {
                await App.Current.MainPage.DisplayAlert("Hold on", "Please enter both an email and a password.", "OK");
                return;
            }

            //  PREVENT SPAMMING
            if (IsBusy) return;
            IsBusy = true;

            try
            {
                var result = await LocalDataService.GetLocalDataService().TryLogin(EmailInput, PasswordInput);

                if (result.isSuccess)
                {
                    ((App)Application.Current).SetAuthenticatedShell();
                    await Shell.Current.GoToAsync("//StockSharesPage");
                }
                else
                {
 
                    await App.Current.MainPage.DisplayAlert("Login Failed", result.errorMessage, "OK");
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
