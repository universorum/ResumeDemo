using JetBrains.Annotations;

namespace ResumeDemo.Server.Areas.Api.Models;

[UsedImplicitly]
public class ExperienceRequest
{
    public int? Id { get; set; }

    public required string Company { get; set; }

    public required string Title { get; set; }

    public required DateOnly Start { get; set; }

    public required DateOnly? End { get; set; }

    public Guid? Version { get; set; }
}