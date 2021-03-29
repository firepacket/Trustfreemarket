using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net.Http;
using AnarkRE.Models;
using System.Drawing;
using System.IO;
using System.Web.Mvc;
using System.Collections.Concurrent;
using System.Web.UI;


namespace AnarkRE
{
    public static class Extensions
    {

        public static bool IsLocal(this HttpRequestMessage request)
        {
            var localFlag = request.Properties["MS_IsLocal"] as Lazy<bool>;
            return localFlag != null && localFlag.Value;
        }

        public static decimal TruncateDecimals(this decimal value, int precision)
        {
            decimal step = (decimal)Math.Pow(10, precision);
            int tmp = (int)Math.Truncate(step * value);
            return tmp / step;
        }

        public static double ToDouble(this decimal value)
        {
            return Convert.ToDouble(value);
        }

        public static decimal RoundTo(this decimal value, int decimals)
        {
            return Math.Round(value, decimals);
        }

        public static string StringWithoutDashes(this Guid value)
        {
            return value.ToString().Replace("-", "");
        }

        public static decimal Normalize(this decimal value)
        {
            return value / 1.000000000000000000000000000000000m;
        }

        public static string Truncate(this string data, int maxlen)
        {
            if (data.Length > maxlen)
                return data.Remove(maxlen, data.Length - maxlen);
            else
                return data;
        }

        public static string[] Split(this string txt, string delimiter)
        {
            return txt.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string FormatMoney(this decimal amount)
        {
            return String.Format("{0:C}", amount).Replace(")", "").Replace("(", "-");
        }

        public static string FormatNumber(this int amount)
        {
            string samount = amount.ToString();
            StringBuilder sb = new StringBuilder(samount);
            if (amount.ToString().Length > 3)
                for (int i = samount.Length - 3; i > 0; i -= 3)
                    sb.Insert(i, ",");
            return sb.ToString();
        }

        public static string FormatNumber(this decimal amount)
        {
            string samount = amount.ToString();
            string dec = string.Empty;
            bool dot = samount.Contains('.');
            if (dot)
            {
                string[] split = samount.Split(new char[] { '.' }, StringSplitOptions.None);
                samount = split[0];
                dec = split[1];
            }
            StringBuilder sb = new StringBuilder(samount);
            if (amount.ToString().Length > 3)
                for (int i = samount.Length - 3; i > 0; i -= 3)
                    sb.Insert(i, ",");

            StringBuilder dsb = new StringBuilder(dec);
            for (int i = dec.Length - 1; i >= 0; i--)
                if (dec[i] == '0')
                    dsb.Remove(i, 1);
                else break;

            return sb.ToString() + (dsb.Length > 0 ? "." + dsb.ToString() : string.Empty);
        }

        public static string Delimit<T>(this T[] array, string delimiter)
        {
            if (array is string[])
                return String.Join(delimiter, array as string[]);
            List<string> strings = new List<string>();
            foreach (T item in array)
                strings.Add(item.ToString());
            return String.Join(delimiter, strings.ToArray());
        }

        public static List<T>[] Divide<T>(this List<T> list, int totalPartitions)
        {
            if (totalPartitions < 1)
                throw new ArgumentOutOfRangeException("totalPartitions");

            List<T>[] partitions = new List<T>[totalPartitions];

            int maxSize = (int)Math.Ceiling(list.Count / (double)totalPartitions);
            int k = 0;

            for (int i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>();
                for (int j = k; j < k + maxSize; j++)
                {
                    if (j >= list.Count)
                        break;
                    partitions[i].Add(list[j]);
                }
                k += maxSize;
            }

            return partitions;
        }
        public static byte[] StringToByteArray(this string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }


        public static List<T>[] Partition<T>(this List<T> list, int partSize)
        {
            if (list == null)
                throw new ArgumentNullException("list");

            int count = (int)Math.Ceiling(list.Count / (double)partSize);
            List<T>[] partitions = new List<T>[count];

            int k = 0;
            for (int i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>(partSize);
                for (int j = k; j < k + partSize; j++)
                {
                    if (j >= list.Count)
                        break;
                    partitions[i].Add(list[j]);
                }
                k += partSize;
            }

            return partitions;
        }

        public static Bitmap Crop(this Bitmap bitmap, Color? fill)
        {
            int w = bitmap.Width;
            int h = bitmap.Height;

            Func<int, bool> IsAllWhiteRow = row =>
            {
                for (int i = 0; i < w; i++)
                {
                    if (bitmap.GetPixel(i, row).R != 255)
                    {
                        return false;
                    }
                }
                return true;
            };

            Func<int, bool> IsAllWhiteColumn = col =>
            {
                for (int i = 0; i < h; i++)
                {
                    if (bitmap.GetPixel(col, i).R != 255)
                    {
                        return false;
                    }
                }
                return true;
            };

            int leftMost = 0;
            for (int col = 0; col < w; col++)
            {
                if (IsAllWhiteColumn(col)) leftMost = col + 1;
                else break;
            }

            int rightMost = w - 1;
            for (int col = rightMost; col > 0; col--)
            {
                if (IsAllWhiteColumn(col)) rightMost = col - 1;
                else break;
            }

            int topMost = 0;
            for (int row = 0; row < h; row++)
            {
                if (IsAllWhiteRow(row)) topMost = row + 1;
                else break;
            }

            int bottomMost = h - 1;
            for (int row = bottomMost; row > 0; row--)
            {
                if (IsAllWhiteRow(row)) bottomMost = row - 1;
                else break;
            }

            if (rightMost == 0 && bottomMost == 0 && leftMost == w && topMost == h)
            {
                return bitmap;
            }

            int croppedWidth = rightMost - leftMost + 1;
            int croppedHeight = bottomMost - topMost + 1;

            try
            {
                //Bitmap target = new Bitmap(croppedWidth, croppedHeight);
                Bitmap target = fill.HasValue ? new Bitmap(w, h) : new Bitmap(croppedWidth, croppedHeight);


                using (Graphics g = Graphics.FromImage(target))
                {
                    if (fill.HasValue)
                    {
                        using (SolidBrush brush = new SolidBrush(fill.Value))
                        {
                            g.FillRectangle(brush, 0, 0, w, h);
                        }

                        g.DrawImage(bitmap,
                            new RectangleF(leftMost, topMost, croppedWidth, croppedHeight),
                            new RectangleF(leftMost, topMost, croppedWidth, croppedHeight),
                            GraphicsUnit.Pixel);
                    }
                    else
                        g.DrawImage(bitmap,
                            new RectangleF(0, 0, croppedWidth, croppedHeight),
                            new RectangleF(leftMost, topMost, croppedWidth, croppedHeight),
                            GraphicsUnit.Pixel);
                }
                return target;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Values are top={0} bottom={1} left={2} right={3}", topMost, bottomMost, leftMost, rightMost), ex);
            }
        }

        public static string PropertiesToString(this object obj)
        {
            var props = obj.GetType().GetProperties();
            var sb = new StringBuilder();
            foreach (var p in props)
            {
                sb.Append(p.Name + "=" + p.GetValue(obj, null) + " ");
            }
            return sb.ToString();
        }

        public static decimal ToCurrency(this decimal value, string curTo, string curFrom)
        {
            return new Price(value, curFrom).ToCurrency(curTo);
        }

        public static T ToEnum<T>(this string enumString)
        {
            return (T)Enum.Parse(typeof(T), enumString);
        }

        public static IHtmlString AntiForgeryTokenValue(this HtmlHelper htmlHelper)
        {
            var field = htmlHelper.AntiForgeryToken().ToHtmlString();
            var beginIndex = field.IndexOf("value=\"") + 7;
            var endIndex = field.IndexOf("\"", beginIndex);
            return new HtmlString(field.Substring(beginIndex, endIndex - beginIndex));
        }

        public static bool Remove<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> self, TKey key)
        {
            string d = "sds";
            
            return ((IDictionary<TKey, TValue>)self).Remove(key);
        }

