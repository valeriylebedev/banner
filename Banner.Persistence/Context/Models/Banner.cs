using System;

namespace BannerTest.Persistence.Context.Models
{
    public class Banner
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Html { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Modified { get; set; }
    }
}
