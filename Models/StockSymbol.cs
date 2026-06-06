using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    public class StockSymbol
    {
        [JsonPropertyName("ticker")]
        public string Ticker { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        public Color ThemeColor { get; set; }
    }
}
