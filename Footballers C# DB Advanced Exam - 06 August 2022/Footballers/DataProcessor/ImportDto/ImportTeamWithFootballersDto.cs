using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Footballers.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportTeamWithFootballersDto
    {
        [JsonProperty(nameof(Name))]
        [MinLength(3)]
        [MaxLength(40)]
        [RegularExpression(@"^([A-Za-z0-9\s.-]{3,})$")]
        public string Name { get; set; }


        [JsonProperty(nameof(Nationality))]
        [MinLength(2)]
        [MaxLength(40)]
        public string Nationality { get; set; }


        [JsonProperty(nameof(Trophies))]
        public string Trophies { get; set; }


        [JsonProperty(nameof(Footballers))]
        public int[] Footballers { get; set; }
    }
}

