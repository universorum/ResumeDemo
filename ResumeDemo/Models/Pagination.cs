namespace ResumeDemo.Models;

internal static class EmptyPagination<T>
{
    internal static readonly Pagination<T> Value = new(AsyncEnumerable.Empty<T>(), false);
}

public record Pagination<T>(IAsyncEnumerable<T> Result, bool HasNext)
{
    public static Pagination<T> Empty => EmptyPagination<T>.Value;
}