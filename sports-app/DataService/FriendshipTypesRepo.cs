using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppModel;

namespace DataService
{
    public class FriendshipTypesRepo : BaseRepo
    {
        public FriendshipTypesRepo() : base()
        {

        }

        public FriendshipTypesRepo(DataEntities db) : base(db)
        {
        }

        public List<FriendshipsType> GetBySection(int sectionId, int unionId)
        {
            return db.FriendshipsTypes.Where(d => d.Union.SectionId == sectionId
                                             && d.UnionId == unionId
                                             && !d.IsArchive)
                .ToList();
        }

        public FriendshipsType GetById(int id)
        {
            return db.FriendshipsTypes.Find(id);
        }

        public void UpdateFriendshipType(int id, string name, int? hierarchy)
        {
            var friendshipType = db.FriendshipsTypes.FirstOrDefault(d => d.FriendshipsTypesId == id);
            if (friendshipType != null)
            {
                friendshipType.Name = name;
                friendshipType.Hierarchy = hierarchy;
                db.SaveChanges();
            }
        }

        public FriendshipsType GetByIdWithUnion(int id)
        {
            return db.FriendshipsTypes
                .Include(d => d.Union)
                .Single(d => d.FriendshipsTypesId == id);
        }

        public void Create(FriendshipsType friendshipsType)
        {
            db.FriendshipsTypes.Add(friendshipsType);
            db.SaveChanges();
        }

        public List<FriendshipsType> GetAllByUnionId(int unionId)
        {
            return db.FriendshipsTypes.Where(d => d.UnionId == unionId && d.IsArchive == false).ToList();
        }
    }
}
