using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Mebster.Myodam.Models.Device;

namespace Mebster.Myodam.Models.TestSubject;

public class TestSubject
{
    public TestSubject() // TODO EF requires at least private param-less ctor, replace.      
    {
        Measurements = new Collection<Measurement>();
    }

    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(50)]
    public string LastName { get; set; }

    [NotMapped]
    public string FullName => FirstName + " " + LastName;

    //public MyodamMechanicalAdjustments CustomizedAdjustments { get; set; }

    //public StimulationParameters CustomizedParameters { get; set; }

    public ICollection<Measurement> Measurements { get; set; }
}

