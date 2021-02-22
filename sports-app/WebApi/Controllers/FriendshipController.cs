using AppModel;
using DataService;
using log4net;
using PushServiceLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [Authorize]
    [RoutePrefix("api/Friends")]
    public class FriendshipController : BaseLogLigApiController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(FriendshipController));
        /// <summary>
        /// שליחת הצעת חבירות
        /// </summary>
        /// <param name="friendId">ID משתמש</param>
        /// <returns></returns>
        [Route("Request/{friendId}")]
        [Authorize]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostFriendRequest(int friendId)
        {
            User user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            if (!db.Users.Any(u => u.UserId == friendId && u.IsArchive == false && u.IsActive == true))
            {
                return NotFound();
            }

            if (AreFriends(user.UserId, friendId))
            {
                return BadRequest("Users are already friend or a friend request already sent.");
            }

            int recoredscount = FriendsService.CreateFriendRequest(user.UserId, friendId);

            if (recoredscount == 1)
            {
                // send notification to the user (who is being requested for friendship)
                var message = "קיבלת בקשת חברות";// "You have received a friendship request";
                NotesMessagesRepo msgRepo = new NotesMessagesRepo();
                msgRepo.SendToUsers(new List<int>() { friendId }, message, (MessageTypeEnum.PushNotifyOnly | MessageTypeEnum.Root));
                GamesNotificationsService gns = new GamesNotificationsService();
                gns.SendPushToDevices(false);

                return Ok();
            }
            else
            {
                return InternalServerError();
            }
        }


        /// <summary>
        /// מחזיר רשימת אנשים שמחכים לאישור חבירות
        /// </summary>
        /// <returns></returns>
        [Route("PendingFriendshipRequests")]
        [Authorize]
        [ResponseType(typeof(List<FanFriendViewModel>))]
        public IHttpActionResult GetPendingFrienshipRequests()
        {
            User user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            List<UserBaseViewModel> friends = db.UsersFriends
                .Where(f => f.IsConfirmed == false &&
                            f.User != null &&
                            f.User.IsActive &&
                            f.User.IsArchive == false &&
                            f.FriendId == user.UserId)
                .ToList()
                .Select(f =>
                    new UserBaseViewModel
                    {
                        Id = f.UserId,
                        UserName = f.User.UserName,
                        FullName = f.User.FullName,
                        Image = f.User.Image,
                        FriendshipStatus = FriendshipStatus.Pending,
                        Timestamp1 = f.RequestedAt
                    })
                .ToList();

            for (var i = 0; i < friends.Count; i++ )
            {
                UserBaseViewModel f = friends.ElementAt(i);
                if (f.Image == null)
                {
                    f.Image = db.PlayerFiles.Where(x => x.PlayerId == f.Id && x.FileType == (int)PlayerFileType.PlayerImage).Select(x => x.FileName).FirstOrDefault() ?? null;
                }
            }
            return Ok(friends);
        }

        /// <summary>
        /// אישור הצעת חבירות
        /// </summary>
        /// <param name="friendId">ID משתמש</param>
        /// <returns></returns>
        [Route("ConfirmRequest/{friendId}")]
        [Authorize]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostConfirmRequest(int friendId)
        {
            User user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            UsersFriend friendship = await db.UsersFriends
                .FirstOrDefaultAsync(uf =>
                    uf.UserId == friendId &&
                    uf.FriendId == user.UserId &&
                    uf.IsConfirmed == false);

            if (friendship == null)
            {
                return NotFound();
            }

            friendship.IsConfirmed = true;

            db.Entry(friendship).State = EntityState.Modified;

            await db.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// בטל הצעת חבירות
        /// </summary>
        /// <param name="friendId">ID משתמש</param>
        /// <returns></returns>
        [Route("CancelFriendRequest/{friendId}")]
        [Authorize]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostCancelFriendRequest(int friendId)
        {
            User user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            UsersFriend friendship = await db.UsersFriends.FirstOrDefaultAsync(uf =>
                uf.UserId == user.UserId && uf.FriendId == friendId
                && uf.IsConfirmed == false);

            if (friendship == null)
            {
                return NotFound();
            }

            db.UsersFriends.Remove(friendship);

            await db.SaveChangesAsync();

            return Ok();
        }


        /// <summary>
        /// בטל חבירות
        /// </summary>
        /// <param name="friendId">ID משתמש</param>
        /// <returns></returns>
        [Route("Unfriend/{friendId}")]
        [Authorize]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostUnfriend(int friendId)
        {
            User user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            UsersFriend friendship = await db.UsersFriends.FirstOrDefaultAsync(uf =>
                ((uf.UserId == friendId && uf.FriendId == user.UserId) ||
                (uf.UserId == user.UserId && uf.FriendId == friendId))
                && uf.IsConfirmed == true);

            if (friendship == null)
            {
                return NotFound();
            }

            db.UsersFriends.Remove(friendship);

            await db.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// דחית הצעת חבירות
        /// </summary>
        /// <param name="friendId">ID משתמש</param>
        /// <returns></returns>
        [Route("RejectFriendRequest/{friendId}")]
        [Authorize]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PostRejectFriendRequest(int friendId)
        {
            User user = CurrentUser;
            if (user == null)
            {
                return NotFound();
            }

            UsersFriend friendship = await db.UsersFriends.FirstOrDefaultAsync(uf =>
                uf.UserId == friendId && 
                uf.FriendId == user.UserId && 
                uf.IsConfirmed == false);

            if (friendship == null)
            {
                return NotFound();
            }

            db.UsersFriends.Remove(friendship);

            await db.SaveChangesAsync();

            return Ok();
        }


        private bool AreFriends(int usr1, int usr2)
        {
            return (db.UsersFriends.FirstOrDefault(uf =>
                (uf.UserId == usr1 && uf.FriendId == usr2) ||
                (uf.UserId == usr2 && uf.FriendId == usr1))
                != null);
        }

    }
}
