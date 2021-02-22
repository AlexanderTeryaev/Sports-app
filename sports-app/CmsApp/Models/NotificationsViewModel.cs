using System.Collections.Generic;
using DataService;
using AppModel;

namespace CmsApp.Models
{
    public class NotificationsViewModel
    {
        public NotificationsViewModel()
        {
            Notifications = new List<NotesMessage>();
        }

        public int EntityId { get; set; }

        public LogicaName RelevantEntityLogicalName { get; set; }

        public List<NotesMessage> Notifications { get; set; }
        public int? SeasonId { get; set; }
        public string FilePath { get; set; }

        //for league notification for selected club managers
        public Dictionary<int, string> UserTeamNames { get; set; }
    }
}