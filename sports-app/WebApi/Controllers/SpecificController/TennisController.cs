using System;
using System.Linq;
using System.Web.Http;
using AppModel;
using DataService;
using WebApi.Models.Tennis;

namespace WebApi.Controllers.SpecificController
{
    /// <summary>
    /// Tennis union specific API
    /// </summary>
    [RoutePrefix("api/tennis")]
    public class TennisController : BaseLogLigApiController
    {
        /// <summary>
        /// Add player to training team
        /// </summary>
        /// <returns></returns>
        [Route("players")]
        public IHttpActionResult Post(PlayersPostRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.IdNum) && string.IsNullOrWhiteSpace(request.PassNum))
            {
                return BadRequest($"{nameof(request.IdNum)} and {nameof(request.PassNum)} are empty");
            }

            User user;
            if (!string.IsNullOrWhiteSpace(request.IdNum))
            {
                if (request.IdNum.Length != 9)
                {
                    return BadRequest($"{nameof(request.IdNum)} must be 9 chars in length");
                }

                user = db.Users.FirstOrDefault(x => x.IdentNum == request.IdNum);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.PassNum))
                {
                    return BadRequest($"Fill in {nameof(request.IdNum)} or {nameof(request.PassNum)}");
                }

                user = db.Users.FirstOrDefault(x => x.PassportNum == request.PassNum);
            }

            //TODO: we should get rid of "name"(FullName) from request and use First, Last and Middle names ASAP
            var playerRepo = new PlayersRepo(db);
            var firstName = playerRepo.GetFirstNameByFullName(request.FullName);
            var lastName = playerRepo.GetLastNameByFullName(request.FullName);

            if (user == null)
            {
                user = new User
                {
                    Password = Protector.Encrypt("123abc12"),
                    TypeId = 4, //player
                    IdentNum = string.IsNullOrWhiteSpace(request.IdNum) ? null : request.IdNum,
                    PassportNum = string.IsNullOrWhiteSpace(request.IdNum) ? request.PassNum : null,

                    FirstName = firstName,
                    LastName = lastName,
                    //MiddleName = request.MiddleName,

                    Email = request.Email,
                    Telephone = request.PhoneNumber,
                    City = request.City,
                    BirthDay = request.BirthDate,
                    GenderId = (int)request.Gender,
                    IsActive = true
                };

                db.Users.Add(user);
                db.SaveChanges();

                user = db.Users.Find(user.UserId);

                if (user == null)
                {
                    return BadRequest("Unable to get user");
                }
            }

            var club = db.Clubs.Find(request.ClubId);
            if (club == null || club.UnionId != 38)
            {
                return BadRequest($"Club '{request.ClubId}' was not found");
            }

            var latestSeason = db.Seasons.OrderByDescending(x => x.Id).FirstOrDefault(x => x.UnionId == 38 && x.IsActive);
            if (latestSeason == null)
            {
                return BadRequest("Unable to get latest season for tennis union");
            }

            var trainingTeam = club.ClubTeams.FirstOrDefault(x => x.SeasonId == latestSeason.Id && x.IsTrainingTeam);
            if (trainingTeam == null)
            {
                return BadRequest($"Training team not found in club '{club.Name}'");
            }

            var teamPlayers = trainingTeam.Team.TeamsPlayers;

            var existingPlayer = teamPlayers.FirstOrDefault(x =>
                x.SeasonId == latestSeason.Id && x.UserId == user.UserId && x.ClubId == club.ClubId);

            if (existingPlayer == null)
            {
                teamPlayers.Add(new TeamsPlayer
                {
                    SeasonId = latestSeason.Id,
                    UserId = user.UserId,
                    ShirtNum = teamPlayers.Any() ? teamPlayers.Max(x => x.ShirtNum) + 1 : 1,
                    IsActive = true,
                    ClubId = club.ClubId
                });
            }
            else
            {
                existingPlayer.IsActive = true;
            }

            user.TenicardValidity = new DateTime(DateTime.Now.Year, 12, 31);
            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                user.Telephone = request.PhoneNumber;
            }
            if (!string.IsNullOrWhiteSpace(request.Email))
            {
                user.Email = request.Email;
            }
            if (!string.IsNullOrWhiteSpace(request.City))
            {
                user.City = request.City;
            }

            db.SaveChanges();

            return Ok();
        }
    }
}