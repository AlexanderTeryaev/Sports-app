using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AppModel;
using DataService;
using WebApi.Models;

namespace WebApi.Services
{
    public static class SectionService
    {   
        internal static List<SectionViewModel> GetAll(int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var ss = (from section in db.Sections
                          where section.Clubs.Count > 0
                          select new SectionViewModel
                          {
                              SectionId = section.SectionId,
                              LangId = section.LangId,
                              Name = section.Name,
                              Alias = section.Alias,
                              IsIndividual = section.IsIndividual,
                              Clubs = (from club in section.Clubs
                                       where club.IsArchive == false
                                       select new ClubTeamInfoViewModel
                                       {
                                           Id = club.ClubId,
                                           Logo = club.Logo,
                                           Title = club.Name,
                                           ParentClubId = club.ParentClubId,
                                           SectionName = section.Name,
                                           Teams = (from team in club.ClubTeams
                                                    where team.Team.IsArchive == false
                                                    select new ClubTeamViewModel
                                                    {
                                                        TeamId = team.Team.TeamId,
                                                        Title = team.Team.Title,
                                                        Logo = team.Team.Logo,
                                                        SeasonId = team.SeasonId,
                                                        ParentId = club.ClubId,
                                                        ParentName = club.Name,
                                                        FanNumber = team.Team.TeamsFans.Count,
                                                        PlayerNumber = team.Team.TeamsPlayers.Count,
                                                        IsSchoolTeam = false
                                                    }).Union(from School in club.Schools
                                                             from team in School.SchoolTeams
                                                             where team.Team.IsArchive == false && School.IsCamp == false
                                                             select new ClubTeamViewModel
                                                             {
                                                                 TeamId = team.Team.TeamId,
                                                                 Title = team.Team.Title,
                                                                 Logo = team.Team.Logo,
                                                                 SeasonId = School.SeasonId,
                                                                 ParentId = club.ClubId,
                                                                 ParentName = club.Name,
                                                                 FanNumber = team.Team.TeamsFans.Count,
                                                                 PlayerNumber = team.Team.TeamsPlayers.Count,
                                                                 IsSchoolTeam = true
                                                             }).ToList()
                                       }).ToList()
                          }).ToList();

                SeasonsRepo _seasonRepo = new SeasonsRepo();
                for (int i = ss.Count - 1; i >= 0; i--)
                {
                    var section = ss[i];
                    if (section.Clubs.Count == 0)
                    {
                        ss.Remove(section);
                        continue;
                    }
                    if (section.Alias != "multi-sport")
                    {
                        foreach (var club in section.Clubs)
                        {
                            club.TotalFans = 0;
                            int clubSeasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId(club.Id);
                            for (int ii = club.Teams.Count - 1; ii >= 0; ii--)
                            {
                                var team = club.Teams[ii];
                                int? leagueid = db.LeagueTeams.Where(x => x.TeamId == team.TeamId).FirstOrDefault()?.LeagueId;
                                team.LeagueId = leagueid ?? 0;
                                if (team.SeasonId != clubSeasonId)
                                {
                                    club.Teams.Remove(team);
                                    continue;
                                }
                                else
                                {
                                    var teamsDetail = db.TeamsDetails.FirstOrDefault(td => td.TeamId == team.TeamId && td.SeasonId == clubSeasonId);
                                    if(teamsDetail != null)
                                        team.Title = teamsDetail.TeamName;
                                }

                                club.TotalFans += team.FanNumber;
                            }
                            club.TotalTeams = club.Teams.Count;
                        }
                    }
                    else
                    {
                        foreach (var club in section.Clubs)
                        {
                            club.TotalFans = 0;
                            if (club.ParentClubId > 0)
                                continue;
                            else
                            {
                                int clubSeasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId(club.Id);
                                club.Departments = (from cc in section.Clubs
                                                    where cc.ParentClubId == club.Id
                                                    select new DepartmentInfoViewModel
                                                    {
                                                        Id = cc.Id,
                                                        Logo = cc.Logo,
                                                        Title = cc.Title,
                                                        SectionName = section.Name,
                                                        Teams = cc.Teams
                                                    }).ToList();
                                foreach (var department in club.Departments)
                                {
                                    department.TotalFans = 0;
                                    for (int ii = club.Teams.Count - 1; ii >= 0; ii--)
                                    {
                                        var team = club.Teams[ii];
                                        int? leagueid = db.LeagueTeams.Where(x => x.TeamId == team.TeamId).FirstOrDefault()?.LeagueId;
                                        team.LeagueId = leagueid ?? 0;
                                        if (team.SeasonId != clubSeasonId)
                                        {
                                            club.Teams.Remove(team);
                                            continue;
                                        }
                                        else
                                        {
                                            var teamsDetail = db.TeamsDetails.FirstOrDefault(td => td.TeamId == team.TeamId && td.SeasonId == clubSeasonId);
                                            if (teamsDetail != null)
                                                team.Title = teamsDetail.TeamName;
                                        }
                                        department.TotalFans += team.FanNumber;
                                    }
                                    department.TotalTeams = department.Teams.Count;
                                    club.TotalFans += department.TotalFans;
                                    club.TotalTeams += department.TotalTeams;
                                }

                                club.Teams.Clear();
                            }
                        }
                        for (int iii = section.Clubs.Count - 1; iii > 0; iii--)
                        {
                            if (section.Clubs[iii].ParentClubId > 0)
                                section.Clubs.RemoveAt(iii);
                        }
                    }
                }
                return ss;
            }
        }
        internal static List<SectionViewModel> GetAllBySeason(int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var ss = (from section in db.Sections
                          where section.Clubs.Count > 0
                          select new SectionViewModel
                          {
                              SectionId = section.SectionId,
                              LangId = section.LangId,
                              Name = section.Name,
                              Alias = section.Alias,
                              IsIndividual = section.IsIndividual,
                              Clubs = (from club in section.Clubs
                                       where club.IsArchive == false
                                       select new ClubTeamInfoViewModel
                                       {
                                           Id = club.ClubId,
                                           Logo = club.Logo,
                                           Title = club.Name,
                                           ParentClubId = club.ParentClubId,
                                           SectionName = section.Name,
                                           Teams = (from team in club.ClubTeams
                                                    where team.Team.IsArchive == false
                                                    select new ClubTeamViewModel
                                                    {
                                                        TeamId = team.Team.TeamId,
                                                        Title = team.Team.Title,
                                                        Logo = team.Team.Logo,
                                                        SeasonId = team.SeasonId,
                                                        ParentId = club.ClubId,
                                                        ParentName = club.Name,
                                                        FanNumber = team.Team.TeamsFans.Count,
                                                        PlayerNumber = team.Team.TeamsPlayers.Count,
                                                        IsSchoolTeam = false
                                                    }).Union(from School in club.Schools
                                                             from team in School.SchoolTeams
                                                             let j = team.Team.LeagueTeams.FirstOrDefault().LeagueId
                                                              where team.Team.IsArchive == false && School.IsCamp == false
                                                              select new ClubTeamViewModel
                                                              {
                                                                  TeamId = team.Team.TeamId,
                                                                  Title = team.Team.Title,
                                                                  Logo = team.Team.Logo,
                                                                  SeasonId = School.SeasonId,
                                                                  ParentId = club.ClubId,
                                                                  //LeagueId = j,
                                                                  ParentName = club.Name,
                                                                  FanNumber = team.Team.TeamsFans.Count,
                                                                  PlayerNumber = team.Team.TeamsPlayers.Count,
                                                                  IsSchoolTeam = true
                                                              }).ToList()
                                       }).ToList()
                          }).ToList();
                SeasonsRepo _seasonRepo = new SeasonsRepo();
                for (int i = ss.Count - 1; i >= 0; i--)
                {
                    var section = ss[i];
                    if (section.Clubs.Count == 0)
                    {
                        ss.Remove(section);
                        continue;
                    }
                    if (section.Alias != "multi-sport")
                    {
                        foreach (var club in section.Clubs)
                        {
                            club.TotalFans = 0;
                            int clubSeasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId(club.Id);
                            for (int ii = club.Teams.Count - 1; ii >= 0; ii--)
                            {
                                var team = club.Teams[ii];
                                int? leagueid = db.LeagueTeams.Where(x => x.TeamId == team.TeamId).FirstOrDefault()?.LeagueId;
                                team.LeagueId = leagueid??0;
                                //if (team.SeasonId != clubSeasonId)
                                if (team.SeasonId  != clubSeasonId)
                                {
                                    club.Teams.Remove(team);
                                    continue;
                                }
                                else
                                {
                                    var teamsDetail = db.TeamsDetails.FirstOrDefault(td => td.TeamId == team.TeamId&& td.SeasonId == clubSeasonId);
                                    if (teamsDetail != null)
                                        team.Title = teamsDetail.TeamName;
                                }

                                club.TotalFans += team.FanNumber;
                            }
                            club.TotalTeams = club.Teams.Count;
                        }
                    }
                    else
                    {
                        foreach (var club in section.Clubs)
                        {
                            club.TotalFans = 0;
                            if (club.ParentClubId > 0)
                                continue;
                            else
                            {
                                int clubSeasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId(club.Id);
                                club.Departments = (from cc in section.Clubs
                                                    where cc.ParentClubId == club.Id
                                                    select new DepartmentInfoViewModel
                                                    {
                                                        Id = cc.Id,
                                                        Logo = cc.Logo,
                                                        Title = cc.Title,
                                                        SectionName = section.Name,
                                                        Teams = cc.Teams
                                                    }).ToList();
                                foreach (var department in club.Departments)
                                {
                                    department.TotalFans = 0;
                                    for (int ii = club.Teams.Count - 1; ii >= 0; ii--)
                                    {
                                        var team = club.Teams[ii];
                                        int? leagueid = db.LeagueTeams.Where(x => x.TeamId == team.TeamId).FirstOrDefault()?.LeagueId;
                                        team.LeagueId = leagueid??0;
                                        //if (team.SeasonId != clubSeasonId)
                                        if (team.SeasonId != clubSeasonId)
                                        {
                                            club.Teams.Remove(team);
                                            continue;
                                        }
                                        else
                                        {
                                            var teamsDetail = db.TeamsDetails.FirstOrDefault(td => td.TeamId == team.TeamId&&td.SeasonId == clubSeasonId);
                                            if (teamsDetail != null)
                                                team.Title = teamsDetail.TeamName;
                                        }
                                        department.TotalFans += team.FanNumber;
                                    }
                                    department.TotalTeams = department.Teams.Count;
                                    club.TotalFans += department.TotalFans;
                                    club.TotalTeams += department.TotalTeams;
                                }

                                club.Teams.Clear();
                            }
                        }
                        for (int iii = section.Clubs.Count - 1; iii > 0; iii--)
                        {
                            if (section.Clubs[iii].ParentClubId > 0)
                                section.Clubs.RemoveAt(iii);
                        }
                    }
                }
                int k, jj;
                for ( k = 0; k < ss.Count();k++)
                {
                    var s = ss[k];
                    for (jj = 0; jj<s.Clubs.Count();jj++)
                    {
                        if (s.Clubs[jj].Teams.Count == 0)
                        {
                            ss[k].Clubs.Remove(s.Clubs[jj]);
                            jj = -1;
                        }
                    }
                    if (ss[k].Clubs.Count == 0)
                    {
                        ss.Remove(ss[k]);
                        k =-1;
                    }
                }
                return ss;
            }
        }
        internal static List<SectionViewModel> GetAllMyClubs(User curUser,int? seasonId)
        {
            using (var db = new DataEntities())
            {
                var ss = new List<SectionViewModel>();
                if (curUser.UsersType.TypeRole == "fans")
                {
                    ss = (from teamsfans in db.TeamsFans
                              from clubteams in db.ClubTeams
                              from clubs in db.Clubs
                              from section in db.Sections
                              where (section.Clubs.Count > 0 && teamsfans.UserId == curUser.UserId && clubs.ClubId == clubteams.ClubId && clubteams.TeamId == teamsfans.TeamId && section.SectionId == clubs.SectionId)
                              select new SectionViewModel
                              {
                                  SectionId = section.SectionId,
                                  LangId = section.LangId,
                                  Name = section.Name,
                                  Alias = section.Alias,
                                  IsIndividual = section.IsIndividual,
                                  Clubs = (from club in section.Clubs
                                           where club.IsArchive == false
                                           select new ClubTeamInfoViewModel
                                           {
                                               Id = club.ClubId,
                                               Logo = club.Logo,
                                               Title = club.Name,
                                               ParentClubId = club.ParentClubId,
                                               SectionName = section.Name,
                                               Teams = (from team in club.ClubTeams
                                                        where team.Team.IsArchive == false
                                                        select new ClubTeamViewModel
                                                        {
                                                            TeamId = team.Team.TeamId,
                                                            Title = team.Team.Title,
                                                            Logo = team.Team.Logo,
                                                            SeasonId = team.SeasonId,
                                                            ParentId = club.ClubId,
                                                            ParentName = club.Name,
                                                            FanNumber = team.Team.TeamsFans.Count,
                                                            IsSchoolTeam = false
                                                        }).Union(from School in club.Schools
                                                                 from team in School.SchoolTeams
                                                                 from league in db.LeagueTeams
                                                                 where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                                 select new ClubTeamViewModel
                                                                 {
                                                                     TeamId = team.Team.TeamId,
                                                                     Title = team.Team.Title,
                                                                     Logo = team.Team.Logo,
                                                                     SeasonId = School.SeasonId,
                                                                     ParentId = league.LeagueId,
                                                                     ParentName = club.Name,
                                                                     FanNumber = team.Team.TeamsFans.Count,
                                                                     IsSchoolTeam = true
                                                                 }).ToList()
                                           }).ToList()
                              }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                    if (ss.Count == 0)
                    {
                        ss = (from teamsfans in db.TeamsFans
                              from clubteams in db.ClubTeams
                              from club in db.Clubs
                              from unions in db.Unions
                              from section in db.Sections
                              where (teamsfans.UserId == curUser.UserId && clubteams.TeamId == teamsfans.TeamId
                              && club.ClubId == clubteams.ClubId && club.UnionId == unions.UnionId && section.SectionId == unions.SectionId)
                              select new SectionViewModel
                              {
                                  SectionId = section.SectionId,
                                  LangId = section.LangId,
                                  Name = section.Name,
                                  Alias = section.Alias,
                                  IsIndividual = section.IsIndividual,
                                  Clubs = (
                                           from sections in db.Sections
                                           where (teamsfans.UserId == curUser.UserId && clubteams.TeamId == teamsfans.TeamId
                                           && club.ClubId == clubteams.ClubId && club.UnionId == unions.UnionId && sections.SectionId == unions.SectionId
                                           && club.IsArchive == false && sections.SectionId == section.SectionId)
                                           select new ClubTeamInfoViewModel
                                           {
                                               Id = club.ClubId,
                                               Logo = club.Logo,
                                               Title = club.Name,
                                               ParentClubId = club.ParentClubId,
                                               SectionName = section.Name,
                                               Teams = (from team in club.ClubTeams
                                                        where team.Team.IsArchive == false
                                                        select new ClubTeamViewModel
                                                        {
                                                            TeamId = team.Team.TeamId,
                                                            Title = team.Team.Title,
                                                            Logo = team.Team.Logo,
                                                            SeasonId = team.SeasonId,
                                                            ParentId = club.ClubId,
                                                            ParentName = club.Name,
                                                            FanNumber = team.Team.TeamsFans.Count,
                                                            IsSchoolTeam = false
                                                        }).Union(from School in club.Schools
                                                                 from team in School.SchoolTeams
                                                                 where team.Team.IsArchive == false && School.ClubId == club.ClubId
                                                                 select new ClubTeamViewModel
                                                                 {
                                                                     TeamId = team.Team.TeamId,
                                                                     Title = team.Team.Title,
                                                                     Logo = team.Team.Logo,
                                                                     SeasonId = School.SeasonId,
                                                                     ParentId = club.ClubId,
                                                                     ParentName = club.Name,
                                                                     FanNumber = team.Team.TeamsFans.Count,
                                                                     IsSchoolTeam = true
                                                                 }).GroupBy(X => X.TeamId).Select(x => x.FirstOrDefault()).ToList()
                                           }).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList()
                              }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                    }
                }
                else if(curUser.UsersType.TypeRole == "players"){
                    ss = (from teamsfans in db.TeamsPlayers
                              from clubteams in db.ClubTeams
                              from clubs in db.Clubs
                              from section in db.Sections
                              where (section.Clubs.Count > 0 && teamsfans.UserId == curUser.UserId && clubs.ClubId == clubteams.ClubId && clubteams.TeamId == teamsfans.TeamId && section.SectionId == clubs.SectionId)
                              select new SectionViewModel
                              {
                                  SectionId = section.SectionId,
                                  LangId = section.LangId,
                                  Name = section.Name,
                                  Alias = section.Alias,
                                  IsIndividual = section.IsIndividual,
                                  Clubs = (from club in section.Clubs
                                           where club.IsArchive == false
                                           select new ClubTeamInfoViewModel
                                           {
                                               Id = club.ClubId,
                                               Logo = club.Logo,
                                               Title = club.Name,
                                               ParentClubId = club.ParentClubId,
                                               SectionName = section.Name,
                                               Teams = (from team in club.ClubTeams
                                                        where team.Team.IsArchive == false
                                                        select new ClubTeamViewModel
                                                        {
                                                            TeamId = team.Team.TeamId,
                                                            Title = team.Team.Title,
                                                            Logo = team.Team.Logo,
                                                            SeasonId = team.SeasonId,
                                                            ParentId = club.ClubId,
                                                            ParentName = club.Name,
                                                            FanNumber = team.Team.TeamsFans.Count,
                                                            IsSchoolTeam = false
                                                        }).Union(from School in club.Schools
                                                                 from team in School.SchoolTeams
                                                                 from league in db.LeagueTeams
                                                                 where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                                 select new ClubTeamViewModel
                                                                 {
                                                                     TeamId = team.Team.TeamId,
                                                                     Title = team.Team.Title,
                                                                     Logo = team.Team.Logo,
                                                                     SeasonId = School.SeasonId,
                                                                     ParentId = league.LeagueId,
                                                                     ParentName = club.Name,
                                                                     FanNumber = team.Team.TeamsFans.Count,
                                                                     IsSchoolTeam = true
                                                                 }).ToList()
                                           }).ToList()
                              }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                    if(ss.Count == 0)
                    {
                        ss = (from teamsfans in db.TeamsPlayers
                              from clubteams in db.ClubTeams
                              from club in db.Clubs
                              from unions in db.Unions
                              from section in db.Sections
                              where (teamsfans.UserId == curUser.UserId && clubteams.TeamId == teamsfans.TeamId
                              && club.ClubId == clubteams.ClubId && club.UnionId == unions.UnionId&& section.SectionId == unions.SectionId)
                              select new SectionViewModel
                              {
                                  SectionId = section.SectionId,
                                  LangId = section.LangId,
                                  Name = section.Name,
                                  Alias = section.Alias,
                                  IsIndividual = section.IsIndividual,
                                  Clubs = (
                                           from sections in db.Sections
                                           where (teamsfans.UserId == curUser.UserId && clubteams.TeamId == teamsfans.TeamId
                                           && club.ClubId == clubteams.ClubId && club.UnionId == unions.UnionId && sections.SectionId == unions.SectionId
                                           && club.IsArchive == false && sections.SectionId == section.SectionId)
                                           select new ClubTeamInfoViewModel
                                           {
                                               Id = club.ClubId,
                                               Logo = club.Logo,
                                               Title = club.Name,
                                               ParentClubId = club.ParentClubId,
                                               SectionName = section.Name,
                                               Teams = (from team in club.ClubTeams
                                                        where team.Team.IsArchive == false
                                                        select new ClubTeamViewModel
                                                        {
                                                            TeamId = team.Team.TeamId,
                                                            Title = team.Team.Title,
                                                            Logo = team.Team.Logo,
                                                            SeasonId = team.SeasonId,
                                                            ParentId = club.ClubId,
                                                            ParentName = club.Name,
                                                            FanNumber = team.Team.TeamsFans.Count,
                                                            IsSchoolTeam = false
                                                        }).Union(from School in club.Schools
                                                                 from team in School.SchoolTeams
                                                                 from league in db.LeagueTeams
                                                                 where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                                 select new ClubTeamViewModel
                                                                 {
                                                                     TeamId = team.Team.TeamId,
                                                                     Title = team.Team.Title,
                                                                     Logo = team.Team.Logo,
                                                                     SeasonId = School.SeasonId,
                                                                     ParentId = league.LeagueId,
                                                                     ParentName = club.Name,
                                                                     FanNumber = team.Team.TeamsFans.Count,
                                                                     IsSchoolTeam = true
                                                                 }).ToList().GroupBy(X=>X.TeamId).Select(x=>x.FirstOrDefault()).ToList()
                                           }).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList()
                              }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                    }
                }
                else
                {
                    ss = (    from section in db.Sections
                              from clubs in db.Clubs
                              from usersjobs in db.UsersJobs
                              from clubteams in db.ClubTeams
                              where (section.Clubs.Count > 0 && section.SectionId == clubs.SectionId && usersjobs.TeamId == clubteams.TeamId
                              && clubs.ClubId == clubteams.ClubId && usersjobs.UserId == curUser.UserId)
                              select new SectionViewModel
                              {
                                  SectionId = section.SectionId,
                                  LangId = section.LangId,
                                  Name = section.Name,
                                  Alias = section.Alias,
                                  IsIndividual = section.IsIndividual,
                                  Clubs = (from club in section.Clubs
                                           where club.IsArchive == false
                                           select new ClubTeamInfoViewModel
                                           {
                                               Id = club.ClubId,
                                               Logo = club.Logo,
                                               Title = club.Name,
                                               ParentClubId = club.ParentClubId,
                                               SectionName = section.Name,
                                               Teams = (from team in club.ClubTeams
                                                        where team.Team.IsArchive == false
                                                        select new ClubTeamViewModel
                                                        {
                                                            TeamId = team.Team.TeamId,
                                                            Title = team.Team.Title,
                                                            Logo = team.Team.Logo,
                                                            SeasonId = team.SeasonId,
                                                            ParentId = club.ClubId,
                                                            ParentName = club.Name,
                                                            FanNumber = team.Team.TeamsFans.Count,
                                                            IsSchoolTeam = false
                                                        }).Union(from School in club.Schools
                                                                 from team in School.SchoolTeams
                                                                 from league in db.LeagueTeams
                                                                 where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                                 select new ClubTeamViewModel
                                                                 {
                                                                     TeamId = team.Team.TeamId,
                                                                     Title = team.Team.Title,
                                                                     Logo = team.Team.Logo,
                                                                     SeasonId = School.SeasonId,
                                                                     ParentId = league.LeagueId,
                                                                     ParentName = club.Name,
                                                                     FanNumber = team.Team.TeamsFans.Count,
                                                                     IsSchoolTeam = true
                                                                 }).ToList()
                                           }).ToList()
                              }).GroupBy(x=>x.SectionId).Select(x=>x.FirstOrDefault()).ToList();
                    var ss1 = (from section in db.Sections
                          from clubs in db.Clubs
                          from usersjobs in db.UsersJobs
                          from leagues in db.Leagues
                          where (usersjobs.UserId == curUser.UserId && usersjobs.LeagueId == leagues.LeagueId
                              && clubs.ClubId == leagues.ClubId && clubs.SectionId == section.SectionId)
                          select new SectionViewModel
                          {
                              SectionId = section.SectionId,
                              LangId = section.LangId,
                              Name = section.Name,
                              Alias = section.Alias,
                              IsIndividual = section.IsIndividual,
                              Clubs = (from club in section.Clubs
                                       where club.IsArchive == false
                                       select new ClubTeamInfoViewModel
                                       {
                                           Id = club.ClubId,
                                           Logo = club.Logo,
                                           Title = club.Name,
                                           ParentClubId = club.ParentClubId,
                                           SectionName = section.Name,
                                           Teams = (from team in club.ClubTeams
                                                    where team.Team.IsArchive == false
                                                    select new ClubTeamViewModel
                                                    {
                                                        TeamId = team.Team.TeamId,
                                                        Title = team.Team.Title,
                                                        Logo = team.Team.Logo,
                                                        SeasonId = team.SeasonId,
                                                        ParentId = club.ClubId,
                                                        ParentName = club.Name,
                                                        FanNumber = team.Team.TeamsFans.Count,
                                                        IsSchoolTeam = false
                                                    }).Union(from School in club.Schools
                                                             from team in School.SchoolTeams
                                                             from league in db.LeagueTeams
                                                             where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                             select new ClubTeamViewModel
                                                             {
                                                                 TeamId = team.Team.TeamId,
                                                                 Title = team.Team.Title,
                                                                 Logo = team.Team.Logo,
                                                                 SeasonId = School.SeasonId,
                                                                 ParentId = league.LeagueId,
                                                                 ParentName = club.Name,
                                                                 FanNumber = team.Team.TeamsFans.Count,
                                                                 IsSchoolTeam = true
                                                             }).ToList()
                                       }).ToList()
                          }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                    var ss2 = (from section in db.Sections
                               from clubs in db.Clubs
                               from usersjobs in db.UsersJobs
                               where (usersjobs.UserId == curUser.UserId && usersjobs.ClubId == clubs.ClubId && clubs.SectionId == section.SectionId)
                               select new SectionViewModel
                               {
                                   SectionId = section.SectionId,
                                   LangId = section.LangId,
                                   Name = section.Name,
                                   Alias = section.Alias,
                                   IsIndividual = section.IsIndividual,
                                   Clubs = (from club in section.Clubs
                                            where club.IsArchive == false
                                            select new ClubTeamInfoViewModel
                                            {
                                                Id = club.ClubId,
                                                Logo = club.Logo,
                                                Title = club.Name,
                                                ParentClubId = club.ParentClubId,
                                                SectionName = section.Name,
                                                Teams = (from team in club.ClubTeams
                                                         where team.Team.IsArchive == false
                                                         select new ClubTeamViewModel
                                                         {
                                                             TeamId = team.Team.TeamId,
                                                             Title = team.Team.Title,
                                                             Logo = team.Team.Logo,
                                                             SeasonId = team.SeasonId,
                                                             ParentId = club.ClubId,
                                                             ParentName = club.Name,
                                                             FanNumber = team.Team.TeamsFans.Count,
                                                             IsSchoolTeam = false
                                                         }).Union(from School in club.Schools
                                                                  from team in School.SchoolTeams
                                                                  from league in db.LeagueTeams
                                                                  where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                                  select new ClubTeamViewModel
                                                                  {
                                                                      TeamId = team.Team.TeamId,
                                                                      Title = team.Team.Title,
                                                                      Logo = team.Team.Logo,
                                                                      SeasonId = School.SeasonId,
                                                                      ParentId = league.LeagueId,
                                                                      ParentName = club.Name,
                                                                      FanNumber = team.Team.TeamsFans.Count,
                                                                      IsSchoolTeam = true
                                                                  }).ToList()
                                            }).ToList()
                               }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                    ss = ss.Concat(ss1).Concat(ss2).ToList();
                }
                if (ss.Count() == 0)
                {
                    ss = (from section in db.Sections
                          from clubs in db.Clubs
                          from usersjobs in db.UsersJobs
                          from unions in db.Unions
                          where (usersjobs.ClubId == clubs.ClubId && usersjobs.UserId==curUser.UserId && clubs.UnionId == unions.UnionId && unions.SectionId == section.SectionId)
                          select new SectionViewModel
                          {
                              SectionId = section.SectionId,
                              LangId = section.LangId,
                              Name = section.Name,
                              Alias = section.Alias,
                              IsIndividual = section.IsIndividual,
                              Clubs = (from club in section.Clubs
                                       where club.IsArchive == false
                                       select new ClubTeamInfoViewModel
                                       {
                                           Id = club.ClubId,
                                           Logo = club.Logo,
                                           Title = club.Name,
                                           ParentClubId = club.ParentClubId,
                                           SectionName = section.Name,
                                           Teams = (from team in club.ClubTeams
                                                    where team.Team.IsArchive == false
                                                    select new ClubTeamViewModel
                                                    {
                                                        TeamId = team.Team.TeamId,
                                                        Title = team.Team.Title,
                                                        Logo = team.Team.Logo,
                                                        SeasonId = team.SeasonId,
                                                        ParentId = club.ClubId,
                                                        ParentName = club.Name,
                                                        FanNumber = team.Team.TeamsFans.Count,
                                                        IsSchoolTeam = false
                                                    }).Union(from School in club.Schools
                                                             from team in School.SchoolTeams
                                                             from league in db.LeagueTeams
                                                             where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                             select new ClubTeamViewModel
                                                             {
                                                                 TeamId = team.Team.TeamId,
                                                                 Title = team.Team.Title,
                                                                 Logo = team.Team.Logo,
                                                                 SeasonId = School.SeasonId,
                                                                 ParentId = league.LeagueId,
                                                                 ParentName = club.Name,
                                                                 FanNumber = team.Team.TeamsFans.Count,
                                                                 IsSchoolTeam = true
                                                             }).ToList().GroupBy(x => x.TeamId).Select(x => x.FirstOrDefault()).ToList()
                                       }).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList()
                          })
                        .GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                }
                if (ss.Count() == 0)
                {
                    ss = (from section in db.Sections
                          from leagues in db.Leagues
                          from usersjobs in db.UsersJobs
                          from unions in db.Unions
                          from clubs in db.Clubs
                          where (usersjobs.UserId == curUser.UserId&& usersjobs.LeagueId == leagues.LeagueId && leagues.UnionId == unions.UnionId && section.SectionId == unions.SectionId)
                          select new SectionViewModel
                          {
                              SectionId = section.SectionId,
                              LangId = section.LangId,
                              Name = section.Name,
                              Alias = section.Alias,
                              IsIndividual = section.IsIndividual,
                              Clubs = (from club in section.Clubs
                                       where club.IsArchive == false
                                       select new ClubTeamInfoViewModel
                                       {
                                           Id = club.ClubId,
                                           Logo = club.Logo,
                                           Title = club.Name,
                                           ParentClubId = club.ParentClubId,
                                           SectionName = section.Name,
                                           Teams = (from team in club.ClubTeams
                                                    where team.Team.IsArchive == false
                                                    select new ClubTeamViewModel
                                                    {
                                                        TeamId = team.Team.TeamId,
                                                        Title = team.Team.Title,
                                                        Logo = team.Team.Logo,
                                                        SeasonId = team.SeasonId,
                                                        ParentId = club.ClubId,
                                                        ParentName = club.Name,
                                                        FanNumber = team.Team.TeamsFans.Count,
                                                        IsSchoolTeam = false
                                                    }).Union(from School in club.Schools
                                                             from team in School.SchoolTeams
                                                             from league in db.LeagueTeams
                                                             where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                             select new ClubTeamViewModel
                                                             {
                                                                 TeamId = team.Team.TeamId,
                                                                 Title = team.Team.Title,
                                                                 Logo = team.Team.Logo,
                                                                 SeasonId = School.SeasonId,
                                                                 ParentId = league.LeagueId,
                                                                 ParentName = club.Name,
                                                                 FanNumber = team.Team.TeamsFans.Count,
                                                                 IsSchoolTeam = true
                                                             }).ToList().GroupBy(x => x.TeamId).Select(x => x.FirstOrDefault()).ToList()
                                       }).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList()
                          }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                }
                if (ss.Count() == 0)
                {
                    ss = (from section in db.Sections
                          from usersjobs in db.UsersJobs
                          from clubs in db.Clubs
                          where (usersjobs.UserId == curUser.UserId && usersjobs.ClubId == clubs.ClubId && section.SectionId == clubs.SectionId)
                          select new SectionViewModel
                          {
                              SectionId = section.SectionId,
                              LangId = section.LangId,
                              Name = section.Name,
                              Alias = section.Alias,
                              IsIndividual = section.IsIndividual,
                              Clubs = (from club in section.Clubs
                                       where club.IsArchive == false
                                       select new ClubTeamInfoViewModel
                                       {
                                           Id = club.ClubId,
                                           Logo = club.Logo,
                                           Title = club.Name,
                                           ParentClubId = club.ParentClubId,
                                           SectionName = section.Name,
                                           Teams = (from team in club.ClubTeams
                                                    where team.Team.IsArchive == false
                                                    select new ClubTeamViewModel
                                                    {
                                                        TeamId = team.Team.TeamId,
                                                        Title = team.Team.Title,
                                                        Logo = team.Team.Logo,
                                                        SeasonId = team.SeasonId,
                                                        ParentId = club.ClubId,
                                                        ParentName = club.Name,
                                                        FanNumber = team.Team.TeamsFans.Count,
                                                        IsSchoolTeam = false
                                                    }).Union(from School in club.Schools
                                                             from team in School.SchoolTeams
                                                             from league in db.LeagueTeams
                                                             where team.Team.IsArchive == false && league.TeamId == team.TeamId
                                                             select new ClubTeamViewModel
                                                             {
                                                                 TeamId = team.Team.TeamId,
                                                                 Title = team.Team.Title,
                                                                 Logo = team.Team.Logo,
                                                                 SeasonId = School.SeasonId,
                                                                 ParentId = league.LeagueId,
                                                                 ParentName = club.Name,
                                                                 FanNumber = team.Team.TeamsFans.Count,
                                                                 IsSchoolTeam = true
                                                             }).ToList().GroupBy(x => x.TeamId).Select(x => x.FirstOrDefault()).ToList()
                                       }).GroupBy(x => x.Id).Select(x => x.FirstOrDefault()).ToList()
                          }).GroupBy(x => x.SectionId).Select(x => x.FirstOrDefault()).ToList();
                }
            SeasonsRepo _seasonRepo = new SeasonsRepo();
                for (int i = ss.Count - 1; i >= 0; i--)
                {
                    var section = ss[i];
                    if (section.Clubs.Count == 0)
                    {
                        ss.Remove(section);
                        continue;
                    }
                    if (section.Alias != "multi-sport")
                    {
                        foreach (var club in section.Clubs)
                        {
                            club.TotalFans = 0;
                            int clubSeasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId(club.Id);
                            for (int ii = club.Teams.Count - 1; ii >= 0; ii--)
                            {
                                var team = club.Teams[ii];
                                if (team.SeasonId != clubSeasonId)
                                {
                                    club.Teams.Remove(team);
                                    continue;
                                }
                                else
                                {
                                    var teamsDetail = db.TeamsDetails.FirstOrDefault(td => td.TeamId == team.TeamId && td.SeasonId == clubSeasonId);
                                    if (teamsDetail != null)
                                        team.Title = teamsDetail.TeamName;
                                }

                                club.TotalFans += team.FanNumber;
                            }
                            club.TotalTeams = club.Teams.Count;
                        }
                    }
                    else
                    { 
                        foreach (var club in section.Clubs)
                        {
                            club.TotalFans = 0;
                            if (club.ParentClubId > 0)
                                continue;
                            else
                            {
                                int clubSeasonId = _seasonRepo.GetLastSeasonIdByCurrentClubId(club.Id);
                                club.Departments = (from cc in section.Clubs
                                                    where cc.ParentClubId == club.Id
                                                    select new DepartmentInfoViewModel
                                                    {
                                                        Id = cc.Id,
                                                        Logo = cc.Logo,
                                                        Title = cc.Title,
                                                        SectionName = section.Name,
                                                        Teams = cc.Teams
                                                    }).ToList();
                                foreach (var department in club.Departments)
                                {
                                    department.TotalFans = 0;
                                    for (int ii = department.Teams.Count - 1; ii >= 0; ii--)
                                    {
                                        var team = department.Teams[ii];
                                        if (team.SeasonId != clubSeasonId)
                                        {
                                            department.Teams.Remove(team);
                                            continue;
                                        }
                                        else
                                        {
                                            var teamsDetail = db.TeamsDetails.FirstOrDefault(td => td.TeamId == team.TeamId && td.SeasonId == clubSeasonId);
                                            if (teamsDetail != null)
                                                team.Title = teamsDetail.TeamName;
                                        }
                                        department.TotalFans += team.FanNumber;
                                    }
                                    department.TotalTeams = department.Teams.Count;
                                    club.TotalFans += department.TotalFans;
                                    club.TotalTeams += department.TotalTeams;
                                }

                                club.Teams.Clear();
                            }
                        }
                        for (int iii = section.Clubs.Count - 1; iii > 0; iii--)
                        {
                            if (section.Clubs[iii].ParentClubId > 0)
                                section.Clubs.RemoveAt(iii);
                        }
                    }
                }
                return ss;
            }
        }
    }
}