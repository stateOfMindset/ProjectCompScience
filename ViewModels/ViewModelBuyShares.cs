using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using ProjectCompScience.View;
using Microsoft.Maui.Controls; // חשוב בשביל הניווט של Shell

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
            Console.WriteLine($"[DEBUG] ApplyQueryAttributes hit. FilteredStocks count is: {FilteredStocks?.Count}");
            if (query.ContainsKey("IsComparing"))
            {
                IsComparing = bool.Parse(query["IsComparing"].ToString());
                // מנקים את זה כדי שלא נישאר במצב השוואה לנצח
                query.Remove("IsComparing");
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