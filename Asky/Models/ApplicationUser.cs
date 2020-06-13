using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Asky.Models
{
    public class ApplicationUser : IdentityUser<string>
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string About { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        public string ImageUri { get; set; }

        public ICollection<Topic> Topics { get; set; }

        public ICollection<Vote> Votes { get; set; }

        public ApplicationUser()
        {
            CreatedAt = DateTime.Now;
            Topics = new List<Topic>();
            Votes = new List<Vote>();
        }

        public string GetName()
        {
            return FirstName + " " + LastName;
        }

        public string GetImage()
        {
            return ImageUri ?? "default.jpg";
        }
    }
}
