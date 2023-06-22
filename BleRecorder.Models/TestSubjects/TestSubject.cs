﻿using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BleRecorder.Common;
using BleRecorder.Models.Device;
using BleRecorder.Models.Measurements;

namespace BleRecorder.Models.TestSubjects;

public class TestSubject: IEntity
{
    public TestSubject() 
    { }

    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [NotMapped]
    public string FullName => FirstName + " " + LastName;

    public string? Notes { get; set; }

    public DeviceMechanicalAdjustments CustomizedAdjustments { get; set; }

    public StimulationParameters CustomizedParameters { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = new Collection<Measurement>();
}

