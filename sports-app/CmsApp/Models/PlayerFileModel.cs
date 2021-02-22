using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class PlayerFileModel
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int? SeasonId { get; set; }
        public int FileType { get; set; }
        public string FileName { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsArchive { get; set; }
    }
}