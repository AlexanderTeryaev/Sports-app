using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppModel;
using CmsApp.Models;
using PayPal.Api;
using Resources;

namespace CmsApp.Helpers
{
    public static class PayPalHelper
    {
        public class PayPalResult
        {
            public string RedirectUrl { get; set; }
            public string PayPalPaymentId { get; set; }
            public Guid PayPalLogLigId { get; set; }
        }

        private static void AddCustomPrices(this List<Item> paymentItems, List<ActivityCustomPriceModel> customPrices)
        {
            if (customPrices?.Any() == true)
            {
                foreach (var customPrice in customPrices)
                {
                    string priceTitle;
                    switch (CultureInfo.CurrentCulture.TwoLetterISOLanguageName.ToLower())
                    {
                        case "en":
                            priceTitle = customPrice.TitleEng;
                            break;

                        case "he":
                            priceTitle = customPrice.TitleHeb;
                            break;

                        case "uk":
                            priceTitle = customPrice.TitleUk;
                            break;

                        default:
                            priceTitle = customPrice.TitleEng;
                            break;
                    }

                    paymentItems.Add(new Item
                    {
                        name = $"{priceTitle}",
                        currency = "USD",
                        price = customPrice.Price.ToString(CultureInfo.InvariantCulture),
                        quantity = customPrice.Quantity.ToString(),
                        //sku = "sku"
                    });
                }
            }
        }

        private static APIContext GetApiContext()
        {
            var config = ConfigManager.Instance.GetProperties();
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            return new APIContext(accessToken);
        }

        public static PayPalResult UnionCustomPersonal_Catchball15(Activity activity, User user, Team team,
            List<ActivityCustomPriceModel> customPrices, decimal priceTotal, LeaguePlayerPrice leaguePlayerPrice)
        {
            var apiContext = GetApiContext();

            var paymentId = Guid.NewGuid();

            var paymentItems = new List<Item>();

            var teamName = team?.TeamsDetails?.FirstOrDefault(x => x.SeasonId == activity.SeasonId)?.TeamName
                           ?? team?.Title
                           ?? Messages.Activity_NoTeamPlaceholder;

            if (activity.RegistrationPrice && leaguePlayerPrice.RegistrationPrice > 0)
            {
                paymentItems.Add(new Item
                {
                    name = $"{Messages.Team}: {teamName}",
                    currency = "USD",
                    price = leaguePlayerPrice.RegistrationPrice.ToString(CultureInfo.InvariantCulture),
                    quantity = "1",
                    //sku = "sku"
                });
            }

            if (activity.InsurancePrice && leaguePlayerPrice.InsurancePrice > 0)
            {
                paymentItems.Add(new Item
                {
                    name = $"{Messages.Insurance} ({Messages.Team}: {teamName})",
                    currency = "USD",
                    price = leaguePlayerPrice.InsurancePrice.ToString(CultureInfo.InvariantCulture),
                    quantity = "1",
                    //sku = "sku"
                });
            }

            if (activity.MembersFee && leaguePlayerPrice.MembersFee > 0)
            {
                paymentItems.Add(new Item
                {
                    name = $"{Messages.LeagueDetail_MemberFees} ({Messages.Team}: {teamName})",
                    currency = "USD",
                    price = leaguePlayerPrice.MembersFee.ToString(CultureInfo.InvariantCulture),
                    quantity = "1",
                    //sku = "sku"
                });
            }

            if (activity.HandlingFee && leaguePlayerPrice.HandlingFee > 0)
            {
                paymentItems.Add(new Item
                {
                    name = $"{Messages.LeagueDetail_HandlingFee} ({Messages.Team}: {teamName})",
                    currency = "USD",
                    price = leaguePlayerPrice.HandlingFee.ToString(CultureInfo.InvariantCulture),
                    quantity = "1",
                    //sku = "sku"
                });
            }

            paymentItems.AddCustomPrices(customPrices);

            var transactionList = new List<Transaction>();
            transactionList.Add(new Transaction
            {
                description = activity.Name,
                invoice_number = paymentId.ToString(),
                
                amount = new Amount
                {
                    currency = "USD",
                    total = priceTotal.ToString(CultureInfo.InvariantCulture), // Total must be equal to sum of shipping, tax and subtotal.
                    //details = new Details
                    //{
                    //    tax = "15",
                    //    shipping = "10",
                    //    subtotal = "75"
                    //}
                },
                item_list = new ItemList
                {
                    items = paymentItems
                }
            });

            var urlHelper = new UrlHelper(HttpContext.Current?.Request.RequestContext);
            var payment = new Payment
            {
                intent = "sale",
                payer = new Payer { payment_method = "paypal" }, //"credit_card"
                redirect_urls = new RedirectUrls
                {
                    cancel_url = urlHelper.Action("PayPalCancel", "Activity", new { llPaymentId = paymentId }, HttpContext.Current?.Request.Url.Scheme),
                    return_url = urlHelper.Action("PayPalSuccess", "Activity", new { llPaymentId = paymentId }, HttpContext.Current?.Request.Url.Scheme)
                },
                transactions = transactionList
            };

            var createdPayment = payment.Create(apiContext);

            var link = createdPayment.links.FirstOrDefault(x => string.Equals(x.rel, "approval_url"));

            return new PayPalResult
            {
                RedirectUrl = link?.href,
                PayPalLogLigId = paymentId,
                PayPalPaymentId = createdPayment.id
            };
        }

        public static void ExecutePayment(string paymentId, string payerId)
        {
            var apiContext = GetApiContext();

            var paymentExecution = new PaymentExecution {payer_id = payerId};
            var payment = new Payment {id = paymentId};

            var executedPayment = payment.Execute(apiContext, paymentExecution);
        }
    }
}