using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Footballers.DataProcessor.ExportDto
{
    [XmlType("Coach")]
    public class ExportCoachWithFootballerDto
    {
        [XmlAttribute(nameof(FootballersCount))]
        public string FootballersCount { get; set; }


        [XmlElement(nameof(CoachName))]
        public string CoachName { get; set; }


        [XmlArray(nameof(Footballers))]
        public ExportFootballerDto[] Footballers { get; set; }
    }
}
