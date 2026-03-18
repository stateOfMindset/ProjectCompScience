using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using ProjectCompScience.Components;
using ProjectCompScience.Models;
using System.Security.Cryptography.X509Certificates;
namespace ProjectCompScience.View;

public partial class test : ContentPage
{
    public test()
    {
        InitializeComponent();
        int xMin = 0;
        int xMax = 0;
        bool isZoom = false;

        List<StockGraphPoint> stocksValues = new List<StockGraphPoint>
        {
        new StockGraphPoint { x = DateTime.Now, y = 12.5f },
        new StockGraphPoint { x = DateTime.Now.AddDays(1), y = 12.8f },
        new StockGraphPoint { x = DateTime.Now.AddDays(2), y = 13.2f },
        new StockGraphPoint { x = DateTime.Now.AddDays(3), y = 13.1f },
        new StockGraphPoint { x = DateTime.Now.AddDays(4), y = 13.5f } ,
        new StockGraphPoint { x = DateTime.Now.AddDays(5), y = 13.8f },
        new StockGraphPoint { x = DateTime.Now.AddDays(6), y = 14.2f },
        new StockGraphPoint { x = DateTime.Now.AddDays(7), y = 13.9f }, // Small dip
        new StockGraphPoint { x = DateTime.Now.AddDays(8), y = 14.5f },
        new StockGraphPoint { x = DateTime.Now.AddDays(9), y = 15.1f }, // Breakout!
        new StockGraphPoint { x = DateTime.Now.AddDays(10), y = 14.8f },
        new StockGraphPoint { x = DateTime.Now.AddDays(11), y = 14.2f }, // Profit taking
        new StockGraphPoint { x = DateTime.Now.AddDays(12), y = 14.6f },
        new StockGraphPoint { x = DateTime.Now.AddDays(13), y = 15.3f },
        new StockGraphPoint { x = DateTime.Now.AddDays(14), y = 15.9f }
        };

        graphicView.Drawable = new GraphPlotter
        {
            StockPoints = stocksValues,
            Xmax = isZoom ? xMax : 1000000,
            Xmin = isZoom ? xMin : 0,
        };

      



        graphicView.Invalidate();


        graphicView.DragInteraction += (s, e) =>
        {
            xMin = (int)e.Touches.First().X;
        };

        graphicView.EndInteraction += (s, e) =>
        {
            xMax = (int)e.Touches.First().X;
            isZoom = true;
            graphicView.Drawable = new GraphPlotter
            {
                Xmax = xMax,
                Xmin =xMin,
                StockPoints = stocksValues
            };
            graphicView.Invalidate();
        };


    }
}