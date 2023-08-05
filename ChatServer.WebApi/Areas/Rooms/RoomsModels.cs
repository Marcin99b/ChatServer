using ChatServer.WebApi.Areas.Commons;

namespace ChatServer.WebApi.Areas.Rooms
{
    public record Room(Guid CallingUserId, Guid ReceivingUserId) : Entity;
    public record CallProposition(Guid CallingUserId, Guid ReceivingUserId) : Entity
    {
        public bool IsAccepted { get; set; }
    }

    public record ProposeCallRequest(Guid ReceivingUserId);
    public record ProposeCallResponse;

    public record AcceptCallRequest(Guid CallingUserId);
    public record AcceptCallResponse(Guid CreatedRoomId);
}
