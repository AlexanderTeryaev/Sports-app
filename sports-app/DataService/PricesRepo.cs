using System;
using System.Collections.Generic;
using System.Linq;
using AppModel;

namespace DataService
{
    public class PricesRepo : BaseRepo
    {
        public PricesRepo() : base()
        {

        }

        public PricesRepo(DataEntities db) : base(db)
        {
        }

        public List<ChipPrice> GetChipPricesBySeasonAndUnion(int seasonId, int unionId)
        {
            return db.ChipPrices
                .Where(cp => cp.SeasonId == seasonId &&
                             cp.UnionId == unionId)
                .ToList();
        }

        public ChipPrice GetChipPriceById(int id)
        {
            return db.ChipPrices.Find(id);
        }

        public void UpdateChipPrice(int id, int? fromAge, int? toAge, int? price)
        {
            var chipPrice = db.ChipPrices.FirstOrDefault(d => d.ChipId == id);
            if (chipPrice != null)
            {
                chipPrice.FromAge = fromAge;
                chipPrice.ToAge = toAge;
                chipPrice.Price = price;
                db.SaveChanges();
            }
        }

        public void CreateChipPrice(ChipPrice chipPrice)
        {
            db.ChipPrices.Add(chipPrice);
            db.SaveChanges();
        }

        public List<FriendshipPrice> GetFriendshipPricesBySeasonAndUnion(int seasonId, int unionId)
        {
            return db.FriendshipPrices.Where(d => d.SeasonId == seasonId && d.UnionId == unionId).ToList();
        }

        public void DeleteChipPrice(int chipId)
        {
            var chipPrice = db.ChipPrices.FirstOrDefault(a => a.ChipId == chipId);

            if (chipPrice != null)
            {
                db.ChipPrices.Remove(chipPrice);
                db.SaveChanges();
            }
        }

        public FriendshipPrice GetFriendshipPriceById(int id)
        {
            return db.FriendshipPrices.Find(id);
        }

        public void UpdateFriendshipPrice(int friendshipPricesId, int? fromAge, int? toAge, int? genderId, int? friendshipsTypeId, int? price, int? friendshipPriceType, int? priceForNew)
        {
            var friendshipPrice = db.FriendshipPrices.FirstOrDefault(d => d.FriendshipPricesId == friendshipPricesId);
            if (friendshipPrice != null)
            {
                friendshipPrice.FromAge = fromAge;
                friendshipPrice.ToAge = toAge;
                friendshipPrice.GenderId = genderId;
                friendshipPrice.FriendshipsTypeId = friendshipsTypeId;
                friendshipPrice.Price = price;
                friendshipPrice.FriendshipPriceType = friendshipPriceType;
                friendshipPrice.PriceForNew = priceForNew;
                db.SaveChanges();
            }
        }

        public void DeleteFriendshipPrice(int friendshipId)
        {
            var friendshipPrice = db.FriendshipPrices.FirstOrDefault(a => a.FriendshipPricesId == friendshipId);

            if (friendshipPrice != null)
            {
                db.FriendshipPrices.Remove(friendshipPrice);
                db.SaveChanges();
            }
        }

        public void CreateFriendshipPrice(FriendshipPrice friendshipPrice)
        {
            db.FriendshipPrices.Add(friendshipPrice);
            db.SaveChanges();
        }
    }
}
