using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;

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

    public ICollection<Measurement> Measurements { get; set; }
}

