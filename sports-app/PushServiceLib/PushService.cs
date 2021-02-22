using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PushServiceLib
{
    public class PushService
    {
        readonly string servProdUrl = "pushservices.yit.co.il";
        readonly string servTestUrl = "ipadcms.test.yit.co.il";

        readonly string tokensServProdUrl = "ipadservices.yit.co.il";
        readonly string tokensServTestUrl = "ipadservicestest.yit.co.il";

        readonly string bundleId = "com.yit.loglig.gcm,com.yit.loglig.apns";
        readonly string userName = "logligyit@push";
        readonly string password = "psh7#12dc";

        string pushUrl = "http://{0}/PushServiceExternal/ServiceIphoneIpadMessages/notifications/";
        string catAddUrl = "http://{0}/PushServiceExternal/ServiceIphoneIpadMessages/channels/";
        string catDelUrl = "http://{0}/PushServiceExternal/ServiceIphoneIpadMessages/channelDelete/";
        string sendTokensUrl = "http://{0}/PushServiceExternal/ServiceIphoneIpadMessages/notificationsTokensIDs/";

        string tokenIdUrl = "http://{0}/WcfRestServiceIphoneIpadMessages/ServiceIphoneIpadMessages/tokens/";

        public PushService(bool isTest)
        {
            string servUrl = this.servProdUrl;
            string tokenUrl = this.tokensServProdUrl;
            if (isTest)
            {
                servUrl = this.servTestUrl;
                tokenUrl = this.tokensServTestUrl;
            }

            this.pushUrl = string.Format(this.pushUrl, servUrl);
            this.catAddUrl = string.Format(this.catAddUrl, servUrl);
            this.catDelUrl = string.Format(this.catDelUrl, servUrl);

            this.sendTokensUrl = string.Format(this.sendTokensUrl, servUrl);

            this.tokenIdUrl = string.Format(this.tokenIdUrl, tokenUrl);
        }

        public class PushItem
        {
            public int Badge { get; set; }
            public string BundleId { get; set; }
            public string Channels { get; set; }
            public bool ContentAvailable { get; set; }
            public string ExtraData { get; set; }
            public bool IsSandBox { get; set; }
            public string Link { get; set; }
            public string Passwd { get; set; }
            public string SendDate { get; set; }
            public string Sound { get; set; }
            public string Text { get; set; }
            public string User { get; set; }
        }

        public class PushBatchItem
        {
            public int Badge { get; set; }
            public string BundleId { get; set; }
            public int[] Ids { get; set; }
            public bool ContentAvailable { get; set; }
            public string ExtraData { get; set; }
            public bool IsSandBox { get; set; }
            public string Link { get; set; }
            public string SendDate { get; set; }
            public string Sound { get; set; }
            public string Text { get; set; }
            public string TimeStamp { get; set; }
            public string User { get; set; }
            public string Passwd { get; set; }
        }

        public class PushCatItem
        {
            public string bundleid { get; set; }
            public string channels { get; set; }
            public string passwd { get; set; }
            public string user { get; set; }
        }

        public bool AddPush(string title, string sendDate, string channels)
        {
            var item = new PushItem
            {
                Badge = 0,
                BundleId = this.bundleId,
                Channels = channels, // "1,15,0"
                ContentAvailable = false,
                ExtraData = "15255",
                IsSandBox = false,
                Link = "",
                SendDate = sendDate, // "20141225120000"
                Sound = "",
                Text = title,
                Passwd = this.password,
                User = this.userName
            };

            var sett = new JsonSerializerSettings();
            sett.ContractResolver = new LowercaseContractResolver();

            string json = JsonConvert.SerializeObject(item, Formatting.Indented, sett);

            var res = NetFunc.MakeRequest(json, this.pushUrl, "POST");
            return res.IsValid;
        }

        public bool AddPushBatch(string text, string sendDate, int[] ids, string extraData, string link)
        {
            var item = new PushBatchItem
            {
                Badge = 0,
                BundleId = this.bundleId,
                Ids = ids, // "1,15,0"
                ContentAvailable = false,
                ExtraData = extraData,
                IsSandBox = false,
                Link = link,
                SendDate = sendDate, // "20141225120000"
                Sound = "",
                Text = text,
                TimeStamp = "",
                Passwd = this.password,
                User = this.userName
            };

            var sett = new JsonSerializerSettings();
            sett.ContractResolver = new LowercaseContractResolver();

            string json = JsonConvert.SerializeObject(item, Formatting.Indented, sett);

            var res = NetFunc.MakeRequest(json, this.sendTokensUrl, "POST");
            return res.IsValid;
        }

        public bool AddCategories(string channels)
        {
            var item = new PushCatItem
            {
                bundleid = this.bundleId,
                channels = channels,
                passwd = this.password,
                user = this.userName
            };

            string json = JsonConvert.SerializeObject(item);

            var res = NetFunc.MakeRequest(json, this.catAddUrl, "POST");
            return res.IsValid;
        }

        public bool DelCategories(string channels)
        {
            var item = new PushCatItem
            {
                bundleid = this.bundleId,
                channels = channels,
                passwd = this.password,
                user = this.userName
            };

            string json = JsonConvert.SerializeObject(item);

            var res = NetFunc.MakeRequest(json, this.catDelUrl, "DELETE");
            return res.IsValid;
        }

        public string GetTokenId(bool isIOS, string token, string channels, string deviceId, int isSandBox)
        {
            string[] boudelArr = this.bundleId.Split(',');

            string boundel = isIOS ? boudelArr[1] : boudelArr[0];

            string servUrl = string.Format(this.tokenIdUrl + "?token={0}&bundleid={1}&channels={2}&deviceid={3}&issandbox={4}",
                token,
                boundel,
                channels,
                deviceId,
                isSandBox);

            return NetFunc.PostData(servUrl, "");
        }
    }
}

public class LowercaseContractResolver : DefaultContractResolver
{
    protected override string ResolvePropertyName(string propertyName)
    {
        return propertyName.ToLower();
    }
}