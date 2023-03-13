using Newtonsoft.Json;
using SoftJail.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportDepartmentCellDto
    {
        [Required]
        [Range(GlobalConstants.CellNumberMinLength, GlobalConstants.CellNumberMaxLength)]
        [JsonProperty("CellNumber")]
        public int CellNumber { get; set; }

        [Required]
        [JsonProperty("HasWindow")]
        public bool HasWindow { get; set; }
    }
}
