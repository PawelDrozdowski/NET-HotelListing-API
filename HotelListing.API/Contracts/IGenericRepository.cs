using HotelListing.API.Models;

namespace HotelListing.API.Contracts
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T> GetAsync(int? id);
#nullable enable
        Task<TResult?> GetAsync<TResult>(int? id);
#nullable disable
        Task<List<TResult>> GetAllAsync<TResult>();
        Task<List<T>> GetAllAsync();
        Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParams queryParams);
        Task<T> AddAsync(T entity);
        Task<TResult> AddAsync<TSource, TResult>(TSource source);
        Task DeleteAsync(int id);
        Task UpdateAsync(T entity);
        Task UpdateAsync<TSource>(int id, TSource source) where TSource : IBaseDto;
        Task<bool> Exists(int id);
    }
}