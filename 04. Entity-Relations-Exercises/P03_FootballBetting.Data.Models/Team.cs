﻿using P03_FootballBetting.Data.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P03_FootballBetting.Data.Models
{
    public class Team
    {
        public Team()
        {
            HomeGames = new HashSet<Game>();
            AwayGames = new HashSet<Game>();
            Players = new HashSet<Player>(); 
        }

        [Key]
        public int TeamId { get; set; }

        [Required]
        [MaxLength(GlobalConstants.TeamNameMaxLength)]    
        public string Name { get; set; }

        [MaxLength(GlobalConstants.TeamLogoUrlMaxLength)]
        public string LogoUrl { get; set; }
        
        [Required]
        [MaxLength(GlobalConstants.TeamInitialsMaxLength)]
        public string Initials { get; set; }

        [Required]
        public decimal Budget { get; set; }


        [ForeignKey(nameof(PrimaryKitColor))]
        public int PrimaryKitColorId { get; set; }

        public virtual Color PrimaryKitColor { get; set; }


        [ForeignKey(nameof(SecondaryKitColor))]
        public int SecondaryKitColorId { get; set; }

        public virtual Color SecondaryKitColor { get; set; }

        [Required]
        [ForeignKey(nameof(Town))]
        public int TownId { get; set; }

        public virtual Town Town { get; set; }


        [InverseProperty(nameof(Game.HomeTeam))]
        public virtual ICollection<Game> HomeGames { get; set; }


        [InverseProperty(nameof(Game.AwayTeam))]
        public virtual ICollection<Game> AwayGames { get; set; }

        public virtual ICollection<Player> Players { get; set; }
    }
}
