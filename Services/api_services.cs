using ProjectCompScience.Components;
using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Firebase.Database;
using Firebase.Database.Query;

namespace ProjectCompScience.Services
{
    internal class api_services
    {
        // 1. HTTP Client for Alpha Vantage
        private readonly HttpClient _httpClient = new HttpClient();

        // 2. Client for the Firebase
        private readonly FirebaseClient _firebase;

        public GraphPlotter _plotter = new GraphPlotter();
        public bool isZoom = false;
        public List<StockGraphPoint> stocksValues { get; set; } = new();

        #region instance 
        private static api_services? instance;
        static public api_services GetStockAPIService()
        {
            if (instance == null)
            {
                instance = new api_services();
            }
            return instance;
        }
        #endregion

        #region constructor
        public api_services()
        {
            try
            {
                //string envPath = LocalDataService.EnvFilePath;
                //if (File.Exists(envPath))
                //{
                //    DotNetEnv.Env.Load(envPath);
                //}

                string firebaseUrl = Environment.GetEnvironmentVariable("url_firebase") ?? null;
                _firebase = new FirebaseClient(firebaseUrl);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load .env file: {ex.Message}");
            }
        }
        #endregion

        #region Alpha Vantage Stock Fetching
        public async Task<List<StockGraphPoint>> FetchStockListAsync(string symbol)
        {
            string api_key = Environment.GetEnvironmentVariable("API_KEY_stocks") ?? "demo";

            string domain = "https://www.alphavantage.co/query";

            string url = $"{domain}?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={api_key}&outputsize=compact";

            try
            {
                string jsonString = await _httpClient.GetStringAsync(url);
                var root = JsonSerializer.Deserialize<api_response_get>(jsonString);
                var stockList = new List<StockGraphPoint>();

                if (root?.TimeSeries != null)
                {
                    foreach (var entry in root.TimeSeries)
                    {
                        var point = entry.Value;
                        point.Timestamp = DateTime.Parse(entry.Key);
                        stockList.Add(point);
                    }
                }
                return stockList.OrderBy(x => x.Timestamp).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching data: {ex.Message}");
                return new List<StockGraphPoint>();
            }
        }
        #endregion

        #region Firebase Economy Methods

        // GET BALANCE (Updated to use userId and exact JSON structure)
        public async Task<double> GetUserBalanceAsync(string userId)
        {
            try
            {
                var balance = await _firebase
                    .Child("users")
                    .Child(userId)
                    .Child("Details") // Balance is inside Details according to your JSON
                    .Child("Balance")
                    .OnceSingleAsync<double>();

                if (balance == 0)
                {
                    await UpdateUserBalanceAsync(userId, 10000.00);
                    return 10000.00;
                }
                return balance;
            }
            catch
            {
                return 10000.00; // Default fallback if network fails
            }
        }

        // UPDATE BALANCE
        public async Task UpdateUserBalanceAsync(string userId, double newBalance)
        {
            await _firebase
                .Child("users")
                .Child(userId)
                .Child("Details")
                .Child("Balance")
                .PutAsync(newBalance);
        }

        // RECORD TRANSACTION
        public async Task RecordTransactionAsync(string userId, Transaction transaction)
        {
            await _firebase
                .Child("users")
                .Child(userId)
                .Child("Transactions")
                .PostAsync(transaction);
        }

        // UPDATE PORTFOLIO SHARES
        public async Task UpdatePortfolioAsync(string userId, PortfolioItem item)
        {
            await _firebase
                .Child("users")
                .Child(userId)
                .Child("Portfolio")
                .Child(item.Ticker)
                .PutAsync(item);
        }

        // GET ENTIRE PORTFOLIO
        public async Task<List<PortfolioItem>> GetPortfolioAsync(string userId)
        {
            try
            {
                // Fetch all items under the user's Portfolio node using exact ID
                var portfolioRecords = await _firebase
                    .Child("users")
                    .Child(userId)
                    .Child("Portfolio")
                    .OnceAsync<PortfolioItem>();

                var portfolioList = new List<PortfolioItem>();

                foreach (var record in portfolioRecords)
                {
                    // We only want to add it to the pie chart if they actually own shares (> 0)
                    if (record.Object != null && record.Object.TotalShares > 0)
                    {
                        portfolioList.Add(record.Object);
                    }
                }

                return portfolioList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching portfolio: {ex.Message}");
                return new List<PortfolioItem>();
            }
        }
        #endregion
    }
}