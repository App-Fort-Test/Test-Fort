using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class CosmeticsServices
    {
        private readonly IHttpClientFactory _httpFactory;

        public CosmeticsServices(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<CosmeticsResponse?> GetCosmeticsAsync()
        {
            var client = _httpFactory.CreateClient("FortniteApi");
            var response = await client.GetFromJsonAsync<CosmeticsResponse>("cosmetics");
            return response;
        }

    }

    public class CosmeticsResponse
    {
        public int Status { get; set; } // antes era string
        public Data Data { get; set; } = new();
    }

    public class Data
    {
        public List<CosmeticData> Br { get; set; } = new();
    }


    public class CosmeticData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public TypeInfo Type { get; set; } = new();
        public RarityInfo Rarity { get; set; } = new();
        public Images Images { get; set; } = new();
        public DateTime Added { get; set; }
    }

    public class TypeInfo
    {
        public string Value { get; set; } = "";
        public string DisplayValue { get; set; } = "";
        public string BackendValue { get; set; } = "";
    }

    public class RarityInfo
    {
        public string Value { get; set; } = "";
        public string DisplayValue { get; set; } = "";
        public string BackendValue { get; set; } = "";
    }

    public class Images
    {
        public string SmallIcon { get; set; } = "";
    }
}
