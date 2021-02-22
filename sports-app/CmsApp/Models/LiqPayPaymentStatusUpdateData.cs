using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace CmsApp.Models
{
    public class LiqPayPaymentStatusUpdateData
    {
        [JsonProperty("acq_id")]
        public int AcquirerId { get; set; }

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("err_code")]
        public string ErrCode { get; set; }

        [JsonProperty("err_description")]
        public string ErrDescription { get; set; }

        [JsonProperty("info")]
        public string Info { get; set; }

        [JsonProperty("liqpay_order_id")]
        public string LiqPayOrderId { get; set; }

        [JsonProperty("order_id")]
        public Guid OrderId { get; set; }

        [JsonProperty("payment_id")]
        public int PaymentId { get; set; }

        [JsonProperty("paytype")]
        public string PayType { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

    }
}