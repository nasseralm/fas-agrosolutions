namespace FCG.Domain.DTOs
{
    public class PopularGamesResponse
    {
        public List<PopularGameItem> TopGamesByName { get; set; } = new List<PopularGameItem>();
        public List<PopularGenreItem> TopGenres { get; set; } = new List<PopularGenreItem>();
        public int TotalSearches { get; set; }
        public int UniqueUsers { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class PopularGameItem
    {
        public string GameName { get; set; }
        public int SearchCount { get; set; }
        public double PopularityScore { get; set; }
        public List<string> RelatedGenres { get; set; } = new List<string>();
    }

    public class PopularGenreItem
    {
        public string Genre { get; set; }
        public int SearchCount { get; set; }
        public double PopularityScore { get; set; }
        public List<string> TopGamesInGenre { get; set; } = new List<string>();
    }
}