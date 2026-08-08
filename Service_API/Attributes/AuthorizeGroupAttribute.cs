using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Entities.Models.ClassHelper;

namespace Service_API.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeGroupAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _group;

        public AuthorizeGroupAttribute(string group)
        {
            _group = group;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Check if user has the specific group claim
            var hasAccess = user.Claims.Any(c => c.Type == CustomClaims.Groups && c.Value == _group);

            if (!hasAccess)
            {
                context.Result = new ForbidResult(); 
            }
        }
    }
}
