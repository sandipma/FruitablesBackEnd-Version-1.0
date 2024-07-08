using FruitStoreModels.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CommonHelperUtility
{
    public class CustomAuthorizeAttribute : TypeFilterAttribute
    {
        public CustomAuthorizeAttribute(string role) : base(typeof(CustomAuthorizationFilter))
        {
            Arguments = new object[] { role };
        }
    }

    public class CustomAuthorizationFilter : IAuthorizationFilter
    {
        private readonly string _role;

        public CustomAuthorizationFilter(string role)
        {
            _role = role;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new JsonResult(new ApiResponse<string>(401, "Your session expired..kindly login"));
                context.HttpContext.Response.StatusCode = 401; 
                return;
            }

            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole != _role)
            {
                context.Result = new JsonResult(new ApiResponse<string>(403, "You do not have permission to access this resource !!"));
                context.HttpContext.Response.StatusCode = 403; 
                return;
            }
        }
    }
}
