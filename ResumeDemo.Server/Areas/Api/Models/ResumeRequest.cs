namespace ResumeDemo.Server.Areas.Api.Models;

public record ResumeRequest
{
    public required string Name { get; init; }

    public required DateOnly BirthDate { get; init; }

    public Guid? Version { get; set; }

    public required ICollection<ExperienceRequest> Experiences { get; init; }
}