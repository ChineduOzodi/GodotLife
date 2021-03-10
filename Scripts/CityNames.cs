using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace RandomNameGen
{
    /// <summary>
    /// RandomName class, used to generate a random name.
    /// </summary>
    public class CityNames
    {  
        /// <summary>
        /// Class for holding the lists of names from names.json
        /// </summary>
        struct City
        {
            public String country;
            public String name;
        }

        Random rand;
        City[] cities;


        /// <summary>
        /// Initialises a new instance of the RandomName class.
        /// </summary>
        /// <param name="rand">A Random that is used to pick names</param>
        public CityNames(Random rand)
        {
            this.rand = rand;
            City[] l = new City[] { };

            JsonSerializer serializer = new JsonSerializer();

            using (StreamReader reader = new StreamReader("resources/cities.json"))
            using (JsonReader jreader = new JsonTextReader(reader))
            {
                l = serializer.Deserialize<City[]>(jreader);
            }

            cities = l;
        }

        /// <summary>
        /// Returns a new random name from given country
        /// </summary>
        /// <param name="sex">The sex of the person to be named. true for male, false for female</param>
        /// <param name="middle">How many middle names do generate</param>
        /// <param name="isInital">Should the middle names be initials or not?</param>
        /// <returns>The random name as a string</returns>
        public string Generate(String country)
        {
            string name = cities[rand.Next(cities.Length)].name;

            return name;
        }

    }

}