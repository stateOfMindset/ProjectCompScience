using ProjectCompScience.View;

namespace ProjectCompScience
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new StockSharesPage();
        }
    }
}
