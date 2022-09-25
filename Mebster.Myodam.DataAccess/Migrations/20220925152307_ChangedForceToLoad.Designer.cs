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
    [Migration("20220925152307_ChangedForceToLoad")]
    partial class ChangedForceToLoad
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.5");

            modelBuilder.Entity("Mebster.Myodam.Models.Device.DeviceMechanicalAdjustments", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<double>("CuffProximalDistalDistance")
                        .HasColumnType("REAL");

                    b.Property<double>("FootplateAdductionAbductionAngle")
                        .HasColumnType("REAL");

                    b.Property<double>("FootplateAnteroPosteriorDistance")
                        .HasColumnType("REAL");

                    b.Property<double>("FootplateProximalDistalDistance")
                        .HasColumnType("REAL");

                    b.HasKey("Id");

                    b.ToTable("DeviceMechanicalAdjustments");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.Device.StimulationParameters", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Current")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Frequency")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PulseWidth")
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("StimulationTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("StimulationParameters");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Current = 10,
                            Frequency = 50,
                            PulseWidth = 50,
                            StimulationTime = new TimeSpan(0, 0, 0, 10, 0)
                        });
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.Measurement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int?>("AdjustmentsDuringMeasurementId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ContractionLoadData")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset?>("Date")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasMaxLength(400)
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParametersDuringMeasurementId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PositionDuringMeasurement")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SiteDuringMeasurement")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TestSubjectId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<int>("Type")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("AdjustmentsDuringMeasurementId");

                    b.HasIndex("ParametersDuringMeasurementId");

                    b.HasIndex("TestSubjectId");

                    b.ToTable("Measurements");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.TestSubject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("CustomizedAdjustmentsId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("CustomizedParametersId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CustomizedAdjustmentsId");

                    b.HasIndex("CustomizedParametersId");

                    b.ToTable("TestSubjects");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.Measurement", b =>
                {
                    b.HasOne("Mebster.Myodam.Models.Device.DeviceMechanicalAdjustments", "AdjustmentsDuringMeasurement")
                        .WithMany()
                        .HasForeignKey("AdjustmentsDuringMeasurementId");

                    b.HasOne("Mebster.Myodam.Models.Device.StimulationParameters", "ParametersDuringMeasurement")
                        .WithMany()
                        .HasForeignKey("ParametersDuringMeasurementId");

                    b.HasOne("Mebster.Myodam.Models.TestSubject.TestSubject", "TestSubject")
                        .WithMany("Measurements")
                        .HasForeignKey("TestSubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AdjustmentsDuringMeasurement");

                    b.Navigation("ParametersDuringMeasurement");

                    b.Navigation("TestSubject");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.TestSubject", b =>
                {
                    b.HasOne("Mebster.Myodam.Models.Device.DeviceMechanicalAdjustments", "CustomizedAdjustments")
                        .WithMany()
                        .HasForeignKey("CustomizedAdjustmentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Mebster.Myodam.Models.Device.StimulationParameters", "CustomizedParameters")
                        .WithMany()
                        .HasForeignKey("CustomizedParametersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CustomizedAdjustments");

                    b.Navigation("CustomizedParameters");
                });

            modelBuilder.Entity("Mebster.Myodam.Models.TestSubject.TestSubject", b =>
                {
                    b.Navigation("Measurements");
                });
#pragma warning restore 612, 618
        }
    }
}
