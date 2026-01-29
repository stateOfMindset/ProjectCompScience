using ProjectCompScience.Services;
using ProjectCompScience.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;


namespace ProjectCompScience.ViewModels
{
    class ViewModelRegister : ViewModelBase
    {
        #region Getters & Setters
        private string emailInput;
        public string EmailInput
        {
            get { return emailInput; }
            set { emailInput = value; }
        }

        private string passwordInput1;
        public string PasswordInput1
        {
            get { return passwordInput1; }
            set { passwordInput1 = value; }
        }

        private string passwordInput2;
        public string PasswordInput2
        {
            get { return passwordInput2; }
            set
            {
                passwordInput2 = value;
                if (passwordInput1 != "" && passwordInput1 == passwordInput2)
                {
                    IsRegisterButtonEnabled = true;
                }
                else
                {
                    IsRegisterButtonEnabled = false;
                }

            }
        }

        private string fullName;
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }

        private bool isRegisterButtonEnabled;
        public bool IsRegisterButtonEnabled
        {
            get { return isRegisterButtonEnabled; }
            set
            {
                isRegisterButtonEnabled = value;
                OnPropertyChanged(nameof(IsRegisterButtonEnabled));
            }
        }

        #endregion

        #region Commands Declaration
        public ICommand SubmitRegisterCommand { get; set; }
        public ICommand GoLoginCommand { get; set; }
        #endregion

        #region Constuctor
        public ViewModelRegister()
        {
            SubmitRegisterCommand = new Command(async () => await Register());
            GoLoginCommand = new Command(async () => await Shell.Current.GoToAsync("//Login"));
        }
        #endregion

        #region Methods / Functions
        private async Task Register()
        {
            bool successed = await LocalDataService.GetLocalDataService().TryRegister(EmailInput, PasswordInput1, FullName);
            if (successed)
            {
                ((App)Application.Current).SetAuthenticatedShell();
                await Shell.Current.GoToAsync("//StockSharesPage");
            }
            else
            {
            }
        }
        #endregion
    }
}

//{
//        public ViewModelRegister()
//{
//    initiateInstances();

//}

//private void initiateInstances()
//{
//    errorLabelNameText = "";
//    nameField = "";
//    ButtonRegisterCommand = new Command(Register);
//    ButtonShowPasswordCommand = new Command(ShowPassword);
//    ButtonShowConfirmPasswordCommand = new Command(ShowConfirmPassword);

//}

//#region nameFieldPropfulls
//private string nameField;

//public string NameField
//{
//    get { return nameField; }
//    set
//    {
//        nameField = value;
//        validateName();
//        OnPropertyChanged();

//    }
//}

//private string errorLabelNameText;

//public string ErrorLabelNameText
//{
//    get { return errorLabelNameText; }
//    set
//    {
//        if (errorLabelNameText != value)
//        {
//            errorLabelNameText = value;
//            OnPropertyChanged();
//        }
//    }
//}

//#endregion

//#region Name-Related-Methods
//private bool validateName()
//{
//    string _srrorLabelName = "";
//    bool returnVal = true;

//    if (string.IsNullOrWhiteSpace(NameField))
//    {
//        _srrorLabelName += "The Name is Empty! ";
//        returnVal = false;
//    }
//    else if (nameField.Length < 3)
//    {
//        _srrorLabelName += "The Name is Too Short! ";
//        returnVal = false;
//    }
//    else if (nameField.Length > 10)
//    {
//        _srrorLabelName += "The Name is Too Long! ";
//        returnVal = false;
//    }

//    ErrorLabelNameText = _srrorLabelName;

//    return returnVal;

//}
//#endregion

//#region EmailFieldPropfulls

//private string emailFieldText;

//public string EmailFieldText
//{
//    get { return emailFieldText; }
//    set
//    {
//        emailFieldText = value;
//        validateEmail();
//        OnPropertyChanged();

//    }
//}


//private string errorLabelEmailField;

//public string ErrorLabelEmailField
//{
//    get { return errorLabelEmailField; }
//    set
//    {
//        if (errorLabelEmailField != value)
//        {
//            errorLabelEmailField = value;
//            OnPropertyChanged();
//        }
//    }
//}


//#endregion

//#region Email-Related-Methods

//private bool validateEmail()
//{
//    string _errorLabelEmailText = "";
//    bool returnVal = true;

//    if (string.IsNullOrWhiteSpace(EmailFieldText))
//    {
//        _errorLabelEmailText += "The Email is Empty! ";
//        returnVal = false;
//    }
//    else if (!IsValidEmail())
//    {
//        _errorLabelEmailText += "Invalid Email ";
//        returnVal = false;
//    }

