using System.Text.Json.Serialization;
using System.Text.Json;

namespace sherlock_cs;

internal class SherlockEntry
{
    [JsonPropertyName("urlMain")] public required string BaseUrl { get; set; }
    [JsonPropertyName("url")] public required string Url { get; set; }
    [JsonPropertyName("urlProbe")] public string? ProbeUrl { get; set; }
    [JsonPropertyName("headers")] public Dictionary<string, string>? Headers { get; set; }
    [JsonPropertyName("regexCheck")] public string? UsernamePattern { get; set; }
    [JsonPropertyName("request_method")] public string? RequestMethod { get; set; }
    [JsonPropertyName("isNSFW")] public bool Nsfw { get; set; }
    [JsonPropertyName("errorType")] public required string ErrorType { get; set; }
    [JsonPropertyName("errorMsg")] public JsonElement? ErrorMessage { get; set; }
    [JsonPropertyName("errorCode")] public int? ErrorCode { get; set; }
    [JsonPropertyName("request_payload")] public Dictionary<string, JsonElement>? Payload { get; set; }
}

internal enum QueryResult { Unknown, Claimed, Illegal, Available }