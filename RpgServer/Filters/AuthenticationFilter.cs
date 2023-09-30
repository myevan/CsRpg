using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RpgServer.Services;

namespace RpgServer.Filters
{
    public class AuthenticationFilter : IAuthorizationFilter
    {
        public AuthenticationFilter(ContextService ctx)
        {
            _ctx = ctx;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            if (!_ctx.IsAuthorized())
            {
                context.Result = new OkObjectResult(new
                {
                    Msg = "NOT_AUTHRORIZED"
                });
            }
        }

        private readonly ContextService _ctx;
    }
}
