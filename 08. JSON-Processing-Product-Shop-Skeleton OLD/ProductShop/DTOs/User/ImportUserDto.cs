using Newtonsoft.Json;
using ProductShop.Common;
using System.ComponentModel.DataAnnotations;

namespace ProductShop.DTOs.User
{
    [JsonObject]
    public class ImportUserDto
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        [Required]
        [MinLength(GlobalConstatns.UserLastNameMinLength)]
        public string LastName { get; set; }

        [JsonProperty("age")]
        public int? Age { get; set; }
    }
}
