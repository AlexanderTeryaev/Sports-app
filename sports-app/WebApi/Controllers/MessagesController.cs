using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Messages")]
    public class MessagesController : BaseLogLigApiController
    {
        /// <summary>
        /// שולח הודעה לקיר של קבוצה
        /// </summary>
        /// <param name="message">הודעה</param>
        /// <returns></returns>
        [Route("SendTeamMessage")]
        public IHttpActionResult PostTeamMessage(TeamMessageBindingModel message)
        {
            MessagesService.SendTeamMessage(message, CurrUserId);
            return Ok();
        }

        /// <summary>
        /// שולח הודעה לקיר של משחק
        /// </summary>
        /// <param name="message">הודעה</param>
        /// <returns></returns>
        [Route("SendGameMessage")]
        public IHttpActionResult PostGameMessage(GameMessageBindingModel message)
        {
            MessagesService.SendGameMessage(message, CurrUserId);
            return Ok();
        }

        /// <summary>
        /// שולח תגובה להודעה על קיר של קבוצה  או משחק
        /// </summary>
        /// <param name="message">הודעה</param>
        /// <returns></returns>
        [Route("SendWallMessageReply")]
        public IHttpActionResult PostWallMessageReply(WallMessageReplyBindingModel message)
        {
            MessagesService.SendWallMessageReply(message, CurrUserId);
            return Ok();
        }

        /// <summary>
        ///   מוחק הודעה מהקיר של קבוצה או משחק
        /// </summary>
        /// <param name="message">הודעה</param>
        /// <returns></returns>
        [Route("DeleteWallMessage/{threadId}")]
        public IHttpActionResult PostDeleteWallMessage(int threadId)
        {
            MessagesService.DeleteWallThread(threadId, CurrUserId);
            return Ok();
        }
    }
}
