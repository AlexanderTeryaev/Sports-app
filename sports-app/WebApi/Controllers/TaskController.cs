using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DataService;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [RoutePrefix("api/task")]
    public class TaskController : ApiController
    {
        // GET: Task
        [Route("gamesTimes")]
        public IHttpActionResult GetGamesTimes()
        {
            var gnServ = new GamesNotificationsService();

            gnServ.SaveNotifications();

            gnServ.SendPushToDevices(Settings.IsTest);

            return Ok();
        }
    }
}
