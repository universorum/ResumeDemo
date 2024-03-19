using System.ComponentModel.DataAnnotations;

namespace ResumeDemo.Models;

public class Resume
{
    public int Id { get; set; }

    [Display(Name = "姓名")]
    public required string Name { get; init; }

    [Display(Name = "生日")]
    public required DateOnly BirthDate { get; init; }

    [ConcurrencyCheck]
    public Guid Version { get; internal set; } = Guid.NewGuid();

    public ICollection<Experience> Experiences { get; init; } = [];
}