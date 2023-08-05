using ChatServer.WebApi.Areas.Commons;

namespace ChatServer.WebApi.Areas.Users
{
    public record User(string Username) : Entity;
    public record UserRoomDetails(User user, bool IsActive);

    public record RegisterRequest(string Username);
    public record RegisterResponse();

    public record LoginRequest(string Username);
    public record LoginResponse();

    public record GetUsersListRequest();
    public record GetUsersListResponse(List<UserRoomDetails> Users);

    public record GetUserRequest(Guid UserId);
    public record GetUserResponse(User User);

    public record GetCurrentUserRequest();
    public record GetCurrentUserResponse(User User);
}
