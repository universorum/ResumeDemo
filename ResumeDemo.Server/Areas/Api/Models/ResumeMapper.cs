using AutoMapper;
using ResumeDemo.Models;

namespace ResumeDemo.Server.Areas.Api.Models;

public class ResumeMapper : Profile
{
    public ResumeMapper()
    {
        CreateMap<ResumeRequest, Resume>().ForMember(dst => dst.Id, opt => opt.Ignore());

        CreateMap<ExperienceRequest, Experience>();
    }
}