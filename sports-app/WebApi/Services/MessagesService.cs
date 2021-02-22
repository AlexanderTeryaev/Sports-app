using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppModel;
using WebApi.Models;
using System.Data.Entity;

namespace WebApi.Services
{
    public class MessagesService
    {
        internal static void SendTeamMessage(TeamMessageBindingModel message, int currentUserId)
        {
            using (var db = new DataEntities())
            {
                var msg = new Message
                {
                    Date = DateTime.Now,
                    Body = message.Body,
                    SenderId = currentUserId,
                    Type = MessageTypeEnum.Root
                };

                var wt = new WallThread
                {
                    TeamId = message.TeamId,
                    CreaterId = currentUserId
                };
                wt.Messages.Add(msg);
                db.WallThreads.Add(wt);
                db.SaveChanges();
            }
        }

        internal static void SendGameMessage(GameMessageBindingModel message, int currentUserId)
        {
            using (var db = new DataEntities())
            {
                var msg = new Message
                {
                    Date = DateTime.Now,
                    Body = message.Body,
                    SenderId = currentUserId,
                    Type = (int)MessageTypeEnum.Root
                };

                var wt = new WallThread
                {
                    GameId = message.GameId,
                    CreaterId = currentUserId
                };
                wt.Messages.Add(msg);
                db.WallThreads.Add(wt);
                db.SaveChanges();
            }
        }

        internal static void SendWallMessageReply(WallMessageReplyBindingModel message, int currentUserId)
        {
            using (var db = new DataEntities())
            {
                var msg = new Message
                {
                    Date = DateTime.Now,
                    Body = message.Body,
                    SenderId = currentUserId,
                    Type = (int)MessageTypeEnum.Reply
                };

                var wt = db.WallThreads.Find(message.ThreadId);
                if (wt != null)
                {
                    wt.Messages.Add(msg);
                    db.SaveChanges();
                }
            }
        }

        internal static void DeleteWallThread(int threadId, int currentUserId)
        {
            using (var db = new DataEntities())
            {
                var thread = db.WallThreads.Find(threadId);
                if (thread != null && thread.CreaterId == currentUserId)
                {
                    thread.IsArchive = true;
                    db.SaveChanges();
                }
            }
        }

        internal static List<WallThreadViewModel> GetTeamMessages(int teamId)
        {
            using (var db = new DataEntities())
            {
                db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                var threads = db.WallThreads
                    .Include(wt => wt.Messages)
                    .Include(wt => wt.Messages.Select(m => m.User))
                    .Where(wt => wt.TeamId == teamId && wt.IsArchive == false)
                    .ToList();
                var threadVMList = ParseMessageThreads(threads);
                return threadVMList;
            }
        }


        internal static List<WallThreadViewModel> GetGameMessages(int gameId)
        {
            using (var db = new DataEntities())
            {
                db.Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
                var threads = db.WallThreads
                    .Include(wt => wt.Messages)
                    .Include(wt => wt.Messages.Select(m => m.User))
                    .Where(wt => wt.GameId == gameId && wt.IsArchive == false)
                    .ToList();
                var threadVMList = ParseMessageThreads(threads);
                return threadVMList;
            }
        }

        private static List<WallThreadViewModel> ParseMessageThreads(List<WallThread> threads)
        {
            var threadVMList = new List<WallThreadViewModel>();
            foreach (var thread in threads)
            {
                var wtvm = new WallThreadViewModel
                {
                    ThreadId = thread.Id,
                    CreaterId = thread.CreaterId
                };

                var mvmList = thread.Messages
                    .ToList()
                    .Select(m =>
                        new MessageBaseViewModel
                        {
                            SenderId = m.SenderId,
                            SenderUserName = m.User.UserName,
                            SenderFullName = m.User.FullName,
                            Date = m.Date,
                            Body = m.Body,
                            Type = m.Type
                        })
                    .ToList();
                wtvm.MainMessage = mvmList.FirstOrDefault(m => ((m.Type & MessageTypeEnum.Reply) == 0));
                wtvm.Replies = mvmList.Where(m => ((m.Type & MessageTypeEnum.Reply) != 0));
                threadVMList.Add(wtvm);
            }

            return threadVMList;
        }
    }
}