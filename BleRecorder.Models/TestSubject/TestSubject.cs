using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BleRecorder.Models.TestSubject;

public class TestSubject
{
    public TestSubject()
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

    public ICollection<Measurement> Measurements { get; set; }
}

