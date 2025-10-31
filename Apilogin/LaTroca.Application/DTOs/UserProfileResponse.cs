using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTroca.Application.DTOs
{
    public class UserProfileResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string ProfilePicUrl { get; set; }
        public string Bio { get; set; }
        public string Role { get; set; }
        public LocationDto Location { get; set; }
        public int Reputation { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LocationDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Manual { get; set; }
    }
}
