using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Test_Taste_Console_Application.Constants;

namespace Test_Taste_Console_Application.Domain.Services
{
    ///<summary>
    /// A service to create the HttpClient. 
    ///</summary>
    public class HttpClientService
    {
        public HttpClient Client { get; }

        public HttpClientService(HttpClient client, IConfiguration configuration)
        {
            //The HTTP client is configured in the constructor.
            Client = client;
            Client.BaseAddress = new Uri(UriPath.BaseUri);
            Client.DefaultRequestHeaders.Accept.Add(new
                MediaTypeWithQualityHeaderValue(HttpClientSettings.JsonType));

            //Since version 2.0.0 of the API every request needs an API key in the "Authorization: Bearer <key>" header.
            //The key is read from configuration (environment variable or user secrets), never from the source code.
            var apiKey = configuration[ConfigurationKey.ApiKeyEnvironmentVariable]
                         ?? configuration[ConfigurationKey.ApiKeyUserSecret];

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException(ExceptionMessage.MissingApiKey);
            }

            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
        }

        /// <summary>
        /// Stops the application with a clear message when the API answers 401. Without this a wrong key would
        /// only show up as an empty result ("No planets found.").
        /// </summary>
        public void EnsureAuthorised(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(ExceptionMessage.InvalidApiKey);
            }
        }
    }
}
