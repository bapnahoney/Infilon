namespace Test_Taste_Console_Application.Constants
{
    public static class ExceptionMessage
    {
        public const string FileOutputOperationFailed = "The file output operation failed with the following error: ";

        public const string ScreenOutputOperationFailed =
            "The screen output operation failed with the following error: ";

        public const string MissingApiKey =
            "No API key found. Set the environment variable SOLAR_SYSTEM_API_KEY or the user secret " +
            "'SolarSystemApi:ApiKey'. See the README for details.";

        public const string InvalidApiKey =
            "The API rejected the request (HTTP 401 Unauthorized). The API key is missing, invalid or expired. " +
            "Check the value of SOLAR_SYSTEM_API_KEY / the user secret 'SolarSystemApi:ApiKey'.";
    }
}
