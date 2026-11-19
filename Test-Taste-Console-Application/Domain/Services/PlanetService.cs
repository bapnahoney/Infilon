using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Newtonsoft.Json;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.DataTransferObjects;
using Test_Taste_Console_Application.Domain.DataTransferObjects.JsonObjects;
using Test_Taste_Console_Application.Domain.Objects;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class PlanetService : IPlanetService
    {
        private readonly HttpClientService _httpClientService;

        public PlanetService(HttpClientService httpClientService)
        {
            _httpClientService = httpClientService;
        }

        //The service is a singleton, so the planets (and the many moon requests behind them) are loaded only once
        //and then shared by every output.
        private List<Planet> _cachedPlanets;

        public IEnumerable<Planet> GetAllPlanets()
        {
            if (_cachedPlanets != null) return _cachedPlanets;

            var planets = LoadPlanets();

            //An empty result (e.g. after a failed request) is not cached, so a next call can try again.
            if (planets.Count > 0) _cachedPlanets = planets;

            return planets;
        }

        private List<Planet> LoadPlanets()
        {
            var allPlanetsWithTheirMoons = new List<Planet>();

            Logger.Instance.Info(OutputString.LoadingPlanets);
            var response = _httpClientService.Client
                .GetAsync(UriPath.GetAllPlanetsWithMoonsQueryParameters)
                .Result;

            //A 401 means the API key is wrong. That is reported to the user instead of returning an empty list.
            _httpClientService.EnsureAuthorised(response);

            //If the status code isn't 200-299, then the function returns an empty collection.
            if (!response.IsSuccessStatusCode)
            {
                Logger.Instance.Warn($"{LoggerMessage.GetRequestFailed}{response.StatusCode}");
                return allPlanetsWithTheirMoons;
            }

            var content = response.Content.ReadAsStringAsync().Result;

            //The JSON converter uses DTO's, that can be found in the DataTransferObjects folder, to deserialize the response content.
            var results = JsonConvert.DeserializeObject<JsonResult<PlanetDto>>(content);

            //The JSON converter can return a null object. 
            if (results?.Bodies == null) return allPlanetsWithTheirMoons;

            foreach (var planet in results.Bodies)
            {
                //The planet only contains references to its moons. The details of every moon (mass,
                //average temperature, ...) have to be requested one by one.
                if (planet.Moons != null)
                {
                    Logger.Instance.Info(
                        $"{OutputString.LoadingMoonDetails}{planet.Id} ({planet.Moons.Count} moons)...");

                    var newMoonsCollection = new Collection<MoonDto>();
                    foreach (var moon in planet.Moons)
                    {
                        newMoonsCollection.Add(GetMoonDetails(moon));
                    }

                    planet.Moons = newMoonsCollection;
                }

                //Planets without moons stay in the collection, the outputs decide what to show (HasMoons()).
                allPlanetsWithTheirMoons.Add(new Planet(planet));
            }

            return allPlanetsWithTheirMoons;
        }

        /// <summary>
        /// Requests the details of one moon. When that fails (other than a 401), the moon reference is returned
        /// instead: the moon stays in the list, only its mass and temperature are unknown. One failing moon must
        /// not break the output for all the other moons.
        /// </summary>
        private MoonDto GetMoonDetails(MoonDto moonReference)
        {
            try
            {
                var moonResponse = _httpClientService.Client
                    .GetAsync(UriPath.GetMoonByIdQueryParameters + moonReference.URLId)
                    .Result;

                _httpClientService.EnsureAuthorised(moonResponse);

                if (!moonResponse.IsSuccessStatusCode)
                {
                    Logger.Instance.Warn(
                        $"{LoggerMessage.MoonRequestFailed}{moonReference.Id} ({moonResponse.StatusCode})");
                    return moonReference;
                }

                var moonContent = moonResponse.Content.ReadAsStringAsync().Result;
                return JsonConvert.DeserializeObject<MoonDto>(moonContent) ?? moonReference;
            }
            //.Result wraps network errors (HttpRequestException, timeouts) in an AggregateException.
            catch (Exception exception) when (exception is AggregateException || exception is JsonException)
            {
                Logger.Instance.Warn($"{LoggerMessage.MoonRequestFailed}{moonReference.Id}: {exception.Message}");
                return moonReference;
            }
        }

        private static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

            for (int i = 0; i < normalizedString.Length; i++)
            {
                char c = normalizedString[i];
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder
                .ToString()
                .Normalize(NormalizationForm.FormC);
        }
    }
}
