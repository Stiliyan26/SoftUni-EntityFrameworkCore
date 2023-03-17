using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theatre.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportTheatreWithTicketsDto
    {
        [Required]
        [MinLength(4)]
        [MaxLength(30)]
        [JsonProperty("Name")]
        public string Name { get; set; }

        [Required]
        [Range(typeof(sbyte), "1", "10")]
        [JsonProperty("NumberOfHalls")]
        public sbyte NumberOfHalls { get; set; }

        [Required]
        [MinLength(4)]
        [MaxLength(30)]
        [JsonProperty("Director")]
        public string Director { get; set; }

        [JsonProperty("Tickets")]
        public ImportTicket[] Tickets { get; set; }
    }
}
