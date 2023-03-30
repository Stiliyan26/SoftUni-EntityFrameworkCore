using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Trucks.DataProcessor.ExportDto
{
    [XmlType("Despatcher")]
    public class ExportDespatcherDto
    {
        [XmlAttribute(nameof(TrucksCount))]
        public string TrucksCount { get; set; }

        [XmlElement(nameof(DespatcherName))]
        public string DespatcherName { get; set; }

        [XmlArray(nameof(Trucks))]
        public ExportTruckDto[] Trucks { get; set; }
    }
}
