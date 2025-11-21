using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http.Json;
   

namespace Backend.Services
{
    public class CosmeticsSevices
    {
        private readonly IHttpClientFactory _httpFactory;

        CosmeticsSevices(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<CosmeticsResponse?> GetCosmeticsAsync()
        {
            var client = _httpFactory.CreateClient("FortniteApi");
            // Chama o endpoint /cosmetics
            var response = await client.GetFromJsonAsync<CosmeticsResponse>("cosmetics");
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
        // Adicione mais propriedades conforme o JSON da API
    }
}