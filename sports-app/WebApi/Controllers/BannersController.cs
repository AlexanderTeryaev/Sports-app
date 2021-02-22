using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AppModel;
using DataService;
using WebApi.Models;
using WebApi.Services;
using System.Collections.Generic;

namespace WebApi.Controllers
{
    [RoutePrefix("api/Banners")]
    public class BannersController : BaseLogLigApiController
    {

        // GET: api/Union/{unionId}
        [Route("Union/{unionId}")]
        [HttpGet]
        //[ResponseType(typeof(FanOwnPrfileViewModel))]
        public IHttpActionResult GetUnionBanners(int unionId)
        {
            List<BannerViewModel> bannerList = new List<BannerViewModel>();
            //Friends
            bannerList = db.AdvBanners.Where(x => x.leagueId == unionId).ToList().Select(u =>
                new BannerViewModel
                {
                    BannerId = u.id,
                    LinkUrl = u.linkurl,
                    ImageName = u.image
                }).ToList();

            Random rnd = new Random();
            int index = rnd.Next(1, 1000);

            List<BannerViewModel> returnList = new List<BannerViewModel>();
            if(bannerList.Count > 0)
            {
                returnList.Add(bannerList[index % bannerList.Count]);
            }
            

            return Ok(returnList);
        }

        [Route("Club/{clubId}")]
        [HttpGet]
        //[ResponseType(typeof(FanOwnPrfileViewModel))]
        public IHttpActionResult GetClubBanners(int clubId)
        {
            List<BannerViewModel> bannerList = new List<BannerViewModel>();
            //Friends
            bannerList = db.AdvBanners.Where(x => x.clubId == clubId).ToList().Select(u =>
                new BannerViewModel
                {
                    BannerId = u.id,
                    LinkUrl = u.linkurl,
                    ImageName = u.image
                }).ToList();

            Random rnd = new Random();
            int index = rnd.Next(1, 1000);

            List<BannerViewModel> returnList = new List<BannerViewModel>();
            returnList.Add(bannerList[index % bannerList.Count]);

            return Ok(returnList);
        }

        [Route("IncreaseVisit/{bannerId}")]
        [HttpGet]
        //[ResponseType(typeof(FanOwnPrfileViewModel))]
        public IHttpActionResult IncVisitCount(int bannerId)
        {
            // Cheng Li. : Modify : get AdvBanners.
            //var item = db.AdvBanners.Where(x => x.id == bannerId).FirstOrDefault();
            var item = db.AdvBanners.FirstOrDefault(ab => ab.id == bannerId);
            item.count++;
            db.SaveChanges();

            return Ok();
        }
    }
}