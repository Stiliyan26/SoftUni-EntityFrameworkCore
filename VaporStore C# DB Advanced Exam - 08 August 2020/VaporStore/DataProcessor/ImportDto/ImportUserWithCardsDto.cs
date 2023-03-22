using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporStore.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportUserWithCardsDto
    {
        [Required]
        [RegularExpression(@"^([A-Z]{1}[a-z]+)\s([A-Z]{1}[a-z]+)$")]
        [JsonProperty("FullName")]
        public string FullName { get; set; }

        [Required]
        [MaxLength(20)]
        [MinLength(3)]
        [JsonProperty("Username")]
        public string Username { get; set; }

        [Required]
        [JsonProperty("Email")]
        public string Email { get; set; }

        [Required]
        [Range(3, 103)]
        [JsonProperty("Age")]
        public int Age { get; set; }

        [JsonProperty("Cards")]
        public virtual ImportCardDto[] Cards { get; set; }
    }
}
