using CountryhouseService.API.Defaults;
using CountryhouseService.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CountryhouseService.API.Data
{
    public static class Initializer
    {
        public static async Task InvokeAsync(RoleManager<IdentityRole> roleManager, AppDbContext db)
        {
            // Seed roles
            foreach (string roleName in UserRoleNames.namesArray)
                if (!db.Roles.Any(r => r.Name == roleName))
                    await roleManager.CreateAsync(
                        new IdentityRole { Name = roleName, NormalizedName = roleName.ToUpper() });

            // Seed ad statuses
            DbSet<AdStatus> adStatusSet = db.AdStatuses;
            foreach (string adStatusName in AdStatusNames.namesArray)
                if (!adStatusSet.Any(s => s.Name == adStatusName))
                    await adStatusSet.AddAsync(
                        new AdStatus { Name = adStatusName });

            // Seed request statuses
            DbSet<RequestStatus> requestStatusSet = db.RequestStatuses;
            foreach (string requestStatusName in RequestStatusNames.namesArray)
                if (!requestStatusSet.Any(s => s.Name == requestStatusName))
                    await requestStatusSet.AddAsync(
                        new RequestStatus { Name = requestStatusName });

            // Save changes
            await db.SaveChangesAsync();
        }
    }
}
