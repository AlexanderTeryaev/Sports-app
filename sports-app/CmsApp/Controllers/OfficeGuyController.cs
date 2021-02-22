using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using AppModel;
using CmsApp.Helpers;
using CmsApp.Models;
using DataService.Utils;

namespace CmsApp.Controllers
{
    public class OfficeGuyController : AdminController
    {
        [HttpGet]
        public ActionResult BicycleFriendship(int teamPlayerId)
        {
            User currentUser = null;
            int currentUserId;
            if (int.TryParse(User.Identity.Name, out currentUserId))
            {
                currentUser = db.Users.Find(currentUserId);
            }

            var teamPlayer = db.TeamsPlayers
                .Include(x => x.Season)
                .Include(x => x.User)
                .Include(x => x.FriendshipsType)
                .Include(x => x.FriendshipsType.FriendshipPrices)
                .FirstOrDefault(x => x.Id == teamPlayerId);
            
            if (teamPlayer == null)
            {
                return Content($"Unable to find teamPlayer '{teamPlayerId}'");
            }

            var payment = OfficeGuyHelper.CreateBicycleFriendshipPayment(currentUser?.FullName, teamPlayer);

            if (payment.Success)
            {
                var kvp = payment.TeamPlayersPrices.FirstOrDefault();

                var bicyclePrices = kvp.Value;

                var newPayment = new BicycleFriendshipPayment
                {
                    UserId = teamPlayer.UserId,
                    ClubId = teamPlayer.ClubId,
                    TeamId = teamPlayer.TeamId,
                    SeasonId = teamPlayer.SeasonId,

                    FriendshipPrice = bicyclePrices.FriendshipPrice,
                    ChipPrice = bicyclePrices.ChipPrice,
                    UciPrice = bicyclePrices.UciPrice,

                    LogLigPaymentId = payment.PaymentIdentifier,
                    DateCreated = DateTime.Now,
                    CreatedBy = currentUser?.UserId
                };

                db.BicycleFriendshipPayments.Add(newPayment);
                db.SaveChanges();

                return Redirect(payment.RedirectUrl);
            }

            return Content($"Unable to create payment for teamPlayer id '{teamPlayerId}'");
        }

        [HttpPost]
        public ActionResult BicycleFriendshipMultiple(int[] teamPlayersIds)
        {
            User currentUser = null;
            int currentUserId;
            if (int.TryParse(User.Identity.Name, out currentUserId))
            {
                currentUser = db.Users.Find(currentUserId);
            }

            teamPlayersIds = teamPlayersIds?.Distinct().ToArray();

            var teamPlayers = db.TeamsPlayers
                .Include(x => x.Season)
                .Include(x => x.User)
                .Include(x => x.FriendshipsType)
                .Include(x => x.FriendshipsType.FriendshipPrices)
                .Where(x => teamPlayersIds.Contains(x.Id))
                .ToList();

            if (!teamPlayers.Any())
            {
                return Content($"Unable to find teamPlayers '{string.Join(", ", teamPlayersIds)}'");
            }

            var payment = OfficeGuyHelper.CreateBicycleFriendshipPayment(currentUser?.FullName, teamPlayers);

            if (payment.Success)
            {
                foreach (var kvp in payment.TeamPlayersPrices)
                {
                    var teamPlayer = kvp.Key;
                    var bicyclePrices = kvp.Value;

                    var newPayment = new BicycleFriendshipPayment
                    {
                        UserId = teamPlayer.UserId,
                        ClubId = teamPlayer.ClubId,
                        TeamId = teamPlayer.TeamId,
                        SeasonId = teamPlayer.SeasonId,

                        FriendshipPrice = bicyclePrices.FriendshipPrice,
                        ChipPrice = bicyclePrices.ChipPrice,
                        UciPrice = bicyclePrices.UciPrice,

                        LogLigPaymentId = payment.PaymentIdentifier,
                        DateCreated = DateTime.Now,
                        CreatedBy = currentUser?.UserId
                    };

                    db.BicycleFriendshipPayments.Add(newPayment);
                }

                db.SaveChanges();

                return Json(new { url = payment.RedirectUrl });
            }

            return Content($"Unable to create payment for teamPlayers ids '{string.Join(", ", teamPlayersIds)}'");
        }

