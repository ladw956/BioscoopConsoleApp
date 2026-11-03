using System.Net.Http.Json;
using System.Text.Json;
using BioscoopApplicatie.Models;

namespace BioscoopApplicatie.Services;

public class CinemaApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CinemaApiClient(string baseUrl, string apiKey)
    {
        _httpClient = new HttpClient { BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/") };
        _apiKey = apiKey;
    }

    public async Task<CinemaAuthResponse> AuthenticateAsync(string email, string password)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, "api/cinema/auth");
        request.Content = JsonContent.Create(new { email, password });

        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        EnsureSuccess(response, body);

        return JsonSerializer.Deserialize<CinemaAuthResponse>(body, _jsonOptions)
            ?? throw new InvalidOperationException("Laravel returned an empty authentication response.");
    }

    public async Task<List<CinemaMovie>> GetMoviesAsync()
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Get, "api/cinema/movies");
        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        EnsureSuccess(response, body);

        CinemaMovieListResponse? result = JsonSerializer.Deserialize<CinemaMovieListResponse>(body, _jsonOptions);
        return result?.Data ?? new List<CinemaMovie>();
    }

    public async Task<CinemaSelectionResponse> SelectMovieAsync(string email, string password, int movieId)
    {
        using HttpRequestMessage request = CreateRequest(HttpMethod.Post, "api/cinema/select");
        request.Content = JsonContent.Create(new { email, password, movie_id = movieId });

        using HttpResponseMessage response = await _httpClient.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        EnsureSuccess(response, body);

        return JsonSerializer.Deserialize<CinemaSelectionResponse>(body, _jsonOptions)
            ?? throw new InvalidOperationException("Laravel returned an empty selection response.");
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        HttpRequestMessage request = new(method, path);
        request.Headers.Add("X-Cinema-App-Key", _apiKey);
        request.Headers.Accept.ParseAdd("application/json");
        return request;
    }

    private static void EnsureSuccess(HttpResponseMessage response, string body)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string message = body;
        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("message", out JsonElement element))
            {
                message = element.GetString() ?? body;
            }
            else if (document.RootElement.TryGetProperty("errors", out JsonElement errors))
            {
                message = errors.ToString();
            }
        }
        catch (JsonException)
        {
        }

        throw new InvalidOperationException($"Laravel API error ({(int)response.StatusCode}): {message}");
    }
}
