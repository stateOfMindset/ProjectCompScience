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
using ProjectCompScience.Components;

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

        private double _totalPortfolioValue;
        public double TotalPortfolioValue
        {
            get => _totalPortfolioValue;
            set { _totalPortfolioValue = value; OnPropertyChanged(); }
        }

        public ICommand ButtonMovePageCommand { get; set; }
        public ICommand ButtonDeleteShareCommand { get; set; }
        public ICommand GoToDetailsCommand { get; set; }

        public ViewModelShare()
        {
            StockShares = new ObservableCollection<PortfolioItem>();

            ButtonMovePageCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync("//BuyShares");
            });

            GoToDetailsCommand = new Command<PortfolioItem>(async (item) =>
            {
                if (item != null)
                {
                    await Shell.Current.GoToAsync($"StockDetails?ticker={item.Ticker}&name={item.CompanyName}&fromPortfolio=true");
                }
            });

            ButtonDeleteShareCommand = new Command<PortfolioItem>(async (item) =>
            {
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
            TotalPortfolioValue = calculatedNetWorth;

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
            // 1. שואלים את המשתמש כמה מניות למכור
            string sharesInput = await App.Current.MainPage.DisplayPromptAsync(
                "Sell Stock",
                $"You own {itemToSell.TotalShares} shares.\nHow many do you want to sell?",
                initialValue: itemToSell.TotalShares.ToString(),
                keyboard: Microsoft.Maui.Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(sharesInput) || !int.TryParse(sharesInput, out int sharesToSell) || sharesToSell <= 0)
                return;

            if (sharesToSell > itemToSell.TotalShares)
            {
                await App.Current.MainPage.DisplayAlert("Error", $"You only have {itemToSell.TotalShares} shares to sell.", "OK");
                return;
            }

            // 2. משיגים מחיר עדכני
            double livePrice = itemToSell.AveragePurchasePrice;
            try
            {
                var apiService = api_services.GetStockAPIService();
                var recentData = await apiService.FetchStockListAsync(itemToSell.Ticker);
                if (recentData != null && recentData.Any())
                {
                    livePrice = double.Parse(recentData.Last().Open ?? "0");
                }
            }
            catch { /* מתעלמים אם אין אינטרנט ומשתמשים במחיר הרכישה כגיבוי */ }

            string currentUserId = Preferences.Get("UserId", "");
            var db = LocalDataService.GetLocalDataService();

            double totalRevenue = sharesToSell * livePrice;
            double currentBalance = await db.GetUserBalanceAsync(currentUserId);

            // 3. אישור סופי
            bool confirm = await App.Current.MainPage.DisplayAlert("Confirm Sale",
                $"Sell {sharesToSell} shares of {itemToSell.Ticker} for ${totalRevenue:F2}?\n\nNew Wallet Balance: ${(currentBalance + totalRevenue):F2}",
                "CONFIRM SELL", "CANCEL");

            if (!confirm) return;

            // 4. עדכון יתרה בארנק
            double newBalance = currentBalance + totalRevenue;
            await db.UpdateUserBalanceAsync(currentUserId, newBalance);

            // 5. שמירת הפעולה בהיסטוריית הטרנזקציות
            string newTransactionId = Guid.NewGuid().ToString().Split('-')[0].ToUpper();
            var transaction = new Transaction
            {
                Id = newTransactionId,
                Ticker = itemToSell.Ticker,
                CompanyName = itemToSell.CompanyName,
                TransactionType = "SELL",
                Shares = sharesToSell,
                PricePerShare = livePrice,
                TotalAmount = totalRevenue,
                Date = DateTime.UtcNow
            };
            await db.RecordTransactionAsync(currentUserId, transaction);

            // 6. ניהול מסד הנתונים: מחיקה מוחלטת או החסרה
            if (sharesToSell == itemToSell.TotalShares)
            {
                await db.RemovePortfolioItemAsync(currentUserId, itemToSell.Ticker);
                StockShares.Remove(itemToSell);
            }
            else
            {
                int remainingShares =  (int)itemToSell.TotalShares - sharesToSell;
                var portfolioUpdate = new PortfolioItem
                {
                    Ticker = itemToSell.Ticker,
                    CompanyName = itemToSell.CompanyName,
                    TotalShares = remainingShares,
                    AveragePurchasePrice = itemToSell.AveragePurchasePrice
                };
                await db.UpdatePortfolioAsync(currentUserId, portfolioUpdate);
            }

            // 7. קבלה למשתמש
            string receiptMessage =
                $"Transaction ID: #{newTransactionId}\n" +
                $"Stock: {itemToSell.CompanyName} ({itemToSell.Ticker})\n" +
                $"Shares Sold: {sharesToSell}\n" +
                $"Price Per Share: ${livePrice:F2}\n" +
                $"Total Revenue: ${totalRevenue:F2}\n\n" +
                $"New Balance: ${newBalance:F2}";

            await App.Current.MainPage.DisplayAlert("Sale Successful!", receiptMessage, "CLOSE");

            // 8. רענון הגרף והרשימה
            await LoadDataAsync();
        }
    }
}