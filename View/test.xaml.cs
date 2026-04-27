using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProjectCompScience.Components;
using ProjectCompScience.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProjectCompScience.View;

public partial class test : ContentPage
{
    private readonly HttpClient _httpClient = new HttpClient();
    private GraphPlotter _plotter = new GraphPlotter();
    private int xMin, xMax;
    private bool isZoom = false;
    public List<StockGraphPoint> stocksValues { get; set; } = new();


    public test()
    {
        InitializeComponent();

        string envPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", ".env");
        DotNetEnv.Env.Load(envPath);

        graphicView.Drawable = _plotter;

        graphicView.StartInteraction += (s, e) =>
        {
            xMin = (int)e.Touches.First().X;
        };

        graphicView.EndInteraction += (s, e) =>
        {
            if (stocksValues == null || !stocksValues.Any()) return;

            xMax = (int)e.Touches.First().X;

            // 2. Ensure xMin is always the smaller number (in case they drag backwards)
            int realMin = Math.Min(xMin, xMax);
            int realMax = Math.Max(xMin, xMax);

            // 3. Update the existing plotter instead of making a new one
            isZoom = true;
            _plotter.Xmin = realMin;
            _plotter.Xmax = realMax;
            _plotter.StockPoints = stocksValues; 

            graphicView.Invalidate();
        };

        InitializeDataAsync();

        

    }
    private async void InitializeDataAsync()
    {
        try
        {
            string symbol = "BMI";
            List<StockGraphPoint> stocksValues = await FetchStockListAsync(symbol);
            int xMin = 0;
            int xMax = 0;
            bool isZoom = false;
            _plotter.Xmax = isZoom ? xMax : 1000000;
            _plotter.Xmin = isZoom ? xMin : 0;
            _plotter.StockPoints = stocksValues;

            graphicView.Invalidate();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Initialization failed: {ex.Message}");
        }
    }




    public async Task<List<StockGraphPoint>> FetchStockListAsync(string symbol)
    {
        string api_key = Environment.GetEnvironmentVariable("API_KEY") ?? "demo";
        string url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={symbol}&apikey={api_key}&outputsize=compact";

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
        catch (Exception ex) {
            Console.WriteLine($"Error fetching data: {ex.Message}");
            return new List<StockGraphPoint>();
        }


    }

}