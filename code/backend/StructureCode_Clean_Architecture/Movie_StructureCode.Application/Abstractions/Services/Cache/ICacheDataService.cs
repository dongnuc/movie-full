namespace Movie_StructureCode.Application.Abstractions.Services.Cache
{
    public interface ICacheDataService
    {
        Task<T?> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan? expiration);
        Task RemoveAsync(string key);

        // version global(apply in cache list)
        Task<long> InCrementTagVersionAsync(string tag);
        Task<string> GetTagVersionAsync(string tag);
    
        // version for specific key
         Task<long> InCrementEntityVersionAsync(string entity, Guid id);
         Task<string> GetEntityVersionAsync(string entity, Guid id);
    }
}
