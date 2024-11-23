using AutoMapper;
using AutoMapper.QueryableExtensions;
using HotelListing.API.Contracts;
using HotelListing.API.data;
using HotelListing.API.Exceptions;
using HotelListing.API.Models;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.EntityFrameworkCore;

namespace HotelListing.API.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly HotelListingDbContext _context;
        private readonly IMapper _mapper;

        public GenericRepository(HotelListingDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<T> AddAsync(T entity)
        {
            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<TResult> AddAsync<TSource, TResult>(TSource source)
        {
            var entity = _mapper.Map<T>(source);

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<TResult>(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetAsync(id);
            if (entity == null)
                throw new NotFoundException(typeof(T).Name, id);

            _context.Set<T>().Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> Exists(int id)
        {
            var entity = await GetAsync(id);
            return entity != null;
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<List<TResult>> GetAllAsync<TResult>()
        {
            return await _context.Set<T>()
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PagedResult<TResult>> GetAllAsync<TResult>(QueryParams queryParams)
        {
            var totalCount = await _context.Set<T>().CountAsync();
            var results = await _context.Set<T>()
                .Skip(queryParams.StartIndex)
                .Take(queryParams.PageSize)
                .ProjectTo<TResult>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return new PagedResult<TResult>
            { Items = results, TotalCount = totalCount, RecordNumber = queryParams.StartIndex };
        }

        public async Task<T> GetAsync(int? id)
        {
            if (id == null)
            {
                throw new BadRequestException("Id not provided");
            }

            return await _context.Set<T>().FindAsync(id);
        }
#nullable enable
        public async Task<TResult?> GetAsync<TResult>(int? id)
        {
            if (id == null)
            {
                throw new BadRequestException("Id not provided");
            }

            var result = await _context.Set<T>().FindAsync(id);
            return _mapper.Map<TResult>(result);
        }
#nullable disable
        public async Task UpdateAsync(T entity)
        {
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync<TSource>(int id, TSource source) where TSource : IBaseDto
        {
            if (id != source.Id)
            {
                throw new BadRequestException("Invalid Id used in request");
            }

            var entity = await GetAsync(id);

            if (entity == null)
            {
                throw new NotFoundException(typeof(T).Name, id);
            }

            _mapper.Map(source, entity);
            _context.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}