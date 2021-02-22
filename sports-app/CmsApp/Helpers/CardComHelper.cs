using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
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
    class CardComInitParameters
    {
        public int MinPayments { get; set; }
        public int MaxPayments { get; set; }

        public string TerminalNumber { get; set; }
        public string UserName { get; set; }
        public string Language { get; set; }

        public User User { get; set; }
        public decimal PriceTotal { get; set; }
    }
    public class CardComRequestResult
    {
        public string RedirectUrl { get; set; }
        public Guid Lpc { get; set; }
    }

    public class CardComIndicatorResult
    {
        public string OriginalResponse { get; set; }

        public bool Success { get; set; }

        public int? NumOfPayments { get; set; }

        public int? InvoiceNumber { get; set; }
    }

    public class CardComIndicatorInfo
    {
        public int? ResponseCode { get; set; }
        public int? OperationResponse { get; set; }
        public int? NumOfPayments { get; set; }
        public int? InvoiceNumber { get; set; }
    }

    public static class CardComHelper
    {
        private static string PostDic(Dictionary<string, string> dic, string postRequestToUrl)
        {
            // Create Vars
            var requstString = new StringBuilder();
            foreach (var keyValuePair in dic)
            {
                requstString.AppendFormat("{0}={1}&", keyValuePair.Key,
                    HttpUtility.UrlEncode(keyValuePair.Value, Encoding.UTF8));
            }

            requstString.Remove(requstString.Length - 1, 1); // Remove the &

            // Post Information
            var request = WebRequest.Create(postRequestToUrl);
            request.Method = "POST";
            var byteArray = Encoding.UTF8.GetBytes(requstString.ToString());
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

        private static string GetDic(string requestUrl)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 |
                                                   SecurityProtocolType.Tls11 |
                                                   SecurityProtocolType.Tls |
                                                   SecurityProtocolType.Ssl3;

            var request = (HttpWebRequest) WebRequest.Create(requestUrl);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (var response = (HttpWebResponse) request.GetResponse())
            using (var stream = response.GetResponseStream())
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static void AddCustomPrices(this Dictionary<string, string> vars, List<ActivityCustomPriceModel> customPrices, int invoiceCount, bool hebrew)
        {
            foreach (var customPrice in customPrices)
            {
                if (customPrice.TotalPrice - customPrice.Paid > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(hebrew ? customPrice.TitleHeb : customPrice.TitleEng)}";
                    vars[$"InvoiceLines{invoiceCount}.Price"] =
                        customPrice.Price.ToString(CultureInfo.CurrentCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = customPrice.Quantity.ToString();
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(customPrice.CardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = customPrice.CardComProductId;
                    }
                    invoiceCount++;
                }
            }
        }

        public static CardComIndicatorResult GetIndicatorResult(string apiUserName, int terminalNumber, Guid cardComLpc)
        {
            var result = new CardComIndicatorResult();

            if (string.IsNullOrWhiteSpace(apiUserName))
            {
                return result;
            }

            var url =
                $"https://secure.cardcom.solutions/Interface/BillGoldGetLowProfileIndicator.aspx?username={apiUserName}&terminalnumber={terminalNumber}&lowprofilecode={cardComLpc}";
            var originalResponse = string.Empty;
            try
            {
                originalResponse = GetDic(url);
            }
            catch (Exception)
            {
                new EmailService().SendAsync("info@loglig.com",
                    $"Request to cardcom failed. ({url}) " +
                    $"CardCom response: {originalResponse}");
                return result;
            }

            if (string.IsNullOrWhiteSpace(originalResponse))
            {
                return result;
            }

            result.OriginalResponse = originalResponse;

            var nameValueData = HttpUtility.ParseQueryString(originalResponse);
            if (nameValueData.AllKeys.Any() != true)
            {
                return result;
            }

            var paramsAsJson =
                JsonConvert.SerializeObject(nameValueData.AllKeys.ToDictionary(x => x, x => nameValueData[x]));

            var indicatorInfo = JsonConvert.DeserializeObject<CardComIndicatorInfo>(paramsAsJson);

            result.Success = indicatorInfo.OperationResponse == 0;
            result.NumOfPayments = indicatorInfo.NumOfPayments;
            result.InvoiceNumber = indicatorInfo.InvoiceNumber;

            return result;
        }

        public static string GetApiUserNameByActivity(Activity activity)
        {
            if (activity == null)
            {
                return string.Empty;
            }

            var activityUnionId = activity.UnionId ?? activity.Club?.UnionId ?? 0;
            var activityClubId = activity.ClubId ?? 0;

            if (activityUnionId > 0)
            {
                switch (activityUnionId)
                {
                    case 15: //catchball union
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComUsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComUsername"];
#endif

                    case 66: //catchball 66 union
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComCatchball66UsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComCatchball66Username"];
#endif

                    case 54: //Rugby union
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComRugbyUsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComRugbyUsername"];
#endif

                    case 41: //wave surfing union
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComWaveSurfingUsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComWaveSurfingUsername"];
#endif

                    case 38: //tennis union
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComTennisUsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComTennisUsername"];
#endif

                    case 32: //waterpolo union
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComWaterpoloUsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComWaterpoloUsername"];
#endif

                    case 59: //climbing union 59
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComClimbingUnion59UsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComClimbingUnion59Username"];
#endif
                }
            }

            if (activityClubId > 0)
            {
                switch (activityClubId)
                {
                    case 1194: //Basketball union club
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComBasketballUsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComBasketballUsername"];
#endif

                    case 2541: //Soccer club
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComSoccerUsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComSoccerUsername"];
#endif

                    case 3610: //Gymnastic 3610 club
#if DEBUG
                        return ConfigurationManager.AppSettings["CardComGymnasticClub3610UsernameDebug"];
#else
                        return ConfigurationManager.AppSettings["CardComGymnasticClub3610Username"];
#endif
                }
            }

            return string.Empty;
        }

        private static CardComRequestResult Execute(this Dictionary<string, string> vars, Activity activity, User user)
        {
            var originalResponse = PostDic(vars, "https://secure.cardcom.co.il/Interface/LowProfile.aspx");
            var parseResponse = new NameValueCollection(HttpUtility.ParseQueryString(originalResponse));

            if (parseResponse["ResponseCode"] == "0")
            {
                var lpc = parseResponse["LowProfileCode"];

                Guid guid;
                if (Guid.TryParse(lpc, out guid))
                {
                    return new CardComRequestResult
                    {
                        RedirectUrl = parseResponse["url"],
                        Lpc = guid
                    };
                }
            }
            else
            {
                try
                {
                    new EmailService().SendAsync("info@loglig.com",
                        $"Request to cardcom failed. ({activity.GetFormType()}) " +
                        $"Activity: {activity.Name}(id {activity.ActivityId}), " +
                        $"User: {user.FullName}(id: {user.UserId}, ident: {user.IdentNum}), " +
                        $"CardCom response: {originalResponse}");
                }
                catch (Exception)
                {
                    // ¯\_(ツ)_/¯
                    return null;
                }
            }
            return null;
        }

        private static Dictionary<string, string> PrepareRequest(CardComInitParameters parameters)
        {
            var vars = new Dictionary<string, string>();

            var urlHelper = new UrlHelper(HttpContext.Current?.Request.RequestContext);
            if (HttpContext.Current?.Request.Url != null)
            {
                var indicatorUrl = urlHelper.Action("CardComIndicator", "Activity", new{}, HttpContext.Current.Request.Url.Scheme);

                var ngrokTunnel = ConfigurationManager.AppSettings["CardComIndicatorUrlNgrokTunnel"];
                if (!string.IsNullOrWhiteSpace(ngrokTunnel) &&
                    !string.IsNullOrWhiteSpace(indicatorUrl))
                {
                    indicatorUrl = indicatorUrl.ReplaceHostToNgrok(ngrokTunnel);
                }

                vars["IndicatorUrl"] = indicatorUrl;
            }

            if (parameters.MinPayments > 0)
            {
                vars["MinNumOfPayments"] = parameters.MinPayments.ToString();
            }

            if (parameters.MaxPayments > 0)
            {
                vars["MaxNumOfPayments"] = parameters.MaxPayments.ToString();
            }
            
            vars["DefaultNumOfPayments "] = "1";

            vars["TerminalNumber"] = parameters.TerminalNumber;
            vars["UserName"] = parameters.UserName;
            vars["APILevel"] = "10";
            vars["codepage"] = "65001"; // unicode
            vars["Operation"] = "1";
            vars["Language"] = parameters.Language;
            vars["CoinID"] =
                "1"; // billing coin , 1- NIS , 2- USD other , article :  http://kb.cardcom.co.il/article/AA-00247/0
            vars["ShowInvoiceHead"] = "true";
            //vars["ProductName"] = "Product name 111223";

            //vars["SuccessRedirectUrl"] = "https://secure.cardcom.co.il/DealWasSuccessful.aspx";

            if (HttpContext.Current?.Request.Url != null)
                vars["SuccessRedirectUrl"] = urlHelper.Action("PaymentSuccessful", "Activity", null,
                    HttpContext.Current.Request.Url.Scheme);
            if (HttpContext.Current?.Request.Url != null)
                vars["ErrorRedirectUrl"] = urlHelper.Action("PaymentFailure", "Activity", null,
                    HttpContext.Current.Request.Url.Scheme);

            //Invoice
            vars["InvoiceHead.CustName"] = $"{parameters.User.FullName} (ID: {parameters.User.IdentNum})";
            vars["InvoiceHead.SendByEmail"] = "true";
            vars["InvoiceHead.Language"] = parameters.Language;
#if DEBUG
            vars["InvoiceHead.Email"] = "info@loglig.com";
#else
            vars["InvoiceHead.Email"] = parameters.User.Email;
#endif

            vars["SumToBill"] = parameters.PriceTotal.ToString(CultureInfo.InvariantCulture);

            return vars;
        }

        public static CardComRequestResult UnionPlayerToClub_WaveSurfing(Activity activity, User user, List<Team> teams,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, UnionClubPlayerPrice prices, bool isCompetitiveMember)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguageDebug"];
#else
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminal"];
            var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsername"];
            var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = isCompetitiveMember ? prices.CompetingRegistrationPrice : prices.RegularRegistrationPrice;

            foreach (var team in teams)
            {
                if (regPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (isCompetitiveMember)
                    {
                        if (!string.IsNullOrWhiteSpace(prices.CompetingRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.CompetingRegistrationCardComProductId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(prices.RegularRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.RegularRegistrationCardComProductId;
                        }
                    }

                    invoiceCount++;
                }

                if (prices.InsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.InsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.InsuranceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (prices.TenicardPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Activity_Tenicard_Price), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Activity_Tenicard_Price), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.TenicardPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.TenicardCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.TenicardCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayerToClub_CatchBall(Activity activity, User user, List<Team> teams,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, UnionClubPlayerPrice prices, bool isCompetitiveMember)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComLanguageDebug"];
#else
            var terminalNumber = ConfigurationManager.AppSettings["CardComTerminal"];
            var userName = ConfigurationManager.AppSettings["CardComUsername"];
            var language = ConfigurationManager.AppSettings["CardComLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = isCompetitiveMember ? prices.CompetingRegistrationPrice : prices.RegularRegistrationPrice;

            foreach (var team in teams)
            {
                if (regPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (isCompetitiveMember)
                    {
                        if (!string.IsNullOrWhiteSpace(prices.CompetingRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.CompetingRegistrationCardComProductId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(prices.RegularRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.RegularRegistrationCardComProductId;
                        }
                    }

                    invoiceCount++;
                }

                if (prices.InsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.InsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.InsuranceCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayerToClub_Climbing59(Activity activity, User user, List<Team> teams,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, UnionClubPlayerPrice prices, bool isCompetitiveMember)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59TerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59UsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComClimbingUnion59LanguageDebug"];
#else
            var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59Terminal"];
            var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59Username"];
            var language = ConfigurationManager.AppSettings["CardComClimbingUnion59Language"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var activityForm = activity.ActivityForms.FirstOrDefault();
            var formDetails = activityForm?.ActivityFormsDetails?.ToList();
            var registrationPriceControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerRegistrationPrice", StringComparison.CurrentCultureIgnoreCase));
            var insurancePriceControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerInsurancePrice", StringComparison.CurrentCultureIgnoreCase));

            var culture = string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                ? CultureInfo.CreateSpecificCulture("he-IL")
                : CultureInfo.CreateSpecificCulture("en-US");

            var invoiceCount = 1;

            var regPrice = isCompetitiveMember ? prices.CompetingRegistrationPrice : prices.RegularRegistrationPrice;

            foreach (var team in teams)
            {
                if (regPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{activity.Name} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (isCompetitiveMember)
                    {
                        if (!string.IsNullOrWhiteSpace(prices.CompetingRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.CompetingRegistrationCardComProductId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(prices.RegularRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.RegularRegistrationCardComProductId;
                        }
                    }

                    invoiceCount++;
                }

                if (prices.InsurancePrice > 0)
                {
                    var priceTitle = (string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                                                  ? insurancePriceControl?.LabelTextHeb
                                                  : insurancePriceControl?.LabelTextEn)
                                              ?? Messages.ResourceManager.GetString(nameof(Messages.Insurance), culture);

                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{priceTitle} ({(Messages.ResourceManager.GetString(nameof(Messages.Team), culture))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.InsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.InsuranceCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayerToClub_Waterpolo(Activity activity, User user, List<Team> teams,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, UnionClubPlayerPrice prices, bool isCompetitiveMember)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguageDebug"];
#else
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminal"];
            var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsername"];
            var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = isCompetitiveMember ? prices.CompetingRegistrationPrice : prices.RegularRegistrationPrice;

            foreach (var team in teams)
            {
                if (regPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (isCompetitiveMember)
                    {
                        if (!string.IsNullOrWhiteSpace(prices.CompetingRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.CompetingRegistrationCardComProductId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(prices.RegularRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.RegularRegistrationCardComProductId;
                        }
                    }

                    invoiceCount++;
                }

                if (prices.InsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.InsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.InsuranceCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayerToClub_Rugby(Activity activity, User user, List<Team> teams,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, UnionClubPlayerPrice prices, bool isCompetitiveMember)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComRugbyUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComRugbyLanguageDebug"];
#else
            var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminal"];
            var userName = ConfigurationManager.AppSettings["CardComRugbyUsername"];
            var language = ConfigurationManager.AppSettings["CardComRugbyLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = isCompetitiveMember ? prices.CompetingRegistrationPrice : prices.RegularRegistrationPrice;

            foreach (var team in teams)
            {
                if (regPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (isCompetitiveMember)
                    {
                        if (!string.IsNullOrWhiteSpace(prices.CompetingRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.CompetingRegistrationCardComProductId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(prices.RegularRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.RegularRegistrationCardComProductId;
                        }
                    }

                    invoiceCount++;
                }

                if (prices.InsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.InsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.InsuranceCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayerToClub_Tennis(Activity activity, User user, List<Team> teams,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, UnionClubPlayerPrice prices, bool isCompetitiveMember)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComTennisUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComTennisLanguageDebug"];
#else
            var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminal"];
            var userName = ConfigurationManager.AppSettings["CardComTennisUsername"];
            var language = ConfigurationManager.AppSettings["CardComTennisLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = isCompetitiveMember ? prices.CompetingRegistrationPrice : prices.RegularRegistrationPrice;

            foreach (var team in teams)
            {
                if (regPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (isCompetitiveMember)
                    {
                        if (!string.IsNullOrWhiteSpace(prices.CompetingRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.CompetingRegistrationCardComProductId;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(prices.RegularRegistrationCardComProductId))
                        {
                            vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.RegularRegistrationCardComProductId;
                        }
                    }

                    invoiceCount++;
                }

                if (prices.InsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.InsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.InsuranceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (prices.TenicardPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Activity_Tenicard_Price), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Activity_Tenicard_Price), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.TenicardPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.TenicardCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.TenicardCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomTeam_Catchball(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComUsername"];
                var language = ConfigurationManager.AppSettings["CardComLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = leagueRegistrationPrice?.Price ?? 0;
            if (regPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }
        
        public static CardComRequestResult UnionCustomTeam_Catchball66(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComCatchball66TerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComCatchball66UsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComCatchball66LanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComCatchball66Terminal"];
                var userName = ConfigurationManager.AppSettings["CardComCatchball66Username"];
                var language = ConfigurationManager.AppSettings["CardComCatchball66Language"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = leagueRegistrationPrice?.Price ?? 0;
            if (regPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomTeam_Rugby(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComRugbyUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComRugbyLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComRugbyUsername"];
                var language = ConfigurationManager.AppSettings["CardComRugbyLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = leagueRegistrationPrice?.Price ?? 0;
            if (regPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomTeam_WaveSurfing(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = leagueRegistrationPrice?.Price ?? 0;
            if (regPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomTeam_Tennis(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComTennisUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComTennisLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComTennisUsername"];
                var language = ConfigurationManager.AppSettings["CardComTennisLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var regPrice = leagueRegistrationPrice?.Price ?? 0;
            if (regPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = regPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_Catchball(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComUsername"];
                var language = ConfigurationManager.AppSettings["CardComLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var activityForm = activity.ActivityForms.FirstOrDefault();
            var formDetails = activityForm?.ActivityFormsDetails?.ToList();
            var registrationPriceControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerRegistrationPrice", StringComparison.CurrentCultureIgnoreCase));
            var insurancePriceControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerInsurancePrice", StringComparison.CurrentCultureIgnoreCase));
            var memberFeeControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerMemberFee", StringComparison.CurrentCultureIgnoreCase));
            var handlingFeeControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerHandlingFee", StringComparison.CurrentCultureIgnoreCase));

            var culture = string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                ? CultureInfo.CreateSpecificCulture("he-IL")
                : CultureInfo.CreateSpecificCulture("en-US");

            var invoiceCount = 1;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.RegistrationPriceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                var priceTitle = (string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                                              ? insurancePriceControl?.LabelTextHeb
                                              : insurancePriceControl?.LabelTextEn)
                                          ?? Messages.ResourceManager.GetString(nameof(Messages.Insurance), culture);

                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{priceTitle} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.InsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                var priceTitle = (string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                                              ? memberFeeControl?.LabelTextHeb
                                              : memberFeeControl?.LabelTextEn)
                                          ?? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), culture);

                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{priceTitle} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.MembersFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.MembersFeeCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                var priceTitle = (string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                                              ? handlingFeeControl?.LabelTextHeb
                                              : handlingFeeControl?.LabelTextEn)
                                          ?? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), culture);

                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{priceTitle} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.HandlingFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.HandlingFeeCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_Rugby(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComRugbyUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComRugbyLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComRugbyUsername"];
                var language = ConfigurationManager.AppSettings["CardComRugbyLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.RegistrationPriceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.InsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.MembersFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.MembersFeeCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.HandlingFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.HandlingFeeCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_WaveSurfing(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.RegistrationPriceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.InsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.MembersFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.MembersFeeCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.HandlingFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.HandlingFeeCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_Tennis(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComTennisUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComTennisLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComTennisUsername"];
                var language = ConfigurationManager.AppSettings["CardComTennisLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.RegistrationPriceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.InsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.MembersFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.MembersFeeCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.HandlingFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.HandlingFeeCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_Waterpolo(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = activity.ActivityId == 5430 
                    ? 2 
                    : 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.RegistrationPriceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.InsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.MembersFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.MembersFeeCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.HandlingFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.HandlingFeeCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionClub_Catchball(Activity activity, User user, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComUsername"];
                var language = ConfigurationManager.AppSettings["CardComLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionClub_Climbing59(Activity activity, User user, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59TerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59UsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComClimbingUnion59LanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59Terminal"];
                var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59Username"];
                var language = ConfigurationManager.AppSettings["CardComClimbingUnion59Language"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionClub_Waterpolo(Activity activity, User user, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionClub_WaveSurfing(Activity activity, User user, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionClub_Tennis(Activity activity, User user, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComTennisUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComTennisLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComTennisUsername"];
                var language = ConfigurationManager.AppSettings["CardComTennisLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionTeam_Catchball(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComUsername"];
                var language = ConfigurationManager.AppSettings["CardComLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var registrationPrice = leagueRegistrationPrice?.Price ?? 0;

            if (registrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = registrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }
        
        public static CardComRequestResult UnionTeam_Waterpolo(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaterpoloTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaterpoloUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaterpoloLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var registrationPrice = leagueRegistrationPrice?.Price ?? 0;

            if (registrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = registrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionTeam_Rugby(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComRugbyUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComRugbyLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComRugbyUsername"];
                var language = ConfigurationManager.AppSettings["CardComRugbyLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var registrationPrice = leagueRegistrationPrice?.Price ?? 0;

            if (registrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = registrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionTeam_WaveSurfing(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var registrationPrice = leagueRegistrationPrice?.Price ?? 0;

            if (registrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = registrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionTeam_Tennis(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguesPrice leagueRegistrationPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComTennisUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComTennisLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComTennisUsername"];
                var language = ConfigurationManager.AppSettings["CardComTennisLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            var registrationPrice = leagueRegistrationPrice?.Price ?? 0;

            if (registrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = registrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leagueRegistrationPrice?.CardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leagueRegistrationPrice.CardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayer_Catchball(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguePlayerPrice playerPrices)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComUsername"];
                var language = ConfigurationManager.AppSettings["CardComLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && playerPrices.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.RegistrationPriceCardComProductId;
                }
                invoiceCount++;
            }

            if (activity.InsurancePrice && playerPrices.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.InsuranceCardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayer_Rugby(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguePlayerPrice playerPrices)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComRugbyUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComRugbyLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComRugbyUsername"];
                var language = ConfigurationManager.AppSettings["CardComRugbyLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && playerPrices.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.RegistrationPriceCardComProductId;
                }
                invoiceCount++;
            }

            if (activity.InsurancePrice && playerPrices.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.InsuranceCardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayer_WaveSurfing(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguePlayerPrice playerPrices)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComWaveSurfingTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComWaveSurfingUsername"];
                var language = ConfigurationManager.AppSettings["CardComWaveSurfingLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && playerPrices.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.RegistrationPriceCardComProductId;
                }
                invoiceCount++;
            }

            if (activity.InsurancePrice && playerPrices.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.InsuranceCardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionPlayer_Tennis(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, LeaguePlayerPrice playerPrices)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComTennisUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComTennisLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComTennisTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComTennisUsername"];
                var language = ConfigurationManager.AppSettings["CardComTennisLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && playerPrices.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.RegistrationPriceCardComProductId;
                }
                invoiceCount++;
            }

            if (activity.InsurancePrice && playerPrices.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = playerPrices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(playerPrices.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = playerPrices.InsuranceCardComProductId;
                }
                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult DepartmentClubPlayer_Basketball(Activity activity, User user, Team team, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, TeamPlayerPrice teamPlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComBasketballTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComBasketballUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComBasketballLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComBasketballTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComBasketballUsername"];
                var language = ConfigurationManager.AppSettings["CardComBasketballLanguage"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 4,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && teamPlayerPrice.PlayerRegistrationAndEquipmentPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerRegistrationAndEquipmentPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && teamPlayerPrice.PlayerInsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerInsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerInsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerInsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.ParticipationPrice && teamPlayerPrice.ParticipationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.ParticipationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(teamPlayerPrice.ParticipationCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.ParticipationCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult ClubPlayer_Basketball(Activity activity, User user, List<Team> teams, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, List<TeamPlayerPrice> teamPlayerPrices, bool skipParticipation = false)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComBasketballTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComBasketballUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComBasketballLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComBasketballTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComBasketballUsername"];
                var language = ConfigurationManager.AppSettings["CardComBasketballLanguage"];
#endif

            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = activity.ClubLeagueTeamsOnly || activity.ActivityId == 3563
                    ? 12
                    : activity.ActivityId == 5415 ||
                      activity.ActivityId == 7851
                        ? 10
                        : 4,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            foreach (var team in teams)
            {
                var teamPlayerPrice = teamPlayerPrices.FirstOrDefault(x => x.TeamId == team?.TeamId);

                if (teamPlayerPrice == null)
                {
                    continue;
                }

                if (activity.RegistrationPrice && teamPlayerPrice.PlayerRegistrationAndEquipmentPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerRegistrationAndEquipmentPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId;
                    }

                    invoiceCount++;
                }

                if (activity.InsurancePrice && teamPlayerPrice.PlayerInsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerInsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerInsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerInsuranceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (!skipParticipation && activity.ParticipationPrice && teamPlayerPrice.ParticipationPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.ParticipationPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.ParticipationCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.ParticipationCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult ClubPlayer_Rugby(Activity activity, User user, List<Team> teams, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, List<TeamPlayerPrice> teamPlayerPrices, bool skipParticipation = false)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComRugbyUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComRugbyLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComRugbyTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComRugbyUsername"];
                var language = ConfigurationManager.AppSettings["CardComRugbyLanguage"];
#endif

            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = activity.ClubLeagueTeamsOnly || activity.ActivityId == 3563
                    ? 12
                    : 4,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            foreach (var team in teams)
            {
                var teamPlayerPrice = teamPlayerPrices.FirstOrDefault(x => x.TeamId == team.TeamId);

                if (teamPlayerPrice == null)
                {
                    continue;
                }

                if (activity.RegistrationPrice && teamPlayerPrice.PlayerRegistrationAndEquipmentPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerRegistrationAndEquipmentPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId;
                    }

                    invoiceCount++;
                }

                if (activity.InsurancePrice && teamPlayerPrice.PlayerInsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerInsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerInsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerInsuranceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (!skipParticipation && activity.ParticipationPrice && teamPlayerPrice.ParticipationPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.ParticipationPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.ParticipationCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.ParticipationCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult ClubPlayer_Soccer(Activity activity, User user, List<Team> teams, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, List<TeamPlayerPrice> teamPlayerPrices, bool skipParticipation = false)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComSoccerTerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComSoccerUsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComSoccerLanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComSoccerTerminal"];
                var userName = ConfigurationManager.AppSettings["CardComSoccerUsername"];
                var language = ConfigurationManager.AppSettings["CardComSoccerLanguage"];
#endif

            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = activity.ActivityId == 6249 || activity.ActivityId == 6250 
				    ? 6
					: activity.ClubLeagueTeamsOnly || activity.ActivityId == 3563
                        ? 12
                        : activity.ActivityId == 3591 || activity.ActivityId == 4790 
					        ? 10 
						    : 4,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            foreach (var team in teams)
            {
                var teamPlayerPrice = teamPlayerPrices.FirstOrDefault(x => x.TeamId == team?.TeamId);
                if (teamPlayerPrice == null)
                {
                    continue;
                }

                if (activity.RegistrationPrice && teamPlayerPrice.PlayerRegistrationAndEquipmentPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerRegistrationAndEquipmentPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId;
                    }

                    invoiceCount++;
                }

                if (activity.InsurancePrice && teamPlayerPrice.PlayerInsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerInsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerInsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerInsuranceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (!skipParticipation && activity.ParticipationPrice && teamPlayerPrice.ParticipationPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.ParticipationPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.ParticipationCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.ParticipationCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult ClubPlayer_GymnasticClub3610(Activity activity, User user, List<Team> teams, List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal, List<TeamPlayerPrice> teamPlayerPrices, bool skipParticipation = false)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComGymnasticClub3610TerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComGymnasticClub3610UsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComGymnasticClub3610LanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComGymnasticClub3610Terminal"];
                var userName = ConfigurationManager.AppSettings["CardComGymnasticClub3610Username"];
                var language = ConfigurationManager.AppSettings["CardComGymnasticClub3610Language"];
#endif

            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = activity.ClubLeagueTeamsOnly || activity.ActivityId == 6577 || activity.ActivityId == 6594
                    ? 7
                    : activity.ActivityId == 7936
                    ? 3
                    : 1,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var activityForm = activity.ActivityForms.FirstOrDefault();
            var formDetails = activityForm?.ActivityFormsDetails?.ToList();
            var registrationPriceControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerRegistrationPrice", StringComparison.CurrentCultureIgnoreCase));
            var insurancePriceControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerInsurancePrice", StringComparison.CurrentCultureIgnoreCase));
            var participationPriceControl = formDetails?.FirstOrDefault(x =>
                string.Equals(x.PropertyName, "playerParticipationPrice", StringComparison.CurrentCultureIgnoreCase));

            var culture = string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                ? CultureInfo.CreateSpecificCulture("he-IL")
                : CultureInfo.CreateSpecificCulture("en-US");

            var invoiceCount = 1;

            foreach (var team in teams)
            {
                var teamPlayerPrice = teamPlayerPrices.FirstOrDefault(x => x.TeamId == team?.TeamId);
                if (teamPlayerPrice == null)
                {
                    continue;
                }

                if (activity.RegistrationPrice && teamPlayerPrice.PlayerRegistrationAndEquipmentPrice > 0)
                {
                    var priceTitle = (string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                                                  ? registrationPriceControl?.LabelTextHeb
                                                  : registrationPriceControl?.LabelTextEn)
                                              ?? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_PlayerRegistrationAndEquipmentPrice), culture);

                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{priceTitle} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerRegistrationAndEquipmentPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerRegistrationAndEquipmentCardComProductId;
                    }

                    invoiceCount++;
                }

                if (activity.InsurancePrice && teamPlayerPrice.PlayerInsurancePrice > 0)
                {
                    var priceTitle = (string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                                                  ? insurancePriceControl?.LabelTextHeb
                                                  : insurancePriceControl?.LabelTextEn)
                                              ?? Messages.ResourceManager.GetString(nameof(Messages.Insurance), culture);

                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{priceTitle} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.PlayerInsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.PlayerInsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.PlayerInsuranceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (!skipParticipation && activity.ParticipationPrice && teamPlayerPrice.ParticipationPrice > 0)
                {
                    var priceTitle = (string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                                                  ? participationPriceControl?.LabelTextHeb
                                                  : participationPriceControl?.LabelTextEn)
                                              ?? Messages.ResourceManager.GetString(nameof(Messages.TeamDetails_ParticipationPrice), culture);

                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{priceTitle} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = teamPlayerPrice.ParticipationPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(teamPlayerPrice.ParticipationCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = teamPlayerPrice.ParticipationCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_Climbing59(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59TerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59UsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComClimbingUnion59LanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59Terminal"];
                var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59Username"];
                var language = ConfigurationManager.AppSettings["CardComClimbingUnion59Language"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var culture = string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                ? CultureInfo.CreateSpecificCulture("he-IL")
                : CultureInfo.CreateSpecificCulture("en-US");

            var invoiceCount = 1;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.RegistrationPriceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{Messages.ResourceManager.GetString(nameof(Messages.Insurance), culture)} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.InsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), culture)} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.MembersFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.MembersFeeCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), culture)} ({Messages.ResourceManager.GetString(nameof(Messages.Team), culture)}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.HandlingFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.HandlingFeeCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_Climbing59CompetitionCategory(
            Activity activity, User user, 
            List<CompetitionDiscipline> competitionDisciplines,
            List<LeaguePlayerPrice> competitionsPrices,
            List<ActivityCustomPriceModel> customPrices,
            decimal priceTotal)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59TerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59UsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComClimbingUnion59LanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComClimbingUnion59Terminal"];
                var userName = ConfigurationManager.AppSettings["CardComClimbingUnion59Username"];
                var language = ConfigurationManager.AppSettings["CardComClimbingUnion59Language"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var culture = string.Equals(language, "he", StringComparison.CurrentCultureIgnoreCase)
                ? CultureInfo.CreateSpecificCulture("he-IL")
                : CultureInfo.CreateSpecificCulture("en-US");

            var invoiceCount = 1;

            foreach (var competitionDiscipline in competitionDisciplines)
            {
                var prices = competitionsPrices.FirstOrDefault(x => x.LeagueId == competitionDiscipline.CompetitionId);
                if (prices == null)
                {
                    continue;
                }

                var categoryName =
                    $"{competitionDiscipline.League.Name} - {competitionDiscipline.CompetitionAge.age_name}";

                if (activity.RegistrationPrice && prices.RegistrationPrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{activity.Name} ({Messages.ResourceManager.GetString(nameof(Messages.Category), culture)}: {categoryName})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.RegistrationPriceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.RegistrationPriceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (activity.InsurancePrice && prices.InsurancePrice > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{Messages.ResourceManager.GetString(nameof(Messages.Insurance), culture)} ({Messages.ResourceManager.GetString(nameof(Messages.Category), culture)}: {categoryName})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.InsuranceCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.InsuranceCardComProductId;
                    }

                    invoiceCount++;
                }

                if (activity.MembersFee && prices.MembersFee > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), culture)} ({Messages.ResourceManager.GetString(nameof(Messages.Category), culture)}: {categoryName})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.MembersFee.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.MembersFeeCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.MembersFeeCardComProductId;
                    }

                    invoiceCount++;
                }

                if (activity.HandlingFee && prices.HandlingFee > 0)
                {
                    vars[$"InvoiceLines{invoiceCount}.Description"] =
                        $"{Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), culture)} ({Messages.ResourceManager.GetString(nameof(Messages.Category), culture)}: {categoryName})";
                    vars[$"InvoiceLines{invoiceCount}.Price"] = prices.HandlingFee.ToString(CultureInfo.InvariantCulture);
                    vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                    vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                    if (!string.IsNullOrWhiteSpace(prices.HandlingFeeCardComProductId))
                    {
                        vars[$"InvoiceLines{invoiceCount}.ProductID"] = prices.HandlingFeeCardComProductId;
                    }

                    invoiceCount++;
                }
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }

        public static CardComRequestResult UnionCustomPersonal_Gymnastic36(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
#if DEBUG
            var terminalNumber = ConfigurationManager.AppSettings["CardComGymnasticUnion36TerminalDebug"];
            var userName = ConfigurationManager.AppSettings["CardComGymnasticUnion36UsernameDebug"];
            var language = ConfigurationManager.AppSettings["CardComGymnasticUnion36LanguageDebug"];
#else
                var terminalNumber = ConfigurationManager.AppSettings["CardComGymnasticUnion36Terminal"];
                var userName = ConfigurationManager.AppSettings["CardComGymnasticUnion36Username"];
                var language = ConfigurationManager.AppSettings["CardComGymnasticUnion36Language"];
#endif
            var vars = PrepareRequest(new CardComInitParameters
            {
                MinPayments = 1,
                MaxPayments = 5,
                Language = language,
                TerminalNumber = terminalNumber,
                UserName = userName,
                User = user,
                PriceTotal = priceTotal
            });

            var invoiceCount = 1;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{activity.Name} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.RegistrationPriceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.RegistrationPriceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Insurance), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.InsuranceCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.InsuranceCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_MemberFees), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.MembersFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.MembersFeeCardComProductId;
                }

                invoiceCount++;
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                vars[$"InvoiceLines{invoiceCount}.Description"] =
                    $"{(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.LeagueDetail_HandlingFee), CultureInfo.CreateSpecificCulture("en-US")))} ({(language == "he" ? Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("he-IL")) : Messages.ResourceManager.GetString(nameof(Messages.Team), CultureInfo.CreateSpecificCulture("en-US")))}: {team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName ?? team?.Title ?? Messages.Activity_NoTeamPlaceholder})";
                vars[$"InvoiceLines{invoiceCount}.Price"] = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture);
                vars[$"InvoiceLines{invoiceCount}.Quantity"] = "1";
                vars[$"InvoiceLines{invoiceCount}.IsPriceIncludeVAT"] = "true";
                if (!string.IsNullOrWhiteSpace(leaguePlayerPrice.HandlingFeeCardComProductId))
                {
                    vars[$"InvoiceLines{invoiceCount}.ProductID"] = leaguePlayerPrice.HandlingFeeCardComProductId;
                }

                invoiceCount++;
            }

            vars.AddCustomPrices(customPrices, invoiceCount, language == "he");

            return vars.Execute(activity, user);
        }
    }
}