using Microsoft.Maui.Graphics;
using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectCompScience.Components
{
    internal class PortfolioPieChart : IDrawable
    {
        public List<PieSlice> Slices { get; set; } = new List<PieSlice>();

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if (Slices == null || !Slices.Any()) return;

            double totalValue = Slices.Sum(s => s.InvestedValue);
            if (totalValue <= 0) return;

            float padding = 10;
            float size = Math.Min(dirtyRect.Width, dirtyRect.Height) - (padding * 2);
            float radius = size / 2;

            float centerX = dirtyRect.Center.X;
            float centerY = dirtyRect.Center.Y;

            float currentAngle = -90f;

            foreach (var slice in Slices)
            {
                float sweepAngle = (float)((slice.Percentage / 100) * 360);
                if (sweepAngle <= 0) continue;

                PathF path = new PathF();
                path.MoveTo(centerX, centerY);

                int points = (int)Math.Max(10, sweepAngle);
                for (int i = 0; i <= points; i++)
                {
                    float angle = currentAngle + (sweepAngle * i / points);
                    float radians = (float)(angle * Math.PI / 180.0);

                    float x = centerX + radius * (float)Math.Cos(radians);
                    float y = centerY + radius * (float)Math.Sin(radians);

                    path.LineTo(x, y);
                }

                path.Close();

                canvas.FillColor = slice.SliceColor;
                canvas.FillPath(path);

                currentAngle += sweepAngle;
            }
        }
    }
}