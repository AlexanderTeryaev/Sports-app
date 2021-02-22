using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataService.Utils
{
    public class MaximumRequiredPerClubException : Exception
    {

        
        private Dictionary<string, int> CompetitionMaxMap = new Dictionary<string, int>();

        public void AddError(string CompetitionName, int maxAllowed)
        {
            CompetitionMaxMap.Add(CompetitionName, maxAllowed);
        }

        public Dictionary<string, int> GetClubsMaxErrorMap() {
            return CompetitionMaxMap;
        }

        public bool HasErrors() {
            return CompetitionMaxMap.Count() > 0 ? true : false;
        }
    }
}
