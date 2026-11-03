using System.Text.Json.Serialization;

namespace BioscoopApplicatie.Models;

public class CinemaSelectionResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("selection")]
    public CinemaSelectionData? Selection { get; set; }
}

public class CinemaSelectionData
{
    [JsonPropertyName("movie_id")]
    public int MovieId { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("user_email")]
    public string UserEmail { get; set; } = string.Empty;

    [JsonPropertyName("selected_at")]
    public string? SelectedAt { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }
}
