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
            //Routing.RegisterRoute("BuyShares", typeof(BuyShares));
            Routing.RegisterRoute("StockDetails", typeof(StockDetails));
            Routing.RegisterRoute("CompareSearch", typeof(ProjectCompScience.View.BuyShares));
        }

        private async void MenuItem_Logout_Clicked(object sender, EventArgs e)
        {
            LocalDataService.GetLocalDataService().Logout();
            ((App)Application.Current).SetUnauthenticatedShell();

        }
    }
}