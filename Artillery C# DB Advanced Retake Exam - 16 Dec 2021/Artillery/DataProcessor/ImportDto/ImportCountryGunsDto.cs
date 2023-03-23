using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artillery.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportCountryGunsDto
    {
        [Required]
        [JsonProperty(nameof(Id))]
        public int Id { get; set; }
    }
}
