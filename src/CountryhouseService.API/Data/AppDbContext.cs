using CountryhouseService.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

#nullable disable
namespace CountryhouseService.API.Data
{
    public sealed class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Ad> Ads { get; set; }
        public DbSet<AdStatus> AdStatuses { get; set; }
        public DbSet<AdImage> AdImages { get; set; }
        public DbSet<Avatar> Avatars { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<RequestStatus> RequestStatuses { get; set; }

    }
}
