using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using AppModel;
using CmsApp.Helpers.Extensions;
using CmsApp.Models;
using CmsApp.Services;
using Newtonsoft.Json;
using Resources;

namespace CmsApp.Helpers
{
    public class LiqPayRequestResult
    {
        public string RedirectUrl { get; set; }
        public Guid OrderId { get; set; }
    }

    public static class LiqPayHelper
    {
        private class LiqPayInitParameters
        {
            public string Description { get; set; }

            public string PublicKey { get; set; }
            public string Email { get; set; }
            public decimal PriceTotal { get; set; }
            public Guid OrderId { get; set; }
            public string Language { get; set; }

            public bool IsSandbox { get; set; }
        }

        private class LiqPayGoodsItem
        {
            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("count")]
            public int Count { get; set; }

            [JsonProperty("unit")]
            public string Unit { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }

        private class LiqPayInvoiceResponse
        {
            public int Id { get; set; }
            public string Status { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public string Description { get; set; }

            [JsonProperty("order_id")]
            public Guid OrderId { get; set; }

            public string Token { get; set; }
            public string Href { get; set; }

            [JsonProperty("receiver_type")]
            public string ReceiverType { get; set; }

            [JsonProperty("receiver_value")]
            public string ReceiverValue { get; set; }

            public string Action { get; set; }
        }

        private static Dictionary<string, string> PrepareRequest(LiqPayInitParameters requestParams)
        {
            var publicKey = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticPublicKey"];
            var language = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticLanguage"];
            var currency = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticCurrency"];
            var isSandbox = string.Equals(ConfigurationManager.AppSettings["LiqPayUkraineGymnasticSandbox"], "true", StringComparison.CurrentCultureIgnoreCase);
            var sandboxEmail = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticSandboxEmail"];

            var result = new Dictionary<string, string>
            {
                ["public_key"] = publicKey,
                ["action"] = "invoice_send",
                //["paytypes"] = "invoice",
                ["email"] = isSandbox ? sandboxEmail : requestParams.Email,
                ["amount"] = requestParams.PriceTotal.ToString(CultureInfo.InvariantCulture),
                ["order_id"] = requestParams.OrderId.ToString(),
                ["currency"] = currency,
                ["version"] = "3",
                ["description"] = requestParams.Description,
                ["language"] = language,
                ["sandbox"] = isSandbox ? "1" : "0",
            };

            var urlHelper = new UrlHelper(HttpContext.Current?.Request.RequestContext);
            if (HttpContext.Current?.Request.Url != null)
            {
                var resultUrl = urlHelper.Action("LiqPayPaymentSuccessful", "Activity",
                    new {requestParams.OrderId}, // Landing for user after payment completed
                    HttpContext.Current.Request.Url.Scheme);
                var serverUrl = urlHelper.Action("LiqPayPaymentUpdate", "Activity",
                    null, // Our API to receive updates about payments status
                    HttpContext.Current.Request.Url.Scheme);

                var ngrokHost = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticNgrokTunnel"];
                if (!string.IsNullOrWhiteSpace(ngrokHost) &&
                    !string.IsNullOrWhiteSpace(resultUrl) &&
                    !string.IsNullOrWhiteSpace(serverUrl))
                {
                    resultUrl = resultUrl.ReplaceHostToNgrok(ngrokHost);
                    serverUrl = serverUrl.ReplaceHostToNgrok(ngrokHost);
                }

                result["result_url"] = resultUrl;

                result["server_url"] = serverUrl;
            }


            return result;
        }

        private static void AddCustomPrices(this List<LiqPayGoodsItem> goods, List<ActivityCustomPriceModel> customPrices)
        {
            var language = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticLanguage"];

            foreach (var customPrice in customPrices)
            {
                if (customPrice.TotalPrice - customPrice.Paid > 0)
                {
                    goods.Add(new LiqPayGoodsItem
                    {
                        Name = $"{(language == "he" ? customPrice.TitleHeb : language == "uk" ? customPrice.TitleUk : customPrice.TitleEng)}",
                        Count = customPrice.Quantity,
                        Amount = customPrice.Price,
                        Unit = Messages.Activity_LiqPay_Unit
                    });
                }
            }
        }

