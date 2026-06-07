using ProjectCompScience.View;
using System.IO;
using ProjectCompScience.Services;

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
                ConnectionService.GetInstance();
            }
            catch (System.IO.FileNotFoundException fnfEx)
            {
                System.Diagnostics.Debug.WriteLine($"\n\n🚨 CRITICAL MISSING FILE: {fnfEx.FileName} 🚨\n\n");
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


