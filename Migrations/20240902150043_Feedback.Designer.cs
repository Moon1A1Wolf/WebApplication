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
    [Migration("20240902150043_Feedback")]
    partial class Feedback
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("WebApplication1.Data.Entities.Feedback", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<Guid>("ProductId")
                    .HasColumnType("uniqueidentifier");

                b.Property<int>("Rate")
                    .HasColumnType("int");

                b.Property<string>("Text")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.HasKey("Id");

                b.HasIndex("ProductId");

                b.ToTable("Feedbacks");
            });

            modelBuilder.Entity("WebApplication1.Data.Entities.Product", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<long>("Amount")
                    .HasColumnType("bigint");

                b.Property<DateTime?>("DeleteDt")
                    .HasColumnType("datetime2");

                b.Property<string>("Description")
                    .HasColumnType("nvarchar(max)");

                b.Property<Guid>("GroupId")
                    .HasColumnType("uniqueidentifier");

                b.Property<string>("Image")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<double>("Price")
                    .HasColumnType("float");

                b.Property<string>("Slug")
                    .HasColumnType("nvarchar(450)");

                b.HasKey("Id");

                b.HasIndex("GroupId");

                b.HasIndex("Slug")
                    .IsUnique()
                    .HasFilter("[Slug] IS NOT NULL");

                b.ToTable("Products");
            });

            modelBuilder.Entity("WebApplication1.Data.Entities.ProductGroup", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uniqueidentifier");

                b.Property<DateTime?>("DeleteDt")
                    .HasColumnType("datetime2");

                b.Property<string>("Description")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Image")
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasColumnType("nvarchar(max)");

                b.Property<string>("Slug")
                    .HasColumnType("nvarchar(450)");

                b.HasKey("Id");

                b.HasIndex("Slug")
                    .IsUnique()
                    .HasFilter("[Slug] IS NOT NULL");

                b.ToTable("Groups");
            });

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

                b.Property<DateTime?>("DeleteDt")
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

            modelBuilder.Entity("WebApplication1.Data.Entities.Feedback", b =>
            {
                b.HasOne("WebApplication1.Data.Entities.Product", "Product")
                    .WithMany("Feedbacks")
                    .HasForeignKey("ProductId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Product");
            });

            modelBuilder.Entity("WebApplication1.Data.Entities.Product", b =>
            {
                b.HasOne("WebApplication1.Data.Entities.ProductGroup", "Group")
                    .WithMany("Products")
                    .HasForeignKey("GroupId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Group");
            });

            modelBuilder.Entity("WebApplication1.Data.Entities.Product", b =>
            {
                b.Navigation("Feedbacks");
            });

            modelBuilder.Entity("WebApplication1.Data.Entities.ProductGroup", b =>
            {
                b.Navigation("Products");
            });
#pragma warning restore 612, 618
        }
    }
}