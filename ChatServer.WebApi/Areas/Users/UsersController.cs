using ChatServer.WebApi.Areas.Commons;
using ChatServer.WebApi.Auth;
using ChatServer.WebApi.Consts;
using ChatServer.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ChatServer.WebApi.Areas.Users
{
    public class UsersController : V1ControllerBase
    {
        private readonly Repository repository;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IUsersSessionsStorage usersSessionsStorage;
        private readonly IAuthManager authManager;
        private readonly CookieOptions cookieOptions;

        public UsersController(
            Repository repository, 
            IOptions<CookieOptions> cookieOptions, 
            IHttpContextAccessor httpContextAccessor, 
            IUsersSessionsStorage usersSessionsStorage,
            IAuthManager authManager)
        {
            this.repository = repository;
            this.httpContextAccessor = httpContextAccessor;
            this.usersSessionsStorage = usersSessionsStorage;
            this.authManager = authManager;
            this.cookieOptions = cookieOptions.Value;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<RegisterResponse> Register([FromBody] RegisterRequest request)
        {
            var user = this.repository.Users.FirstOrDefault(x => x.Username == request.Username);
            if(user != null)
            {
                throw new ArgumentException("Username exist");
            }
            var newUser = new User(request.Username.Trim());
            this.repository.Users.Add(newUser);
            await Task.CompletedTask;
            return new RegisterResponse();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<LoginResponse> Login([FromBody] LoginRequest request)
        {
            var user = this.repository.Users.First(x => x.Username == request.Username);
            var token = this.authManager.CreateToken(user.Id, "user").AccessToken;

            this.AddCookie(AuthConsts.ACCESS_TOKEN_COOKIE, token);

            var ip = this.Request.HttpContext.Connection.RemoteIpAddress!.ToString();
            var userAgent = this.Request.Headers[AuthConsts.USER_AGENT_HEADER].ToString();
            var now = DateTime.UtcNow;

            var session = new UserSession(user.Id, token, now, ip, userAgent);
            usersSessionsStorage.Add(session);
            await Task.CompletedTask;
            return new LoginResponse();
        }

        [HttpPost]
        public async Task<GetUsersListResponse> GetUsersList([FromBody] GetUsersListRequest request)
        {
            var currentUser = this.User.GetUserId();
            var users = repository.Users
                .Where(x => x.Id != currentUser)
                .Select(x => new UserRoomDetails(x, repository.Connections.Any(c => c.UserId == x.Id)))
                .ToList();
            await Task.CompletedTask;
            return new GetUsersListResponse(users);
        }

        [HttpPost]
        public async Task<GetUserResponse> GetUser([FromBody] GetUserRequest request)
        {
            var user = this.repository.Users.First(x => x.Id == request.UserId);
            await Task.CompletedTask;
            return new GetUserResponse(user);
        }

        [HttpPost]
        public async Task<GetCurrentUserResponse> GetCurrentUser([FromBody] GetCurrentUserRequest request)
        {
            var id = this.User.GetUserId();
            var user = this.repository.Users.First(x => x.Id == id);
            await Task.CompletedTask;
            return new GetCurrentUserResponse(user);
        }

        private void AddCookie(string key, string value) => this.Response.Cookies.Append(key, value, this.cookieOptions);
        private void DeleteCookie(string key) => this.Response.Cookies.Delete(key, this.cookieOptions);
        private string GetCookieValue(string key) => this.httpContextAccessor.HttpContext!.Request.Cookies[key]!;
    }
}
