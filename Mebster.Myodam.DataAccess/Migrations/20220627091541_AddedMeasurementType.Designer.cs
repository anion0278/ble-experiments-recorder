﻿// <auto-generated />
using System;
using Mebster.Myodam.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Mebster.Myodam.DataAccess.Migrations
{
    [DbContext(typeof(ExperimentsDbContext))]
    [Migration("20220627091541_AddedMeasurementType")]
    partial class AddedMeasurementType
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.5");

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.Measurement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("ForceData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .IsRequired()
                        .HasMaxLength(400)
                        .HasColumnType("TEXT");

                    b.Property<int>("TestSubjectId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("TestSubjectId");

                    b.ToTable("Measurements");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.TestSubject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("TestSubjects");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.Measurement", b =>
                {
                    b.HasOne("Mebster.Myodam.Models.TestSubject.TestSubject", "TestSubject")
                        .WithMany("Measurements")
                        .HasForeignKey("TestSubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TestSubject");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.TestSubject", b =>
                {
                    b.Navigation("Measurements");
                });
#pragma warning restore 612, 618
        }
    }
}
