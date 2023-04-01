using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Boardgames.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportSellerDto
    {
        [Required]
        [MinLength(5)]
        [MaxLength(20)]
        [JsonProperty(nameof(Name))]
        public string Name { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(30)]
        [JsonProperty(nameof(Address))]

        public string Address { get; set; }

        [Required]
        [JsonProperty(nameof(Country))]
        public string Country { get; set; }

        [Required]
        [RegularExpression(@"^(w{3}.)([A-Za-z0-9-])+(.com)$")]
        [JsonProperty(nameof(Website))]
        public string Website { get; set; }

        [JsonProperty(nameof(Boardgames))]
        public int[] Boardgames { get; set; }
    }
}
