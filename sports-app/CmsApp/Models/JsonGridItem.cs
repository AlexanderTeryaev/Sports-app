using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class JsonGridItem
    {
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<string[]> data { get; set; }
    }
}