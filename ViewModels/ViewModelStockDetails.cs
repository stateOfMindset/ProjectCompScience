using ProjectCompScience.Models;
using ProjectCompScience.Services;
using ProjectCompScience.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Controls;

namespace ProjectCompScience.ViewModels
{
    [QueryProperty(nameof(Ticker), "ticker")]
    [QueryProperty(nameof(CompanyName), "name")]
    [QueryProperty(nameof(IsFromPortfolio), "fromPortfolio")]
    [QueryProperty(nameof(ReturnedCompareTicker), "compareTicker")]
    internal class StockDetailsViewModel : ViewModelBase
    {
        public event Action OnGraphDataChanged;

        #region Fields
        private string _ticker;
        private string _companyName;
        private string _currentPrice;
        private string _percentageChange;
        private Color _trendColor;
        private List<StockGraphPoint> _allFetchedPoints = new();
        private List<StockGraphPoint> _allCompare1Points = new();
        private List<StockGraphPoint> _allCompare2Points = new();
        private string _currentTimeRange = "1M";
        #endregion

        #region Properties
        private bool _isFromPortfolio;
        public bool IsFromPortfolio
        {
            get => _isFromPortfolio;
            set { _isFromPortfolio = value; OnPropertyChanged(); }
        }

        public string Ticker
        {
            get => _ticker;
            set
            {
                _ticker = value;
                OnPropertyChanged();
                _ = LoadInitialDataAsync();
            }
        }

        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(); }
        }

        public string CurrentPrice
        {
            get => _currentPrice;
            set { _currentPrice = value; OnPropertyChanged(); }
        }

        public string PercentageChange
        {
            get => _percentageChange;
            set { _percentageChange = value; OnPropertyChanged(); }
        }

        public Color TrendColor
        {
            get => _trendColor;
            set { _trendColor = value; OnPropertyChanged(); }
        }

        public GraphPlotter MyGraphDrawable => api_services.GetStockAPIService()._plotter;

        // Catch the returning ticker from the BuyShares page
        public string ReturnedCompareTicker
        {
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _ = LoadAndDrawComparisonStockAsync(value);
                }
            }
        }
        #endregion

        #region Comparison Properties
        private bool _isCompare1Visible;
        public bool IsCompare1Visible { get => _isCompare1Visible; set { _isCompare1Visible = value; OnPropertyChanged(); } }
        private string _compare1Ticker, _compare1Price, _compare1Change;
        public string Compare1Ticker { get => _compare1Ticker; set { _compare1Ticker = value; OnPropertyChanged(); } }
        public string Compare1Price { get => _compare1Price; set { _compare1Price = value; OnPropertyChanged(); } }
        public string Compare1Change { get => _compare1Change; set { _compare1Change = value; OnPropertyChanged(); } }

        private bool _isCompare2Visible;
        public bool IsCompare2Visible { get => _isCompare2Visible; set { _isCompare2Visible = value; OnPropertyChanged(); } }
        private string _compare2Ticker, _compare2Price, _compare2Change;
        public string Compare2Ticker { get => _compare2Ticker; set { _compare2Ticker = value; OnPropertyChanged(); } }
        public string Compare2Price { get => _compare2Price; set { _compare2Price = value; OnPropertyChanged(); } }
        public string Compare2Change { get => _compare2Change; set { _compare2Change = value; OnPropertyChanged(); } }

        public ICommand AddCompareCommand { get; }
        public ICommand RemoveCompare1Command { get; }
        public ICommand RemoveCompare2Command { get; }
        #endregion

        #region Commands
        public ICommand ChangeTimeRangeCommand { get; }
        public ICommand PredictFutureCommand { get; }
        public ICommand BuyStockCommand { get; }
        public ICommand SellStockCommand { get; }
        #endregion

        #region Constructor
        public StockDetailsViewModel()
        {
            ChangeTimeRangeCommand = new Command<string>(UpdateGraphRange);
            PredictFutureCommand = new Command(async () => await GeneratePredictionAsync());
            BuyStockCommand = new Command(async () => await ExecuteBuyStockAsync());
            SellStockCommand = new Command(async () => await ExecuteSellStockAsync());
            AddCompareCommand = new Command(async () => await ExecuteAddCompareAsync());

            RemoveCompare1Command = new Command(() => { IsCompare1Visible = false; _allCompare1Points.Clear(); api_services.GetStockAPIService()._plotter.ComparePoints1 = null; OnGraphDataChanged?.Invoke(); });
            RemoveCompare2Command = new Command(() => { IsCompare2Visible = false; _allCompare2Points.Clear(); api_services.GetStockAPIService()._plotter.ComparePoints2 = null; OnGraphDataChanged?.Invoke(); });

        }
        #endregion

        #region Logic
        private async Task ExecuteAddCompareAsync()
        {
            if (IsCompare1Visible && IsCompare2Visible)
            {
                await Shell.Current.DisplayAlert("Limit Reached", "You can only compare a maximum of two additional stocks at a time.", "OK");
                return;
            }

            // Navigate to the Search Page in "Compare Mode"
            await Shell.Current.GoToAsync("CompareSearch?IsComparing=true");
        }

        private async Task LoadAndDrawComparisonStockAsync(string tickerInput)
        {
            tickerInput = tickerInput.ToUpper().Trim();
            var apiService = api_services.GetStockAPIService();
            var rawData = await apiService.FetchStockListAsync(tickerInput);

            if (rawData == null || !rawData.Any())
            {
                await Shell.Current.DisplayAlert("Error", $"Could not load data for {tickerInput}.", "OK");
                return;
            }

            DateTime endDate = rawData.Last().Timestamp;
            DateTime startDate = _currentTimeRange switch
            {
                "1W" => endDate.AddDays(-7),
                "2W" => endDate.AddDays(-14),
                "1M" => endDate.AddMonths(-1),
                "2M" => endDate.AddMonths(-2),
                "3M" => endDate.AddMonths(-3),
                _ => endDate.AddMonths(-1)
            };

            var filtered = rawData.Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate).OrderBy(p => p.Timestamp).ToList();

            if (filtered.Count < 2) return;

            float firstPrice = float.Parse(filtered.First().Open);
            float lastPrice = float.Parse(filtered.Last().Open);
            float priceDiff = lastPrice - firstPrice;
            float percentDiff = (priceDiff / firstPrice) * 100;
            string sign = priceDiff >= 0 ? "+" : "";

            if (!IsCompare1Visible)
            {
                Compare1Ticker = tickerInput;
                Compare1Price = $"${lastPrice:F2}";
                Compare1Change = $"{sign}{percentDiff:F2}%";

                _allCompare1Points = rawData; 
                apiService._plotter.ComparePoints1 = filtered;
                IsCompare1Visible = true;
            }
            else if (!IsCompare2Visible)
            {
                Compare2Ticker = tickerInput;
                Compare2Price = $"${lastPrice:F2}";
                Compare2Change = $"{sign}{percentDiff:F2}%";

                _allCompare2Points = rawData; 
                apiService._plotter.ComparePoints2 = filtered;
                IsCompare2Visible = true;
            }

            OnGraphDataChanged?.Invoke();
        }

        private async Task LoadInitialDataAsync()
        {
            var apiService = api_services.GetStockAPIService();
            _allFetchedPoints = await apiService.FetchStockListAsync(Ticker);
            UpdateGraphRange("1M");
        }

        private void UpdateGraphRange(string timeRange)
        {
            if (_allFetchedPoints == null || !_allFetchedPoints.Any()) return;

            _currentTimeRange = timeRange;

            DateTime endDate = _allFetchedPoints.Last().Timestamp;
            DateTime startDate = timeRange switch
            {
                "1W" => endDate.AddDays(-7),
                "2W" => endDate.AddDays(-14),
                "1M" => endDate.AddMonths(-1),
                "2M" => endDate.AddMonths(-2),
                "3M" => endDate.AddMonths(-3),
                _ => endDate.AddMonths(-1)
            };


            var filteredPoints = _allFetchedPoints
                .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
                .OrderBy(p => p.Timestamp)
                .ToList();

            if (filteredPoints.Count >= 2)
            {
                float firstPrice = float.Parse(filteredPoints.First().Open);
                float lastPrice = float.Parse(filteredPoints.Last().Open);
                float priceDiff = lastPrice - firstPrice;
                float percentDiff = (priceDiff / firstPrice) * 100;

                CurrentPrice = $"{lastPrice:F2} usd";
                string sign = priceDiff >= 0 ? "+" : "";
                PercentageChange = $"{sign}{percentDiff:F2}% | {sign}{priceDiff:F2}";
                TrendColor = priceDiff >= 0 ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF5252");
            }

            var apiService = api_services.GetStockAPIService();
            apiService.stocksValues = filteredPoints;
            apiService._plotter.StockPoints = filteredPoints;

            if (IsCompare1Visible && _allCompare1Points != null && _allCompare1Points.Any())
            {
                apiService._plotter.ComparePoints1 = _allCompare1Points
                    .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
                    .OrderBy(p => p.Timestamp)
                    .ToList();
            }

            if (IsCompare2Visible && _allCompare2Points != null && _allCompare2Points.Any())
            {
                apiService._plotter.ComparePoints2 = _allCompare2Points
                    .Where(p => p.Timestamp >= startDate && p.Timestamp <= endDate)
                    .OrderBy(p => p.Timestamp)
                    .ToList();
            }

            if (apiService._plotter.PredictionData != null)
            {
                apiService._plotter.PredictionData.Clear();
            }

            OnGraphDataChanged?.Invoke();
        }

        private async Task GeneratePredictionAsync()
        {
            var apiService = api_services.GetStockAPIService();
            var currentPoints = apiService._plotter.StockPoints;

            if (currentPoints == null || !currentPoints.Any()) return;

            int daysToPredict = _currentTimeRange switch
            {
                "1W" => 1,
                "2W" => 2,
                "1M" => 5,
                "2M" => 10,
                "3M" => 15,
                _ => 5
            };

            var historicalPrices = currentPoints.Select(p => double.Parse(p.Open ?? "0")).ToList();

            var aiService = new SemanticPredictionService();
            var predictedPrices = await aiService.GetFuturePointsAsync(historicalPrices, daysToPredict);

            var newPredictionList = new List<StockGraphPoint>();
            DateTime lastKnownDate = currentPoints.Last().Timestamp;

            for (int i = 0; i < predictedPrices.Count; i++)
            {
                newPredictionList.Add(new StockGraphPoint
                {
                    Timestamp = lastKnownDate.AddDays(i + 1),
                    Open = predictedPrices[i].ToString(),
                    High = (predictedPrices[i] * 1.02).ToString(),
                    Low = (predictedPrices[i] * 0.98).ToString(),
                    Close = predictedPrices[i].ToString()
                });
            }

            apiService._plotter.PredictionData = newPredictionList;
            OnGraphDataChanged?.Invoke();
        }

        private async Task ExecuteBuyStockAsync()
        {
            if (string.IsNullOrEmpty(CurrentPrice)) return;
            string rawPrice = CurrentPrice.Replace(" usd", "").Trim();
            if (!double.TryParse(rawPrice, out double price)) return;

            string sharesInput = await Shell.Current.DisplayPromptAsync(
                "Buy Stock",
                $"How many shares of {Ticker} do you want to buy at ${price:F2}?",
                initialValue: "1",
                keyboard: Microsoft.Maui.Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(sharesInput) || !int.TryParse(sharesInput, out int shares) || shares <= 0)
                return;

            double totalCost = shares * price;

            var dbService = LocalDataService.GetLocalDataService();
            string currentUserId = Preferences.Get("UserId", "UnknownID");

            double balance = await dbService.GetUserBalanceAsync(currentUserId);

            if (totalCost > balance)
            {
                await Shell.Current.DisplayAlert("Insufficient Funds",
                    $"This transaction costs ${totalCost:F2}, but you only have ${balance:F2} in your wallet.", "OK");
                return;
            }

            bool confirm = await Shell.Current.DisplayAlert("Confirm Purchase",
                $"Buy {shares} shares of {Ticker} for ${totalCost:F2}?\n\nRemaining Balance: ${(balance - totalCost):F2}",
                "CONFIRM BUY", "CANCEL");

            if (!confirm) return;

            double newBalance = balance - totalCost;
            await dbService.UpdateUserBalanceAsync(currentUserId, newBalance);

            string newTransactionId = Guid.NewGuid().ToString().Split('-')[0].ToUpper();

            var transaction = new Transaction
            {
                Id = newTransactionId,
                Ticker = Ticker,
                CompanyName = CompanyName,
                TransactionType = "BUY",
                Shares = shares,
                PricePerShare = price,
                TotalAmount = totalCost,
                Date = DateTime.UtcNow
            };

            await dbService.RecordTransactionAsync(currentUserId, transaction);

            var portfolioUpdate = new PortfolioItem
            {
                Ticker = Ticker,
                CompanyName = CompanyName,
                TotalShares = shares,
                AveragePurchasePrice = price
            };
            await dbService.UpdatePortfolioAsync(currentUserId, portfolioUpdate);

            string receiptMessage =
                $"Transaction ID: #{newTransactionId}\n" +
                $"Stock: {CompanyName} ({Ticker})\n" +
                $"Shares Bought: {shares}\n" +
                $"Price Per Share: ${price:F2}\n" +
                $"Total Cost: ${totalCost:F2}\n\n" +
                $"Remaining Balance: ${(balance - totalCost):F2}";

            await Shell.Current.DisplayAlert("Trade Successful!", receiptMessage, "CLOSE");

            string userEmailAddress = dbService.fullDetaillsLoggedInUser?.Email;

            if (!string.IsNullOrEmpty(userEmailAddress))
            {
                _ = Email_service.SendReceiptAsync(
                    userEmailAddress,
                    $"Trade Confirmation: {shares} shares of {Ticker}",
                    $"Thank you for your purchase!\n\n{receiptMessage}\n\nHappy Trading!");
            }
        }

        private async Task ExecuteSellStockAsync()
        {
            if (string.IsNullOrEmpty(CurrentPrice)) return;
            string rawPrice = CurrentPrice.Replace(" usd", "").Trim();
            if (!double.TryParse(rawPrice, out double price)) return;

            var dbService = LocalDataService.GetLocalDataService();
            string currentUserId = Preferences.Get("UserId", "UnknownID");

            int ownedShares = await dbService.GetOwnedSharesAsync(currentUserId, Ticker);

            if (ownedShares <= 0)
            {
                await Shell.Current.DisplayAlert("No Shares", $"You don't own any shares of {Ticker} to sell.", "OK");
                return;
            }

            string sharesInput = await Shell.Current.DisplayPromptAsync(
                "Sell Stock",
                $"You own {ownedShares} shares.\nHow many do you want to sell at ${price:F2}?",
                initialValue: ownedShares.ToString(),
                keyboard: Microsoft.Maui.Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(sharesInput) || !int.TryParse(sharesInput, out int sharesToSell) || sharesToSell <= 0)
                return;

            if (sharesToSell > ownedShares)
            {
                await Shell.Current.DisplayAlert("Error", $"You cannot sell more shares than you own! You only have {ownedShares} shares.", "OK");
                return;
            }

            double totalRevenue = sharesToSell * price;
            double currentBalance = await dbService.GetUserBalanceAsync(currentUserId);

            bool confirm = await Shell.Current.DisplayAlert("Confirm Sale",
                $"Sell {sharesToSell} shares of {Ticker} for ${totalRevenue:F2}?\n\nNew Wallet Balance: ${(currentBalance + totalRevenue):F2}",
                "CONFIRM SELL", "CANCEL");

            if (!confirm) return;

            double newBalance = currentBalance + totalRevenue;
            await dbService.UpdateUserBalanceAsync(currentUserId, newBalance);

            string newTransactionId = Guid.NewGuid().ToString().Split('-')[0].ToUpper();

            var transaction = new Transaction
            {
                Id = newTransactionId,
                Ticker = Ticker,
                CompanyName = CompanyName,
                TransactionType = "SELL",
                Shares = sharesToSell,
                PricePerShare = price,
                TotalAmount = totalRevenue,
                Date = DateTime.UtcNow
            };
            await dbService.RecordTransactionAsync(currentUserId, transaction);

            var portfolioUpdate = new PortfolioItem
            {
                Ticker = Ticker,
                CompanyName = CompanyName,
                TotalShares = -sharesToSell,
                AveragePurchasePrice = price
            };
            await dbService.UpdatePortfolioAsync(currentUserId, portfolioUpdate);

            string receiptMessage =
                $"Transaction ID: #{newTransactionId}\n" +
                $"Stock: {CompanyName} ({Ticker})\n" +
                $"Shares Sold: {sharesToSell}\n" +
                $"Price Per Share: ${price:F2}\n" +
                $"Total Revenue: ${totalRevenue:F2}\n\n" +
                $"New Balance: ${newBalance:F2}";

            await Shell.Current.DisplayAlert("Sale Successful!", receiptMessage, "CLOSE");

            string userEmailAddress = dbService.fullDetaillsLoggedInUser?.Email;
            if (!string.IsNullOrEmpty(userEmailAddress))
            {
                _ = Email_service.SendReceiptAsync(
                    userEmailAddress,
                    $"Trade Confirmation: Sold {sharesToSell} shares of {Ticker}",
                    $"Your sale was successfully executed!\n\n{receiptMessage}\n\nHappy Trading!");
            }
        }
        #endregion
    }
}