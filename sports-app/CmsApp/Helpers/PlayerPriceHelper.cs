using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using AppModel;
using CmsApp.Controllers;
using CmsApp.Models;
using DataService;
using DataService.Utils;

namespace CmsApp.Helpers
{
    public class PlayerPriceHelper : AdminController
    {
        public UnionClubPlayerPrice GetUnionClubPlayerPrice(int unionId, int seasonId, DateTime? birthDate)
        {
            var result = new UnionClubPlayerPrice();

            var union = unionsRepo.GetById(unionId);

            if (union == null || !union.EnablePaymentsForPlayerClubRegistrations)
            {
                return result;
            }

            var unionPrices = union.UnionPrices.Where(x => x.SeasonId == seasonId).ToList();

            var competingPrice = GetUnionPrice(unionPrices, UnionPriceType.UnionToClubCompetingRegistrationPrice, birthDate);

            result.CompetingRegistrationPrice = competingPrice?.Price ?? 0;
            result.CompetingRegistrationCardComProductId = competingPrice?.CardComProductId;

            var regularPrice = GetUnionPrice(unionPrices, UnionPriceType.UnionToClubRegularRegistrationPrice, birthDate);

            result.RegularRegistrationPrice = regularPrice?.Price ?? 0;
            result.RegularRegistrationCardComProductId = regularPrice?.CardComProductId;

            var insurancePrice = GetUnionPrice(unionPrices, UnionPriceType.UnionToClubInsurancePrice, birthDate);

            result.InsurancePrice = insurancePrice?.Price ?? 0;
            result.InsuranceCardComProductId = insurancePrice?.CardComProductId;

            var tenicardPrice = string.Equals(union.Section?.Alias, SectionAliases.Tennis, StringComparison.CurrentCultureIgnoreCase)
                ? GetUnionPrice(unionPrices, UnionPriceType.UnionToClubTenicardPrice, birthDate)
                : null;
            result.TenicardPrice = tenicardPrice?.Price ?? 0;
            result.TenicardCardComProductId = tenicardPrice?.CardComProductId;

            return result;
        }

        private UnionPrice GetUnionPrice(List<UnionPrice> prices, UnionPriceType priceType, DateTime? birthDate)
        {
            return prices.FirstOrDefault(x =>
                x.PriceType == (int) priceType &&
                x.StartDate <= DateTime.Now && x.EndDate > DateTime.Now &&
                (x.FromBirthday == null || x.FromBirthday <= birthDate) &&
                (x.ToBirthday == null || x.ToBirthday >= birthDate));
        }

