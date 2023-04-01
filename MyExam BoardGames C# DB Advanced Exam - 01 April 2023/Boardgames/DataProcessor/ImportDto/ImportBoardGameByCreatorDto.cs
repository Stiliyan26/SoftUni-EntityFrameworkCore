using Boardgames.Data.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Boardgames.DataProcessor.ImportDto
{
    [XmlType("Boardgame")]
    public class ImportBoardGameByCreatorDto
    {
        [Required]
        [MinLength(10)]
        [MaxLength(20)]
        [XmlElement(nameof(Name))]
        public string Name { get; set; }

        [Required]
        [Range(1, 10.00)]
        [XmlElement(nameof(Rating))]
        public double Rating { get; set; }

        [Required]
        [Range(2018, 2023)]
        [XmlElement(nameof(YearPublished))]
        public int YearPublished { get; set; }

        [Required]
        [XmlElement(nameof(CategoryType))]
        public string CategoryType { get; set; }

        [Required]
        [XmlElement(nameof(Mechanics))]
        public string Mechanics { get; set; }
    }
}
