using ProjectCompScience.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelLoginPage
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
        #endregion

        #region Commands Declaration
        public ICommand SubmitLoginCommand { get; set; }
        public ICommand GoRegisterCommand { get; set; }
        #endregion

        #region Constructor
        public ViewModelLoginPage()
        {
            EmailInput = "monke@zoo.com";
            SubmitLoginCommand = new Command(async () => await Login());
            GoRegisterCommand = new Command(async () => await Shell.Current.GoToAsync("//Register"));
        }
        #endregion

        #region Methods / Functions
        private async Task Login()//
        {
            bool successed = await LocalDataService.GetLocalDataService().TryLogin(emailInput, passwordInput);
            if (successed)
            {
                ((App)Application.Current).SetAuthenticatedShell();
                // This will also navigate to the 1st page int the AuthenticatedShell 
            }
        }
        #endregion
    }
}
