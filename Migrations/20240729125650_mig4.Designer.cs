﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SunucuBakimKontrol.Data;

#nullable disable

namespace SunucuBakimKontrol.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20240729125650_mig4")]
    partial class mig4
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("SunucuBakimKontrol.Models.Holiday", b =>
                {
                    b.Property<int>("HolidayId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("HolidayId"));

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsHoliday")
                        .HasColumnType("bit");

                    b.HasKey("HolidayId");

                    b.ToTable("Holidays");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.MaintenanceLog", b =>
                {
                    b.Property<int>("LogId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("LogId"));

                    b.Property<bool>("IsCompleted")
                        .HasColumnType("bit");

                    b.Property<DateTime>("LogDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("MaintenancePointId")
                        .HasColumnType("int");

                    b.Property<string>("NotCompletedReason")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ServerId")
                        .HasColumnType("int");

                    b.HasKey("LogId");

                    b.HasIndex("MaintenancePointId");

                    b.HasIndex("ServerId");

                    b.ToTable("MaintenanceLogs");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.MaintenancePoint", b =>
                {
                    b.Property<int>("MaintenancePointId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("MaintenancePointId"));

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<string>("MaintenancePointName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ServerId")
                        .HasColumnType("int");

                    b.HasKey("MaintenancePointId");

                    b.HasIndex("ServerId");

                    b.ToTable("MaintenancePoints");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.Server", b =>
                {
                    b.Property<int>("ServerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ServerId"));

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<int>("ResponsibleId")
                        .HasColumnType("int");

                    b.Property<string>("ServerAddress")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ServerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ServerId");

                    b.HasIndex("ResponsibleId");

                    b.ToTable("Servers");
                });

            modelBuilder.Entity("User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserId"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Role")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("deneme")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.MaintenanceLog", b =>
                {
                    b.HasOne("SunucuBakimKontrol.Models.MaintenancePoint", "MaintenancePoint")
                        .WithMany("MaintenanceLogs")
                        .HasForeignKey("MaintenancePointId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SunucuBakimKontrol.Models.Server", "Server")
                        .WithMany("MaintenanceLogs")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("MaintenancePoint");

                    b.Navigation("Server");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.MaintenancePoint", b =>
                {
                    b.HasOne("SunucuBakimKontrol.Models.Server", "Server")
                        .WithMany("MaintenancePoints")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Server");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.Server", b =>
                {
                    b.HasOne("User", "Responsible")
                        .WithMany("Servers")
                        .HasForeignKey("ResponsibleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Responsible");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.MaintenancePoint", b =>
                {
                    b.Navigation("MaintenanceLogs");
                });

            modelBuilder.Entity("SunucuBakimKontrol.Models.Server", b =>
                {
                    b.Navigation("MaintenanceLogs");

                    b.Navigation("MaintenancePoints");
                });

            modelBuilder.Entity("User", b =>
                {
                    b.Navigation("Servers");
                });
#pragma warning restore 612, 618
        }
    }
}
