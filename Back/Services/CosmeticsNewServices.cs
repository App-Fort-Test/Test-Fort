using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class CosmeticsNewServices
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private NewCosmeticsResponse? _cachedNew;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly object _cacheLock = new object();
        private const int CACHE_DURATION_MINUTES = 10; // Cache por 10 minutos (muda menos frequentemente)

        public CosmeticsNewServices(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<NewCosmeticsResponse> GetNewCosmeticsAsync()
        {
            // Verificar cache
            lock (_cacheLock)
            {
                if (_cachedNew != null && DateTime.UtcNow < _cacheExpiry)
                {
                    return _cachedNew;
                }
            }

            // Buscar da API
            var client = _httpFactory.CreateClient("FortniteApi");
            var response = await client.GetFromJsonAsync<NewCosmeticsResponse>("cosmetics/new", _jsonOptions);
            var newResponse = response ?? new NewCosmeticsResponse();

            // Atualizar cache
            lock (_cacheLock)
            {
                _cachedNew = newResponse;
                _cacheExpiry = DateTime.UtcNow.AddMinutes(CACHE_DURATION_MINUTES);
            }

            return newResponse;
        }
    }

    // Modelos para a resposta da API /cosmetics/new
    public class NewCosmeticsResponse
    {
        public int Status { get; set; }
        public NewCosmeticsData Data { get; set; } = new();
    }

    public class NewCosmeticsData
    {
        public string Date { get; set; } = "";
        public string Build { get; set; } = "";
        public string PreviousBuild { get; set; } = "";
        public Hashes Hashes { get; set; } = new();
        public LastAdditions LastAdditions { get; set; } = new();
        public NewCosmeticsItems Items { get; set; } = new();
    }

    public class Hashes
    {
        public string All { get; set; } = "";
        public string Br { get; set; } = "";
        public string Tracks { get; set; } = "";
        public string Cars { get; set; } = "";
        public string Lego { get; set; } = "";
    }

    public class LastAdditions
    {
        public string All { get; set; } = "";
        public string Br { get; set; } = "";
        public string Tracks { get; set; } = "";
        public string Instruments { get; set; } = "";
        public string Cars { get; set; } = "";
        public string Lego { get; set; } = "";
        public string LegoKits { get; set; } = "";
        public string Beans { get; set; } = "";
    }

    public class NewCosmeticsItems
    {
        public List<NewBrCosmeticData> Br { get; set; } = new();
        public List<CarCosmeticData> Cars { get; set; } = new();
        public List<LegoCosmeticData> Lego { get; set; } = new();
    }

    public class NewBrCosmeticData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public TypeInfo Type { get; set; } = new();
        public RarityInfo Rarity { get; set; } = new();
        public SetInfo Set { get; set; } = new();
        public IntroductionInfo Introduction { get; set; } = new();
        public NewBrImages Images { get; set; } = new();
        public string DynamicPakId { get; set; } = "";
        public DateTime Added { get; set; }
    }

    public class SetInfo
    {
        public string Value { get; set; } = "";
        public string Text { get; set; } = "";
        public string BackendValue { get; set; } = "";
    }

    public class IntroductionInfo
    {
        public string Chapter { get; set; } = "";
        public string Season { get; set; } = "";
        public string Text { get; set; } = "";
        public int BackendValue { get; set; } = 0;
    }

    public class NewBrImages
    {
        public string SmallIcon { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Featured { get; set; } = "";
        public LegoImageInfo Lego { get; set; } = new();
    }

    public class LegoImageInfo
    {
        public string Small { get; set; } = "";
        public string Large { get; set; } = "";
    }

    public class CarCosmeticData
    {
        public string Id { get; set; } = "";
        public string VehicleId { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public TypeInfo Type { get; set; } = new();
        public RarityInfo Rarity { get; set; } = new();
        public CarImages Images { get; set; } = new();
        public SeriesInfo Series { get; set; } = new();
        public DateTime Added { get; set; }
    }

    public class CarImages
    {
        public string Small { get; set; } = "";
        public string Large { get; set; } = "";
    }

    public class SeriesInfo
    {
        public string Value { get; set; } = "";
        public List<string> Colors { get; set; } = new();
        public string BackendValue { get; set; } = "";
    }

    public class LegoCosmeticData
    {
        public string Id { get; set; } = "";
        public string CosmeticId { get; set; } = "";
        public LegoImages Images { get; set; } = new();
        public DateTime Added { get; set; }
    }

    public class LegoImages
    {
        public string Small { get; set; } = "";
        public string Large { get; set; } = "";
    }
}
