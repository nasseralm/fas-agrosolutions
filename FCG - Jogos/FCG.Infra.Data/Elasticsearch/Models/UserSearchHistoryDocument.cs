namespace FCG.Infra.Data.Elasticsearch.Models
{
    public class UserSearchHistoryDocument
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int UsuarioId { get; set; }
        public string SearchTerm { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string SessionId { get; set; }
        public int ResultCount { get; set; }
        public List<string> FoundGenres { get; set; } = new List<string>();
        public List<string> FoundDevelopers { get; set; } = new List<string>();
        public List<string> FoundPlatforms { get; set; } = new List<string>();
        public List<string> FoundGameNames { get; set; } = new List<string>();
    }
}