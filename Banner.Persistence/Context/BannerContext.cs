using BannerTest.Persistence.Context.Models;
using Microsoft.EntityFrameworkCore;

namespace BannerTest.Persistence.Context
{
    public class BannerContext : DbContext
    {
        public BannerContext(DbContextOptions<BannerContext> options) : base(options)
        { }

        public virtual DbSet<Banner> Banners { get; set; }
    }
}
