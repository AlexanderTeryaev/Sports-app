using AppModel;
using CmsApp.Models;
using DataService;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CmsApp.Helpers
{
    public class RefereeService
    {
        private int leagueId;
        private int cycleId;
        private List<string> selectedReferees;
        private UsersRepo usersRepo;
        private LeagueRepo leagueRepo;
        private GamesRepo gamesRepo;

        public RefereeService(int cycleId, int leagueId, List<string> selectedReferees)
        {
            this.leagueId = leagueId;
            this.selectedReferees = selectedReferees;
            this.cycleId = cycleId;
            this.leagueRepo = new LeagueRepo();
            this.usersRepo = new UsersRepo();
            this.gamesRepo = new GamesRepo();
        }
        public RefereeService(int cycleId, int leagueId)
        {
            this.leagueId = leagueId;
            this.cycleId = cycleId;
            if (gamesRepo == null)
            {
                this.gamesRepo = new GamesRepo();
            }
            if (this.leagueRepo == null)
            {
                this.leagueRepo = new LeagueRepo();
            }
            if (this.usersRepo == null)
            {
                this.usersRepo = new UsersRepo();
            }
        }
        
        public void UseSameInstanceForNewData(int cycleId, int leagueId)
        {
            this.leagueId = leagueId;
            this.cycleId = cycleId;
        }


        public Dictionary<int, User> GetRefereesInstance(int leagueId)
        {
            var league = leagueRepo.GetById(leagueId);
            var unionId = league.UnionId;
            var clubId = league.ClubId;

            Dictionary<int, User> referees = new Dictionary<int, User>();
            if (unionId.HasValue)
            {
                referees = usersRepo.GetUnionAndLeagueReferees(unionId.Value, leagueId)
                    .Union(usersRepo.GetUnionAndLeagueSpectators(unionId.Value, leagueId))
                    .ToDictionary(u => u.UserId, u => u);
            }
            else if (clubId.HasValue)
            {
                referees = usersRepo.GetClubAndLeagueReferees(clubId.Value, leagueId)
                    .Union(usersRepo.GetClubAndLeagueSpectators(clubId.Value, leagueId))
                    .ToDictionary(u => u.UserId, u => u);
            }

            return referees;
        }

        public Referees AddReferee()
        {
            try
            {
                if (selectedReferees != null)
                    selectedReferees.Add("");
                else
                {
                    selectedReferees = new List<string>();
                    selectedReferees.Add("");
                }
                var refereeIdsString = String.Join(",", selectedReferees.ToArray());
                gamesRepo.UpdateRefereesInCycle(cycleId, refereeIdsString);
                gamesRepo.UpdateRefereesInCycle(cycleId, refereeIdsString);
                var referee = new Referees
                {
                    CycleId = cycleId,
                    RefereeIds = selectedReferees,
                    RefereesItems = GetRefereesInstance(leagueId).Values,
                    LeagueId = leagueId,
                    RefereesNames = GetRefereesNamesString(cycleId) ?? Messages.NoReferees
                };
                return referee;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Referees SaveReferees()
        {
            try
            {
                string refereeIdsString;
                if (selectedReferees.Count != 0)
                {
                    refereeIdsString = String.Join(",", selectedReferees.ToArray());
                    gamesRepo.UpdateRefereesInCycle(cycleId, refereeIdsString);
                }
                var referee = new Referees
                {
                    CycleId = cycleId,
                    LeagueId = leagueId,
                    RefereeIds = selectedReferees,
                    RefereesItems = GetRefereesInstance(leagueId).Values,
                    RefereesNames = GetRefereesNamesString(cycleId) ?? Messages.NoReferees
                };
                return referee;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public string GetRefereesNamesString(int cycleId)
        {
            var listOfRefereesNames = gamesRepo.GetRefereesNames(cycleId);
            if (listOfRefereesNames == null)
                return null;
            var builder = new StringBuilder();
            for (int i = 0; i < listOfRefereesNames.Count; i++)
            {
                var punktuation = (i == (listOfRefereesNames.Count - 1)) ? "." : " ,";
                if (i == 0)
                {
                    builder.Append($"{Messages.MainReferee}: {listOfRefereesNames[i]}");
                    builder.Append(punktuation);
                }
                else
                {
                    builder.Append($"{Messages.Referee} #{i + 1}: {listOfRefereesNames[i]}");
                    builder.Append(punktuation);
                }
            }
            return builder.ToString();
        }

        public string GetMainRefereeName(int cycleId)
        {
            var refereesNames = gamesRepo.GetRefereesNames(cycleId);

            string mainRefereeName = Messages.NoReferees;
            if(refereesNames != null)
            {
                if (refereesNames.Count == 1)
                    mainRefereeName = refereesNames[0];
                else if (refereesNames.Count > 1) {
                    StringBuilder sb = new StringBuilder(refereesNames[0]);
                    for (int i = 1; i < refereesNames.Count(); i++)
                    {
                        var refereeName = refereesNames[i];
                        sb.AppendFormat(", {0}", refereeName);
                    }
                    mainRefereeName = sb.ToString();
                }
                    
            }
            return mainRefereeName;
        }

        public Referees DeleteReferee(string refereeToDelete, int refereeOrder)
        {
            var currentRefereeIds = gamesRepo.GetRefereesIds(cycleId)?.ToList();
            if (currentRefereeIds != null)
            {
                if (String.IsNullOrEmpty(refereeToDelete))
                    currentRefereeIds.RemoveAt(refereeOrder);
                else if (currentRefereeIds == null)
                    gamesRepo.SetRefereesToNull(cycleId);
                else
                    currentRefereeIds.Remove(refereeToDelete);

                var finalRefereeIds = String.Join(",", currentRefereeIds.ToArray());
                gamesRepo.UpdateRefereesInCycle(cycleId, finalRefereeIds);
            }
            var referee = new Referees
            {
                CycleId = cycleId,
                LeagueId = leagueId,
                RefereeIds = currentRefereeIds,
                RefereesItems = GetRefereesInstance(leagueId).Values,
                RefereesNames = GetRefereesNamesString(cycleId) ?? Messages.NoReferees
            };
            return referee;
        }
    }
}