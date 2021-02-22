using System.Collections.Generic;

namespace DataService.DTO
{
    public class PlayerToClubModalDataDTO
    {
        public string ClubManagerName { get; set; }
        public List<PlayerToClubInfo> PlayersToClubs { get; set; }

        public PlayerToClubModalDataDTO()
        {
            PlayersToClubs = new List<PlayerToClubInfo>();
        }

    }

    public class PlayerToClubInfo
    {
        public string PlayerName { get; set; }
        public string NewClubName { get; set; }
    }
}
