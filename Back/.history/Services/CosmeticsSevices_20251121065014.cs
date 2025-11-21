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
            
            var response = await client.GetFromJsonAsync<CosmeticsResponse>("/cosmetics");
            return response;
        }
    }

    public class CosmeticsResponse
    {
        public string Status { get; set; } = "";
        public CosmeticData[] Data { get; set; } = Array.Empty<CosmeticData>();
    }

    public class CosmeticData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
    }
}
