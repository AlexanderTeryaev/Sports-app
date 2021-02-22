using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ActivityCustomPriceModel
    {
        public string PropertyName { get; set; }
        public string TitleEng { get; set; }
        public string TitleHeb { get; set; }
        public string TitleUk { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Paid { get; set; }
        public string CardComProductId { get; set; }
    }
}