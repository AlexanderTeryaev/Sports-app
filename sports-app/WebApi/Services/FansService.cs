using AppModel;
using System.Linq;
using WebApi.Models;

namespace WebApi.Services
{
    public static class FansService
    {

        internal static FanPrfileViewModel GetFanProfileAsAnonymousUser(User fan, int? seasonId)
        {
            var fpvm = new FanPrfileViewModel
            {
                Id = fan.UserId,
                UserName = fan.UserName,
                Image = fan.Image == null && fan.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(fan, seasonId).Image : fan.Image,
                FriendshipStatus = FriendshipStatus.No,
                Teams = TeamsService.GetFanTeamsWithStatistics(fan, seasonId),
                Friends = null
            };
            return fpvm;
        }

        internal static FanPrfileViewModel GetFanProfileAsLoggedInUser(User user, User fan, int? seasonId)
        {
            var Friends = FriendsService.GetAllFanFriends(fan.UserId, user.UserId);
            var fpvm = new FanPrfileViewModel
            {
                Id = fan.UserId,
                UserName = fan.UserName,
                Image = fan.Image == null && fan.UsersType.TypeRole == "players" ? PlayerService.GetPlayerProfile(fan, seasonId).Image : fan.Image,
                FriendshipStatus = FriendsService.AreFriends(user.UserId, fan.UserId),
                Teams = TeamsService.GetFanTeamsWithStatistics(fan, seasonId),
                NumberOfFriends = Friends.Count,
                NumberOfCommonFriends = Friends.Count(f => f.FriendshipStatus == FriendshipStatus.Yes)
            };

            fpvm.Friends = Friends.Select(u =>
                new FanFriendViewModel1
                {
                    Id = u.Id,
                    UserName = u.UserName,
                    Image = u.Image,
                    IsFriend = u.IsFriend,
                    FriendshipStatus = u.FriendshipStatus,
                    CanRcvMsg = u.CanRcvMsg,
                    FullName = u.FullName,
                    UserRole = u.UserRole,
                    Timestamp1 = u.Timestamp1
                }).ToList();

            return fpvm;
        }
    }
}