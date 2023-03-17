using Newtonsoft.Json;
using SoftJail.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace SoftJail.DataProcessor.ImportDto
{
    [JsonObject]
    public class ImportDepartmentWithCellsDto
    {
        [Required]
        [MinLength(GlobalConstants.DepartmentNameMinLength)]
        [MaxLength(GlobalConstants.DepartmentNameMaxLength)]
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Cells")]
        public ImportDepartmentCellDto[] Cells { get; set; }
    }
}
