using CountryhouseService.API.Defaults;
using CountryhouseService.API.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CountryhouseService.API.Helpers
{
    public static class ControllerHelpers
    {
        public static async Task<bool> IsAdminAsync(UserManager<User> userManager, ClaimsPrincipal userClaims)
        {
            User currentUser = await userManager.GetUserAsync(userClaims);
            return await userManager.IsInRoleAsync(currentUser, UserRoleNames.ADMIN);
        }


        public static bool IsAdAuthor(Ad ad, ClaimsPrincipal userClaims) =>
            userClaims.FindFirstValue(ClaimTypes.NameIdentifier) == ad.AuthorId;


        public static bool IsRequestAuthor(Request request, ClaimsPrincipal userClaims) =>
            request.ContractorId == userClaims.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
