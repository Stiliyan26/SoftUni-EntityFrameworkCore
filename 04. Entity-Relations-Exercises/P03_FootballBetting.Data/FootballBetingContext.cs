using Microsoft.EntityFrameworkCore;
using P03_FootballBetting.Data.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace P03_FootballBetting.Data
{
    public class FootballBetingContext : DbContext
    {
        public FootballBetingContext()
        {

        }

        public FootballBetingContext(DbContextOptions options)
            :base(options) 
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(Config.ConnectionString);
            }
        }

        public DbSet<Team> Teams { get; set; }

        public DbSet<Color> Colors { get; set; }

        public DbSet<Town> Towns { get; set; }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Player> Players { get; set; }

        public DbSet<Position> Positions { get; set; }

        public DbSet<PlayerStatistic> PlayerStatistics { get; set; }

        public DbSet<Game> Games { get; set; }

        public DbSet<Bet> Bets { get; set; }

        public DbSet<User> Users { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<PlayerStatistic>(e =>
                {
                    e.HasKey(e => new { e.PlayerId, e.GameId });
                });

            /*  modelBuilder
                  .Entity<Team>()
                  .HasOne(t => t.PrimaryKitColor)
                  .WithMany(c => c.PrimaryKitTeams)
                  .HasForeignKey(t => t.PrimaryKitColorId)
                  .OnDelete(DeleteBehavior.SetNull);*/

            modelBuilder
                .Entity<Team>(e =>
                {
                    e
                        .HasOne(t => t.PrimaryKitColor)
                        .WithMany(c => c.PrimaryKitTeams)
                        .HasForeignKey(t => t.PrimaryKitColorId)
                        .OnDelete(DeleteBehavior.NoAction);

                    e
                        .HasOne(t => t.SecondaryKitColor)
                        .WithMany(c => c.SecondaryKitTeams)
                        .HasForeignKey(t => t.SecondaryKitColorId)
                        .OnDelete(DeleteBehavior.NoAction);
                });

            modelBuilder
                .Entity<Game>(e =>
                {
                    e
                        .HasOne(g => g.HomeTeam)
                        .WithMany(t => t.HomeGames)
                        .HasForeignKey(g => g.HomeTeamId)
                        .OnDelete(DeleteBehavior.NoAction);

                    e
                        .HasOne(g => g.AwayTeam)
                        .WithMany(e=> e.AwayGames)
                        .HasForeignKey(g => g.AwayTeamId)
                        .OnDelete(DeleteBehavior.NoAction);
                });
        }
       /* private void DiscoverDbSets() 
        {
            Assembly assembly = Assembly.GetAssembly(typeof(Player));

            Type[] entities = assembly
                .GetTypes();

            Type dbContext = this
                .GetType();

            foreach (Type enitity in entities)
            {
                object dbSet = typeof(DbSet<>)
                    .MakeGenericType(enitity);
            }
        }*/
    }
}
