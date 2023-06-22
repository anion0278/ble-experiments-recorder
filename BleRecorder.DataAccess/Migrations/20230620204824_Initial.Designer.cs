﻿// <auto-generated />
using System;
using BleRecorder.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace BleRecorder.DataAccess.Migrations
{
    [DbContext(typeof(ExperimentsDbContext))]
    [Migration("20230620204824_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.5")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("BleRecorder.Models.Device.DeviceMechanicalAdjustments", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<double>("CuffProximalDistalDistance")
                        .HasColumnType("float");

                    b.Property<double>("FixtureAdductionAbductionAngle")
                        .HasColumnType("float");

                    b.Property<double>("FixtureAnteroPosteriorDistance")
                        .HasColumnType("float");

                    b.Property<double>("FixtureProximalDistalDistance")
                        .HasColumnType("float");

                    b.HasKey("Id");

                    b.ToTable("DeviceMechanicalAdjustments");
                });

            modelBuilder.Entity("BleRecorder.Models.Device.StimulationParameters", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("Amplitude")
                        .HasColumnType("int");

                    b.Property<int>("Frequency")
                        .HasColumnType("int");

                    b.Property<int>("IntermittentRepetitions")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("IntermittentStimulationTime")
                        .HasColumnType("time");

                    b.Property<int>("PulseWidth")
                        .HasColumnType("int");

                    b.Property<TimeSpan>("RestTime")
                        .HasColumnType("time");

                    b.Property<TimeSpan>("StimulationTime")
                        .HasColumnType("time");

                    b.HasKey("Id");

                    b.ToTable("StimulationParameters");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Amplitude = 10,
                            Frequency = 50,
                            IntermittentRepetitions = 4,
                            IntermittentStimulationTime = new TimeSpan(0, 0, 0, 1, 0),
                            PulseWidth = 50,
                            RestTime = new TimeSpan(0, 0, 0, 5, 0),
                            StimulationTime = new TimeSpan(0, 0, 0, 5, 0)
                        });
                });

            modelBuilder.Entity("BleRecorder.Models.Measurements.Measurement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int?>("AdjustmentsDuringMeasurementId")
                        .HasColumnType("int");

                    b.Property<string>("ContractionLoadData")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("Date")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("Notes")
                        .HasMaxLength(400)
                        .HasColumnType("nvarchar(400)");

                    b.Property<int?>("ParametersDuringMeasurementId")
                        .HasColumnType("int");

                    b.Property<int>("PositionDuringMeasurement")
                        .HasColumnType("int");

                    b.Property<int>("SiteDuringMeasurement")
                        .HasColumnType("int");

                    b.Property<int>("TestSubjectId")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AdjustmentsDuringMeasurementId");

                    b.HasIndex("ParametersDuringMeasurementId");

                    b.HasIndex("TestSubjectId");

                    b.ToTable("Measurements");
                });

            modelBuilder.Entity("BleRecorder.Models.TestSubjects.TestSubject", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("CustomizedAdjustmentsId")
                        .HasColumnType("int");

                    b.Property<int>("CustomizedParametersId")
                        .HasColumnType("int");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CustomizedAdjustmentsId");

                    b.HasIndex("CustomizedParametersId");

                    b.ToTable("TestSubjects");
                });

            modelBuilder.Entity("BleRecorder.Models.Measurements.Measurement", b =>
                {
                    b.HasOne("BleRecorder.Models.Device.DeviceMechanicalAdjustments", "AdjustmentsDuringMeasurement")
                        .WithMany()
                        .HasForeignKey("AdjustmentsDuringMeasurementId");

                    b.HasOne("BleRecorder.Models.Device.StimulationParameters", "ParametersDuringMeasurement")
                        .WithMany()
                        .HasForeignKey("ParametersDuringMeasurementId");

                    b.HasOne("BleRecorder.Models.TestSubjects.TestSubject", "TestSubject")
                        .WithMany("Measurements")
                        .HasForeignKey("TestSubjectId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AdjustmentsDuringMeasurement");

                    b.Navigation("ParametersDuringMeasurement");

                    b.Navigation("TestSubject");
                });

            modelBuilder.Entity("BleRecorder.Models.TestSubjects.TestSubject", b =>
                {
                    b.HasOne("BleRecorder.Models.Device.DeviceMechanicalAdjustments", "CustomizedAdjustments")
                        .WithMany()
                        .HasForeignKey("CustomizedAdjustmentsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("BleRecorder.Models.Device.StimulationParameters", "CustomizedParameters")
                        .WithMany()
                        .HasForeignKey("CustomizedParametersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CustomizedAdjustments");

                    b.Navigation("CustomizedParameters");
                });

            modelBuilder.Entity("BleRecorder.Models.TestSubjects.TestSubject", b =>
                {
                    b.Navigation("Measurements");
                });
#pragma warning restore 612, 618
        }
    }
}