using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using TeisterMask.Data.Models.Enums;
using TeisterMask.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace TeisterMask.DataProcessor.ImportDto
{
    [XmlType("Project")]
    public class ImportProjectDto
    {
        [Required]
        [MaxLength(40)]
        [MinLength(2)]
        [XmlElement("Name")]
        public string Name { get; set; }

        [Required]
        [XmlElement("OpenDate")]

        public string OpenDate { get; set; }

        [XmlElement("DueDate")]
        public string? DueDate { get; set; }

        [XmlArray("Tasks")]
        public virtual ImportTaskDto[] Tasks { get; set; }
    }
}

/*
 *  < Name > S </ Name >
    < OpenDate > 25 / 01 / 2018 </ OpenDate >
    < DueDate > 16 / 08 / 2019 </ DueDate >
    < Tasks >
     
*/