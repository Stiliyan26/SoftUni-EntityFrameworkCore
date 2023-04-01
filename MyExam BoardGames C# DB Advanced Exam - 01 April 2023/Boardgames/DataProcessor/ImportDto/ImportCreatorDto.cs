using Boardgames.Data.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Boardgames.DataProcessor.ImportDto
{
    [XmlType("Creator")]
    public class ImportCreatorDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(7)]
        [XmlElement(nameof(FirstName))]
        public string FirstName { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(7)]
        [XmlElement(nameof(LastName))]
        public string LastName { get; set; }

        [XmlArray(nameof(Boardgames))]
        public virtual ImportBoardGameByCreatorDto[] Boardgames { get; set; }
    }
}
