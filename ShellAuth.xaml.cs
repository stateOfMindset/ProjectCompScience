using ProjectCompScience.Services;
using ProjectCompScience.View;

namespace ProjectCompScience
{
    public partial class ShellAuth : Shell
    {
        public ShellAuth()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(StockDetails), typeof(StockDetails));
            Routing.RegisterRoute(nameof(TransactionDetailsViewl), typeof(TransactionDetailsViewl));
            //Routing.RegisterRoute("CompareSearch", typeof(BuyShares));
        }

        private async void MenuItem_Logout_Clicked(object sender, EventArgs e)
        {
            LocalDataService.GetLocalDataService().Logout();
            ((App)Application.Current).SetUnauthenticatedShell();

        }
    }
}