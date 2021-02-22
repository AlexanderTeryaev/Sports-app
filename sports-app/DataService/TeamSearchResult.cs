using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataService
{
    public class TeamSearchResult
    {
        public int Id { get; set; }

        public AppModel.Team Team { get; set; }
    }
}
