using ChatServer.WebApi.Areas.Commons;

namespace ChatServer.WebApi.Areas.WebRtc
{
    public record SdpData(
        string sdp,
        string type);

    public record Candidate(
        string candidate,
        int sdpMLineIndex,
        string sdpMid,
        string usernameFragment);

    public record WebRtcRoom(Guid RoomId, SdpData Offer) : Entity
    {
        public List<Candidate> OfferCandidates { get; } = new();
        public List<Candidate> AnswerCandidates { get; } = new();
        public SdpData? Answer { get; set; }
    }

    public record TwillioIceServersResponse(
        string username,
        Ice_Servers[] ice_servers,
        string date_updated,
        string account_sid,
        string ttl,
        string date_created,
        string password);

    public record Ice_Servers(
        string url,
        string urls,
        string username,
        string credential);

    public record GetIceServersRequest(Guid RoomId);
    public record GetIceServersResponse(Ice_Servers[] IceServers);

    public record CreateRoomRequest(Guid RoomId, SdpData Offer);
    public record CreateRoomResponse(WebRtcRoom RtcRoom);

    public record AddCandidateRequest(Guid WebRtcRoomId, Candidate Candidate);
    public record AddCandidateResponse;

    public record SetAnswerRequest(Guid WebRtcRoomId, SdpData Answer);
    public record SetAnswerResponse;

    public record NotifyCallerAboutRoomConfiguredRequest(Guid RoomId);
    public record NotifyCallerAboutRoomConfiguredResponse;

    public record NotifyReceiverAboutRoomConfiguredRequest(Guid RoomId);
    public record NotifyReceiverAboutRoomConfiguredResponse;
}
