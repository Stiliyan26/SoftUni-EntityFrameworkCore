using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SoftJail.Data.Models
{
    public class OfficerPrisoner
    {
        [Required]
        [ForeignKey(nameof(Prisoner))]
        public int PrisonerId { get; set; }

        [Required]
        public virtual Prisoner Prisoner { get; set; }

        [Required]
        [ForeignKey(nameof(Officer))]
        public int OfficerId { get; set; }

        [Required]
        public virtual Officer Officer { get; set; }
    }
}