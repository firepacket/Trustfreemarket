using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using AnarkRE.Bitcoin;

namespace AnarkRE.Models
{
    public class Price
    {
        public string PegCurrency { get; set; }
        public decimal BTC { get; set; }
        public decimal USD { get; set; }
        public decimal Value { get; set; }

        public Price(decimal amount, string currency)
        {
            if (currency != "BTC" && !BitcoinPriceData.GetAllCodes().Any(s => s.ToUpper() == currency.ToUpper()))
                throw new Exception("Unsupported currency");
            
            
            PegCurrency = currency.ToUpper();
            Value = amount;

            if (PegCurrency == "BTC")
            {
                BTC = amount;
                USD = BitcoinPriceData.ToFiat(amount, "USD");
            }
            else if (PegCurrency == "USD")
            {
                BTC = BitcoinPriceData.ToBTC(amount, "USD");
                USD = amount;
            }
            else
            {
                BTC = BitcoinPriceData.ToBTC(amount, PegCurrency);
                USD = BitcoinPriceData.ToFiat(BTC, "USD");
            }
        }

        public decimal ToCurrency(string currency)
        {
            if (currency != "BTC" && !BitcoinPriceData.GetAllCodes().Any(s => s.ToUpper() == currency.ToUpper()))
                throw new Exception("Unsupported currency");

            if (currency == PegCurrency)
                return Value;
            else if (currency == "BTC")
                return BTC;
            else if (currency == "USD")
                return USD;
            else
                return BitcoinPriceData.ToFiat(BTC, currency);
        }

        public Price Add(Price price)
        {
            return new Price(price.BTC + this.BTC, "BTC");
        }

        public bool CheckValid(out List<string> errs)
        {
            errs = new List<string>();
            if (USD >= 922337203685477M)
                errs.Add("USD value too large");
            if (Value > 922337203685477M)
                errs.Add(PegCurrency + " value too large");
            if (Value == 0)
                errs.Add("Value too small");
            if (USD < Globals.MinListingPriceUsd)
                errs.Add("Value must be greater than " + Globals.MinListingPriceUsd);
            if (BTC >= 21000000)
                errs.Add("Bitcoin value too large");

            return errs.Count == 0;
        }
    }
}