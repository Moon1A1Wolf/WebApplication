﻿// <auto-generated />
using System;
using WebApplication1.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace WebApplication1.Migrations
{
    [DbContext(typeof(DataContext))]
    [Migration("20240712160000_UserUpdates")]
    partial class UserUpdates
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WebApplication1.Data.Entities.Token", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime>("ExpiresAt")
                    .HasColumnType("datetime2");

                b.Property<Guid>("UserId")
                    .HasColumnType("uniqueidentifier");

                b.HasKey("Id");

                b.ToTable("Tokens");
            });

            modelBuilder.Entity("WebApplication1.Data.Entities.User", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Avatar")
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTime?>("Birthdate")
                    .HasColumnType("datetime2");

                b.Property<string>("Dk")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Email")
                    .IsRequired()
                    .HasColumnType("nvarchar(450)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<DateTime>("Registered")
                    .HasColumnType("datetime2");

                b.Property<string>("Salt")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("Email")
                    .IsUnique();

                b.ToTable("Users");
            });
#pragma warning restore 612, 618
        }
    }
}
