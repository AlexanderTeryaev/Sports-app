using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Utils
{
    public class AverageAgeException : Exception
    {
        public int AverageAgeValue { get; set; }
    }
}
