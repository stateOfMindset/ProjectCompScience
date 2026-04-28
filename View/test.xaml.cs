using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProjectCompScience.Components;
using ProjectCompScience.Models;
using ProjectCompScience.Services;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ProjectCompScience.View;

public partial class test : ContentPage
{

    private List<StockGraphPoint> _stocksValues;
    private int _xMin, _xMax;
    private bool _isZoom = false;
    private readonly GraphPlotter _plotter = new GraphPlotter();
    private string symbol;

    public test()
    {
        InitializeComponent();
       


        graphicView.Drawable = _plotter;

        graphicView.StartInteraction += (s, e) =>
        {
            _xMin = (int)e.Touches.First().X;
        };

        graphicView.EndInteraction += (s, e) =>
        {
            if (_stocksValues == null || !_stocksValues.Any()) return;

            _xMax = (int)e.Touches.First().X;

            int realMin = Math.Min(_xMin, _xMax);
            int realMax = Math.Max(_xMin, _xMax);

            _isZoom = true;
            _plotter.Xmin = realMin;
            _plotter.Xmax = realMax;
            _plotter.StockPoints = _stocksValues; 

            graphicView.Invalidate();
        };

        

        

    }

    private void OnUpdateClicked(Object sender, EventArgs e) {
        string userSymbol = StockEntry.Text;

        if (!string.IsNullOrWhiteSpace(userSymbol) && !string.IsNullOrEmpty(userSymbol))
            symbol = userSymbol;
            InitializeDataAsync();


    }

    private async void InitializeDataAsync()
    {
        try
        {
            
            List<StockGraphPoint> stocksValues = await api_services.GetStockAPIService().FetchStockListAsync(symbol);
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




    

}