using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class ShopServices
    {
        private readonly IHttpClientFactory _httpFactory;
        private readonly JsonSerializerOptions _jsonOptions;
        private ShopResponse? _cachedShop;
        private DateTime _cacheExpiry = DateTime.MinValue;
        private readonly object _cacheLock = new object();
        private const int CACHE_DURATION_MINUTES = 5; // Cache por 5 minutos

        public ShopServices(IHttpClientFactory httpFactory)
        {
            _httpFactory = httpFactory;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public async Task<ShopResponse> GetShopAsync()
        {
            // Verificar cache
            lock (_cacheLock)
            {
                if (_cachedShop != null && DateTime.UtcNow < _cacheExpiry)
                {
                    return _cachedShop;
                }
            }

            // Buscar da API
            var client = _httpFactory.CreateClient("FortniteApi");
            var response = await client.GetFromJsonAsync<ShopResponse>("shop", _jsonOptions);
            var shopResponse = response ?? new ShopResponse();

            // Atualizar cache
            lock (_cacheLock)
            {
                _cachedShop = shopResponse;
                _cacheExpiry = DateTime.UtcNow.AddMinutes(CACHE_DURATION_MINUTES);
            }

            return shopResponse;
        }
    }

    // Modelos para a resposta da API /shop
    public class ShopResponse
    {
        public int Status { get; set; }
        public ShopData Data { get; set; } = new();
    }

    public class ShopData
    {
        public string Hash { get; set; } = "";
        public string Date { get; set; } = "";
        public string VbuckIcon { get; set; } = "";
        public List<ShopEntry> Entries { get; set; } = new();
    }

    public class ShopEntry
    {
        public int RegularPrice { get; set; }
        public int FinalPrice { get; set; }
        public string DevName { get; set; } = "";
        public string OfferId { get; set; } = "";
        public string InDate { get; set; } = "";
        public string OutDate { get; set; } = "";
        public bool Giftable { get; set; }
        public bool Refundable { get; set; }
        public int SortPriority { get; set; }
        public string LayoutId { get; set; } = "";
        public LayoutInfo Layout { get; set; } = new();
        public ColorsInfo Colors { get; set; } = new();
        public string TileSize { get; set; } = "";
        public string DisplayAssetPath { get; set; } = "";
        public string NewDisplayAssetPath { get; set; } = "";
        public NewDisplayAsset NewDisplayAsset { get; set; } = new();
        public List<ShopBrItem> BrItems { get; set; } = new();
        public BannerInfo Banner { get; set; }
        public OfferTagInfo OfferTag { get; set; }
        public string TileBackgroundMaterial { get; set; } = "";
    }

    public class LayoutInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Index { get; set; }
        public int Rank { get; set; }
        public string ShowIneligibleOffers { get; set; } = "";
        public bool UseWidePreview { get; set; }
        public string DisplayType { get; set; } = "";
        public List<TextureMetadata> TextureMetadata { get; set; } = new();
        public List<StringMetadata> StringMetadata { get; set; } = new();
        public List<TextMetadata> TextMetadata { get; set; } = new();
    }

    public class TextureMetadata
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class StringMetadata
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class TextMetadata
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class ColorsInfo
    {
        public string Color1 { get; set; } = "";
        public string Color2 { get; set; } = "";
        public string Color3 { get; set; } = "";
        public string TextBackgroundColor { get; set; } = "";
    }

    public class NewDisplayAsset
    {
        public string Id { get; set; } = "";
        public string CosmeticId { get; set; } = "";
        public List<object> MaterialInstances { get; set; } = new();
        public List<RenderImage> RenderImages { get; set; } = new();
    }

    public class RenderImage
    {
        public string ProductTag { get; set; } = "";
        public string FileName { get; set; } = "";
        public string Image { get; set; } = "";
    }

    public class ShopBrItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public TypeInfo Type { get; set; } = new();
        public RarityInfo Rarity { get; set; } = new();
        public SetInfo Set { get; set; }
        public SeriesInfo Series { get; set; }
        public IntroductionInfo Introduction { get; set; } = new();
        public ShopImages Images { get; set; } = new();
        public List<string> MetaTags { get; set; } = new();
        public string ShowcaseVideo { get; set; } = "";
        public DateTime Added { get; set; }
        public List<VariantInfo> Variants { get; set; } = new();
    }

    public class ShopImages
    {
        public string SmallIcon { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Featured { get; set; } = "";
    }

    public class VariantInfo
    {
        public string Channel { get; set; } = "";
        public string Type { get; set; } = "";
        public List<VariantOption> Options { get; set; } = new();
    }

    public class VariantOption
    {
        public string Tag { get; set; } = "";
        public string Name { get; set; } = "";
        public string Image { get; set; } = "";
    }

    public class BannerInfo
    {
        public string Value { get; set; } = "";
        public string Intensity { get; set; } = "";
        public string BackendValue { get; set; } = "";
    }

    public class OfferTagInfo
    {
        public string Id { get; set; } = "";
        public string Text { get; set; } = "";
    }
}
