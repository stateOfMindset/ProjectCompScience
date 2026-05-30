using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    internal class PieSlice
    {
        public string Ticker { get; set; }
        public double InvestedValue { get; set; } // Shares * Price
        public Color SliceColor { get; set; }

        public double Percentage { get; set; }
    }
}
