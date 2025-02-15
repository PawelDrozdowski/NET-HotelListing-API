﻿using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.data;
using HotelListing.API.Repositories;

namespace HotelListing.API.Repositories
{
    public class HotelsRepository : GenericRepository<Hotel>, IHotelsRepository
    {
        public HotelsRepository(HotelListingDbContext context, IMapper mapper) : base(context, mapper)
        {
        }
    }
}