        private static string GetResourceString(string stringName)
        {
            var language = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticLanguage"];

            string cultureName;
            switch (language.ToLower())
            {
                case "he":
                    cultureName = "he-IL";
                    break;

                case "uk":
                    cultureName = "uk-UA";
                    break;

                default:
                    cultureName = "en-US";
                    break;
            }

            return Messages.ResourceManager.GetString(stringName, CultureInfo.CreateSpecificCulture(cultureName));
        }

        private static string PostDic(Dictionary<string, string> dic, string postRequestToUrl)
        {
            // Create Vars
            var requestString = new StringBuilder();
            foreach (var keyValuePair in dic)
            {
                requestString.AppendFormat("{0}={1}&", keyValuePair.Key,
                    HttpUtility.UrlEncode(keyValuePair.Value, Encoding.UTF8));
            }

            requestString.Remove(requestString.Length - 1, 1); // Remove the &

            // Post Information
            var request = WebRequest.Create(postRequestToUrl);
            request.Method = "POST";
            var byteArray = Encoding.UTF8.GetBytes(requestString.ToString());
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using (var writeStream = request.GetRequestStream())
            {
                writeStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        private static string PostDicRedirect(Dictionary<string, string> dic, string postRequestToUrl)
        {
            // Create Vars
            var requestString = new StringBuilder();
            foreach (var keyValuePair in dic)
            {
                requestString.AppendFormat("{0}={1}&", keyValuePair.Key,
                    HttpUtility.UrlEncode(keyValuePair.Value, Encoding.UTF8));
            }

            requestString.Remove(requestString.Length - 1, 1); // Remove the &

            // Post Information
            var request = (HttpWebRequest)WebRequest.Create(postRequestToUrl);
            request.AllowAutoRedirect = false;
            request.Method = "POST";
            var byteArray = Encoding.UTF8.GetBytes(requestString.ToString());
            //request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = byteArray.Length;
            using (var writeStream = request.GetRequestStream())
            {
                writeStream.Write(byteArray, 0, byteArray.Length);
            }

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
                {
                    return response.Headers["Location"];
                }

                return string.Empty;
            }
        }

        public static string GetSignature(string jsonBase64Data)
        {
            var privateKey = ConfigurationManager.AppSettings["LiqPayUkraineGymnasticPrivateKey"];

            var sha1Signature = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(privateKey + jsonBase64Data + privateKey));

            return Convert.ToBase64String(sha1Signature);
        }

        private static Dictionary<string, string> GenerateData(Dictionary<string, string> requestParams)
        {
            var result = new Dictionary<string, string>();

            var paramsJson = JsonConvert.SerializeObject(requestParams);
            var data = Convert.ToBase64String(Encoding.UTF8.GetBytes(paramsJson));

            result.Add("data", data);

            result.Add("signature", GetSignature(data));

            return result;
        }

        private static LiqPayRequestResult Execute(this Dictionary<string, string> vars, Activity activity, User user, Guid orderId)
        {
            var data = GenerateData(vars);

            var originalResponse = PostDic(data, "https://www.liqpay.ua/api/request");
            //var response = PostDicRedirect(data, "https://www.liqpay.ua/api/checkout");
            var parsedResponse = JsonConvert.DeserializeObject<LiqPayInvoiceResponse>(originalResponse);

            if (parsedResponse.Status != "error" && parsedResponse.Status != "failure")
            {
                return new LiqPayRequestResult
                {
                    RedirectUrl = parsedResponse.Href,
                    OrderId = parsedResponse.OrderId
                };
            }
            else
            {
                try
                {
                    new EmailService().SendAsync("info@loglig.com",
                        $"Request to LiqPay failed. ({activity.GetFormType()}) " +
                        $"Activity: {activity.Name}(id {activity.ActivityId}), " +
                        $"User: {user.FullName}(id: {user.UserId}, ident: {user.IdentNum}), " +
                        $"LiqPay request: {JsonConvert.SerializeObject(vars)}");
                }
                catch (Exception)
                {
                    // ¯\_(ツ)_/¯
                    return null;
                }
            }

            return null;
        }

