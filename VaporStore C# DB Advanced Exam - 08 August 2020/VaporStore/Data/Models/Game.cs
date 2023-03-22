using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VaporStore.Data.Models
{
    public class Game
    {
        public Game()
        {
            Purchases = new List<Purchase>();
            GameTags = new List<GameTag>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }

        [Required]
        [ForeignKey(nameof(Developer))]
        public int DeveloperId { get; set; }

        [Required]
        public virtual Developer Developer { get; set; }

        [Required]
        [ForeignKey(nameof(Genre))]
        public int GenreId { get; }

        [Required]
        public virtual Genre Genre { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }

        public virtual ICollection<GameTag> GameTags  { get; set; }
    }
}
