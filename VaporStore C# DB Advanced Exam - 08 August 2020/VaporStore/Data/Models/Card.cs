﻿using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VaporStore.Data.Models.Enums;

namespace VaporStore.Data.Models
{
    public class Card
    {
        public Card()
        {
            Purchases = new List<Purchase>();
        }

        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(19)]
        public string Number { get; set; }

        [Required]
        [MaxLength(3)]
        public string Cvc { get; set; }

        [Required]
        public CardType Type { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [Required]
        public virtual User User { get; set; }

        public virtual ICollection<Purchase> Purchases { get; set; }
    }
}