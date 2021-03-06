﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WamBotRewrite.Data
{
    public class WamBotContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Channel> Channels { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        [NotMapped]
        public Lazy<User> BotUser => new Lazy<User>(() => Users.Find((long)Program.Client.CurrentUser.Id) ?? new User(Program.Client.CurrentUser) { Balance = int.MaxValue / 16 });

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.
                Entity<Transaction>()
                .HasOne(t => t.To)
                .WithMany(u => u.TransactionsRecieved);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.From)
                .WithMany(u => u.TransactionsSent);

            modelBuilder.Entity<Channel>()
                .HasOne(t => t.Guild)
                .WithMany(g => g.Channels)
                .HasForeignKey(c => c.GuildId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseNpgsql(Program.Config.Database?.ConnectionString ?? "Server=localhost;Database=WamBot")
                .UseLoggerFactory(new UILoggerFactory());
        }
    }
}
