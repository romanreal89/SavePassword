using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SavePassword.Core;
using System;

namespace SavePassword.Web.Filters
{
    public class DataContextAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!DataContext.GetInstance().IsSignedin)
                context.Result = new RedirectToActionResult("Logout", "Account", null);
        }
    }
}
