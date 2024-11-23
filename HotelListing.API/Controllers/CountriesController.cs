using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.data;
using AutoMapper;
using HotelListing.API.Models.Country;
using HotelListing.API.Contracts;
using HotelListing.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using HotelListing.API.Exceptions;
using HotelListing.API.Models;
using Microsoft.AspNetCore.OData.Query;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountriesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesController> logger)
        {
            _mapper = mapper;
            _countriesRepository = countriesRepository;
            _logger = logger;
        }

        // GET: api/Countries
        [HttpGet("GetAll")]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            return await _countriesRepository.GetAllAsync<GetCountryDto>();
        }
        // GET: api/PagedCountries
        [HttpGet]
        public async Task<ActionResult<PagedResult<GetCountryDto>>> GetPagedCountries([FromQuery] QueryParams queryParams)
        {
            return await _countriesRepository.GetAllAsync<GetCountryDto>(queryParams);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            
            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountry),id);
            }

            return _mapper.Map<CountryDto>(country); ;
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryDto)
        {
            if (id != countryDto.Id)
            {
                return BadRequest();
            }

            //var country = _mapper.Map<Country>(countryDto);
            //_context.Entry(country).State = EntityState.Modified;

            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
                throw new NotFoundException(nameof(PutCountry), id);
            _mapper.Map(countryDto, country);

            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _countriesRepository.Exists(id))
                {
                    throw new NotFoundException(nameof(PutCountry), id);
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto countryDto)
        {
            /*old way
            var country = _mapper.Map<Country>(countryDto);
            await _countriesRepository.AddAsync(country);
            */
            var country = await _countriesRepository.AddAsync<CreateCountryDto, GetCountryDto>(countryDto);
            return CreatedAtAction(nameof(GetCountry), new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                throw new NotFoundException(nameof(DeleteCountry), id);
            }

            await _countriesRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
