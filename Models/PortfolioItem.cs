using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    internal class PortfolioItem
    {
        public string Ticker { get; set; }
        public string CompanyName { get; set; }
        public double TotalShares { get; set; }
        public double AveragePurchasePrice { get; set; }
    }
}
