using Newtonsoft.Json;
using SoftJail.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportPrisonerMailDto
    {
        [JsonProperty("Description")]
        [Required]
        public string Description { get; set; }

        [JsonProperty("Sender")]
        [Required]
        public string Sender { get; set; }

        [JsonProperty("Address")]
        [Required]
        [RegularExpression(GlobalConstants.MailAddressRegex)]
        public string Address { get; set; }
    }
}
