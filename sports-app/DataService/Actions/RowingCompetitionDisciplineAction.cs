using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppModel;
using DataService.DTO;

namespace DataService.Actions
{
	public class RowingCompetitionDisciplineAction
	{
		private PlayersRepo _repo;

		public RowingCompetitionDisciplineAction(PlayersRepo repo)
		{
			_repo = repo;
		}

		public int ImportCompetitionRegistrationList(IEnumerable<AthleticRegDto> list)
		{
			var updated = 0;
			foreach (var item in list) 
			{
				var found = _repo.db.CompetitionDisciplineRegistrations.Where(p =>
					p.CompetitionDisciplineId == item.DisciplineId &&
					p.ClubId == item.ClubId &&
					p.CompetitionDisciplineTeam.TeamNumber == item.TeamNumber &&
					p.User.IdentNum == item.IdentNum)
					.ToList();
				if (!found.Any())
					throw new InvalidOperationException($"{item.IdentNum} was not found for given registration");
				if (found.Count > 1)
					throw new ArgumentException($"Unable to set Rank and Result for {item.IdentNum}, clubId={item.ClubId} and teamId={item.CompetitionDisciplineTeamId}: found {found.Count} matches");
				if (found == null)
					continue;
				found.First().Rank = item.Rank;
				found.First().Result = item.Result;
				updated++;
			}

			_repo.db.SaveChanges();
			return updated;
		}
	}
}
