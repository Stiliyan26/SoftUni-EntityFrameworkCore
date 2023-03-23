using Artillery.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Artillery.DataProcessor.ExportDto
{
    [XmlType("Gun")]
    public class ExportGunDto
    {
        [XmlAttribute(nameof(Manufacturer))]
        public string Manufacturer { get; set; }


        [XmlAttribute(nameof(GunType))]
        public string GunType { get; set; }


        [XmlAttribute(nameof(GunWeight))]
        public string GunWeight { get; set; }


        [XmlAttribute(nameof(BarrelLength))]
        public string BarrelLength { get; set; }


        [XmlAttribute(nameof(Range))]
        public string Range { get; set; }

        [XmlArray(nameof(Countries))]
        public ExportCountryDto[] Countries { get; set; }
    }
}
