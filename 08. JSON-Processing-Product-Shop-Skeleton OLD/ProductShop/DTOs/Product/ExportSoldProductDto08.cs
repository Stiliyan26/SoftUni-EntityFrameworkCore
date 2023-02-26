using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProductShop.DTOs.Product
{
    [JsonObject]
    public class ExportSoldProductDto08
    {
        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("products")]
        public ExportSoldProductInfoDto08[] Products { get; set; }
    }
}
