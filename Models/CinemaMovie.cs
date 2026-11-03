using System.Text.Json.Serialization;

namespace BioscoopApplicatie.Models;

public class CinemaMovie
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("release_year")]
    public int? ReleaseYear { get; set; }

    [JsonPropertyName("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [JsonPropertyName("genre")]
    public string? Genre { get; set; }

    [JsonPropertyName("age_rating")]
    public string? AgeRating { get; set; }
}

public class CinemaMovieListResponse
{
    [JsonPropertyName("data")]
    public List<CinemaMovie> Data { get; set; } = new();
}
