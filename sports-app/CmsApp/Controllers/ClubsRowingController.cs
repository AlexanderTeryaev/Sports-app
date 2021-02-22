using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ClosedXML.Excel;
using DataService.Actions;
using DataService.DTO;
using Resources;

namespace CmsApp.Controllers
{
    public class ClubsRowingController : AdminController
	{
		[HttpPost]
		public ActionResult ImportCategoryFromExcel(
			HttpPostedFileBase importedExcel,
			int? clubId, 
			int? disciplineId, 
			int? leagueId, 
			int seasonId)
		{
			try
			{
				if (importedExcel != null && importedExcel.ContentLength > 0)
				{
					var dto = new List<AthleticRegDto>();
					//Vitaly: that should be taken from cache
					var teamsDictionary = teamRepo.GetAllTeams();
					var clubsDictionary = clubsRepo.GetAllClubs();

					using (var workBook = new XLWorkbook(importedExcel.InputStream))
					{
						var sheet = workBook.Worksheet(1);
						var valueRows = sheet.RowsUsed().Skip(1).ToList();
						var i = 0;
						foreach (var row in valueRows)
						{
							var localCulture = CultureInfo.CurrentCulture.ToString();

							if (!int.TryParse(sheet.Cell(i + 2, 17).Value.ToString(), out var rank))
								rank = -1;
							double.TryParse(sheet.Cell(i + 2, 16).Value.ToString(), out var resultDouble);
							var result = sheet.Cell(i + 2, 16).Value is DateTime 
								? Convert.ToDateTime(sheet.Cell(i + 2, 16).Value).TimeOfDay.ToString("c") 
								: sheet.Cell(i + 2, 16).Value is TimeSpan 
								? Convert.ToDateTime(sheet.Cell(i + 2, 16).Value).TimeOfDay.ToString("c")
								: sheet.Cell(i + 2, 16).Value.ToString();
							if (string.IsNullOrWhiteSpace(result))
								try
								{
									result = TimeSpan.Parse(result).ToString("c");
								}
								catch
								{
									throw new FormatException("Invalid format, expected: time");
								}
							var teamNumber = sheet.Cell(i + 2, 12).Value.ToString().StartsWith(Messages.Team + " ")
								? int.Parse(sheet.Cell(i + 2, 12).Value.ToString().Replace(Messages.Team + " ", ""))
								: 0;
							dto.Add(new AthleticRegDto
							{
								DisciplineId = disciplineId,
								ClubId = clubsDictionary.ContainsValue(sheet.Cell(i + 2, 6).Value.ToString())
									? clubsDictionary.FirstOrDefault(p => p.Value == sheet.Cell(i + 2, 6).Value.ToString()).Key
									: null as int?,
								SeassonId = seasonId,
								IdentNum = sheet.Cell(i + 2, 8).Value.ToString(),
								TeamNumber = teamNumber,
								Rank = rank == -1 ? null as int? : rank,
								Result = resultDouble > 0 ? DateTime.FromOADate(resultDouble).TimeOfDay.ToString("c") : result
							});

							i++;
						}
					}

					new RowingCompetitionDisciplineAction(playersRepo).ImportCompetitionRegistrationList(dto);

					return Redirect(Request.UrlReferrer.ToString());
				}
				// Vitaly: test that out, had never seen a wrap up like this
				return Redirect(Request.UrlReferrer.ToString());
			}
			catch (Exception ex)
			{
				return new HttpNotFoundResult(ex.Message);
			}
		}
	}
}