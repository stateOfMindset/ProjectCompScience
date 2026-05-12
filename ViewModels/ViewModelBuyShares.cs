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

namespace ProjectCompScience.ViewModels
{
    internal class ViewModelBuyShares : ViewModelBase
    {
        #region Fields
        private List<StockSymbol> _allStocks = new List<StockSymbol>();
        private ObservableCollection<StockSymbol> _filteredStocks;
        private StockSymbol _selectedStock;
        #endregion

        #region Properties
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

            SelectedStock = stock;

           
            await Shell.Current.GoToAsync($"{nameof(StockDetails)}?ticker={stock.Ticker}&name={stock.Title}");

            FilteredStocks.Clear();
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