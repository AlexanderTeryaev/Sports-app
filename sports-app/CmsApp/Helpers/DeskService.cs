using AppModel;
using CmsApp.Models;
using DataService;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace CmsApp.Helpers
{
    public class DeskService
    {
        private int leagueId;
        private int cycleId;
        private List<string> selectedDesks;
        private UsersRepo usersRepo;
        private LeagueRepo leagueRepo;
        private GamesRepo gamesRepo;

        public DeskService(int cycleId, int leagueId, List<string> selectedDesks)
        {
            this.leagueId = leagueId;
            this.selectedDesks = selectedDesks;
            this.cycleId = cycleId;
            this.leagueRepo = new LeagueRepo();
            this.usersRepo = new UsersRepo();
            this.gamesRepo = new GamesRepo();
        }
        public DeskService(int cycleId, int leagueId)
        {
            this.leagueId = leagueId;
            this.cycleId = cycleId;
            this.gamesRepo = new GamesRepo();
            this.leagueRepo = new LeagueRepo();
            this.usersRepo = new UsersRepo();
        }

        public Dictionary<int, User> GetDeskInstance(int leagueId)
        {
            var league = leagueRepo.GetById(leagueId);
            var unionId = league.UnionId;
            var clubId = league.ClubId;

            Dictionary<int, User> result = new Dictionary<int, User>();

            if (unionId.HasValue)
            {
                var desks = usersRepo.GetUnionAndLeageDesks(unionId.Value, leagueId)
                    .ToDictionary(u => u.UserId, u => u);
                var referees = usersRepo.GetUnionAndLeagueReferees(unionId.Value, leagueId)
                    .ToDictionary(u => u.UserId, u => u);
                var spectators = usersRepo.GetUnionAndLeagueSpectators(unionId.Value, leagueId)
                    .ToDictionary(u => u.UserId, u => u);

                result = desks.Union(referees).Union(spectators).ToDictionary(k => k.Key, v => v.Value);
            }
            else if(clubId.HasValue)
            {
                result = usersRepo.GetClubAndLeagueDesks(clubId.Value, leagueId)
                    .ToDictionary(u => u.UserId, u => u);
            }

            return result;
        }

        public Desks AddDesk()
        {
            try
            {
                if (selectedDesks != null)
                    selectedDesks.Add("");
                else
                {
                    selectedDesks = new List<string>();
                    selectedDesks.Add("");
                }
                var refereeIdsString = String.Join(",", selectedDesks.ToArray());
                gamesRepo.UpdateDesksInCycle(cycleId, refereeIdsString);
                var desk = new Desks
                {
                    CycleId = cycleId,
                    DesksIds = selectedDesks,
                    DesksItems = GetDeskInstance(leagueId).Values,
                    LeagueId = leagueId,
                    DesksNames = GetDeskNamesString(cycleId) ?? Messages.NoDesks
                };
                return desk;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public Desks SaveDesks()
        {
            try
            {
                string deskIdsString;
                if (selectedDesks.Count != 0)
                {
                    deskIdsString = String.Join(",", selectedDesks.ToArray());
                    gamesRepo.UpdateDesksInCycle(cycleId, deskIdsString);
                }
                var desk = new Desks
                {
                    CycleId = cycleId,
                    LeagueId = leagueId,
                    DesksIds = selectedDesks,
                    DesksItems = GetDeskInstance(leagueId).Values,
                    DesksNames = GetDeskNamesString(cycleId) ?? Messages.NoDesks
                };
                return desk;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public string GetDeskNamesString(int cycleId)
        {
            var listOfDesksNames = gamesRepo.GetDeskNames(cycleId);
            if (listOfDesksNames == null)
                return null;
            var builder = new StringBuilder();
            for (int i = 0; i < listOfDesksNames.Count; i++)
            {
                var punktuation = i == listOfDesksNames.Count - 1 ? "." : " ,";
                if (i == 0)
                {
                    builder.Append($"{Messages.MainDesk}: {listOfDesksNames[i]}");
                    builder.Append(punktuation);
                }
                else
                {
                    builder.Append($"{Messages.Desk} #{i + 1}: {listOfDesksNames[i]}");
                    builder.Append(punktuation);
                }
            }
            return builder.ToString();
        }

        public string GetMainDeskName(List<string> deskIds, int cycleId)
        {
            var deskNames = gamesRepo.GetDeskNames(cycleId);

            string mainDeskName = Messages.NoDesks;
            if (deskNames != null)
            {
                if (deskNames.Count == 1)
                    mainDeskName = deskNames[0];
                else if (deskNames.Count > 1)
                    mainDeskName = $"{deskNames[0]}...";
            }
            return mainDeskName;
        }

        public Desks DeleteDesk(string deskToDelete, int deskOrder)
        {
            var currentDeskIds = gamesRepo.GetDesksIds(cycleId)?.ToList();
            if (currentDeskIds != null)
            {
                if (String.IsNullOrEmpty(deskToDelete))
                    currentDeskIds.RemoveAt(deskOrder);
                else if (currentDeskIds == null)
                    gamesRepo.SetDescsToNull(cycleId);
                else
                    currentDeskIds.Remove(deskToDelete);

                var finalDeskIds = String.Join(",", currentDeskIds.ToArray());
                gamesRepo.UpdateDesksInCycle(cycleId, finalDeskIds);
            }
            var desks = new Desks
            {
                CycleId = cycleId,
                LeagueId = leagueId,
                DesksIds = currentDeskIds,
                DesksItems = GetDeskInstance(leagueId).Values,
                DesksNames = GetDeskNamesString(cycleId) ?? Messages.NoDesks
            };
            return desks;
        }
    }
}