using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using DataAnnotationsExtensions;
using System.Web;
using System.Web.Mvc;
using System.Drawing.Imaging;
using System.Threading;
using System.Web.Helpers;
using AnarkRE.ImageTools;
using System.Drawing;
using System.IO;

using AnarkRE.Filters;

namespace AnarkRE.Models
{
    public class BaseListingModel
    {
        [Required(ErrorMessage = "Please add a picture")]
        [HttpPostedFileExtensions(ErrorMessage = "Only png,jpg,jpeg,gif allowed")]
        public HttpPostedFileBase Picture { get; set; }

        private Image img = null;

        public void ProcessPicture(Guid listId)
        {
            string id = listId.StringWithoutDashes();
            if (img != null)
            {
                //Image img = Image.FromStream(Picture.InputStream);
                //Thread thread = new Thread(delegate()
                //{
                    string imageDir = Globals.PicturePath;
                    string picDir = Path.Combine(imageDir, id);

                    Bitmap large = ImageHandler.ResizeWithoutCropping(img, 870, 420);
                    

                    if (!Directory.Exists(picDir))
                        Directory.CreateDirectory(picDir);

                    int num = 0;

                    while (File.Exists(Path.Combine(picDir, num + "_l.png")))
                        num++;

                    if (num <= Globals.ListAddOpts[ListingAdditionType.Shipping] - 1)
                    {
                        large.Save(Path.Combine(picDir, num + "_l.png"), ImageFormat.Png);
                        if (num == 0)
                        {
                            Bitmap med = ImageHandler.ResizeWithoutCropping(img, 270, 200);
                            med.Save(Path.Combine(picDir, num + "_m.png"), ImageFormat.Png);

                            Bitmap small = ImageHandler.ResizeWithoutCropping(img, 100, 74);
                            small.Save(Path.Combine(picDir, num + "_s.png"), ImageFormat.Png);
                        }
                    }
                //});

                //thread.IsBackground = true;
                //thread.Start();

            }
        }

        public bool CheckImageValidAndPrep()
        {
            bool goodimg = true;
            if (Picture == null
                || Picture.InputStream == null
                || Picture.InputStream.Length < 128)
                    goodimg = false;

            if (goodimg)
            {
                try
                {
                    img = Image.FromStream(Picture.InputStream);
                }
                catch
                {
                    goodimg = false;
                }
            }

            return goodimg;
        }
    }
}