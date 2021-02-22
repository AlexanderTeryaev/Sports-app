using System;
using AppModel;

namespace CmsApp.Models
{
    public class ActivityFormSuccessResultModel
    {
        public Activity Activity { get; set; }
        public Guid? PaymentIdentifier { get; set; }
        public bool IsLiqPay { get; set; }
    }
}