namespace Test_Taste_Console_Application.Constants
{
    /// <summary>
    /// Names of the configuration values the application reads. The API key is never stored in the source code.
    /// </summary>
    public static class ConfigurationKey
    {
        //Environment variable (works on every OS and in CI).
        public const string ApiKeyEnvironmentVariable = "SOLAR_SYSTEM_API_KEY";

        //Key used with "dotnet user-secrets set".
        public const string ApiKeyUserSecret = "SolarSystemApi:ApiKey";
    }
}
