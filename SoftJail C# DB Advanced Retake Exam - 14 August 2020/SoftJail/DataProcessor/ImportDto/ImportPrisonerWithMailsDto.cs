using Newtonsoft.Json;
using SoftJail.Common;
using SoftJail.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportPrisonerWithMailsDto
    {
        [JsonProperty("FullName")]
        [Required]
        [MinLength(GlobalConstants.PrisonerFullNameMinLength)]
        [MaxLength(GlobalConstants.PrisonerFullNameMaxLength)]
        public string FullName { get; set; }

        [JsonProperty("Nickname")]
        [Required]
        [RegularExpression(GlobalConstants.PrisonerNickNameRegex)]
        public string Nickname { get; set; }

        [JsonProperty("Age")]
        [Required]
        [Range(GlobalConstants.PrisonerMinAgeValue, GlobalConstants.PrisonerMaxAgeValue)]
        public int Age { get; set; }

        [JsonProperty("IncarcerationDate")]
        [Required]
        public string IncarcerationDate { get; set; }

        [JsonProperty("ReleaseDate")]
        public string? ReleaseDate { get; set; }

        [JsonProperty("Bail")]
        [Range(typeof(decimal), GlobalConstants.DecimalMinValue, GlobalConstants.DecimalMaxValue)]
        public decimal? Bail { get; set; }

        [JsonProperty("CellId")]
        public int? CellId { get; set; }

        [JsonProperty("Mails")]
        public ImportPrisonerMailDto[] Mails { get; set; }
    }
}
