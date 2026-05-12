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
        public List<StockGraphPoint> PredictionData { get; set; } = new List<StockGraphPoint>();
        public List<PointMine> Points { get; set; } = new();

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

            if (StockPoints == null || !StockPoints.Any()) return;

            // 1. CREATE A MASTER LIST FOR SCALING
            // We need the grid to be big enough to fit both history AND the future prediction
            var allPoints = new List<StockGraphPoint>(StockPoints);
            if (PredictionData != null && PredictionData.Any())
            {
                allPoints.AddRange(PredictionData);
            }

            xmin = xmin + _padding;
            Xmax = Xmax - _padding;

            // Calculate min/max prices using ALL points
            float minP = allPoints.Min(p => float.Parse(p.Low ?? p.Open));
            float maxP = allPoints.Max(p => float.Parse(p.High ?? p.Open));
            prettyMax = MathF.Ceiling(maxP / 9) * 10;
            prettyMin = MathF.Ceiling(minP - minP / 10);

            // Calculate absolute start and end dates
            long globalMinX = allPoints.Min(p => p.Timestamp.Ticks / 86400);
            long globalMaxX = allPoints.Max(p => p.Timestamp.Ticks / 86400);

            // Pass the master list to labels so future dates render on the X-Axis
            DrawLabels(canvas, dirtyRect, minP, prettyMax, allPoints);

            // 2. DRAW HISTORY (Solid Lime)
            canvas.StrokeColor = Microsoft.Maui.Graphics.Color.FromArgb("#FF2ECC71");
            canvas.StrokeDashPattern = null;
            drawLineSeries(canvas, dirtyRect, StockPoints, globalMinX, globalMaxX, p => float.Parse(p.Open));

            // 3. DRAW PREDICTION (Dotted Purple)
            if (PredictionData != null && PredictionData.Any())
            {
                canvas.StrokeColor = Colors.MediumPurple;
                canvas.StrokeDashPattern = new float[] { 5, 5 }; // The magic dotted line array!

                // To make the purple line seamlessly connect to the green line, 
                // we prepend the very last history point to the prediction drawing list.
                var connectionList = new List<StockGraphPoint> { StockPoints.Last() };
                connectionList.AddRange(PredictionData);

                drawLineSeries(canvas, dirtyRect, connectionList, globalMinX, globalMaxX, p => float.Parse(p.Open));
            }
        }

        // Updated to accept specific data to draw, and the global X bounds
        public void drawLineSeries(ICanvas canvas, RectF dirtyRect, List<StockGraphPoint> dataToDraw, long globalMinX, long globalMaxX, Func<StockGraphPoint, float> priceSelector)
        {
            List<PointMine> Points = new List<PointMine>();

            foreach (var p in dataToDraw)
            {
                long px = p.Timestamp.Ticks / 86400; //86400 - seconds per day
                float py = priceSelector(p);

                Points.Add(new PointMine { x = px, y = py });
            }

            if (Points == null || Points.Count < 2) return;

            canvas.StrokeSize = 2;

            // XRange is now based on the GLOBAL timeline, not just this specific line segment
            long XRange = globalMaxX - globalMinX;
            float YRange = prettyMax - prettyMin;

            if (XRange == 0) XRange = 1;
            if (YRange == 0) YRange = 0.00001f;

            float usableWidth = dirtyRect.Width - (2 * _padding);
            float usableHeight = dirtyRect.Height - (2 * _padding);

            List<PointF> screenPoints = new();

            foreach (var p in Points)
            {
                // Calculate percentage based on the GLOBAL minX
                float xPercent = ((float)(p.x - globalMinX)) / XRange;
                float yPercent = (prettyMax - p.y) / YRange;

                float x = _padding + (xPercent * usableWidth);
                float y = _padding + (yPercent * usableHeight);
                screenPoints.Add(new PointF(x, y));
            }

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

        // Updated to accept the master list of points so future dates are calculated
        private void DrawLabels(ICanvas canvas, RectF dirtyRect, float minPrice, float maxPrice, List<StockGraphPoint> allPoints)
        {
            if (allPoints == null || !allPoints.Any()) return;

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
                    y - 10,
                    _padding - 5,
                    20,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Center);
            }

            int dateDivisions = 5;
            for (int i = 0; i <= dateDivisions; i++)
            {
                // uses allPoints to grab the dates
                int index = i * (allPoints.Count - 1) / dateDivisions;
                var point = allPoints[index];

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
            if (price >= 100000) return $"${(price / 1000):F0}K";
            if (price >= 1000) return $"${(price / 1000):F1}K";
            return $"${price:F0}";
        }
    }

    class PointMine
    {
        public long x { get; set; }
        public float y { get; set; }
    }
}