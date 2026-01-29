using ProjectCompScience.Services;

namespace ProjectCompScience
{
    public partial class ShellAuth : Shell
    {
        public ShellAuth()
        {
            InitializeComponent();
        }

        private async void MenuItem_Logout_Clicked(object sender, EventArgs e)
        {
            LocalDataService.GetLocalDataService().Logout();
            ((App)Application.Current).SetUnauthenticatedShell();
        }
    }
}