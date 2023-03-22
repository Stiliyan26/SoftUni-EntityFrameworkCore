using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VaporStore.Data.Models;

namespace VaporStore.DataProcessor.ImportDto
{
    [XmlType("Purchase")]
    public class ImportPurchaseDto
    {
        [XmlAttribute("title")]
        public string title { get; set; }

        [Required]
        [XmlElement("Type")]
        public string Type { get; set; }


        [Required]
        [StringLength(14, MinimumLength = 14)]
        [RegularExpression(@"^([A-Z0-9]){4}-([A-Z0-9]){4}-([A-Z0-9]){4}$")]
        [XmlElement("Key")]
        public string Key { get; set; }

        [Required]
        [XmlElement("Card")]
        public string Card { get; set; }

        [Required]
        [XmlElement("Date")]
        public string Date { get; set; }
    }
}


