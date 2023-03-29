using System.Text.Json.Serialization;

namespace AnakinRaW.ExternalUpdater;

public record UpdateInformation
{
    [JsonPropertyName("update")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public FileCopyInformation? Update { get; init; }

    [JsonPropertyName("backup")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BackupInformation? Backup { get; init; }
}

public record FileCopyInformation
{
    [JsonPropertyName("file")]
    public required string File { get; init; }

    [JsonPropertyName("destination")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Destination { get; init; }
}


public record BackupInformation
{
    [JsonPropertyName("destination")]
    public required string Destination { get; init; }

    [JsonPropertyName("source")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Source { get; init; }
}