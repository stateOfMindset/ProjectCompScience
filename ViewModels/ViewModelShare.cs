using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel;
using ProjectCompScience.Models;
using ProjectCompScience.Services;
using ProjectCompScience.Components; // החיבור לקומפוננטת הציור!

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelShare : ViewModelBase
    {
        // --- אזעקת הציור והאובייקט של הפאי ---
        public event Action OnPortfolioDataChanged;

        private PortfolioPieChart _myPieChart = new PortfolioPieChart();
        public PortfolioPieChart MyPieChart
        {
            get => _myPieChart;
            set { _myPieChart = value; OnPropertyChanged(); }
        }

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
            double calculatedNetWorth = 0;

            // 1. טוענים את הרשימה ומחשבים את סך הכסף הכולל
            foreach (var item in items)
            {
                StockShares.Add(item);
                calculatedNetWorth += item.TotalShares * item.AveragePurchasePrice;
            }

            // 2. בונים את חתיכות הפאי עם האחוזים!
            var chartColors = new[] { Colors.Cyan, Color.FromArgb("#9B59B6"), Colors.Gold, Color.FromArgb("#E74C3C"), Color.FromArgb("#2ECC71") };
            var newSlices = new List<PieSlice>();
            int colorIndex = 0;

            if (calculatedNetWorth > 0)
            {
                foreach (var item in items)
                {
                    double investedValue = item.TotalShares * item.AveragePurchasePrice;
                    newSlices.Add(new PieSlice
                    {
                        Ticker = item.Ticker,
                        InvestedValue = investedValue,
                        Percentage = (investedValue / calculatedNetWorth) * 100,
                        SliceColor = chartColors[colorIndex % chartColors.Length]
                    });
                    colorIndex++;
                }
            }

            var updatedChart = new PortfolioPieChart();
            updatedChart.Slices = newSlices;
            MyPieChart = updatedChart;

            // 3. מפעילים את האזעקה ל-UI כדי שיצייר את הפאי
            MainThread.BeginInvokeOnMainThread(() =>
            {
                OnPortfolioDataChanged?.Invoke();
            });
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

            // SHOW THE REAL RECEIPT
            await App.Current.MainPage.DisplayAlert("Trade Executed!",
                $"Stock: {itemToSell.Ticker}\n" +
                $"Shares Sold: {itemToSell.TotalShares}\n" +
                $"Sell Price: ${livePrice:F2}\n" +
                $"Total Return: ${refundAmount:F2}\n\n" +
                $"{profitLossText}\n\n" +
                $"New Wallet Balance: ${(currentBalance + refundAmount):F2}", "AWESOME");

            // קוראים שוב לטעינה כדי שהגרף יתעדכן וימחק את המניה שנמכרה!
            await LoadDataAsync();
        }
    }
}