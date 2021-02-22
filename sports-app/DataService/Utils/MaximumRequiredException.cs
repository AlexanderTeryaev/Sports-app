using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Utils
{
    public class MaximumRequiredException : Exception
    {
        public int NumOfRegistrationsLeft { get; set; }
    }
}
