using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace PushServiceLib
{
    public static class NetFunc
    {
        public class RequestResult
        {
            public bool IsValid { get; set; }
            public string Data { get; set; }

            public RequestResult(bool isValid, string data)
            {
                this.IsValid = isValid;
                this.Data = data;
            }
        }

        public static RequestResult MakeRequest(string json, string servUrl, string method)
        {
            Uri url = new Uri(servUrl, UriKind.Absolute);
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.ContentType = "application/json";
            request.Method = method;

            using (var sw = new StreamWriter(request.GetRequestStream()))
            {
                sw.Write(json);
                sw.Flush();
            }

            using (var res = request.GetResponse() as HttpWebResponse)
            {
                if (res.StatusCode == HttpStatusCode.OK)
                {
                    var encoding = UTF8Encoding.UTF8;
                    using (var reader = new StreamReader(res.GetResponseStream(), encoding))
                    {
                        string resText = reader.ReadToEnd();
                        return new RequestResult(true, resText);
                    }
                }
                else
                {
                    string errMsg = String.Format("Server error (HTTP {0}: {1}).",
                        res.StatusCode,
                        res.StatusDescription);

                    return new RequestResult(false, errMsg);
                }
            }
        }

        public static string PostData(string url, string data)
        {
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                return wc.UploadString(url, "POST", data);
            }
        }
    }
}