using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AppModel;
using AutoMapper;

namespace DataService
{
    public class PlayerAchievementsRepo : BaseRepo
    {
        public PlayerAchievementsRepo()
        {
        }

        public PlayerAchievementsRepo(DataEntities db) : base(db)
        {
        }

        public IEnumerable<PlayerAchievement> GetCollection(Expression<Func<PlayerAchievement, bool>> expression)
        {
            return db.PlayerAchievements.Where(expression);
        }

        public void Add(IEnumerable<PlayerAchievement> achievements)
        {
            db.PlayerAchievements.AddRange(achievements);
            db.SaveChanges();
        }

        public List<TeamPlayerItem> GetAllPlayersInRanks(List<int> ranksIds)
        {
            var listOfPlayers = new List<TeamPlayerItem>();
            foreach (var rankId in ranksIds)
            {
                var ranks = db.PlayerAchievements.Where(pa => pa.RankId == rankId).ToList();
                foreach (var rank in ranks)
                {
                    var teamPlayersDb = db.TeamsPlayers.Where(t => t.UserId == rank.PlayerId).ToList();
                    Mapper.Initialize(cfg => cfg.CreateMap<TeamsPlayer, TeamPlayerItem>());
                    listOfPlayers.AddRange(Mapper.Map<List<TeamsPlayer>, List<TeamPlayerItem>>(teamPlayersDb));
                }
            }
            return listOfPlayers;
        }
    }
}
