using Microsoft.EntityFrameworkCore;

namespace ResumeDemo.Extensions;

public static class QueryableExtension
{
    public static IQueryable<T> AsTracking<T>(this IQueryable<T> source, bool tracking) where T : class
    {
        return tracking ? source.AsTracking() : source.AsNoTracking();
    }
}