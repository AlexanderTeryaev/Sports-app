using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace DataService.Utils
{
    public class BicycleFriendshipPriceHelper
    {
        public class BicycleFriendshipPrice
        {
            public BicycleFriendshipPrice(decimal friendshipPrice, decimal chipPrice, decimal uciPrice)
            {
                FriendshipPrice = friendshipPrice;
                ChipPrice = chipPrice;
                UciPrice = uciPrice;
            }

            public decimal FriendshipPrice { get; }
            public decimal ChipPrice { get; }
            public decimal UciPrice { get; }

            public decimal Total
            {
                get { return FriendshipPrice + ChipPrice + UciPrice; }
            }
        }

        public BicycleFriendshipPrice GetFriendshipPrice(TeamsPlayer teamPlayer)
        {
            return GetFriendshipPrice(new List<TeamsPlayer> { teamPlayer }).FirstOrDefault().Value;
        }

        public Dictionary<TeamsPlayer, BicycleFriendshipPrice> GetFriendshipPrice(List<TeamsPlayer> teamPlayers)
        {
            var result = new Dictionary<TeamsPlayer, BicycleFriendshipPrice>();

            if (teamPlayers?.Any() != true)
            {
                throw new Exception("No team players were provided");
            }

            foreach (var teamPlayer in teamPlayers)
            {
                int seasonYear;
                if (!int.TryParse(teamPlayer.Season?.Name, out seasonYear))
                {
                    throw new Exception($"Unable to get year of season '{teamPlayer.Season?.Name}', season id: '{teamPlayer.SeasonId}'");
                }

                var playerSeasonAge = teamPlayer.User?.BirthDay == null
                    ? 0
                    : seasonYear - teamPlayer.User.BirthDay?.Year;

                var unionFriendshipPrices = teamPlayer
                    .FriendshipsType
                    ?.FriendshipPrices
                    ?.FirstOrDefault(x => x.FriendshipPriceType == teamPlayer.FriendshipPriceType &&
                                          x.FromAge <= playerSeasonAge &&
                                          x.ToAge >= playerSeasonAge &&
                                          (x.GenderId == teamPlayer.User.GenderId || x.GenderId == 3));

                var friendshipPrice = (teamPlayer.IsNewPlayerInUnion
                                          ? unionFriendshipPrices?.PriceForNew ?? unionFriendshipPrices?.Price
                                          : unionFriendshipPrices?.Price)
                                      ?? 0;
                var chipPrice = 0;
                var uciPrice = 0;

                if (teamPlayer.User?.PaymentForChipNumber == true)
                {
                    var pricesRepo = new PricesRepo();

                    var chipPrices = pricesRepo.GetChipPricesBySeasonAndUnion(teamPlayer.SeasonId ?? 0, teamPlayer.Season?.UnionId ?? 0);
                    chipPrice = chipPrices
                                    .FirstOrDefault(x => x.FromAge <= playerSeasonAge &&
                                                         x.ToAge >= playerSeasonAge)
                                    ?.Price ?? 0;
                }

                if (teamPlayer.User?.PaymentForUciId == true)
                {
                    uciPrice = 50;
                }

                result.Add(teamPlayer, new BicycleFriendshipPrice(friendshipPrice, chipPrice, uciPrice));
            }

            return result;
        }
    }
}
