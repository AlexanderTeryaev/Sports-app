using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using WebApi.Services;
using DataService;


namespace WebApi.Controllers
{
    [RoutePrefix("api/Sections")]
    public class SectionsController : BaseLogLigApiController
    {
        readonly SeasonsRepo _seasonsRepo = new SeasonsRepo();
        /// <summary>
        /// Get list of clubs
        /// </summary>
        /// <returns></returns>
        //[Route("list")]
        [ResponseType(typeof(List<Models.SectionViewModel>))]
        public IHttpActionResult GetAll(int? unionId=null)
        {
            int? seasonId;
            if (unionId != 15)
            {
                seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                  (int?)null;
            }
            else
            {
                seasonId = null;
            }

            var sections = SectionService.GetAll(seasonId);

            return Ok(sections);
        }
        [ResponseType(typeof(List<Models.SectionViewModel>))]
        [Route("GetAllBySeason")]
        public IHttpActionResult GetAllBySeason(int? unionId = null)
        {
            int? seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionId(unionId.Value) :
                                  (int?)null;
            var sections = SectionService.GetAllBySeason(seasonId);

            return Ok(sections);
        }
        [ResponseType(typeof(List<Models.SectionViewModel>))]
        [Route("GetAllMyClubs")]
        public IHttpActionResult GetAllMyClubs(int? unionId = null)
        {

            int? seasonId = unionId != null ? _seasonsRepo.GetLastSeasonByCurrentUnionIdByUser(unionId.Value) : (int?)null;
            var sections = SectionService.GetAllMyClubs(CurrentUser,seasonId);

            return Ok(sections);
        }
    }
}
