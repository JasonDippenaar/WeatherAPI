using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WeatherAPI.Data.Entities;

namespace WeatherAPI.Data
{
    public class WeatherAPIContext : DbContext
    {
        public WeatherAPIContext (DbContextOptions<WeatherAPIContext> options)
            : base(options)
        {
        }

        public DbSet<WeatherAPI.Data.Entities.FavLocation> FavLocations { get; set; } = default!;
    }
}