        public static bool IsEqualTo<T>(this T source, params T[] list)
        {
            if (null == source) throw new ArgumentNullException("source");
            return list.Contains(source);
        }

        public static void ForEach<T>(this IEnumerable<T> @enum, Action<T> mapFunction)
        {
            foreach (var item in @enum) mapFunction(item);
        }

        public static string ToReadableString(this TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0} day{1}, ", span.Days, span.Days == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0} hour{1}, ", span.Hours, span.Hours == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0} minute{1}, ", span.Minutes, span.Minutes == 1 ? String.Empty : "s") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0} second{1}", span.Seconds, span.Seconds == 1 ? String.Empty : "s") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 seconds";

            return formatted;
        }

        public static string RenderView(this Controller controller, string viewName, object model)
        {
            return RenderView(controller, viewName, new ViewDataDictionary(model));
        }

        public static string RenderView(this Controller controller, string viewName, ViewDataDictionary viewData)
        {
            var controllerContext = controller.ControllerContext;

            var viewResult = ViewEngines.Engines.FindView(controllerContext, viewName, null);

            StringWriter stringWriter;

            using (stringWriter = new StringWriter())
            {
                var viewContext = new ViewContext(
                    controllerContext,
                    viewResult.View,
                    viewData,
                    controllerContext.Controller.TempData,
                    stringWriter);

                viewResult.View.Render(viewContext, stringWriter);
                viewResult.ViewEngine.ReleaseView(controllerContext, viewResult.View);
            }

            return stringWriter.ToString();
        }

        public static string RenderToString(this PartialViewResult partialView)
        {
            var httpContext = HttpContext.Current;
            if (httpContext == null)
            {
                throw new NotSupportedException("An HTTP context is required to render the partial view to a string");
            }

            var controllerName = httpContext.Request.RequestContext.RouteData.Values["controller"].ToString();
            var controller = (ControllerBase)ControllerBuilder.Current.GetControllerFactory().CreateController(httpContext.Request.RequestContext, controllerName);
            var controllerContext = new ControllerContext(httpContext.Request.RequestContext, controller);
            var view = ViewEngines.Engines.FindPartialView(controllerContext, partialView.ViewName).View;

            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                using (var tw = new HtmlTextWriter(sw))
                {
                    view.Render(new ViewContext(controllerContext, view, partialView.ViewData, partialView.TempData, tw), tw);
                }
            }

            return sb.ToString();
        }

        public static string RenderRazorViewToString(this Controller controller, string viewName, object model)
        {
            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }


        public static string ToProperCase(this string str)
        {
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower().Trim());
        }

        public static void Set<T>(this T obj, Action<T> act) { act(obj); }

        public static TReturn NullOr<TIn, TReturn>(this TIn obj, Func<TIn, TReturn> func,
        TReturn elseValue = default(TReturn)) where TIn : class
        { return obj != null ? func(obj) : elseValue; }


    }
}