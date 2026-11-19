using System;
using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Services;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application
{
    class Program
    {
        static void Main(string[] args)
        {
            //The API key is read from user secrets or an environment variable (see README), never from the source code.
            //Environment variables are added last, so they win over user secrets.
            var configuration = new ConfigurationBuilder()
                .AddUserSecrets<Program>()
                .AddEnvironmentVariables()
                .Build();

            var serviceCollection = new ServiceCollection();
            //The ConfigureServices function configures the services.
            ConfigureServices(serviceCollection, configuration);

            //The RunServiceOperations function executes the code that can create the outputs.
            RunServiceOperations(serviceCollection);
        }

        private static void RunServiceOperations(IServiceCollection serviceCollection)
        {
            var serviceProvider = serviceCollection.BuildServiceProvider();

            try
            {
                //The services are resolved inside the try block: creating the HTTP client fails when the API key is missing.
                var screenOutputService = serviceProvider.GetRequiredService<IOutputService>();
                var fileOutputService = serviceProvider.GetRequiredService<IFileOutputService>();

                //The user sees which step is running. The planets are loaded once (and cached) by the first step.
                RunStep("Planets and their average moon gravity", screenOutputService.OutputAllPlanetsAndTheirAverageMoonGravityToConsole);
                RunStep("Moons and their mass", screenOutputService.OutputAllMoonsAndTheirMassToConsole);
                RunStep("Planets and their moons", screenOutputService.OutputAllPlanetsAndTheirMoonsToConsole);
                RunStep("Planets and their average moon temperature (console)", screenOutputService.OutputAllPlanetsAndTheirAverageMoonTemperatureToConsole);
                RunStep("Planets and their average moon temperature (file)", fileOutputService.OutputAllPlanetsAndTheirAverageMoonTemperatureToFile);
            }
            catch (Exception exception)
            {
                //The users and developers can see the thrown exceptions.
                //Dependency injection can wrap the original exception, so the innermost message is shown.
                var message = exception.GetBaseException().Message;
                Logger.Instance.Error($"{LoggerMessage.ScreenOutputOperationFailed}{message}");
                Console.WriteLine($"{ExceptionMessage.ScreenOutputOperationFailed}{message}");
                System.Diagnostics.Debug.WriteLine($"{ExceptionMessage.ScreenOutputOperationFailed}{message}");
            }

            serviceProvider.Dispose();
        }

        private static void RunStep(string title, Action step)
        {
            Console.WriteLine($"=== {title} ===");
            step();
            Console.WriteLine(OutputString.Done);
            Console.WriteLine();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            //The function configures all the services.
            XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()),
                new FileInfo(ConfigurationFileName.Logger));
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddHttpClient<HttpClientService>();
            serviceCollection.AddSingleton<IPlanetService, PlanetService>();
            serviceCollection.AddSingleton<IOutputService, ScreenOutputService>();
            serviceCollection.AddSingleton<IFileOutputService, FileOutputService>();
            serviceCollection.AddSingleton<IMoonService, MoonService>();
        }
    }
}
