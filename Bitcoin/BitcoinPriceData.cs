using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Collections;

using System.Threading;
using AnarkRE;

namespace AnarkRE.Bitcoin
{
    public class BitcoinPriceData
    {
        static TimeSpan ExchangeRateCachetime = TimeSpan.FromMinutes(5);
     
        public static decimal MinerFee = 0.0001M;

        public string code { get; set; }
        public string name { get; set; }
        public double rate { get; set; }


        //private static BitcoinPriceData ExchangeRate = null;
        private static Dictionary<string, BitcoinPriceData> CurrencyExchangeRates = null;

        private static ReaderWriterLockSlim locking = new ReaderWriterLockSlim();
        private static volatile bool updateInProgress = false;
        private static volatile int fails = 0;
        private static int maxfails = 10;
        private static DateTime? Fetched = null;

        public static decimal GetBtcExchangeRate(string code)
        {
            locking.EnterUpgradeableReadLock();
            try
            {
                if (CurrencyExchangeRates == null || Fetched == null)
                {

                    Dictionary<string, BitcoinPriceData> dbpd = FetchData();
                    if (dbpd != null)
                    {

                        locking.EnterWriteLock();
                        CurrencyExchangeRates = dbpd;
                        locking.ExitWriteLock();
                        return Convert.ToDecimal(CurrencyExchangeRates[code].rate);
                    }
                    else if (CurrencyExchangeRates != null)
                    {
                        return Convert.ToDecimal(CurrencyExchangeRates[code].rate);
                    }
                    else
                        throw new Exception("Unable to get exchange rate");

                }
                else if (Fetched < DateTime.Now.Subtract(ExchangeRateCachetime))
                {
                    if (fails > maxfails)
                        throw new Exception("Failed to get exchange rate " + fails + " times");
                    else if (!updateInProgress)
                    {
                        updateInProgress = true;
                        Thread thread = new Thread(delegate()
                            {
                                Dictionary<string, BitcoinPriceData> bpd = FetchData();
                                if (bpd != null)
                                {
                                    fails = 0;
                                    locking.EnterWriteLock();
                                    CurrencyExchangeRates = bpd;
                                    locking.ExitWriteLock();
                                }
                                else
                                    fails++;
                                updateInProgress = false;
                            });
                        thread.Start();
                    }
                }

                return Convert.ToDecimal(CurrencyExchangeRates[code].rate);
            }
            finally
            {
                locking.ExitUpgradeableReadLock();
            }
        }

        public static decimal GetBalance(string address)
        {
            try
            {
                return decimal.Parse(new WebClient().DownloadString("https://blockchain.info/q/addressbalance/" + address)) / 10000000M;
            }
            catch
            {
                return 0;
            }
        }

        private static Dictionary<string, BitcoinPriceData> FetchData()
        {
          
            try
            {
                using (var wc = new WebClient())
                {
                    var json_data = string.Empty;
                    Dictionary<string, BitcoinPriceData> dbpd = null;

                    try
                    {
                        json_data = wc.DownloadString("https://bitpay.com/api/rates");
                        dbpd = !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<BitcoinPriceData[]>(json_data).ToDictionary(s => s.code) : null;
                    }
                    catch (Exception) {}


                    if (dbpd == null)
                    {
                        try
                        {
                            json_data = wc.DownloadString("https://api.bitcoinaverage.com/ticker/global/all");
                            if (!string.IsNullOrEmpty(json_data))
                            {
                                dynamic dynObj = JsonConvert.DeserializeObject(json_data);
                                JObject jobj = (JObject)dynObj;
                                var map = JsonConvert.DeserializeObject<Dictionary<string, object>>(jobj.ToString());
                                List<BitcoinPriceData> dlist = new List<BitcoinPriceData>();
                                foreach (string cur in map.Keys.Where(s => s.Length == 3))
                                    dlist.Add(new BitcoinPriceData() { code = cur, name = cur, rate = ((dynamic)map[cur]).last });
                                dbpd = dlist.ToDictionary(s => s.code);
                                //dbpd = !string.IsNullOrEmpty(json_data) ? JsonConvert.DeserializeObject<BitcoinPriceData[]>(json_data).ToDictionary(s => s.code) : null;
                            }
                        }
                        catch (Exception) { }
                    }


                   
                    if (dbpd != null)
                        Fetched = DateTime.Now;

                    return dbpd;
                }
            }
            catch
            {
                return null;
            }
        }

        public static string[] GetAllCodes()
        {
            if (CurrencyExchangeRates == null)
                GetBtcExchangeRate("USD");
            return CurrencyExchangeRates.Keys.ToArray();
        }


        public static decimal ToFiat(decimal btc, string currCode)
        {
            return (btc * GetBtcExchangeRate(currCode)).RoundTo(2);
        }

        public static decimal ToBTC(decimal currency, string currCode)
        {
            return (currency / GetBtcExchangeRate(currCode)).RoundTo(8);
        }
    }
    
}