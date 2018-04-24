﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using System;
using WamBotRewrite.Data;

namespace WamBotRewrite.Migrations
{
    [DbContext(typeof(WamBotContext))]
    [Migration("20180324211429_Tweets2TwitterBoogaloo")]
    partial class Tweets2TwitterBoogaloo
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "2.0.2-rtm-10011");

            modelBuilder.Entity("WamBotRewrite.Data.Channel", b =>
                {
                    b.Property<long>("ChannelId")
                        .ValueGeneratedOnAdd();

                    b.Property<long>("GuildId");

                    b.Property<bool>("MarkovEnabled");

                    b.HasKey("ChannelId");

                    b.HasIndex("GuildId");

                    b.ToTable("Channels");
                });

            modelBuilder.Entity("WamBotRewrite.Data.Guild", b =>
                {
                    b.Property<long>("GuildId")
                        .ValueGeneratedOnAdd();

                    b.Property<long?>("AnnouncementChannelId");

                    b.HasKey("GuildId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("WamBotRewrite.Data.Transaction", b =>
                {
                    b.Property<Guid>("TransactionId")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Amount");

                    b.Property<long>("FromUserId");

                    b.Property<string>("Reason");

                    b.Property<DateTimeOffset>("TimeStamp");

                    b.Property<long>("ToUserId");

                    b.HasKey("TransactionId");

                    b.HasIndex("FromUserId");

                    b.HasIndex("ToUserId");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("WamBotRewrite.Data.User", b =>
                {
                    b.Property<long>("UserId")
                        .ValueGeneratedOnAdd();

                    b.Property<decimal>("Balance");

                    b.Property<long>("CommandsRun");

                    b.Property<sbyte>("Happiness");

                    b.Property<bool>("MarkovEnabled");

                    b.Property<bool>("MarkovTwitterEnabled");

                    b.Property<long>("TwitterId");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("WamBotRewrite.Data.Channel", b =>
                {
                    b.HasOne("WamBotRewrite.Data.Guild", "Guild")
                        .WithMany("Channels")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("WamBotRewrite.Data.Transaction", b =>
                {
                    b.HasOne("WamBotRewrite.Data.User", "From")
                        .WithMany("TransactionsSent")
                        .HasForeignKey("FromUserId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("WamBotRewrite.Data.User", "To")
                        .WithMany("TransactionsRecieved")
                        .HasForeignKey("ToUserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}