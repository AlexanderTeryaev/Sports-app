using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using WebApi.Models;

namespace WebApi.Photo
{
    public class LocalPhotoManager
    {

        private string workingFolder { get; set; }
        private string fileName { get; set; }

        public LocalPhotoManager()
        {

        }

        public LocalPhotoManager(string workingFolder, string fileName)
        {
            this.workingFolder = workingFolder;
            this.fileName = fileName;
            CheckTargetDirectory();
        }

        public async Task<IEnumerable<PhotoViewModel>> Add(HttpRequestMessage request)
        {
            var provider = new PhotoMultipartFormDataStreamProvider(this.workingFolder, this.fileName);
            
            await request.Content.ReadAsMultipartAsync(provider);
   
            var photos = new List<PhotoViewModel>();

            foreach(var file in provider.FileData)
            {
                var fileInfo = new FileInfo(file.LocalFileName);

                photos.Add(new PhotoViewModel
                {
                    Name = fileInfo.Name,
                    Created = fileInfo.CreationTime,
                    Modified = fileInfo.LastWriteTime,
                    Size = fileInfo.Length /1024
                });                
            }

            return photos;            
        }

        public async void Delete(string fileName)
        {
            try
            {
                var filePath = Directory.GetFiles(this.workingFolder, fileName)
                                .FirstOrDefault();

                await Task.Factory.StartNew(() =>
                {
                    File.Delete(filePath);
                });

            }
            catch (Exception ex)
            {
            }
        }

        public bool FileExists(string fileName)
        {
            var file = Directory.GetFiles(this.workingFolder, fileName)
                                .FirstOrDefault();

            return file != null;
        }

        private void CheckTargetDirectory()
        {            
            if (!Directory.Exists(this.workingFolder))
            {
                throw new ArgumentException("the destination path " + this.workingFolder + " could not be found");
            }
        }
    }
}