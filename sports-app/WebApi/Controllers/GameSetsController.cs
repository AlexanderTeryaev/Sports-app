using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using WebApi.Exceptions;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/gamesets")]
    public class GameSetsController : ApiController
    {
        /// <summary>
        /// Reutrn game sets for gameId
        /// </summary>
        /// <param name="gameId"></param>
        /// <returns></returns>
        [Route("{gameId}")]
        [HttpGet]
        public IEnumerable<GameSetViewModel> GameSetsByGameId(int gameId)
        {
            IEnumerable<GameSetViewModel> gaemesSets = GamesService.GetGameSets(gameId);
            return gaemesSets;
        }

        /// <summary>
        /// Create game set
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult CreateGameSet(CreateGameSetViewModel viewModel)
        {
            try
            {
                GameSetViewModel gameSetViewModel = GamesService.CreateGameSet(viewModel);

                return Content(HttpStatusCode.Created, gameSetViewModel);
            }
            catch (NotFoundEntityException ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }

        /// <summary>
        /// Update game set
        /// </summary>
        /// <param name="gameSetId"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{gameSetId}")]
        public IHttpActionResult UpdateGameSet(int gameSetId, [FromBody]CreateGameSetViewModel viewModel)
        {
            try
            {
                GamesService.UpdateGameSet(gameSetId, viewModel);

                return Content(HttpStatusCode.OK, viewModel);
            }
            catch (NotFoundEntityException ex)
            {
                return Content(HttpStatusCode.BadRequest, ex.Message);
            }
        }
    }
}