using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CmsApp.Models
{
    public class ImportPlayersImageViewModel
    {
        public ImportPlayersImageViewModel()
        {
        }

        public IEnumerable<HttpPostedFileBase> ImportFile { get; set; }

        public int SeasonId { get; set; }

        public ImportPlayersImage? ImportResult { get; set; }
    }

    public enum ImportPlayersImage
    {
        NoFiles,
        Completed,
        Error,
    }
}