//    ErrorLabelEmailField = _errorLabelEmailText;

//    return returnVal;

//}

//private bool IsValidEmail()
//{
//    try
//    {
//        var addr = new System.Net.Mail.MailAddress(EmailFieldText);
//        return addr.Address == EmailFieldText;
//    }
//    catch
//    {
//        return false;
//    }
//}

//#endregion

//#region PasswordFieldPropfull

//private string passwordField;

//public string PasswordField
//{
//    get { return passwordField; }
//    set
//    {
//        passwordField = value;
//        validatePassword();
//        OnPropertyChanged();
//    }
//}

//private string errorLabelPasswordField;

//public string ErrorLabelPasswordField
//{
//    get { return errorLabelPasswordField; }
//    set
//    {
//        errorLabelPasswordField = value;
//        OnPropertyChanged();
//    }
//}

//private bool isVisiblePasswordField;

//public bool IsVisiblePasswordField
//{
//    get { return isVisiblePasswordField; }
//    set
//    {
//        isVisiblePasswordField = value;
//        PasswordVisibilityChanged();
//        OnPropertyChanged();
//    }
//}


//#endregion

//#region Password-Related-Methods

//private bool validatePassword()
//{
//    string _errorPassword = "";
//    bool isValid = true;

//    if (string.IsNullOrWhiteSpace(PasswordField))
//    {
//        _errorPassword += "The Password is Empty !";
//        isValid = false;
//    }
//    if (PasswordField.Length < 8)
//    {
//        _errorPassword += "Password must be at least 8 characters long. ";
//        isValid = false;
//    }

//    if (!Regex.IsMatch(PasswordField, "[A-Z]"))
//    {
//        _errorPassword += "Password must contain at least one uppercase letter. ";
//        isValid = false;
//    }

//    if (!Regex.IsMatch(PasswordField, "[a-z]"))
//    {
//        _errorPassword += "Password must contain at least one lowercase letter. ";
//        isValid = false;
//    }

//    if (!Regex.IsMatch(PasswordField, "[0-9]"))
//    {
//        _errorPassword += "Password must contain at least one digit. ";
//        isValid = false;
//    }

//    if (!Regex.IsMatch(PasswordField, "[^a-zA-Z0-9]"))
//    {
//        _errorPassword += "Password must contain at least one special character.";
//        isValid = false;
//    }

//    if (PasswordField.ToLowerInvariant() == "password" || PasswordField == "12345678")
//    {
//        _errorPassword += "Avoid using common and easily guessable passwords.";
//        isValid = false;
//    }

//    ErrorLabelPasswordField = _errorPassword;
//    return isValid;
//}

//private void ShowPassword()
//{

//    IsVisiblePasswordField = !IsVisiblePasswordField;
//}

//private void PasswordVisibilityChanged()
//{

//}

//#endregion

//#region ConfirmPasswordFieldPropfull

//private string confirmPasswordField;

//public string ConfirmPasswordField
//{
//    get { return confirmPasswordField; }
//    set
//    {
//        confirmPasswordField = value;
//        validateConfirmPassword();
//        OnPropertyChanged();
//    }
//}

//private string errorLabelConfirmPasswordField;

//public string ErrorLabelConfirmPasswordField
//{
//    get { return errorLabelConfirmPasswordField; }
//    set
//    {
//        errorLabelConfirmPasswordField = value;
//        OnPropertyChanged();
//    }
//}

//private bool isVisibleConfirmPasswordField;

//public bool IsVisibleConfirmPasswordField
//{
//    get { return isVisibleConfirmPasswordField; }
//    set
//    {
//        isVisibleConfirmPasswordField = value;
//        OnPropertyChanged();
//    }
//}



//#endregion

//#region Confirm-Password-Related-Methods

//private bool validateConfirmPassword()
//{
//    string _errorPassword = "";
//    bool returnVal = true;

//    if (string.IsNullOrWhiteSpace(ConfirmPasswordField))
//    {
//        _errorPassword += "The Password is Empty !";
//        returnVal = false;
//    }
//    if (!ConfirmPasswordField.Equals(PasswordField))
//    {
//        _errorPassword += "The Passwords Do Not Match.";
//        returnVal = false;
//    }
//    ErrorLabelConfirmPasswordField = _errorPassword;
//    return returnVal;
//}

//private void ShowConfirmPassword()
//{
//    IsVisibleConfirmPasswordField = !IsVisibleConfirmPasswordField;
//}

//#endregion

//#region Commands
//public ICommand ButtonRegisterCommand { get; set; }
//public ICommand ButtonShowPasswordCommand { get; set; }
//public ICommand ButtonShowConfirmPasswordCommand { get; set; }



