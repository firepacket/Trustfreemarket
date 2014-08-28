using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;
using DataAnnotationsExtensions;

namespace AnarkRE.Models
{
    public class EscrowWebCreate
    {
        [Required]
        [StringLength(450, ErrorMessage = "Public Key must be 217 digits", MinimumLength = 450)]
        public string PubKey { get; set; }

        [Required]
        [StringLength(3788, ErrorMessage = "Bad length. Keys must be 2048 bits", MinimumLength = 3604)]
        [RegularExpression("^[a-z0-9]+$", ErrorMessage = "Only lowercase hexadecimal characters allowed")]
        public string EncPrivKey { get; set; }

        
        [Integer(ErrorMessage = "Must be an integer")]
        public int? ShippingId { get; set; }

        [Integer(ErrorMessage = "Must be an integer")]
        public int? VariationId { get; set; }
    }

    public class EscrowWebAccept
    {
        [Required]
        [StringLength(450, ErrorMessage = "Public Key must be 217 digits", MinimumLength = 450)]
        public string PubKey { get; set; }

        [Required]
        [StringLength(3788, ErrorMessage = "Bad length. Keys must be 2048 bits", MinimumLength = 3604)]
        [RegularExpression("^[a-z0-9]+$", ErrorMessage = "Only lowercase hexadecimal characters allowed")]
        public string EncPrivKey { get; set; }

        [Required]
        [StringLength(34, ErrorMessage = "Bitcoin address must be between 34-27 characters long.", MinimumLength = 27)]
        [RegularExpression("^[13][1-9A-HJ-NP-Za-km-z]{26,33}$", ErrorMessage = "Bitcoin address is invalid")]
        public string BitcoinAddr { get; set; }

        [Required]
        [StringLength(344, ErrorMessage = "Encrypted Pinv should be 344 characters", MinimumLength = 344)]
        [RegularExpression("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$", ErrorMessage = "Only Base64 encoding allowed")]
        public string PinvEncBuyer { get; set; }

        [StringLength(344, ErrorMessage = "Encrypted Pinv should be 344 characters", MinimumLength = 344)]
        [Required]
        [RegularExpression("^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$", ErrorMessage = "Only Base64 encoding allowed")]
        public string PinvEncSeller { get; set; }
    }
}