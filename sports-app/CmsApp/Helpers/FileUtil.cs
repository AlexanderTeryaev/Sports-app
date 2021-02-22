using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Net;

namespace CmsApp.Helpers
{
    public class FileUtil
    {
        public bool IsValidFile(string fileName, string[] extensions)
        {
            string ext = Path.GetExtension(fileName).ToLower();
            return extensions.Contains(ext);
        }

        public static void DeleteFile(string filePath)
        {
            if(File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string GetUnicName(string fileName, int id)
        {
            string ext = Path.GetExtension(fileName);
            string randName = Path.GetRandomFileName();

            return string.Format("{0}_{1}", id, Path.GetFileNameWithoutExtension(randName) + ext);
        }

        public Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);

            ms.Write(imageBytes, 0, imageBytes.Length);
            Image image = Image.FromStream(ms, true);
            return image;
        }

        public Image GetFromStream(Stream stream)
        {
            return Image.FromStream(stream);
        }

        public void ResizeImg(Image img, int width, string savePath)
        {
            if (img.Width <= width)
            {
                img.Save(savePath);
                return;
            }

            double aspectRatio = img.Width / (double)img.Height;
            double targetHeight = Convert.ToDouble(width) / aspectRatio;

            Bitmap bmp = new Bitmap(width, (int)targetHeight);
            Graphics grp = Graphics.FromImage(bmp);
            grp.CompositingQuality = CompositingQuality.HighSpeed;
            grp.InterpolationMode = InterpolationMode.HighQualityBicubic;
            grp.CompositingMode = CompositingMode.SourceCopy;

            grp.DrawImage(img, new Rectangle(0, 0, bmp.Width, bmp.Height), new Rectangle(0, 0, img.Width, img.Height), GraphicsUnit.Pixel);

            var qual = GetQuality(90);

            ImageFormat format = img.RawFormat;
            ImageCodecInfo imgCodec = GetEncoderInfo("image/jpeg");

            bmp.Save(savePath, imgCodec, qual);
        }

        public EncoderParameters GetQuality(int val)
        {
            EncoderParameters encoderParameters;
            encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, val);
            return encoderParameters;
        }

        private static ImageCodecInfo GetEncoderInfo(string mimeType)
        {
            // Get image codecs for all image formats 
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();

            // Find the correct image codec 
            for (int i = 0; i < codecs.Length; i++)
                if (codecs[i].MimeType == mimeType)
                    return codecs[i];

            return null;
        } 

        public Image CropImg(Image img, double aspectRatioX, double aspectRatioY)
        {
            int imgWidth = img.Width;
            int imgHeight = img.Height;

            double aspectRatio = aspectRatioX / aspectRatioY;
            double maxAspect = (double)imgWidth / (double)imgHeight;

            double resizeMaxWidth = imgHeight / aspectRatio;
            double resizeMaxHeight = imgWidth / aspectRatio;

            int cropWidth = imgWidth;
            int cropHeight = (int)(imgWidth / aspectRatio);
            int cropX = 0, cropY = 0;

            if (maxAspect > aspectRatio && imgWidth > resizeMaxWidth)
            {
                cropHeight = imgHeight;
                cropWidth = (int)(imgHeight * aspectRatio);
                cropX = (imgWidth - cropWidth) / 2;
            }
            else if (maxAspect <= aspectRatio && imgHeight > resizeMaxHeight)
            {
                cropHeight = (int)resizeMaxHeight;
                cropWidth = (int)(resizeMaxHeight * aspectRatio);
                cropY = (imgHeight - cropHeight) / 2;
            }

            Bitmap bmp = new Bitmap(img);
            return bmp.Clone(new Rectangle(cropX, cropY, cropWidth, cropHeight), bmp.PixelFormat);
        }

        public Image SimpleCrop(string imgPath, int x, int y, int width, int height)
        {
            using(Bitmap bmp = new Bitmap(imgPath))
            {
                return bmp.Clone(new Rectangle(x, y, width, height), bmp.PixelFormat);
            }
        }


        public static string SaveSalaryReportFile(MemoryStream stream, int userId)
        {
            var fileName = $"SalaryReport_{userId}_{AppFunc.GetUniqName()}.pdf";

            var savePath = HttpContext.Current.Server.MapPath(GlobVars.ContentPath + "/salaryreports/");

            var di = new DirectoryInfo(savePath);
            if (!di.Exists)
            {
                di.Create();
            }

            var filePath = $"{savePath}{fileName}";

            using (var fileStream = File.Create(filePath))
            {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
            }

            return fileName;
        }

        public static string SaveFile(HttpPostedFileBase file, string path, string entityId, PlayerFileType fileType = PlayerFileType.Unknown)
        {
            if (file?.FileName == null)
            {
                return null;
            }

            var ext = Path.GetExtension(file.FileName).ToLower();

            if (!GlobVars.ValidImages.Contains(ext))
            {
                return null;
            }

            var newName = $"{fileType}_{entityId}_{AppFunc.GetUniqName()}{ext}";

            var di = new DirectoryInfo(path);
            if (!di.Exists)
                di.Create();

            // start security checking
            byte[] imgData;
            using (var reader = new BinaryReader(file.InputStream))
            {
                imgData = reader.ReadBytes(file.ContentLength);
            }
            File.WriteAllBytes(path + newName, imgData);
            return newName;
        }
    }
}