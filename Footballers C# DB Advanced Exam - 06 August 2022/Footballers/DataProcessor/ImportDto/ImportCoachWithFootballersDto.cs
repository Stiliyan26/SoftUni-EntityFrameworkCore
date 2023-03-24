using Footballers.Data.Models.Enums;
using Footballers.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Footballers.DataProcessor.ImportDto
{
    [XmlType("Coach")]
    public class ImportCoachWithFootballersDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(40)]
        [XmlElement(nameof(Name))]
        public string Name { get; set; }

        [Required]
        [XmlElement(nameof(Nationality))]
        public string Nationality { get; set; }

        [XmlArray(nameof(Footballers))]
        public ImportFootballerDto[] Footballers { get; set; }
    }
}

      
