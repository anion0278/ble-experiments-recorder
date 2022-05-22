﻿using System;
using System.Collections.Generic;
using Mebster.Myodam.Models.TestSubject;

namespace Mebster.Myodam.UI.WPF.Wrapper
{
    public class TestSubjectWrapper : ModelWrapper<TestSubject>
    {
        public TestSubjectWrapper(TestSubject model) : base(model)
        {
        }

        public int Id { get { return Model.Id; } }

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
