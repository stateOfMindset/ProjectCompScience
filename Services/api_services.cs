using ProjectCompScience.Components;
using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProjectCompScience.Services
{
    internal class api_services
    {
        private readonly HttpClient _httpClient = new HttpClient();
        public GraphPlotter _plotter = new GraphPlotter();
        public int xMin {get; set;}
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
        public api_services() {
            string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", ".env");
            DotNetEnv.Env.Load(envPath);
        }

        #endregion
        public async Task<List<StockGraphPoint>> FetchStockListAsync(string symbol)
        {
            
            string api_key = Environment.GetEnvironmentVariable("API_KEY_stocks") ?? "demo";
            string domain = Environment.GetEnvironmentVariable("API_SITE_stocks") ?? string.Empty;
            string url = $"{domain}?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={api_key}&outputsize=compact";
            //string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=IBM&apikey=demo";
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
    }


}
