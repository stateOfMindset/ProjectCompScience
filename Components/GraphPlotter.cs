using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Graphics;
using ProjectCompScience.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public double Xmin {
            set { xmin = value; }
            get { return xmin; }
        }



        public double Xmax { get; set; }
        
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            List<PointMine> points = new List<PointMine>();

            foreach (var p in StockPoints) {
                long px = p.x.Ticks / 86400; 
                float py = p.y;
                PointMine newP = new PointMine{
                    x = px ,
                    y = py
                };

                points.Add(newP);       
            }

            if (points == null || points.Count < 2)
                return;

            canvas.StrokeColor = Colors.CadetBlue;
            canvas.StrokeSize = 3;

            // 1. Find bounds
            long minX = points.Min(l => l.x);
            long maxX = points.Max(l => l.x);
            float minY = points.Min(l => l.y);
            float maxY = points.Max(l => l.y);

            long XRange = maxX - minX;
            float YRange = maxY - minY;

            // Avoid division by zero
            if (XRange == 0) XRange = 1;
            if (YRange == 0) YRange = 0.00001f;

            // 2. Convert each GPS point to canvas coordinates
            List<PointF> screenPoints = new();

            foreach (var p in points)
            {
                float x = ((float)(p.x - minX)) / XRange * dirtyRect.Width;
                float y = (float)((maxY - p.y) / YRange * dirtyRect.Height);
                if (x < Xmin || x>Xmax)
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
    }

    class PointMine {
        public long x { get; set; }
        public float y { get; set; }
    }
}
