using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Models
{
    public class MessageBindingModel
    {
        public string Body { get; set; }
    }

    public class TeamMessageBindingModel : MessageBindingModel
    {
        public int TeamId { get; set; }
    }

    public class GameMessageBindingModel : MessageBindingModel
    {
        public int GameId { get; set; }
    }

    public class WallMessageReplyBindingModel : MessageBindingModel
    {
        public int ThreadId { get; set; }
    }

    public class WallThreadViewModel
    {
        public int ThreadId { get; set; }
        public MessageBaseViewModel MainMessage { get; internal set; }
        public IEnumerable<MessageBaseViewModel> Replies { get; internal set; }
        public int CreaterId { get; internal set; }
    }

    public class MessageBaseViewModel
    {
        public string Body { get; internal set; }
        public DateTime Date { get; internal set; }
        public string SenderFullName { get; internal set; }
        public int SenderId { get; internal set; }
        public string SenderUserName { get; internal set; }
        public int Type { get; internal set; }
    }
}