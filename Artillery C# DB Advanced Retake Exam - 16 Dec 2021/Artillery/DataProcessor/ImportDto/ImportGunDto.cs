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
    public class ImportGunDto
    {
        [Required]
        [JsonProperty(nameof(ManufacturerId))]
        public int ManufacturerId { get; set; }

        [Required]
        [Range(100, 1_350_000)]
        [JsonProperty(nameof(GunWeight))]
        public int GunWeight { get; set; }

        [Required]
        [Range(2.00, 35.00)]
        [JsonProperty(nameof(BarrelLength))]
        public double BarrelLength { get; set; }

        [JsonProperty(nameof(NumberBuild))]
        public int? NumberBuild { get; set; }

        [Required]
        [Range(1, 100_000)]
        [JsonProperty(nameof(Range))]
        public int Range { get; set; }

        [Required]
        [JsonProperty(nameof(GunType))]
        public string GunType { get; set; }

        [Required]
        [JsonProperty(nameof(ShellId))]
        public int ShellId { get; set; }

        [JsonProperty(nameof(Countries))]
        public virtual ICollection<ImportCountryGunsDto> Countries { get; set; }
    }
}
