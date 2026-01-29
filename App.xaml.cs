using ProjectCompScience.View;

namespace ProjectCompScience
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new ShellNotAuth();
        }

        public void SetAuthenticatedShell()
        {
            MainPage = new ShellAuth();
        }

        public void SetUnauthenticatedShell()
        {
            MainPage = new ShellNotAuth();
        }
    }
}


