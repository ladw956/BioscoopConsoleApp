# Bioscoop Applicatie - Laravel connected

This remains a separate .NET 8 console application. Laravel owns the GameVault users, cinema collection and selected movie; the C# app communicates through JSON HTTP endpoints.

## Login and menu flow

1. The C# application asks for an existing GameVault e-mail and password.
2. `POST /api/cinema/auth` verifies the password against Laravel's hashed account password.
3. Invalid credentials stay on the login screen;
4. After login the app stays in a menu loop until the user logs out or exits.
5. The movie collection has `0 = back` so choosing a movie is no longer a one-shot program flow, (the program now resides within a loop).

[Simple security :]
6. Saving a selection sends the same credentials again, so an API-key holder cannot select a movie merely by typing another user's e-mail.
7. The password is kept only in process memory during the current console session and is never written to `appsettings.json` or another file.

## Connection

1. In Laravel `.env`:
   `CINEMA_APP_API_KEY=school-demo-cinema-key`
2. Start Laravel:
   `php artisan serve`
3. Keep `appsettings.json` pointing to `http://127.0.0.1:8000` with the same API key.
4. Register a GameVault user or use the seeded demo account.
5. Run:
   `dotnet run`
6. Log in with the GameVault e-mail and password.

The movie collection itself is managed only from Laravel Admin > Cinema collection and does not use an external movie API.
