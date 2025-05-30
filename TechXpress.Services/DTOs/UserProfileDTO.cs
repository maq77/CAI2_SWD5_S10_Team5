using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechXpress.Services.DTOs.ViewModels;

namespace TechXpress.Services.DTOs
{
    public class UserProfileDTO
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Email { get; set; }
        public bool   IsConfirmed { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public AddressViewModel Address_ { get; set; } = new();
        public IFormFile? Image { get; set; } //to upload a new image
        public UserImageDTO UserImage { get; set; } = new(); // to display the current image
    }
}
