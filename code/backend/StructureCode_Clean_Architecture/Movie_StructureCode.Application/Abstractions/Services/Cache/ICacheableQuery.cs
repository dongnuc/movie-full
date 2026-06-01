namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public interface ICacheableQuery
    {
        string CacheKey { get; }
        TimeSpan? Expiration { get; }
        bool BypassCache { get; }

        Type ValueType { get; }

    }
}
