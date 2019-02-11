using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BannerTest.Api.Models
{
    public class UpdateBannerModel
    {
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string Html { get; set; }
    }
}
