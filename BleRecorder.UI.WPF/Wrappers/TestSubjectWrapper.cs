﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BleRecorder.Models.TestSubject;

namespace BleRecorder.UI.WPF.Wrappers
{
    public class TestSubjectWrapper : ModelWrapper<TestSubject>
    {
        public TestSubjectWrapper(TestSubject model) : base(model)
        {
        }

        public int Id { get { return Model.Id; } }

        [Required]
        public string FirstName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public string LastName
        {
            get { return GetValue<string>(); }
            set { SetValue(value); }
        }

        public ICollection<Measurement> Measurements
        {
            get { return GetValue<ICollection<Measurement>>(); }
            set { SetValue(value); }
        }

        protected override IEnumerable<string> ValidateProperty(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.Equals(FirstName, "Invalid property", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return "Error";
                    }
                    break;
            }
        }
    }
}