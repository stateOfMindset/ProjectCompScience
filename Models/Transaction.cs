using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    internal class Transaction
    {
        public string Id { get; set; }
        public string Ticker { get; set; }
        public string CompanyName { get; set; }
        public string TransactionType { get; set; } // "BUY" or "SELL"
        public int Shares { get; set; }
        public double PricePerShare { get; set; }
        public double TotalAmount { get; set; }
        public DateTime Date { get; set; }
    }
}
