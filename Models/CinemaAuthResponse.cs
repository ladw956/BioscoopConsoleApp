using System.Text.Json.Serialization;

namespace BioscoopApplicatie.Models;

public class CinemaAuthResponse
{
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public CinemaUserData User { get; set; } = new();

    [JsonPropertyName("selection")]
    public CinemaSelectionData? Selection { get; set; }
}

public class CinemaUserData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("is_admin")]
    public bool IsAdmin { get; set; }
}
