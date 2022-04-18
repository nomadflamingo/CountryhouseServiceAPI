using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CountryhouseService.Tests.Helpers
{
    public static class ControllerTestExtensions
    {
        public static T WithIdentity<T>(this T controller, string nameIdentifier, string name) where T : ControllerBase
        {
            // Ensure that the http context for controller is set up
            controller.EnsureHttpContext();

            Claim[] claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, nameIdentifier),
                new Claim(ClaimTypes.Name, name)
            };
            ClaimsIdentity identity = new(claims, "TestAuthentication");
            ClaimsPrincipal principal = new(identity);

            controller.ControllerContext.HttpContext.User = principal;

            return controller;
        }

        public static T WithAnonymousIdentity<T>(this T controller) where T : ControllerBase
        {
            // Ensure that the http context for controller is set up
            controller.EnsureHttpContext();

            var principal = new ClaimsPrincipal(new ClaimsIdentity());

            controller.ControllerContext.HttpContext.User = principal;

            return controller;
        }

        private static T EnsureHttpContext<T>(this T controller) where T : ControllerBase
        {
            if (controller.ControllerContext == null)
            {
                controller.ControllerContext = new ControllerContext();
            }

            if (controller.ControllerContext.HttpContext == null)
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext();
            }

            return controller;
        }
    }
}
