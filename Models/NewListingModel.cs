using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using DataAnnotationsExtensions;
using System.Web;
using System.Web.Mvc;
using AnarkRE.Filters;

using System.Web.Helpers;
using AnarkRE.ImageTools;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Threading;

namespace AnarkRE.Models
{
    public class NewListingModel : BaseListingModel
    {
        [Required]
        [StringLength(80, MinimumLength = 5, ErrorMessage = "Between 5-80 characters")]
        public string Title { get; set; }

        [Required]
        [StringLength(3520, MinimumLength = 50, ErrorMessage = "Between 50-3520 characters")]
        public string Description { get; set; }

        [Required]
        [Numeric(ErrorMessage="Numbers only")]
        [Min(0.001, ErrorMessage= "Price is too low")]
        public decimal Price { get; set; }


        [Required]
        [StringLength(3, ErrorMessage = "Three digit currency code", MinimumLength = 3)]
        [RegularExpression("^(USD|BTC|EUR|GBP|CAD|JPY|CNY|AUD)$", ErrorMessage = "Acceptable values are USD,EUR,GBP,CAD,JPY,CNY,AUD,BTC")]
        public string PegCurrency { get; set; }

        
        [Required]
        [MustBeTrue(ErrorMessage= "You must agree to the terms of service")]
        public bool AgreeToTerms { get; set; }

        

        
        [MustBeSelected(ErrorMessage = "Please select category")]
        public int? Category { set; get; }

        public SelectList Categories { set; get; }

        public int UserId { get; set; }

        
    }
}