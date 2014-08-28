using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Globalization;
using System.Web.Security;

namespace AnarkRE.Models
{
    public class FeedbackModel
    {
        [Required]
        [Range(1,5)]
        public int Score { get; set; }

        [Required]
        [MaxLength(300)]
        public string Message { get; set; }

        [Required]
        public string EscrowId { get; set; }
    }
}