using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;
using Trucks.Data.Models.Enums;

namespace Trucks.DataProcessor.ImportDto
{
    [XmlType("Truck")]
    public class ImportTruckDto
    {
        [MinLength(8)]
        [MaxLength(8)]
        [RegularExpression(@"^[A-Z]{2}[0-9]{4}[A-Z]{2}$")]
        [XmlElement("RegistrationNumber")]
        public string RegistrationNumber { get; set; }

        [Required]
        [MaxLength(17)]
        [XmlElement("VinNumber")]
        public string VinNumber { get; set; }

        [Required]
        [Range(950, 1420)]
        [XmlElement("TankCapacity")]
        public int TankCapacity { get; set; }

        [Required]
        [Range(5000, 29000)]
        [XmlElement("CargoCapacity")]
        public int CargoCapacity { get; set; }

        [Required]
        [XmlElement("CategoryType")]
        public string CategoryType { get; set; }

        [Required]
        [XmlElement("MakeType")]
        public string MakeType { get; set; }

    }
}
