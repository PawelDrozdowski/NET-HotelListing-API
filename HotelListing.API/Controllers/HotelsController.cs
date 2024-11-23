using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.data;
using HotelListing.API.Contracts;
using HotelListing.API.Repositories;
using AutoMapper;
using HotelListing.API.Models.Hotel;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Models;
using HotelListing.API.Models.Country;
using HotelListing.API.Exceptions;

namespace HotelListing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private readonly IHotelsRepository _hotelsRepository;
        private readonly IMapper _mapper;

        public HotelsController(IHotelsRepository hotelsRepository, IMapper mapper)
        {
            _hotelsRepository = hotelsRepository;
            _mapper = mapper;
        }

        // GET: api/Hotels
        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<HotelDto>>> GetHotels()
        {
            return await _hotelsRepository.GetAllAsync<HotelDto>();
        }

        // GET: api/Hotels
        [HttpGet]
        public async Task<ActionResult<PagedResult<HotelDto>>> GetPagedHotels([FromQuery] QueryParams queryParams)
        {
            return await _hotelsRepository.GetAllAsync<HotelDto>(queryParams);
        }

        // GET: api/Hotels/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HotelDto>> GetHotel(int id)
        {
            /*old way
             * var hotel = await _hotelsRepository.GetAsync(id);

            if (hotel == null)
            {
                throw new NotFoundException(nameof(HotelDto),id);
            }
            */
            return await _hotelsRepository.GetAsync<HotelDto>(id);
        }

        // PUT: api/Hotels/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutHotel(int id, HotelDto hotelDto)
        {
            if (id != hotelDto.Id)
            {
                return BadRequest();
            }

            var hotel = await _hotelsRepository.GetAsync(id);//todo
            if (hotel == null)
                return NotFound();
            _mapper.Map(hotelDto, hotel);

            try
            {
                await _hotelsRepository.UpdateAsync(hotel);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _hotelsRepository.Exists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Hotels
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<HotelDto>> PostHotel(CreateHotelDto hotelDto)
        {
            /*old way
             * var hotel = _mapper.Map<Hotel>(hotelDto);
             * await _hotelsRepository.AddAsync(hotel);
            */
            var hotel = await _hotelsRepository.AddAsync<CreateHotelDto, HotelDto>(hotelDto);
            return CreatedAtAction("GetHotel", new { id = hotel.Id }, hotel);
        }

        // DELETE: api/Hotels/5
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteHotel(int id)
        {
            var hotel = await _hotelsRepository.GetAsync(id);
            if (hotel == null)
            {
                return NotFound();
            }

            await _hotelsRepository.DeleteAsync(id);

            return NoContent();
        }
    }
}
