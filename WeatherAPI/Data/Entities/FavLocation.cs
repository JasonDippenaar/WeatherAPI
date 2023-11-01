using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WeatherAPI.Data.Entities
{
    [Index(nameof(Name), IsUnique = true)]
    public class FavLocation
    {
        [Key]
        [Required]
        public string Name { get; set; }
        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
    }
}
