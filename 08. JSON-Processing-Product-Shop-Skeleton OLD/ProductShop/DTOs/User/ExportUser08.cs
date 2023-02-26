using Newtonsoft.Json;
using ProductShop.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.DTOs.User
{
    [JsonObject]
    public class ExportUser08
    {
        [JsonProperty("usersCount")]
        public int UsersCount { get; set; }

        [JsonProperty("users")]
        public ExportUserAtLeastOneSoldProductDto08[] Users { get; set; }
    }
}
