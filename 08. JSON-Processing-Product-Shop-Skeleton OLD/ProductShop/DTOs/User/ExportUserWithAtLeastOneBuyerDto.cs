using Newtonsoft.Json;
using ProductShop.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.DTOs.User
{
    [JsonObject]
    public class ExportUserWithAtLeastOneBuyerDto
    {
        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("age")]
        public int Age { get; set; }

        [JsonProperty("soldProducts")]
        public ExportSoldProductDto08[] SoldProducts { get; set; }
    }
}
