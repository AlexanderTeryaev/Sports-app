using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AppModel;
using DataService.Utils;
using OfficeGuy.APIs.Billing;
using Resources;

namespace CmsApp.Helpers
{
    public static class OfficeGuyHelper
    {
        public class OfficeGuyResponse
        {
            public bool Success { get; set; }
            public string RedirectUrl { get; set; }
            public Guid PaymentIdentifier { get; set; }

            public Dictionary<TeamsPlayer, BicycleFriendshipPriceHelper.BicycleFriendshipPrice> TeamPlayersPrices { get; set; }
        }

        private static APICredentials GetCredentials()
        {
#if DEBUG
            var companyId = ConfigurationManager.AppSettings["OfficeGuyBicycleCompanyIdDebug"];
            var apiKey = ConfigurationManager.AppSettings["OfficeGuyBicycleApiKeyDebug"];
#else
            var companyId = ConfigurationManager.AppSettings["OfficeGuyBicycleCompanyId"];
            var apiKey = ConfigurationManager.AppSettings["OfficeGuyBicycleApiKey"];
#endif

            long longCompanyId;
            if (long.TryParse(companyId, out longCompanyId))
            {
                return new APICredentials
                {
                    CompanyID = longCompanyId,
                    APIKey = apiKey
                };
            }

            throw new Exception("Unable to get API credentials");
        }

        public static Payment GetPaymentDetails(long paymentId)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var client = new PaymentsClient();

            var request = new Payments_Get_Request
            {
                Credentials = GetCredentials(),
                PaymentID = paymentId
            };

            var result = client.Get(request);

            if (result.Status == ResponseStatus.Success)
            {
                return result.Data.Payment;
            }

            throw new Exception(
                $"Unable to get payment details for payment '{paymentId}'. Error: '{result.UserErrorMessage}', technical details: '{result.TechnicalErrorDetails}'");
        }

        public static OfficeGuyResponse CreateBicycleFriendshipPayment(string customerName, TeamsPlayer teamPlayer)
        {
            return CreateBicycleFriendshipPayment(customerName, new List<TeamsPlayer> {teamPlayer});
        }

        public static OfficeGuyResponse CreateBicycleFriendshipPayment(string customerName, List<TeamsPlayer> teamPlayers)
        {
            if (teamPlayers?.Any() != true)
            {
                return new OfficeGuyResponse();
            }

            var bicycleFriendshipPriceHelper = new BicycleFriendshipPriceHelper();
            var teamPlayersPrices = bicycleFriendshipPriceHelper.GetFriendshipPrice(teamPlayers);

            if (!teamPlayersPrices.Any(x => x.Value.Total > 0))
            {
                return new OfficeGuyResponse();
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

            var client = new PaymentsClient();

            var urlHelper = new UrlHelper(HttpContext.Current?.Request.RequestContext);

            var logLigPaymentId = Guid.NewGuid();

            var request = new Payments_BeginRedirect_Request
            {
                Credentials = GetCredentials(),
                VATIncluded = true,
                RedirectURL = urlHelper.Action("PaymentSuccess", "OfficeGuy", new { },
                    HttpContext.Current?.Request.Url.Scheme),
                ExternalIdentifier = logLigPaymentId.ToString().ToUpper(),
                MaximumPayments = 5,
                //SendUpdateByEmailAddress = "info@loglig.com",
                Items = new List<ChargeItem>(),
                Customer = new Customer
                {
                    Name = customerName ?? "<unknown>",
                    //ExternalIdentifier = "ext ID test",
                    //Address = "test addr",
                    //City = "test city",
                    //CompanyNumber = "test company number",
                    //EmailAddress = "test@email.addres",
                    //Folder = "test folder",
                    //Phone = "test phone"
                }
            };

            foreach (var bicycleFriendshipPrice in teamPlayersPrices)
            {
                if (bicycleFriendshipPrice.Value.Total <= 0)
                {
                    continue;
                }

                var teamPlayer = bicycleFriendshipPrice.Key;
                var user = teamPlayer.User;
                var price = bicycleFriendshipPrice.Value;

                if (price.FriendshipPrice > 0)
                {
                    request.Items.Add(new ChargeItem
                    {
                        Description = $"{Messages.FriendshipPrices} ({user.FullName} - {user.IdentNum})",
                        Quantity = 1,
                        UnitPrice = price.FriendshipPrice,
                        Item = new IncomeItem()
                    });
                }

                if (price.ChipPrice > 0)
                {
                    request.Items.Add(new ChargeItem
                    {
                        Description = $"{Messages.ChipPrices} ({user.FullName} - {user.IdentNum})",
                        Quantity = 1,
                        UnitPrice = price.ChipPrice,
                        Item = new IncomeItem()
                    });
                }

                if (price.UciPrice > 0)
                {
                    request.Items.Add(new ChargeItem
                    {
                        Description = $"{Messages.UciId} ({user.FullName} - {user.IdentNum})",
                        Quantity = 1,
                        UnitPrice = price.UciPrice,
                        Item = new IncomeItem()
                    });
                }
            }

            var result = client.BeginRedirect(request);

            if (result.Status != ResponseStatus.Success)
            {
                throw new Exception($"User error: {result.UserErrorMessage}{Environment.NewLine}Technical error: {result.TechnicalErrorDetails}");
            }

            return new OfficeGuyResponse
            {
                Success = true,
                PaymentIdentifier = logLigPaymentId,
                RedirectUrl = result.Data.RedirectURL,
                TeamPlayersPrices = teamPlayersPrices
            };
        }
    }
}