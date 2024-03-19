using Microsoft.Extensions.DependencyInjection;
using ResumeDemo.Data;
using ResumeDemo.Services;

namespace ResumeDemo.Extensions;

public static class ServiceCollectionExtension
{
    public static void AddResumeManager<TContext>(this IServiceCollection services) where TContext : IAppDbContext
    {
        services.AddScoped<ResumeManager>();
        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<TContext>());
    }
}