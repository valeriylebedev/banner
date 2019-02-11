using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BannerTest.Api.Models
{
    public class BannerModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Html { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
