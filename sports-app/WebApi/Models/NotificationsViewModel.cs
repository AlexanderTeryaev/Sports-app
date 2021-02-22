using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebApi.Models
{
    public class NotificationsViewModel
    {
        public int MsgId { get; set; }
        public string Message { get; set; }
        public DateTime SendDate { get; set; }
        public bool IsRead { get; set; }
    }
}