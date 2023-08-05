using ChatServer.WebApi.Areas.Commons;
using ChatServer.WebApi.Consts;
using Microsoft.AspNetCore.Authorization;

namespace ChatServer.WebApi.Auth
{
    public class SessionCheckHandler : AuthorizationHandler<SessionCheckRequirement>
    {
        private readonly IUsersSessionsStorage usersSessionsStorage;
        private readonly IHttpContextAccessor httpContextAccessor;

        public SessionCheckHandler(IUsersSessionsStorage usersSessionsStorage, IHttpContextAccessor httpContextAccessor)
        {
            this.usersSessionsStorage = usersSessionsStorage;
            this.httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SessionCheckRequirement requirement)
        {
            var request = this.httpContextAccessor.HttpContext?.Request;
            if (request == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var accessToken = request!.Cookies[AuthConsts.ACCESS_TOKEN_COOKIE];
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                context.Fail();
                return Task.CompletedTask;
            }
            var session = this.usersSessionsStorage.Get(accessToken!);
            if (session == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (session.UserAgent != request.Headers[AuthConsts.USER_AGENT_HEADER])
            {
                context.Fail();
                return Task.CompletedTask;
            }
            if (session.IpAddress != request.HttpContext.Connection.RemoteIpAddress!.ToString())
            {
                context.Fail();
                return Task.CompletedTask;
            }
            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
