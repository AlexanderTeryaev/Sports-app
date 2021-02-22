using System;
using System.Collections.Generic;
using CmsApp.Models;
using DataService;
using System.Linq;
using System.Web.Mvc;

namespace CmsApp.Helpers
{
    public class AthletesFilter
    {
        private PlayerAchievementsRepo playerAchievementsRepo;
        private TeamsRepo teamRepo;
        private AthletesModel settings;
        private const double WEIGHTCOEF = 2.2046;
        public AthletesFilter(AthletesModel settings)
        {
            this.settings = settings;
            playerAchievementsRepo = new PlayerAchievementsRepo();
            teamRepo = new TeamsRepo();
        }

        public List<TeamPlayerItem> FilterAthletes(List<TeamPlayerItem> athletes)
        {
            if (settings == null)
                return null;
            if (settings.IsAgesEnabled)
                athletes = FilterByAges(athletes.Where(a => a.Birthday.HasValue).ToList());
            if (settings.IsWeightEnabled)
                athletes = FilterByWeight(athletes.Where(a => a.Weight.HasValue).ToList());
            if (settings.IsRankedEnabled)
                athletes = FilterByRank(athletes);
            return athletes;
        }

        public List<TeamPlayerItem> FilterByAges(List<TeamPlayerItem> athletes)
        {
            var startAge = settings.AgeStart;
            var endAge = settings.AgeEnd;
            var filtredAthletes = new List<TeamPlayerItem>();
            if (!startAge.HasValue && !endAge.HasValue)
                return athletes;
            else if (!startAge.HasValue)
                filtredAthletes = athletes.Where(a => a.Birthday.Value <= endAge).ToList();
            else if (!endAge.HasValue)
                filtredAthletes = athletes.Where(a => a.Birthday.Value >= startAge).ToList();
            else if (startAge.HasValue && endAge.HasValue)
                filtredAthletes = athletes.Where(a => a.Birthday >= startAge && a.Birthday <= endAge).ToList();
            else throw new ArgumentNullException("Wrong start age/ end age values");
            return filtredAthletes;
        }

        public List<TeamPlayerItem> FilterByWeight(List<TeamPlayerItem> athletes)
        {
            var unit = settings.WeightType;
            var weightFrom = settings.WeightFrom;
            var weightTo = settings.WeightTo;

            if (String.IsNullOrEmpty(unit) || (!weightFrom.HasValue && !weightTo.HasValue))
                return athletes;

            var unitFiltredAthletes = ConvertToNecessaryWeightUnit(unit, athletes);
            var filtredAthletes = new List<TeamPlayerItem>();

            if (!weightFrom.HasValue)
                filtredAthletes = unitFiltredAthletes.Where(a => a.Weight <= weightTo).ToList();
            else if (!weightTo.HasValue)
                filtredAthletes = unitFiltredAthletes.Where(a => a.Weight >= weightFrom).ToList();
            else
                filtredAthletes = unitFiltredAthletes.Where(a => a.Weight >= weightFrom && a.Weight <= weightTo)
                    .ToList();
            return filtredAthletes;
        }

        private List<TeamPlayerItem> ConvertToNecessaryWeightUnit(string unit, List<TeamPlayerItem> athletes)
        {
            var unitFiltredAthletes = new List<TeamPlayerItem>();
            var athletesWithKgValue = athletes.Where(a => a.WeightUnits == "Kg").ToList();
            var athletesWithLbValue = athletes.Where(a => a.WeightUnits == "Lb").ToList();

            switch (unit)
            {
                case "Kg":
                    unitFiltredAthletes.AddRange(athletesWithKgValue);
                    unitFiltredAthletes.AddRange(athletesWithLbValue
                        .Select(a =>
                        {
                            a.Weight = Convert.ToInt32(a.Weight / WEIGHTCOEF);
                            a.WeightUnits = "Kg"; return a;
                        })
                        .ToList());
                    break;
                case "Lb":
                    unitFiltredAthletes.AddRange(athletesWithLbValue);
                    unitFiltredAthletes.AddRange(athletesWithKgValue
                        .Select(a =>
                        {
                            a.Weight = Convert.ToInt32(a.Weight * WEIGHTCOEF);
                            a.WeightUnits = "Lb"; return a;
                        })
                        .ToList());
                    break;
            }

            return unitFiltredAthletes;
        }

        public List<TeamPlayerItem> FilterByRank(List<TeamPlayerItem> athletes)
        {
            var settedPlayerRanks = settings.SelectedRanks;
            if (settedPlayerRanks[0] == 0)
                return athletes;
            var filtredByRanks = playerAchievementsRepo.GetAllPlayersInRanks(settedPlayerRanks);
            var filtredPlayers = new List<TeamPlayerItem>();
            foreach (var athlet in athletes)
            {
                foreach (var selectedAthlet in filtredByRanks)
                {
                    if (athlet.Id == selectedAthlet.Id)
                        filtredPlayers.Add(athlet);
                }
            }
            return filtredPlayers;
        }

        public List<SelectListItem> GetAthletesFinalData(List<TeamPlayerItem> athletes, List<TeamPlayerItem> includedAthletes)
        {
            var filtredAthletesList = new List<SelectListItem>();
            if (athletes != null)
            {
                foreach (var player in athletes.OrderBy(c => c.FullName).ToList())
                {
                    foreach (var includedAthlete in includedAthletes)
                    {
                        if (includedAthlete.Id == player.Id)
                        {
                            athletes.Remove(player);
                        }
                    }
                }
                foreach (var player in athletes.OrderBy(c => c.FullName).ToList())
                {
                    filtredAthletesList.Add(new SelectListItem
                    {
                        Value = player.Id.ToString(),
                        Text = $"{player.FullName} ({teamRepo.GetTeamNameById(player.TeamId)})"
                    });
                }
            }
            return filtredAthletesList;
        }
    }
}