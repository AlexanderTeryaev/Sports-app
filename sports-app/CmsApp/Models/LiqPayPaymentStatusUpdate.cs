using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CmsApp.Models
{
    public class LiqPayPaymentStatusUpdate
    {
        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}