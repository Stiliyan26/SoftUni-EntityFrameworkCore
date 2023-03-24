using System.Xml.Serialization;

namespace Footballers.DataProcessor.ExportDto
{
    [XmlType("Footballer")]
    public class ExportFootballerDto
    {
        [XmlElement(nameof(Name))]
        public string Name { get; set; }


        [XmlElement(nameof(Position))]
        public string Position { get; set; }
    }
}