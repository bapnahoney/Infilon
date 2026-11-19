using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Test_Taste_Console_Application.Constants;
using Test_Taste_Console_Application.Domain.Services.Interfaces;
using Test_Taste_Console_Application.Utilities;

namespace Test_Taste_Console_Application.Domain.Services
{
    /// <inheritdoc />
    public class FileOutputService : IFileOutputService
    {
        private readonly IPlanetService _planetService;

        public FileOutputService(IPlanetService planetService)
        {
            _planetService = planetService;
        }

        public void OutputAllPlanetsAndTheirAverageMoonTemperatureToFile()
        {
            //The planets come from the (cached) planet service, so no extra API requests are needed.
            var planets = _planetService.GetAllPlanets().Where(planet => planet.HasMoons()).ToArray();
            if (!planets.Any())
            {
                Console.WriteLine(OutputString.NoPlanetsWithMoonsFound);
                return;
            }

            Console.WriteLine(OutputString.WritingToFile);

            try
            {
                if (!Directory.Exists(PathName.PathToOutputFolder))
                {
                    Directory.CreateDirectory(PathName.PathToOutputFolder);
                    Console.WriteLine($"{OutputString.FolderCreated}{PathName.PathToOutputFolder}");
                }

                var filePath = Path.Combine(PathName.PathToOutputFolder,
                    PathName.AllPlanetsAndTheirAverageMoonTemperatureFile);

                //The file always uses the invariant culture (decimal point), so it can be read on any machine.
                //An unknown temperature is written as n/a (same as on the console).
                var lines = new List<string>
                {
                    "PlanetId,TotalMoons,AverageMoonTemperatureKelvin"
                };
                lines.AddRange(planets.Select(planet => string.Join(",",
                    planet.Id,
                    planet.Moons.Count.ToString(CultureInfo.InvariantCulture),
                    planet.AverageMoonTemperature.HasValue
                        ? planet.AverageMoonTemperature.Value.ToString("F2", CultureInfo.InvariantCulture)
                        : OutputString.TemperatureUnknown)));

                File.WriteAllLines(filePath, lines, Encoding.UTF8);
                Console.WriteLine($"{OutputString.FileCreated}{filePath}");
            }
            catch (Exception exception) when (exception is IOException ||
                                              exception is UnauthorizedAccessException ||
                                              exception is NotSupportedException)
            {
                //A file problem is reported, but does not stop the rest of the application.
                Logger.Instance.Error($"{LoggerMessage.FileOutputOperationFailed}{exception.Message}");
                Console.WriteLine($"{ExceptionMessage.FileOutputOperationFailed}{exception.Message}");
            }
        }
    }
}
