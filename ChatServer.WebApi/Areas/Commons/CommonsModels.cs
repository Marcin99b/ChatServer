namespace ChatServer.WebApi.Areas.Commons
{
    public record Entity 
    { 
        public Guid Id { get; } = Guid.NewGuid();
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}
