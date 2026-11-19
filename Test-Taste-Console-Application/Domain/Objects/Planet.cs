using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Test_Taste_Console_Application.Domain.DataTransferObjects;

namespace Test_Taste_Console_Application.Domain.Objects
{
    public class Planet
    {
        public string Id { get; set; }
        public float SemiMajorAxis { get; set; }
        public ICollection<Moon> Moons { get; set; }
        public float AverageMoonGravity
        {
            get => 0.0f;
        }

        /// <summary>
        /// Average temperature of the planet in Kelvin, null when unknown. The API sends 0 for "unknown", and
        /// 0 K (absolute zero) is physically impossible, so 0 (or less) is mapped to null.
        /// </summary>
        public float? AvgTemp { get; set; }

        /// <summary>
        /// The average temperature (Kelvin) of the moons of this planet. By assumption (given in the exercise) this
        /// is the average temperature of the planet itself: the moons are at the same distance from the sun.
        /// (The API has no temperature for the moons: it returns 0 for all of them.)
        /// Null when the planet has no moons or when its temperature is unknown, so "unknown" is never shown as a number.
        /// </summary>
        public float? AverageMoonTemperature
        {
            get => HasMoons() ? AvgTemp : null;
        }

        public Planet(PlanetDto planetDto)
        {
            Id = planetDto.Id;
            SemiMajorAxis = planetDto.SemiMajorAxis;
            AvgTemp = planetDto.AvgTemp.HasValue && planetDto.AvgTemp.Value > 0 ? planetDto.AvgTemp : null;
            Moons = new Collection<Moon>();
            if(planetDto.Moons != null)
            {
                foreach (MoonDto moonDto in planetDto.Moons)
                {
                    Moons.Add(new Moon(moonDto));
                }
            }
        }

        public Boolean HasMoons()
        {
            return (Moons != null && Moons.Count > 0);
        }
    }
}