        public static LiqPayRequestResult UnionClub_UaGymnastics(Activity activity, User user,
            List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal)
        {
            var orderId = Guid.NewGuid();

            var vars = PrepareRequest(new LiqPayInitParameters
            {
                Email = user.Email,
                Description = activity.Name,
                PriceTotal = priceTotal,
                OrderId = orderId
            });

            var goods = new List<LiqPayGoodsItem>();

            goods.AddCustomPrices(customPrices);

            vars["goods"] = JsonConvert.SerializeObject(goods);

            return vars.Execute(activity, user, orderId);
        }

        public static LiqPayRequestResult UnionPlayer_UaGymnastic(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguePlayerPrice playerPrices)
        {
            var orderId = Guid.NewGuid();

            var vars = PrepareRequest(new LiqPayInitParameters
            {
                Email = user.Email,
                Description = activity.Name,
                PriceTotal = priceTotal,
                OrderId = orderId
            });

            var goods = new List<LiqPayGoodsItem>();

            if (activity.RegistrationPrice && playerPrices.RegistrationPrice > 0)
            {
                goods.Add(new LiqPayGoodsItem
                {
                    Name = $"{activity.Name} ({GetResourceString(nameof(Messages.Team))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})",
                    Count = 1,
                    Amount = playerPrices.RegistrationPrice,
                    Unit = Messages.Activity_LiqPay_Unit
                });
            }

            if (activity.InsurancePrice && playerPrices.InsurancePrice > 0)
            {
                goods.Add(new LiqPayGoodsItem
                {
                    Name = $"{GetResourceString(nameof(Messages.Insurance))} ({GetResourceString(nameof(Messages.Team))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})",
                    Count = 1,
                    Amount = playerPrices.InsurancePrice,
                    Unit = Messages.Activity_LiqPay_Unit
                });
            }

            goods.AddCustomPrices(customPrices);

            vars["goods"] = JsonConvert.SerializeObject(goods);

            return vars.Execute(activity, user, orderId);
        }

        public static LiqPayRequestResult UnionPlayerToClub_UaGymnastic(Activity activity, User user,
            List<Team> teams, List<ActivityCustomPriceModel> customPrices, decimal priceTotal,
            UnionClubPlayerPrice prices, bool isCompetitiveMember)
        {
            var orderId = Guid.NewGuid();

            var vars = PrepareRequest(new LiqPayInitParameters
            {
                Email = user.Email,
                Description = activity.Name,
                PriceTotal = priceTotal,
                OrderId = orderId
            });

            var goods = new List<LiqPayGoodsItem>();

            var regPrice = isCompetitiveMember ? prices.CompetingRegistrationPrice : prices.RegularRegistrationPrice;

            foreach (var team in teams)
            {
                if (regPrice > 0)
                {
                    goods.Add(new LiqPayGoodsItem
                    {
                        Name = $"{activity.Name} ({GetResourceString(nameof(Messages.Team))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})",
                        Count = 1,
                        Amount = regPrice,
                        Unit = Messages.Activity_LiqPay_Unit
                    });
                }

                if (prices.InsurancePrice > 0)
                {
                    goods.Add(new LiqPayGoodsItem
                    {
                        Name = $"{GetResourceString(nameof(Messages.Insurance))} ({GetResourceString(nameof(Messages.Team))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})",
                        Count = 1,
                        Amount = prices.InsurancePrice,
                        Unit = Messages.Activity_LiqPay_Unit
                    });
                }
            }

            goods.AddCustomPrices(customPrices);

            vars["goods"] = JsonConvert.SerializeObject(goods);

            return vars.Execute(activity, user, orderId);
        }
    }
}