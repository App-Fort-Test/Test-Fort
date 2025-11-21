using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;

namespace Backend.Services
{
    public class CosmeticsServices
    {
        private readonly IHttpClientFactory _httpFactory;
        private List<CosmeticData>? _cachedCosmetics;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly object _cacheLock = new object();
        private const int CACHE_DURATION_MINUTES = 30; // Cache por 30 minutos (muda raramente)

        public CosmeticsServices(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
        }

        public async Task<List<CosmeticData>> GetBrCosmeticsPagedAsync(int page = 1, int pageSize = 50)
        {
            // Buscar todos os cosméticos (com cache)
            var allCosmetics = await GetAllCosmeticsAsync();

            // Paginar em memória
            return allCosmetics
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        public async Task<List<CosmeticData>> GetAllCosmeticsAsync()
        {
            // Verificar cache
            lock (_cacheLock)
            {
                if (_cachedCosmetics != null && DateTime.UtcNow < _cacheExpiry)
                {
                    return _cachedCosmetics;
                }
            }

            // Buscar da API
            var client = _httpFactory.CreateClient("FortniteApi");
            var response = await client.GetFromJsonAsync<CosmeticsResponse>("cosmetics");

            var cosmetics = response?.Data?.Br ?? new List<CosmeticData>();

            // Atualizar cache
            lock (_cacheLock)
            {
                _cachedCosmetics = cosmetics;
                _cacheExpiry = DateTime.UtcNow.AddMinutes(CACHE_DURATION_MINUTES);
            }

            return cosmetics;
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
