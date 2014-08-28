using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Drawing;
using System.Web.Helpers;
using System.Web.Hosting;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace AnarkRE.ImageTools
{
    public static class ImageHandler
    {
        public static Bitmap ResizeWithCropping(Image image, int width, int height)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)width / (float)sourceWidth);
            nPercentH = ((float)height / (float)sourceHeight);

            if (nPercentH < nPercentW)
            {
                nPercent = nPercentW;
                destY = (int)((height - (sourceHeight * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentH;
                destX = (int)((width - (sourceWidth * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(width,
                    height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(image.HorizontalResolution,
                    image.VerticalResolution);

            using (var grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.Clear(Color.White);

                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.SmoothingMode = SmoothingMode.HighQuality;
                grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;

                grPhoto.DrawImage(image,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }
            bmPhoto.MakeTransparent(Color.White);
            return bmPhoto;
        }

        public static Bitmap ResizeWithoutCropping(Image image, int width, int height)
        {
            int sourceWidth = image.Width;
            int sourceHeight = image.Height;

            var currentRatio = ((float)sourceWidth / (float)sourceHeight);
            if (height == 0 && width > 0)
            {
                //resize to width
                height = (int)(width / currentRatio);
            }
            if (width == 0 && height > 0)
            {
                //resize to height
                width = (int)(height * currentRatio);
            }

            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)width / (float)sourceWidth);
            nPercentH = ((float)height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = Convert.ToInt16((width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = Convert.ToInt16((height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            var bmPhoto = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var grPhoto = Graphics.FromImage(bmPhoto))
            {
                grPhoto.Clear(Color.White);

                grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;
                grPhoto.SmoothingMode = SmoothingMode.HighQuality;
                grPhoto.PixelOffsetMode = PixelOffsetMode.HighQuality;
                grPhoto.DrawImage(image,
                    new Rectangle(destX, destY, destWidth, destHeight),
                    new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                    GraphicsUnit.Pixel);
            }
            bmPhoto.MakeTransparent(Color.White);
            return bmPhoto;
        }
    }
}