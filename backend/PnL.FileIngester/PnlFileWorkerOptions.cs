namespace PnlFileIngester;

public class PnlFileWorkerOptions
{
    public const string Section = "PnlFileIngester";

    /// <summary>UTC time to run the job daily. Example: "06:00:00"</summary>
    public TimeOnly RunAtUtc { get; set; } 

    /// <summary>Full path to the PnL flat file.</summary>
    public string FilePath { get; init; } = string.Empty;

    /// <summary>Kafka bootstrap servers. Example: "localhost:9092"</summary>
    public string KafkaBootstrapServers { get; init; } = "localhost:9092";
}
