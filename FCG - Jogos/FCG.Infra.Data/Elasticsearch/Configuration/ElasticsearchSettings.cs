namespace FCG.Infra.Data.Elasticsearch.Configuration
{
    public class ElasticsearchSettings
    {
        public string Uri { get; set; }
        public string DefaultIndex { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}