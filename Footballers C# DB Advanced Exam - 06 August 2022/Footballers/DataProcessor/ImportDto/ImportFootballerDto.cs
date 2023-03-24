using Footballers.Data.Models.Enums;
using Footballers.Data.Models;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Footballers.DataProcessor.ImportDto
{
    [XmlType("Footballer")]
    public class ImportFootballerDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(40)]
        [XmlElement(nameof(Name))]
        public string Name { get; set; }

        [Required]
        [XmlElement(nameof(ContractStartDate))]
        public string ContractStartDate { get; set; }

        [Required]
        [XmlElement(nameof(ContractEndDate))]
        public string ContractEndDate { get; set; }

        [Required]
        [XmlElement(nameof(BestSkillType))]
        public string BestSkillType { get; set; }

        [Required]
        [XmlElement(nameof(PositionType))]
        public string PositionType { get; set; }
    }
}