        public TeamPlayerPrice GetTeamPlayerPrice(int playerId, int teamId, int clubId, int seasonId, Activity activity,
            bool applyDiscounts = true, string brotherIdNum = "", DateTime? startDateForPriceAdjustment = null)
        {
            var player = usersRepo.GetById(playerId);
            var club = clubsRepo.GetById(clubId);
            var team = teamRepo.GetById(teamId, seasonId);
            if (club == null || team == null)
            {
                return new TeamPlayerPrice {TeamId = teamId};
            }

            var baseRegistrationAndEquipmentPrice = team.ClubTeamPrices
                .FirstOrDefault(x => x.PriceType == (int?) ClubTeamPriceType.PlayerRegistrationAndEquipmentPrice &&
                                     x.ClubId == club.ClubId &&
                                     x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);

            var baseInsurancePrice = team.ClubTeamPrices
                .FirstOrDefault(x => x.PriceType == (int?) ClubTeamPriceType.PlayerInsurancePrice &&
                                     x.ClubId == club.ClubId &&
                                     x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);

            var baseParticipationPrice = team.ClubTeamPrices
                .FirstOrDefault(x => x.PriceType == (int?) ClubTeamPriceType.ParticipationPrice &&
                                     x.ClubId == club.ClubId &&
                                     x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);

            var resultPrice = new TeamPlayerPrice
            {
                TeamId = teamId,
                PlayerInsurancePrice = baseInsurancePrice?.Price ?? 0,
                ParticipationPrice = baseParticipationPrice?.Price ?? 0,
                PlayerRegistrationAndEquipmentPrice = baseRegistrationAndEquipmentPrice?.Price ?? 0
            };

            if (baseRegistrationAndEquipmentPrice != null && activity?.AdjustRegistrationPriceByDate == true)
            {
                resultPrice.PlayerRegistrationAndEquipmentPrice = AdjustPriceByDate(
                    resultPrice.PlayerRegistrationAndEquipmentPrice,
                    baseRegistrationAndEquipmentPrice.StartDate,
                    baseRegistrationAndEquipmentPrice.EndDate,
                    startDateForPriceAdjustment);
            }

            if (baseParticipationPrice != null && activity?.AdjustParticipationPriceByDate == true)
            {
                resultPrice.ParticipationPrice = AdjustPriceByDate(
                    resultPrice.ParticipationPrice,
                    baseParticipationPrice.StartDate,
                    baseParticipationPrice.EndDate,
                    startDateForPriceAdjustment);
            }

            if (baseInsurancePrice != null && activity?.AdjustInsurancePriceByDate == true)
            {
                resultPrice.PlayerInsurancePrice = AdjustPriceByDate(
                    resultPrice.PlayerInsurancePrice,
                    baseInsurancePrice.StartDate,
                    baseInsurancePrice.EndDate,
                    startDateForPriceAdjustment);
            }

            if (activity?.EnableBrotherDiscount == true && !string.IsNullOrWhiteSpace(brotherIdNum))
            {
                var brotherRegistrations =
                    activity.ActivityFormsSubmittedDatas
                        .Where(x => x.User.IdentNum == brotherIdNum)
                        .ToList();

                var brotherRegistrationsTeams = brotherRegistrations
                    .Where(x => x.TeamId > 0)
                    .Select(x => x.TeamId)
                    .ToArray();
                var brotherRegistrationsClubs = brotherRegistrations
                    .Where(x => x.ClubId > 0)
                    .Select(x => x.ClubId)
                    .ToArray();

                var brotherTeamsPrices = db.ClubTeamPrices
                    .Where(x => brotherRegistrationsTeams.Contains(x.TeamId) &&
                                brotherRegistrationsClubs.Contains(x.ClubId) &&
                                x.PriceType == (int?) ClubTeamPriceType.ParticipationPrice &&
                                x.StartDate <= DateTime.Now &&
                                x.EndDate > DateTime.Now &&
                                x.Price > 0)
                    .ToList();

                var lowestPriceOfBrotherRegistrations = brotherTeamsPrices.Any()
                    ? brotherTeamsPrices.Min(x => x.Price)
                    : 0;

                if (lowestPriceOfBrotherRegistrations > 0)
                {
                    var lowestPrice = Math.Min(lowestPriceOfBrotherRegistrations.Value,
                        resultPrice.ParticipationPrice);

                    if (activity.BrotherDiscountInPercent)
                    {
                        resultPrice.ParticipationPrice -= lowestPrice * (activity.BrotherDiscountAmount / 100);
                    }
                    else
                    {
                        resultPrice.ParticipationPrice -= activity.BrotherDiscountAmount;
                    }

                    resultPrice.ParticipationPrice =
                        decimal.Round(resultPrice.ParticipationPrice, 2, MidpointRounding.AwayFromZero);
                }
            }

            if (player == null)
            {
                return resultPrice;
            }

            //benefactor magic should be here 

            if (applyDiscounts)
            {
                var managerDiscount =
                    player.PlayerDiscounts.FirstOrDefault(x => x.SeasonId == seasonId &&
                                                               x.ClubId == clubId &&
                                                               x.TeamId == teamId &&
                                                               x.DiscountType == (int) PlayerDiscountTypes
                                                                   .ManagerParticipationDiscount);

                if (managerDiscount != null)
                {
                    resultPrice.ParticipationPrice =
                        Math.Max(0, resultPrice.ParticipationPrice - managerDiscount.Amount);
                }
            }

            if (player.NoInsurancePayment)
            {
                resultPrice.PlayerInsurancePrice = 0m;
            }

            return resultPrice;
        }

        private decimal AdjustPriceByDate(decimal price, DateTime? priceStartDate, DateTime? priceEndDate,
            DateTime? playerStartDate)
        {
            if (price <= 0 ||
                !priceStartDate.HasValue ||
                !priceEndDate.HasValue)
            {
                return price;
            }

            var priceTotalDays = (priceEndDate.Value - priceStartDate.Value).Days;
            if (priceTotalDays <= 0)
            {
                return price;
            }

            playerStartDate = playerStartDate > DateTime.MinValue &&
                              playerStartDate >= priceStartDate &&
                              playerStartDate <= priceEndDate
                ? playerStartDate.Value.Date
                : DateTime.Now.Date;

            var totalDaysFromNow = (priceEndDate.Value - playerStartDate.Value).Days;
            if (totalDaysFromNow <= 0)
            {
                return price;
            }

            var pricePerDay = price / priceTotalDays;

            return decimal.Round(pricePerDay * totalDaysFromNow, 2, MidpointRounding.AwayFromZero);
        }

        public LeaguePlayerPrice GetCompetitionDisciplinePrices(int competitionDisciplineId)
        {
            var result = new LeaguePlayerPrice();

            var competitionDiscipline = db.CompetitionDisciplines.Find(competitionDisciplineId);
            if (competitionDiscipline == null)
            {
                return result;
            }

            var competitionLeague = db.Leagues.Find(competitionDiscipline.CompetitionId);
            if (competitionLeague == null)
            {
                return result;
            }

            result.LeagueId = competitionLeague.LeagueId;

            var competitionRegistrationPrice = db.LeaguesPrices
                .FirstOrDefault(x => x.LeagueId == competitionLeague.LeagueId &&
                                     x.PriceType == (int)LeaguePriceType.PlayerRegistrationPrice &&
                                     x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);

            result.RegistrationPrice = competitionRegistrationPrice?.Price ?? 0;
            result.RegistrationPriceCardComProductId = competitionRegistrationPrice?.CardComProductId;

            return result;
        }

