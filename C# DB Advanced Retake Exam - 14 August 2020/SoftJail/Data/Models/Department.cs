using SoftJail.Common;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SoftJail.Data.Models
{
    public class Department
    {
        public Department()
        {
            Cells = new HashSet<Cell>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(GlobalConstants.DepartmentNameMaxLength)]
        public string Name { get; set; }

        public virtual ICollection<Cell> Cells { get; set; }
    }
}