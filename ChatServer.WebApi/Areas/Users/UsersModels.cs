using ChatServer.WebApi.Areas.Commons;

namespace ChatServer.WebApi.Areas.Users
{
    public record User(string Username) : Entity;

    public record RegisterRequest(string Username);
    public record RegisterResponse();

    public record LoginRequest(string Username);
    public record LoginResponse();

    public record GetUsersListRequest();
    public record GetUsersListResponse(List<User> Users);

    public record GetUserRequest(Guid UserId);
    public record GetUserResponse(User User);
}
