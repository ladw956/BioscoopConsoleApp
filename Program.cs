using System.Text.Json;
using BioscoopApplicatie.Controllers;
using BioscoopApplicatie.Models;
using BioscoopApplicatie.Services;

namespace BioscoopApplicatie;

public class Program
{
    private readonly Controller _controller = new();

    public static async Task Main()
    {
        Program program = new();
        await program.RunAsync();
    }

    private async Task RunAsync()
    {
        AppSettings settings;
        try
        {
            settings = LoadSettings();
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Configuratiefout: {exception.Message}");
            return;
        }

        CinemaApiClient api = new(settings.LaravelBaseUrl, settings.CinemaApiKey);

        while (true)
        {
            Console.Clear();
            PrintHeader();
            Console.WriteLine("Log in met een bestaand GameVault account voordat je de bioscoopcollectie kunt openen.");
            Console.WriteLine();

            string email = ReadRequired("GameVault e-mail (0 = afsluiten): ");
            if (email == "0")
            {
                return;
            }

            string password = ReadPassword("GameVault wachtwoord: ");

            CinemaAuthResponse authentication;
            try
            {
                authentication = await api.AuthenticateAsync(email, password);
            }
            catch (HttpRequestException exception)
            {
                Console.WriteLine();
                Console.WriteLine($"Kan Laravel niet bereiken: {exception.Message}");
                Console.WriteLine("Start eerst Laravel met: php artisan serve");
                Pause();
                continue;
            }
            catch (InvalidOperationException exception)
            {
                Console.WriteLine();
                Console.WriteLine(exception.Message);
                Console.WriteLine("De combinatie van e-mail en wachtwoord is niet geaccepteerd.");
                Pause();
                continue;
            }

            int age = _controller.ReadNumber("Wat is jouw leeftijd? ", 1, 120);
            bool exitApplication = await RunAuthenticatedMenuAsync(api, authentication, password, age);
            if (exitApplication)
            {
                return;
            }
        }
    }

    private async Task<bool> RunAuthenticatedMenuAsync(CinemaApiClient api, CinemaAuthResponse authentication, string password, int age)
    {
        while (true)
        {
            Console.Clear();
            PrintHeader();
            Console.WriteLine($"Ingelogd als: {authentication.User.Name} ({authentication.User.Email})");
            Console.WriteLine($"Leeftijd voor deze bioscoopsessie: {age}");
            Console.WriteLine();
            Console.WriteLine("1. Filmcollectie bekijken / film kiezen");
            Console.WriteLine("2. Mijn huidige filmkeuze");
            Console.WriteLine("3. Uitloggen");
            Console.WriteLine("0. Applicatie afsluiten");
            Console.WriteLine();

            int choice = _controller.ReadNumber("Keuze: ", 0, 3);

            switch (choice)
            {
                case 1:
                    CinemaSelectionData? selection = await OpenMovieCollectionAsync(
                        api,
                        authentication.User.Email,
                        password,
                        authentication.Selection);

                    if (selection is not null)
                    {
                        authentication.Selection = selection;
                    }
                    break;

                case 2:
                    ShowCurrentSelection(authentication.Selection);
                    break;

                case 3:
                    Console.WriteLine("Je bent uitgelogd.");
                    Pause();
                    return false;

                case 0:
                    return true;
            }
        }
    }

