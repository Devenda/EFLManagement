using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EFLManagementAPI.Entities;
using Microsoft.EntityFrameworkCore;
using MySql.Data.EntityFrameworkCore.Extensions;

namespace EFLManagementAPI.Context
{
    public class EFLContext : DbContext
    {
        public DbSet<Card> Card { get; set; }
        public DbSet<Presence> Presence { get; set; }
        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySQL($"???");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<Card>(entity =>
            //{
            //    entity.HasKey(e => e.CardId);
            //    entity.HasOne(e => e.User)
            //      .WithMany(c => c.Cards);
            //});

            //modelBuilder.Entity<Presence>(entity =>
            //{
            //    entity.HasKey(e => e.PresenceId);
            //    entity.HasOne(e => e.User)
            //      .WithMany(p => p.Presence);
            //});

            //modelBuilder.Entity<User>(entity =>
            //{
            //    entity.HasKey(e => e.UserId);
            //    entity.HasMany(e => e.Cards);
            //});
        }
    }
}