        public ActionResult PaymentSuccess(
            [Bind(Prefix = "OG-CustomerID")]long customerId,
            [Bind(Prefix = "OG-PaymentID")]long paymentId,
            [Bind(Prefix = "OG-ExternalIdentifier")]Guid externalIdentifier,
            [Bind(Prefix = "OG-DocumentNumber")]int documentNumber)
        {
            //Example: ?OG-CustomerID=50807780&OG-PaymentID=50824748&OG-ExternalIdentifier=f9245e83-290e-4133-a2d8-2ab3ed140b35&OG-DocumentNumber=10003
            var paymentDetails = OfficeGuyHelper.GetPaymentDetails(paymentId);
            var bicyclePayments = db.BicycleFriendshipPayments
                .Where(x => x.LogLigPaymentId == externalIdentifier)
                .ToList();

            if (!bicyclePayments.Any())
            {
                return Content($"Unable to find payment '{externalIdentifier}'");
            }

            var totalAmountToPay = bicyclePayments.Sum(x => x.FriendshipPrice) +
                                   bicyclePayments.Sum(x => x.ChipPrice) +
                                   bicyclePayments.Sum(x => x.UciPrice);

            if (totalAmountToPay != paymentDetails.Amount)
            {
                return Content($"Amount paid({paymentDetails.Amount}) does not equals to what should be paid({totalAmountToPay})");
            }

            foreach (var bicyclePayment in bicyclePayments)
            {
                bicyclePayment.OfficeGuyCustomerId = customerId;
                bicyclePayment.OfficeGuyPaymentId = paymentId;
                bicyclePayment.OfficeGuyDocumentNumber = documentNumber;
                bicyclePayment.IsPaid = true;
                bicyclePayment.DatePaid = DateTime.Now;
            }

            db.SaveChanges();

            return PartialView("PaymentSuccessful", new OfficeGuyPaymentSuccessfulModel
            {
                PaymentIdentifier = externalIdentifier
            });
        }

        [HttpPost]
        public ActionResult DiscardBicycleFriendshipPayment(int id)
        {
            var payment = db.BicycleFriendshipPayments.Find(id);

            if (payment == null)
            {
                return HttpNotFound();
            }

            User currentUser = null;
            int currentUserId;
            if (int.TryParse(User.Identity.Name, out currentUserId))
            {
                currentUser = db.Users.Find(currentUserId);
            }

            payment.Discarded = true;
            payment.DiscardedBy = currentUser?.UserId;
            payment.DiscardDate = DateTime.Now;

            db.SaveChanges();

            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult CreateFriendshipPaymentDialog(int[] teamPlayersIds)
        {
            var model = new FriendshipPaymentDialogModel
            {
                TeamsPlayersIds = teamPlayersIds,
                DatePaid = DateTime.Now,
                CanSubmitManualPayment = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager)
            };

            return PartialView("_FrienshipPaymentDialog", model);
        }

        [HttpPost]
        public ActionResult AddFriendshipPaymentManually(FriendshipPaymentDialogModel model)
        {
			model.CanSubmitManualPayment = User.IsInAnyRole(AppRole.Admins) || User.HasTopLevelJob(JobRole.UnionManager);
			if(!model.CanSubmitManualPayment){
                model.Error = true;
                return PartialView("_FrienshipPaymentDialog", model);
			}
			
            User currentUser = null;
            int currentUserId;
            if (int.TryParse(User.Identity.Name, out currentUserId))
            {
                currentUser = db.Users.Find(currentUserId);
            }

            var teamPlayersIds = model.TeamsPlayersIds?.Distinct().ToArray();

            var teamPlayers = db.TeamsPlayers
                .Include(x => x.Season)
                .Include(x => x.User)
                .Include(x => x.FriendshipsType)
                .Include(x => x.FriendshipsType.FriendshipPrices)
                .Where(x => teamPlayersIds.Contains(x.Id))
                .ToList();

            if (!teamPlayers.Any())
            {
                model.Error = true;
                return PartialView("_FrienshipPaymentDialog", model);
            }

            var bicycleFriendshipPriceHelper = new BicycleFriendshipPriceHelper();
            var teamPlayersPrices = bicycleFriendshipPriceHelper.GetFriendshipPrice(teamPlayers);

            var logLigPaymentId = Guid.NewGuid();

            foreach (var kvp in teamPlayersPrices)
            {
                var teamPlayer = kvp.Key;
                var bicyclePrices = kvp.Value;

                var newPayment = new BicycleFriendshipPayment
                {
                    UserId = teamPlayer.UserId,
                    ClubId = teamPlayer.ClubId,
                    TeamId = teamPlayer.TeamId,
                    SeasonId = teamPlayer.SeasonId,

                    FriendshipPrice = bicyclePrices.FriendshipPrice,
                    ChipPrice = bicyclePrices.ChipPrice,
                    UciPrice = bicyclePrices.UciPrice,

                    DateCreated = DateTime.Now,
                    CreatedBy = currentUser?.UserId,

                    DatePaid = model.DatePaid,
                    IsPaid = true,
                    IsManual = true,
                    Comment = model.Comment,
                    LogLigPaymentId = logLigPaymentId
                };

                db.BicycleFriendshipPayments.Add(newPayment);
            }

            db.SaveChanges();

            model.Completed = true;			

            return PartialView("_FrienshipPaymentDialog", model);
        }
    }
}