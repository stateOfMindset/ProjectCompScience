using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    internal class api_response_get
    {
        [JsonPropertyName("Meta Data")]
        public Dictionary<string, string> MetaData { get; set; }

        [JsonPropertyName("Time Series (Daily)")]
        public Dictionary<string, StockGraphPoint> TimeSeries { get; set; }
    }
}
