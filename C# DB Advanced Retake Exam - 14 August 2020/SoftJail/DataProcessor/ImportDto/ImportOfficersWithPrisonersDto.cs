using SoftJail.Common;
using SoftJail.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;

namespace SoftJail.DataProcessor.ImportDto
{
    [XmlType("Officer")]
    public class ImportOfficersWithPrisonersDto
    {
        [Required]
        [MinLength(GlobalConstants.OfficerFullNameMinLength)]
        [MaxLength(GlobalConstants.OfficerFullNameMaxLength)]
        [XmlElement("Name")]
        public string FullName { get; set; }

        [Range(typeof(decimal), GlobalConstants.DecimalMinValue, GlobalConstants.DecimalMaxValue)]
        [XmlElement("Money")]
        public decimal Salary { get; set; }

        [Required]
        [XmlElement("Position")]
        public string Position { get; set; }

        [Required]
        [XmlElement("Weapon")]
        public string Weapon { get; set; }

        [XmlElement("DepartmentId")]
        public int DepartmentId { get; set; }

        [XmlArray("Prisoners")]
        public ImportOfficerPrisonerDto[] Prisoners { get; set; }
    }
}
