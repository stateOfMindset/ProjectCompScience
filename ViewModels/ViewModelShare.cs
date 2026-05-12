using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using ProjectCompScience.Models;
using ProjectCompScience.Services;

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelShare : ViewModelBase
    {
        private ObservableCollection<PortfolioItem> _stockShares;
        public ObservableCollection<PortfolioItem> StockShares
        {
            get => _stockShares;
            set { _stockShares = value; OnPropertyChanged(); }
        }

        public ICommand ButtonMovePageCommand { get; set; }
        public ICommand ButtonDeleteShareCommand { get; set; }
        public ICommand GoToDetailsCommand { get; set; }

        public ViewModelShare()
        {
            StockShares = new ObservableCollection<PortfolioItem>();

            ButtonMovePageCommand = new Command(async () => {
                await Shell.Current.GoToAsync("//BuyShares");
            });

   
            GoToDetailsCommand = new Command<PortfolioItem>(async (item) => {
                if (item != null)
                {
       
                    await Shell.Current.GoToAsync($"StockDetails?ticker={item.Ticker}&name={item.CompanyName}&fromPortfolio=true");
                }
            });

            ButtonDeleteShareCommand = new Command<PortfolioItem>(async (item) => {
                if (item != null)
                {
                    await SellShareAsync(item);
                }
            });
        }

        public async Task LoadDataAsync()
        {
            string currentUserId = Preferences.Get("UserId", "");
            if (string.IsNullOrEmpty(currentUserId)) return;

            var db = LocalDataService.GetLocalDataService();
            var items = await db.GetUserPortfolioAsync(currentUserId);

            StockShares.Clear();
            foreach (var item in items)
            {
                StockShares.Add(item);
            }
        }

        private async Task SellShareAsync(PortfolioItem itemToSell)
        {
            bool confirm = await App.Current.MainPage.DisplayAlert(
                "Sell Stock",
                $"Are you sure you want to sell all {itemToSell.TotalShares} shares of {itemToSell.Ticker}?",
                "Yes, Sell", "Cancel");

            if (!confirm) return;

            double livePrice = itemToSell.AveragePurchasePrice; // Fallback just in case
            try
            {
                var apiService = api_services.GetStockAPIService();
                var recentData = await apiService.FetchStockListAsync(itemToSell.Ticker);

                if (recentData != null && recentData.Any())
                {

                    livePrice = double.Parse(recentData.Last().Open ?? "0");
                }
            }
            catch
            {
  
                await App.Current.MainPage.DisplayAlert("Market Error", "Could not fetch the live market price. Check your connection.", "OK");
                return;
            }

            string currentUserId = Preferences.Get("UserId", "");
            var db = LocalDataService.GetLocalDataService();

            double refundAmount = itemToSell.TotalShares * livePrice;
            double originalCost = itemToSell.TotalShares * itemToSell.AveragePurchasePrice;
            double profitLoss = refundAmount - originalCost;

  
            string profitLossText = profitLoss >= 0 ? $"📈 Profit: +${profitLoss:F2}" : $"📉 Loss: -${Math.Abs(profitLoss):F2}";

            double currentBalance = await db.GetUserBalanceAsync(currentUserId);
            await db.UpdateUserBalanceAsync(currentUserId, currentBalance + refundAmount);

            await db.RemovePortfolioItemAsync(currentUserId, itemToSell.Ticker);

            StockShares.Remove(itemToSell);

            // 4. SHOW THE REAL RECEIPT
            await App.Current.MainPage.DisplayAlert("Trade Executed!",
                $"Stock: {itemToSell.Ticker}\n" +
                $"Shares Sold: {itemToSell.TotalShares}\n" +
                $"Sell Price: ${livePrice:F2}\n" +
                $"Total Return: ${refundAmount:F2}\n\n" +
                $"{profitLossText}\n\n" +
                $"New Wallet Balance: ${(currentBalance + refundAmount):F2}", "AWESOME");
        }
    }
}