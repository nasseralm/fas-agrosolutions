namespace FCG.Domain.DTOs
{
    public class UserPreferencesDto
    {
        public int UsuarioId { get; set; }
        public List<string> TopSearchTerms { get; set; } = new List<string>();
        public List<string> TopGenres { get; set; } = new List<string>();
        public List<string> TopDevelopers { get; set; } = new List<string>();
        public List<string> TopPlatforms { get; set; } = new List<string>();
        public List<string> TopGameNames { get; set; } = new List<string>();
        public int TotalSearches { get; set; }
        public DateTime LastSearchDate { get; set; }
        public TimeSpan LastSearchAgo => DateTime.UtcNow - LastSearchDate;
    }
}