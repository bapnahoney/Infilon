# Solar System console application

Console application that reads the `bodies` of the [Solar System OpenData API](https://api.le-systeme-solaire.net/en/)
and writes them to the console (and, for the temperature list, to a CSV file).

Outputs:

1. All planets and their average moon gravity (existing, still a stub that returns 0)
2. All moons and their mass (existing)
3. All planets and their moons (existing)
4. **New:** all planets that have at least one moon, with the average temperature of their moons (console and
   `FileOutput/AllPlanetsAndTheirAverageMoonTemperature.csv`, next to the executable)

**Assumption (given by the interviewer):** the average temperature of a planet's moons is the `avgTemp` of the planet
itself. The report therefore only *shows* that value for every planet that has at least one moon. (The API has no
temperature for moons: `avgTemp` is `0`, i.e. unknown, for all 479 of them.)

## Requirements

* .NET 10 SDK (`net10.0`, the current LTS). To use another installed version (e.g. .NET 8), change `<TargetFramework>` in the `.csproj`; the code and packages work unchanged.
* A free API key (since API v2.0.0 every request needs one).

## Getting an API key

1. Open <https://api.le-systeme-solaire.net/generatekey.html> (use the "En" switch for English).
2. Enter your e-mail address, solve the captcha and click "Obtenir ma clé". You receive a GUID.

## Configuring the key (it is never stored in the source code)

The key is sent as `Authorization: Bearer <key>`. It is read from **either**:

**Option A - user secrets** (stored outside the repository, in your user profile):

```bash
cd Test-Taste-Console-Application
dotnet user-secrets set "SolarSystemApi:ApiKey" "<your key>"
```

**Option B - environment variable** (wins over user secrets when both are set):

```bash
# Windows (PowerShell, current session)
$env:SOLAR_SYSTEM_API_KEY = "<your key>"
# Linux / macOS
export SOLAR_SYSTEM_API_KEY="<your key>"
```

In Visual Studio you can also use *Project > Manage User Secrets* or set the variable in
*Debug > Properties > Environment variables*. If the key is missing or rejected (HTTP 401) the application
stops with an explanatory message.

## Running

```bash
cd Test-Taste-Console-Application
dotnet run
```

The application prints every step it is in (`Loading planets...`, `Loading moon details of jupiter...`,
`Writing data to file...`, `Done.`). Loading takes a while because every moon is requested separately.

## Design decisions

**Domain model**

* `PlanetDto.AvgTemp` / `Planet.AvgTemp` (`float?`, Kelvin): the temperature did not exist in the model. It is nullable
  because the API returns `0` (or nothing) when the temperature is unknown. 0 K is impossible, so `Planet` maps
  `<= 0` to `null` instead of showing a fake 0.
* `Planet.AverageMoonTemperature` (`float?`) implements the assumption of the exercise: for a planet with moons it is
  the planet's own `AvgTemp`; for a planet without moons, or with an unknown temperature, it is `null` and the outputs
  show `n/a`. Keeping the rule in the domain object means the console and the CSV can't disagree.
* `Planet.HasMoons()` (existing) is used to list only planets with at least one moon.
* Why not average the moons' own temperatures? The API has no moon temperatures (all 479 are `0`), so every average
  would be `n/a`. If the API ever gets them, only `AverageMoonTemperature` has to change.

**API interface / services**

* `HttpClientService` adds the Bearer header from configuration and fails fast with a clear message when the key is
  missing. `EnsureAuthorised` turns HTTP 401 into an explicit error; before, a wrong key only showed up as
  "No planets found.".
* The planets query now also asks for `avgTemp` (`data=...,avgTemp`), so the temperature arrives with the planets, in
  the same single request.
* A planet only holds *references* to its moons (`id` and `rel`); `PlanetService` (existing behaviour) requests the
  details of every moon. It now keeps a moon in the list with unknown values when its request fails, instead of
  crashing or returning garbage, and catches network and JSON errors per moon.
* `PlanetService` is a singleton and now **caches** the loaded planets. Before, every output that needs planets repeated all moon requests (one request per moon, so several hundred in total); now they are made once per run.
* `FileOutputService` (new, behind `IFileOutputService`) writes the CSV, so file output is separated from the
  console output. Both outputs reuse the same loaded data. File problems are logged and reported; they do not stop
  the application.
* `Program.cs`: configuration (user secrets + environment variables), dependency injection for the new service, step
  messages, and a fix for a compile error in the original `Debug.WriteLine` line.

**Not changed:** `AverageMoonGravity` is still the original stub (returns 0), as it is outside this exercise.
