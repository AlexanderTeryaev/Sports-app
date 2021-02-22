using AppModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebApi.Models;

namespace WebApi.Services
{
    public static class FriendsService
    {

        internal static int CreateFriendRequest(int userId, int friendId)
        {
            using (DataEntities db = new DataEntities())
            {
                db.UsersFriends.Add(new UsersFriend
                {
                    UserId = userId,
                    FriendId = friendId,
                    IsConfirmed = false,
                    RequestedAt = DateTime.UtcNow
                });
                return db.SaveChanges();
            }
        }

        internal static List<User> GetAllConfirmedFriendsAsUsers(User user)
        {
            var sd = user.UsersFriends.Where(uf => uf.IsConfirmed).Select(uf => uf.Friend);
            var sd2 = user.Friends.Where(uf => uf.IsConfirmed).Select(uf => uf.User);
            return sd.Concat(sd2).ToList();
        }

        /**
         * Get all friends of a user, along with Friendship status with the currently authenticated user
         */
        internal static List<FanFriendViewModel> GetAllFanFriends(int userId, int currentUserId)
        {
            using (DataEntities db = new DataEntities())
            {
                // current user's friends
                var cuFriends = db.UsersFriends
                    .Where(f => f.FriendId == currentUserId || f.UserId == currentUserId)
                    .ToList()
                    .Select(f => new FanFriendViewModel
                    {
                        Id = (f.FriendId == userId ? f.UserId : f.FriendId),
                        UserName = (f.FriendId == userId ? f.User : f.Friend).UserName,
                        //Image = (f.FriendId == userId ? f.User : f.Friend).Image == null && f.User.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(f.User, null).Image : (f.FriendId == userId ? f.User : f.Friend).Image,
                        Image = (f.FriendId == userId ? f.User : f.Friend).Image,
                        FullName = (f.FriendId == userId ? f.User : f.Friend).FullName,
                        UserRole = (f.FriendId == userId ? f.User : f.Friend).UsersType.TypeRole,
                        CanRcvMsg = true,
                        FriendshipStatus = f.IsConfirmed ? FriendshipStatus.Yes : FriendshipStatus.Pending
                    })
                    .ToList();
                // fan's friends
                var fanFriends = db.UsersFriends
                    .Where(uf => uf.FriendId == userId || uf.UserId == userId)
                    .ToList()
                    .Select(f => new FanFriendViewModel
                    {
                        Id = (f.FriendId == userId ? f.UserId : f.FriendId),
                        UserName = (f.FriendId == userId ? f.User : f.Friend).UserName,
                        //Image = (f.FriendId == userId ? f.User : f.Friend).Image == null && f.User.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(f.User, null).Image : (f.FriendId == userId ? f.User : f.Friend).Image,
                        Image = (f.FriendId == userId ? f.User : f.Friend).Image,
                        FullName = (f.FriendId == userId ? f.User : f.Friend).FullName,
                        UserRole = (f.FriendId == userId ? f.User : f.Friend).UsersType.TypeRole,
                        CanRcvMsg = true
                    })
                    .ToList();

                // add fan's friends and current user's friendship status
                fanFriends.ForEach(ff => {
                        var cf = cuFriends.FirstOrDefault(cuf => cuf.Id == ff.Id);
                        ff.FriendshipStatus = (cf == null ? FriendshipStatus.No : cf.FriendshipStatus);
                    }
                );
                return fanFriends;
            }
        }

        internal static string AreFriends(int user1Id, int user2Id)
        {
            using (DataEntities db = new DataEntities())
            {
                var friendship =  db.UsersFriends.FirstOrDefault(uf => ((uf.UserId == user1Id && uf.FriendId == user2Id) || (uf.UserId == user2Id && uf.FriendId == user1Id)));
                if (friendship != null)
                    return friendship.IsConfirmed ? FriendshipStatus.Yes : FriendshipStatus.Pending;
                else
                    return FriendshipStatus.No;
            }
        }

        internal static void AreFriends(List<CompactPlayerViewModel> players, int currentUserId)
        {
            using (DataEntities db = new DataEntities())
            {
                // get players' userIds
                var playerIds = players.Select(p => p.Id);

                // Query Friendship table for all players
                var friendship = db.UsersFriends.Where(uf => 
                    ((playerIds.Contains(uf.UserId) && uf.FriendId == currentUserId) ||
                    (uf.UserId == currentUserId && (playerIds.Contains(uf.FriendId)))));

                // set friendship status for each players
                players.ForEach(p =>
                {
                    var f = friendship.FirstOrDefault(uf => p.Id == uf.UserId || p.Id == uf.FriendId);
                    if (f != null)
                        p.FriendshipStatus = f.IsConfirmed ? FriendshipStatus.Yes : FriendshipStatus.Pending;
                    else
                        p.FriendshipStatus = FriendshipStatus.No;
                });
            }
        }

        //internal static void AreFansFriends(List<UserBaseViewModel> fans, int currentUserId)
        //{
        //    if (currentUserId != 0)
        //    {
        //        using (DataEntities db = new DataEntities())
        //        {
        //            var user = db.Users.Find(currentUserId);
        //            var userFriends = user.UsersFriends
        //                .Where(uf => uf.IsConfirmed == true)
        //                .Select(uf => uf.UserId)
        //                .Union(user.Friends
        //                .Where(uf => uf.IsConfirmed == true)
        //                .Select(uf => uf.FriendId))
        //                .ToList();
        //            foreach (var f in fans)
        //            {
        //                f.IsFriend = userFriends.Contains(f.Id);
        //            }
        //        }
        //    }
        //}

    }
}