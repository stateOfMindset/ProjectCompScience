using Microsoft.Maui;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointF = Microsoft.Maui.Graphics.PointF;


namespace ProjectCompScience.Components
{
    class GraphPlotter : IDrawable
    {

        public List<StockGraphPoint> StockPoints { get; set; } = new List<StockGraphPoint>();

        private double xmin;
        private float prettyMin;
        private float prettyMax;
        private int _padding = 30;

        public double Xmin
        {
            set { xmin = value; }
            get { return xmin; }
        }



        public double Xmax { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            DrawGrid(canvas, dirtyRect);

            if (StockPoints == null || StockPoints.Count() == 0) return;

            xmin = xmin + _padding;
            Xmax = Xmax - _padding;
            float minP = StockPoints.Min(p => float.Parse(p.Low));
            float maxP = StockPoints.Max(p => float.Parse(p.High));
            prettyMax = MathF.Ceiling(maxP / 9) * 10;
            prettyMin = MathF.Ceiling(minP - minP / 10);
            DrawLabels(canvas, dirtyRect, minP, prettyMax);

            canvas.StrokeColor = Microsoft.Maui.Graphics.Color.FromArgb("#FF2ECC71");
            drawLineSeries(canvas, dirtyRect, p => float.Parse(p.Open));

            //canvas.StrokeColor = Microsoft.Maui.Graphics.Color.FromArgb("#FF3498DB");
            //drawLineSeries(canvas, dirtyRect, p => float.Parse(p.Close));

            //canvas.StrokeColor = Microsoft.Maui.Graphics.Color.FromArgb("#FFF1C40F");
            //drawLineSeries(canvas, dirtyRect, p => float.Parse(p.High));

            //canvas.StrokeColor = Microsoft.Maui.Graphics.Color.FromArgb("#FFE74C3C");
            //drawLineSeries(canvas, dirtyRect, p => float.Parse(p.Low));



        }

        public void drawLineSeries(ICanvas canvas, RectF dirtyRect, Func<StockGraphPoint, float> priceSelector)
        {
            List<PointMine> points = new List<PointMine>();

            foreach (var p in StockPoints)
            {
                long px = p.Timestamp.Ticks / 86400; //86400 - seconds per day
                float py = priceSelector(p); //open or close so we can do 2 graphs.

                PointMine newP = new PointMine
                {
                    x = px,
                    y = py
                };

                points.Add(newP);
            }

            if (points == null || points.Count < 2)
                return;


            canvas.StrokeSize = 2;

            // 1. Find bounds
            long minX = points.Min(l => l.x);
            long maxX = points.Max(l => l.x);

            long XRange = maxX - minX;
            float YRange = prettyMax - prettyMin;

            // Avoid division by zero
            if (XRange == 0) XRange = 1;
            if (YRange == 0) YRange = 0.00001f;

            // 2. Convert each GPS point to canvas coordinates
            List<PointF> screenPoints = new();

            foreach (var p in points)
            {
                float x = ((float)(p.x - minX)) / XRange * dirtyRect.Width - _padding;
                float y = (float)((prettyMax - p.y) / YRange * dirtyRect.Height) - _padding;
                if (x < Xmin || x > Xmax)
                    continue;
                screenPoints.Add(new PointF(x, y)); // TO DO : FIX TS
            }



            // 3. Draw polyline
            for (int i = 0; i < screenPoints.Count - 1; i++)
            {
                var p1 = screenPoints[i];
                var p2 = screenPoints[i + 1];

                canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y);
            }
        }

        private void DrawGrid(ICanvas canvas, RectF dirtyRect)
        {
            float usableWidth = dirtyRect.Width - (2 * _padding);
            float usableHeight = dirtyRect.Height - (2 * _padding);

            canvas.StrokeColor = Colors.DimGray; 
            canvas.StrokeSize = 0.5f;
            canvas.StrokeDashPattern = new float[] { 2, 2 };

            int gridCount = 10;

            for (int i = 0; i <= gridCount; i++)
            {
                float y = _padding + (i * (usableHeight / gridCount));

                canvas.DrawLine(_padding, y, dirtyRect.Width - _padding, y);
            }

            for (int i = 0; i <= gridCount; i++)
            {
                float x = _padding + (i * (usableWidth / gridCount));

                canvas.DrawLine(x, _padding, x, dirtyRect.Height - _padding);
            }

            canvas.StrokeDashPattern = null;
            canvas.StrokeColor = Colors.WhiteSmoke;
            canvas.StrokeSize = 1.5f;

            canvas.DrawLine(_padding, _padding, _padding, dirtyRect.Height - _padding);
            canvas.DrawLine(_padding, dirtyRect.Height - _padding, dirtyRect.Width - _padding, dirtyRect.Height - _padding);
        }

        private void DrawLabels(ICanvas canvas, RectF dirtyRect, float minPrice, float maxPrice)
        {
            if (StockPoints == null || !StockPoints.Any()) return;

            float usableWidth = dirtyRect.Width - (2 * _padding);
            float usableHeight = dirtyRect.Height - (2 * _padding);

            canvas.FontColor = Colors.WhiteSmoke; 
            canvas.FontSize = 10;

            int priceDivisions = 5;
            for (int i = 0; i <= priceDivisions; i++)
            {
                float priceVal = prettyMin + (i * (prettyMax - prettyMin) / priceDivisions);

                float y = _padding + usableHeight - (i * (usableHeight / priceDivisions));
                canvas.DrawString(
                    $"{FormatPrice(priceVal)}",
                    0,             
                    y -10,        
                    _padding - 5 ,  
                    20,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Center);
            }

            int dateDivisions = 5;
            for (int i = 0; i <= dateDivisions; i++)
            {
                int index = i * (StockPoints.Count - 1) / dateDivisions;
                var point = StockPoints[index];

                float x = _padding + (i * (usableWidth / dateDivisions));

                canvas.DrawString(
                    point.Timestamp.ToString("dd/MM"),
                    x - 25,                     
                    dirtyRect.Height - _padding + 5, 
                    50,
                    20,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Top);
            }
        }

        private string FormatPrice(float price)
        {
            if (price >= 100000) // $100,000+
                return $"${(price / 1000):F0}K"; //F.0 , shows 0 numbers after the decimel [$300,021 = $300k]

            if (price >= 1000) // $1,000 - $99,999
                return $"${(price / 1000):F1}K"; //F.1 , show 1 number after the decimal [$3542 =  $3.5K]

            return $"${price:F0}"; // Standard under $1,000
        }
    }
    class PointMine {
        public long x { get; set; }
        public float y { get; set; }
    }
}
