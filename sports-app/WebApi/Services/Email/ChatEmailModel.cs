using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApi.Services.Email
{
    public class ChatEmailModel
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Sender { get; set; }
        public int SenderId { get; set; }
        public string ImgName { get; set; }
        public string ParentName { get; set; }
        public int ParentId { get; set; }
        public DateTime Date { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        // Cheng Li. : Add url of image, video
        public string ImgUrl { get; set; }
        public string VideoUrl { get; set; }
    }
}