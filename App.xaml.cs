using ProjectCompScience.View;

namespace ProjectCompScience
{
    public partial class App : Application
    {
        public App()
        {
            try
            {
                InitializeComponent();
                MainPage = new ShellNotAuth();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FATAL STARTUP ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"INNER EXCEPTION: {ex.InnerException?.Message}");
            }
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


