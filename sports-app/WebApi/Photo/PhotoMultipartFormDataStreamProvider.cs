using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace WebApi.Photo
{
    public class PhotoMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
    {
        private string FileName { get; set; }

        public PhotoMultipartFormDataStreamProvider(string path, string fileName)
            : base(path)
        {
            this.FileName = fileName;
        }

        public override string GetLocalFileName(System.Net.Http.Headers.HttpContentHeaders headers)
        {
            return FileName.Trim(new char[] { '"' })
                        .Replace("&", "and");
        }
    }
}