    private async Task<CinemaSelectionData?> OpenMovieCollectionAsync(
        CinemaApiClient api,
        string email,
        string password,
        CinemaSelectionData? currentSelection)
    {
        while (true)
        {
            Console.Clear();
            PrintHeader();
            Console.WriteLine("Filmcollectie");
            Console.WriteLine("---------------");

            List<CinemaMovie> movies;
            try
            {
                movies = await api.GetMoviesAsync();
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
            {
                Console.WriteLine($"De collectie kon niet worden geladen: {exception.Message}");
                Pause();
                return currentSelection;
            }

            if (movies.Count == 0)
            {
                Console.WriteLine("Er staan nog geen actieve films in de Laravel collectie.");
                Pause();
                return currentSelection;
            }

            for (int i = 0; i < movies.Count; i++)
            {
                CinemaMovie movie = movies[i];
                string year = movie.ReleaseYear?.ToString() ?? "?";
                string duration = movie.DurationMinutes is null ? "? min" : $"{movie.DurationMinutes} min";
                string genre = string.IsNullOrWhiteSpace(movie.Genre) ? "Onbekend genre" : movie.Genre;
                string rating = string.IsNullOrWhiteSpace(movie.AgeRating) ? "?" : movie.AgeRating;
                string selected = currentSelection?.MovieId == movie.Id ? "  [HUIDIGE KEUZE]" : string.Empty;

                Console.WriteLine($"{i + 1,2}. {movie.Title} | {year} | {genre} | {duration} | leeftijd {rating}{selected}");
            }

            Console.WriteLine();
            Console.WriteLine("0. Terug naar hoofdmenu");
            int choice = _controller.ReadNumber("Filmnummer: ", 0, movies.Count);

            if (choice == 0)
            {
                return currentSelection;
            }

            CinemaMovie selectedMovie = movies[choice - 1];

            try
            {
                CinemaSelectionResponse result = await api.SelectMovieAsync(email, password, selectedMovie.Id);
                currentSelection = result.Selection;
                Console.WriteLine();
                Console.WriteLine($"Geselecteerd: {result.Selection?.Title ?? selectedMovie.Title}");
                Console.WriteLine("De keuze is opgeslagen in Laravel en is zichtbaar in jouw GameVault account.");
                Pause();
            }
            catch (Exception exception) when (exception is HttpRequestException or InvalidOperationException)
            {
                Console.WriteLine();
                Console.WriteLine($"De keuze kon niet worden opgeslagen: {exception.Message}");
                Pause();
            }
        }
    }

    private static void ShowCurrentSelection(CinemaSelectionData? selection)
    {
        Console.Clear();
        PrintHeader();
        Console.WriteLine("Mijn huidige filmkeuze");
        Console.WriteLine("----------------------");

        if (selection is null)
        {
            Console.WriteLine("Je hebt nog geen film geselecteerd.");
        }
        else
        {
            Console.WriteLine(selection.Title);
            if (!string.IsNullOrWhiteSpace(selection.SelectedAt))
            {
                Console.WriteLine($"Opgeslagen: {selection.SelectedAt}");
            }
            if (!string.IsNullOrWhiteSpace(selection.Source))
            {
                Console.WriteLine($"Bron: {selection.Source.ToUpperInvariant()}");
            }
        }

        Pause();
    }

    private static void PrintHeader()
    {
        Console.WriteLine("Bioscoop Applicatie - Laravel connected school project");
        Console.WriteLine("======================================================");
        Console.WriteLine();
    }

    private static string ReadRequired(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? value = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }

            Console.WriteLine("Vul een geldige waarde in.");
        }
    }

    private static string ReadPassword(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            List<char> characters = new();

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }

                if (key.Key == ConsoleKey.Backspace)
                {
                    if (characters.Count > 0)
                    {
                        characters.RemoveAt(characters.Count - 1);
                        Console.Write("\b \b");
                    }
                    continue;
                }

                if (!char.IsControl(key.KeyChar))
                {
                    characters.Add(key.KeyChar);
                    Console.Write('*');
                }
            }

            string password = new(characters.ToArray());
            if (!string.IsNullOrWhiteSpace(password))
            {
                return password;
            }

            Console.WriteLine("Vul een wachtwoord in.");
        }
    }

    private static void Pause()
    {
        Console.WriteLine();
        Console.Write("Druk op Enter om verder te gaan...");
        Console.ReadLine();
    }

    private static AppSettings LoadSettings()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        if (!File.Exists(path))
        {
            path = "appsettings.json";
        }

        AppSettings? settings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(path), new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        });

        if (settings is null || string.IsNullOrWhiteSpace(settings.LaravelBaseUrl) || string.IsNullOrWhiteSpace(settings.CinemaApiKey))
        {
            throw new InvalidOperationException("appsettings.json mist LaravelBaseUrl of CinemaApiKey.");
        }

        return settings;
    }

    private class AppSettings
    {
        public string LaravelBaseUrl { get; set; } = string.Empty;
        public string CinemaApiKey { get; set; } = string.Empty;
    }
}
