using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Net.Http;
using System.Web.Http;
using AnarkRE.Models;

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
    }
}