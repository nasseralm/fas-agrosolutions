namespace FCG.API.Configuration
{
    /// <summary>
    /// Configurações do OpenTelemetry para telemetria da aplicação
    /// </summary>
    public class OpenTelemetrySettings
    {
        /// <summary>
        /// Nome do serviço para identificação nas métricas e traces
        /// </summary>
        public string ServiceName { get; set; } = "FCG.API";

        /// <summary>
        /// Versão do serviço
        /// </summary>
        public string ServiceVersion { get; set; } = "1.0.0";

        /// <summary>
        /// Habilita o exportador de console para desenvolvimento
        /// </summary>
        public bool EnableConsoleExporter { get; set; } = false;

        /// <summary>
        /// Habilita o exportador Azure Application Insights
        /// </summary>
        public bool EnableApplicationInsights { get; set; } = true;

        /// <summary>
        /// Connection String do Azure Application Insights
        /// </summary>
        public string ApplicationInsightsConnectionString { get; set; } = string.Empty;

        /// <summary>
        /// Instância única do OpenTelemetrySettings
        /// </summary>
        public static readonly string SectionName = "OpenTelemetry";
    }
}