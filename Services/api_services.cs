using ProjectCompScience.Components;
using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

// --- NEW USINGS FOR FIREBASE ---
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
        public int xMin { get; set; }
        public int xMax { get; set; }
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
               
                string envPath = LocalDataService.EnvFilePath;
                if (File.Exists(envPath))
                {
                    DotNetEnv.Env.Load(envPath);
                }
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
            string domain = Environment.GetEnvironmentVariable("API_SITE_stocks") ?? string.Empty;


            if (string.IsNullOrEmpty(domain))
                domain = "https://www.alphavantage.co/query";

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

        // Helper to format emails safely for Firebase
        private string SanitizeEmail(string email) => email.Replace(".", "_");

        // GET BALANCE
        public async Task<double> GetUserBalanceAsync(string userEmail)
        {
            try
            {
                string safeEmail = SanitizeEmail(userEmail);
                var balance = await _firebase
                    .Child("Users")
                    .Child(safeEmail)
                    .Child("Balance")
                    .OnceSingleAsync<double>();

                // If they have 0 or don't exist yet, seed them with $10,000!
                if (balance == 0)
                {
                    await UpdateUserBalanceAsync(userEmail, 10000.00);
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
        public async Task UpdateUserBalanceAsync(string userEmail, double newBalance)
        {
            string safeEmail = SanitizeEmail(userEmail);
            await _firebase
                .Child("Users")
                .Child(safeEmail)
                .Child("Balance")
                .PutAsync(newBalance);
        }

        // RECORD TRANSACTION
        public async Task RecordTransactionAsync(string userEmail, Transaction transaction)
        {
            string safeEmail = SanitizeEmail(userEmail);
            await _firebase
                .Child("Users")
                .Child(safeEmail)
                .Child("Transactions")
                .PostAsync(transaction);
        }

        // UPDATE PORTFOLIO SHARES
        public async Task UpdatePortfolioAsync(string userEmail, PortfolioItem item)
        {
            string safeEmail = SanitizeEmail(userEmail);

            // We use the Ticker (e.g., "AAPL") as the key so it's easy to look up later
            await _firebase
                .Child("Users")
                .Child(safeEmail)
                .Child("Portfolio")
                .Child(item.Ticker)
                .PutAsync(item);
        }
        #endregion
    }
}