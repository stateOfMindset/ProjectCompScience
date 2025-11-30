using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;

namespace ProjectCompScience.Models
{
    internal class Company
    {
        public string? CompanyName { get; set; }
        public string? CompanyID { get; set; }

       public Company(string name , string id)
        {
            CompanyName = name;
            CompanyID = id;
        }
    }
}
