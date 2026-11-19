using System.Collections.Generic;
using Newtonsoft.Json;

namespace Test_Taste_Console_Application.Domain.DataTransferObjects
{
    public class PlanetDto
    {
        public string Id { get; set; }
        public float SemiMajorAxis { get; set; }

        //Average temperature in Kelvin. The API sends 0 (or nothing) when the value is unknown, so it is nullable.
        public float? AvgTemp { get; set; }
        public ICollection<MoonDto> Moons { get; set; }
    }
}