        public LeaguePlayerPrice GetPlayerPrices(int playerId, int teamId, int leagueId, int seasonId)
        {
            var player = db.Users.Find(playerId);
            var league = db.Leagues
                .Include(x => x.LeaguesPrices)
                .FirstOrDefault(x => x.LeagueId == leagueId);
            var team = db.Teams
                .Include(x => x.ActivityFormsSubmittedDatas)
                .FirstOrDefault(x => x.TeamId == teamId);
            if (league == null)
            {
                return new LeaguePlayerPrice();
            }

            var teamRegistration = team?.ActivityFormsSubmittedDatas.FirstOrDefault(x =>
                x.LeagueId == leagueId &&
                x.Activity.SeasonId == seasonId &&
                x.Activity.IsAutomatic.HasValue && x.Activity.IsAutomatic.Value &&
                x.Activity.Type == ActivityType.Group);

            var resultPrice = new LeaguePlayerPrice();

            var baseInsurance = league.LeaguesPrices
                .FirstOrDefault(x => x.PriceType == (int?) LeaguePriceType.PlayerInsurancePrice &&
                                     x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);
            var baseInsurancePrice = baseInsurance?.Price;
            resultPrice.InsuranceCardComProductId = baseInsurance?.CardComProductId;

            var baseRegistration = league.LeaguesPrices
                .FirstOrDefault(x => x.PriceType == (int?) LeaguePriceType.PlayerRegistrationPrice &&
                                     x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);
            var baseRegistrationPrice = baseRegistration?.Price;
            resultPrice.RegistrationPriceCardComProductId = baseRegistration?.CardComProductId;

            var baseMembersFee = league.MemberFees
                .FirstOrDefault(x => x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);
            var membersFee = baseMembersFee?.Amount;
            resultPrice.MembersFeeCardComProductId = baseMembersFee?.CardComProductId;

            var baseHandlingFee = league.HandlingFees
                .FirstOrDefault(x => x.StartDate <= DateTime.Now &&
                                     x.EndDate > DateTime.Now);
            var handlingFee = baseHandlingFee?.Amount;
            resultPrice.HandlingFeeCardComProductId = baseHandlingFee?.CardComProductId;

            var benefactor = teamRepo.GetBenefactorByTeamId(teamId);
            if (benefactor == null)
            {
                resultPrice.RegistrationPrice = baseRegistrationPrice ?? 0;
                resultPrice.InsurancePrice = baseInsurancePrice ?? 0;
                resultPrice.MembersFee = membersFee ?? 0;
                resultPrice.HandlingFee = handlingFee ?? 0;
            }
            else
            {
                var pricesByBenefactor =
                    benefactor.PlayersBenefactorPrices.FirstOrDefault(
                        x => x.PlayerId == playerId && x.LeagueId == leagueId && x.SeasonId == seasonId);

                if (pricesByBenefactor != null)
                {
                    resultPrice.RegistrationPrice = pricesByBenefactor.RegistrationPrice;
                    resultPrice.InsurancePrice = pricesByBenefactor.InsurancePrice;

                    if (!pricesByBenefactor.League.LeaguesPrices.Any(
                        x => x.PriceType == (int?) LeaguePriceType.PlayerRegistrationPrice &&
                             x.StartDate <= DateTime.Now &&
                             x.EndDate > DateTime.Now))
                    {
                        resultPrice.RegistrationPrice = baseRegistrationPrice ?? 0;
                    }
                    if (!pricesByBenefactor.League.LeaguesPrices.Any(
                        x => x.PriceType == (int?) LeaguePriceType.PlayerInsurancePrice &&
                             x.StartDate <= DateTime.Now &&
                             x.EndDate > DateTime.Now))
                    {
                        resultPrice.InsurancePrice = baseInsurancePrice ?? 0;
                    }
                }
                else
                {
                    resultPrice.RegistrationPrice = baseRegistrationPrice ?? 0;
                    resultPrice.InsurancePrice = baseInsurancePrice ?? 0;
                }

                resultPrice.MembersFee = membersFee ?? 0;
                resultPrice.HandlingFee = handlingFee ?? 0;
            }

            if (player != null)
            {
                var managerDiscount =
                    player.PlayerDiscounts.FirstOrDefault(x => x.SeasonId == seasonId &&
                                                               x.LeagueId == league.LeagueId &&
                                                               x.TeamId == teamId &&
                                                               x.DiscountType == (int)PlayerDiscountTypes.ManagerRegistrationDiscount);
                if (managerDiscount != null)
                {
                    resultPrice.RegistrationPrice = Math.Max(0, resultPrice.RegistrationPrice - managerDiscount.Amount);
                }

                if (player.NoInsurancePayment)
                {
                    resultPrice.InsurancePrice = 0m;
                }
            }

            if (teamRegistration?.IsActive == true && teamRegistration.SelfInsurance)
            {
                resultPrice.InsurancePrice = 0;
            }

            return resultPrice;
        }
    }
}