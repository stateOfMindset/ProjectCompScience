using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    internal class StockShare
    {
        public string? Id { get; set; }
        public Company? Company { get; set; }

        public string? classType { get; set; } // Class type A or B or C | A - you have 1 vote per share | B - 10 votes per share | C - 0 votes per share
        
        public int? price { get; set; }

        public int? quantity { get; set; }
    }
}
