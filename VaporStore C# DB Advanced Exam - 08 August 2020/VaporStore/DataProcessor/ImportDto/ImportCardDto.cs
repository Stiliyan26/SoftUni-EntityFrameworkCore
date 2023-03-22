using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporStore.DataProcessor.ImportDto
{
    public class ImportCardDto
    {
        [Required]
        [MaxLength(19)]
        [RegularExpression(@"^([0-9]{4})\s([0-9]{4})\s([0-9]{4})\s([0-9]{4})$")]
        [JsonProperty("Number")]
        public string Number { get; set; }

        [Required]
        [MaxLength(3)]
        [MinLength(3)]
        [RegularExpression(@"^([0-9]{3})$")]
        [JsonProperty("CVC")]
        public string CVC { get; set; }

        [Required]
        [JsonProperty("Type")]
        public string Type { get; set; }
    }
}







