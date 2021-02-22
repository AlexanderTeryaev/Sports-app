using System;
using System.Web;
using System.Web.Mvc;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;
using System.Web.SessionState;

namespace CmsApp.Helpers
{
    public class ImageResult : ActionResult, IRequiresSessionState
    {
        public override void ExecuteResult(ControllerContext ctx)
        {
            var res = ctx.HttpContext.Response;
            res.Clear();
            res.Cache.SetCacheability(HttpCacheability.NoCache);
            res.ContentType = "image/png";

            var v = new ImageVerification();
            v.BackColor = "#ffffff";
            v.TextColor = "#333333";
            v.TextFont = new Font("Lucida Console", 18, FontStyle.Bold);

            string code = v.CreateImageOnTheFly(270, 40);
            ctx.Controller.TempData["Captcha"] = code;
        }
        /// <summary>
        /// Summary description for Captcha
        /// </summary>
        public class ImageVerification
        {
            #region Private Properties

            /// <summary>
            /// Enumerate for the storing type of the passwords
            /// </summary>
            private string _BackColor = "#000000";
            private string _TextColor = "#ffffff";
            private Font _TextFont = new Font("Arial", 12);

            #endregion

            /// <summary>
            /// Creates the image on the fly and save it in the output of httpcontext
            /// </summary>
            /// <param name="width">Width of the image</param>
            /// <param name="height">Height of the image</param>
            public string CreateImageOnTheFly(int width, int height)
            {
                //string p = this.Password;
                var drawBrush = new SolidBrush(ColorTranslator.FromHtml(_TextColor));
                string p = SetRandomString(6, false);
                if (p != null)
                {
                    Random r = new Random();
                    Bitmap b = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                    Graphics g = Graphics.FromImage(b);

                    // set antialias for the text
                    g.TextRenderingHint = TextRenderingHint.AntiAlias;

                    // set the color
                    var txtColor = ColorTranslator.FromHtml(_TextColor);
                    var color = ColorTranslator.FromHtml(_BackColor);
                    g.Clear(color);

                    Matrix m = new Matrix();

                    for (int i = 0; i <= p.Length - 1; i++)
                    {
                        m.Reset();
                        int x = width / p.Length * i;
                        int y = height / 2;

                        //Rotate text Random
                        m.RotateAt(r.Next(-30, 30), new PointF(x, y));
                        g.Transform = m;

                        x += 8;

                        g.DrawString(p.Substring(i, 1), _TextFont, new SolidBrush(txtColor), x, r.Next(9, 9));

                        g.ResetTransform();
                    }

                    int noiseLevel = 650;
                    for (int i = 0; i < noiseLevel; i++)
                    {
                        int num = r.Next(0, 256);
                        int x = r.Next(0, width);
                        int y = r.Next(0, height);
                        b.SetPixel(x, y, Color.FromArgb(255, num, num, num));
                    }

                    // save the created image to the output stream of httpcontext
                    b.Save(HttpContext.Current.Response.OutputStream, ImageFormat.Png);

                    g.Dispose();
                    b.Dispose();
                }
                return p;
            }

            public Bitmap GenerateNoise(int width, int height)
            {
                Bitmap finalBmp = new Bitmap(width, height);
                Random r = new Random();

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int num = r.Next(0, 256);
                        finalBmp.SetPixel(x, y, Color.FromArgb(255, num, num, num));
                    }
                }

                return finalBmp;
            }

            /// <summary>
            /// Gets a generated random string
            /// </summary>
            /// <param name="size">Length of the string</param>
            /// <param name="lowerCase">If true, all the chars are in lowercase</param>
            public string SetRandomString(int size, bool lowerCase)
            {
                Random r = new Random();

                string input = ("abcdefghijklmnopqrstuvwxyz0123456789").ToUpper();
                var sb = new StringBuilder();
                char ch;
                for (int i = 0; i < size; i++)
                {
                    ch = input[r.Next(0, input.Length)];
                    sb.Append(ch);
                }

                return sb.ToString();
            }

            #region Public Properties

            /// <summary>
            /// Gets or sets the background color of the image
            /// </summary>
            public string BackColor
            {
                get { return _BackColor; }
                set { _BackColor = value; }
            }

            /// <summary>
            /// Gets or sets the font text
            /// </summary>
            public Font TextFont
            {
                get { return _TextFont; }
                set { _TextFont = value; }
            }
            /// <summary>
            /// Gets or sets the font color
            /// </summary>
            public string TextColor
            {
                get { return _TextColor; }
                set { _TextColor = value; }
            }
            #endregion
        }
    }
}