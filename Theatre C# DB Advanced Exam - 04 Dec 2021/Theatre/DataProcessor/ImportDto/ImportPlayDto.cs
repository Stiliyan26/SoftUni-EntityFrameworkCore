using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Theatre.DataProcessor.ImportDto
{
    [XmlType("Play")]
    public class ImportPlayDto
    {
        [Required]
        [MinLength(4)]
        [MaxLength(50)]
        [XmlElement("Title")]
        public string Title { get; set; }

        [Required]
        [XmlElement("Duration")]
        public string Duration { get; set; }

        [Required]
        [Range(typeof(float), "0.00", "10.00")]
        [XmlElement("Raiting")]
        public float Raiting { get; set; }

        [Required]
        [XmlElement("Genre")]
        public string Genre { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(700)]
        [XmlElement("Description")]
        public string Description { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MinLength(4)]
        [MaxLength(30)]
        [XmlElement("Screenwriter")]
        public string Screenwriter { get; set; }
    }
}
