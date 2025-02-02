﻿// <auto-generated />
using GameLogsMVC.Models.DBData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameLogsMVC.Migrations
{
    [DbContext(typeof(ApplicationDBContext))]
    partial class ApplicationDBContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("GameLogsMVC.Models.DBData.Badge", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Badge");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.Game", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("Away")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("AwayTeamID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Date")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Duration")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("GameNote")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Home")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("HomeTeamID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ImpactPlay")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("League")
                        .HasColumnType("text");

                    b.Property<string>("Location")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("NeutralSite")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Score")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Season")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Game");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.Player", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("HOF")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("MVP")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Player");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.PlayerGame", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("GameID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("PlayerID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Position")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Stats")
                        .HasColumnType("text");

                    b.Property<string>("Team")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("PlayerGame");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.Team", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Url")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("Team");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.User", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("text");

                    b.Property<string>("FavMLB")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FavNBA")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FavNCAAB")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FavNCAAF")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("FavNFL")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("User");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.UserBadge", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("BadgeID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("UserBadge");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.UserFollow", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("FollowingID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("UserFollow");
                });

            modelBuilder.Entity("GameLogsMVC.Models.DBData.UserGame", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ID"));

                    b.Property<string>("Attended")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("GameID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("League")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("UserID")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("ID");

                    b.ToTable("UserGame");
                });
#pragma warning restore 612, 618
        }
    }
}
