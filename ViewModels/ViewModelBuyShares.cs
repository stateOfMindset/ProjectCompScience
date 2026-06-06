using Microsoft.Maui.Controls; // חשוב בשביל הניווט של Shell
using ProjectCompScience.Models;
using ProjectCompScience.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using ProjectCompScience.Services;

namespace ProjectCompScience.ViewModels
{
    // הוספנו את IQueryAttributable כדי שנוכל לקבל פרמטרים מבחוץ
    internal class ViewModelBuyShares : ViewModelBase, IQueryAttributable
    {
        #region Fields
        private List<StockSymbol> _allStocks = new List<StockSymbol>();
        private ObservableCollection<StockSymbol> _filteredStocks;
        private StockSymbol _selectedStock;
        #endregion

        #region Properties
        // דגל חדש שאומר לנו אם אנחנו במצב "הוספת מניה להשוואה"
        public bool IsComparing { get; set; } = false;
        private Color _targetColor = Color.FromArgb("#2ECC71");

        public ObservableCollection<StockSymbol> FilteredStocks
        {
            get => _filteredStocks;
            set
            {
                _filteredStocks = value;
                OnPropertyChanged();
            }
        }

        public StockSymbol SelectedStock
        {
            get => _selectedStock;
            set
            {
                _selectedStock = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand SearchStocksCommand { get; }
        public ICommand SelectStockCommand { get; }
        public ICommand SaveDataSilentlyCommand { get; }
        #endregion

        #region Constructor
        public ViewModelBuyShares()
        {
            FilteredStocks = new ObservableCollection<StockSymbol>();

            SearchStocksCommand = new Command<string>(SearchStocks);
            SelectStockCommand = new Command<StockSymbol>(async (stock) => await GoToStockPage(stock));
            SaveDataSilentlyCommand = new Command(SaveDataSilently);

            _ = LoadEmbeddedStocksAsync();
        }


        #endregion

        #region Navigation Arguments
        // הפונקציה הזו תופסת את הדגל שמגיע מעמוד הגרף
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int currentIndex = 0;

            if (query.ContainsKey("IsComparing"))
            {
                IsComparing = bool.Parse(query["IsComparing"].ToString());
                query.Remove("IsComparing");
            }

            if (query.ContainsKey("compareIndex"))
            {
                currentIndex = int.Parse(query["compareIndex"].ToString());
                query.Remove("compareIndex");
            }

            // מעדכנים את המשתנה הגלובלי לפי מצב ההשוואה
            if (IsComparing && currentIndex == 1)
            {
                _targetColor = Colors.Cyan;
            }
            else if (IsComparing && currentIndex == 2)
            {
                _targetColor = Colors.Orange;
            }
            else
            {
                _targetColor = Color.FromArgb("#2ECC71");
            }

            // אם המניות כבר נטענו (הקריאה מהקובץ הסתיימה), נעדכן אותן עכשיו
            if (_allStocks != null && _allStocks.Any())
            {
                foreach (var stock in _allStocks)
                {
                    stock.ThemeColor = _targetColor;
                }

                // מרעננים את ה-UI
                FilteredStocks = new ObservableCollection<StockSymbol>(_allStocks);
            }
        }
        #endregion

            #region Logic Methods
        private void SearchStocks(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                FilteredStocks = new ObservableCollection<StockSymbol>(_allStocks);
            }
            else
            {
                var results = _allStocks
                    .Where(s => (s.Title != null && s.Title.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                                (s.Ticker != null && s.Ticker.Contains(query, StringComparison.OrdinalIgnoreCase)));

                FilteredStocks = new ObservableCollection<StockSymbol>(results);
            }
        }

        private async Task GoToStockPage(StockSymbol stock)
        {
            if (stock == null) return;

            if (IsComparing)
            {
                var navParams = new Dictionary<string, object> { { "compareTicker", stock.Ticker } };
                await Shell.Current.GoToAsync("..", navParams);
                IsComparing = false;
            }
            else
            {
                await Shell.Current.GoToAsync($"{nameof(StockDetails)}?ticker={stock.Ticker}&name={stock.Title}");
            }

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(100);
                SelectedStock = null;
            });
        }

        private void SaveDataSilently()
        {
            try
            {
                string jsonText = JsonSerializer.Serialize(FilteredStocks);
                string fileName = "InternalStockData.json";
                string filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);
                File.WriteAllText(filePath, jsonText);
                Console.WriteLine($"Saved silently to: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save: {ex.Message}");
            }
        }

        private async Task LoadEmbeddedStocksAsync()
        {
            try
            {
                using var stream = await FileSystem.OpenAppPackageFileAsync("company_tickers.json");
                using var reader = new StreamReader(stream);
                string jsonText = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var secData = JsonSerializer.Deserialize<Dictionary<string, StockSymbol>>(jsonText, options);

                if (secData != null)
                {
                    _allStocks = secData.Values.ToList();

                    // צובעים לפי הצבע הנוכחי שנבחר (אם הניווט כבר הספיק לעדכן אותו - הוא יצבע בתכלת/כתום, ואם לא - בירוק)
                    foreach (var stock in _allStocks)
                    {
                        stock.ThemeColor = _targetColor;
                    }

                    FilteredStocks = new ObservableCollection<StockSymbol>(_allStocks);
                    Console.WriteLine($"Successfully loaded {_allStocks.Count} stocks from the SEC!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading SEC file: {ex.Message}");
            }
        }
        #endregion
    }
}