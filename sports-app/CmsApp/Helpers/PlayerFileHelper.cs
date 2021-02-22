using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CmsApp.Helpers
{
    public static class PlayerFileHelper
    {
        public static string SaveFile(HttpPostedFileBase file, int id, PlayerFileType fileType = PlayerFileType.Unknown)
        {
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
            {
                return null;
            }

            var newName = $"{fileType}_{id}_{AppFunc.GetUniqName()}{ext}";

            var savePath = HttpContext.Current.Server.MapPath(GlobVars.ContentPath + "/players/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
                di.Create();

            // start security checking
            byte[] imgData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }
            File.WriteAllBytes(savePath + newName, imgData);
            return newName;
        }
    }
}