using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    internal class Transaction
    {
        public int pricePerUnit;
        public int totalPrice;
        public int totalAmount;
        public string? Id;
        public DateTime DateTime;
        public StockShare? StockShare;

    }
}
