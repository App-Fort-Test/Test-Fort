using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Services
{
    public class CosmeticsEnrichedServices
    {
        private readonly CosmeticsServices _cosmeticsServices;
        private readonly CosmeticsNewServices _newServices;
        private readonly ShopServices _shopServices;
        private readonly UserInventoryService _inventoryService;

        public CosmeticsEnrichedServices(
            CosmeticsServices cosmeticsServices,
            CosmeticsNewServices newServices,
            ShopServices shopServices,
            UserInventoryService inventoryService)
        {
            _cosmeticsServices = cosmeticsServices;
            _newServices = newServices;
            _shopServices = shopServices;
            _inventoryService = inventoryService;
        }

        // Função auxiliar para gerar preço aleatório determinístico baseado no ID
        private int GenerateRandomPrice(string cosmeticId)
        {
            // Usar hash do ID para gerar um número determinístico entre 1 e 10000
            var hash = cosmeticId.GetHashCode();
            var random = new Random(Math.Abs(hash));
            return random.Next(1, 10001);
        }

        public async Task<List<EnrichedCosmetic>> GetEnrichedCosmeticsAsync(int page = 1, int pageSize = 50, string sortBy = "", int? userId = null)
        {
            // Buscar todos os cosméticos
            var cosmeticsTask = _cosmeticsServices.GetBrCosmeticsPagedAsync(page, pageSize);
            
            // Buscar novos cosméticos e shop em paralelo (com cache)
            var newCosmeticsTask = _newServices.GetNewCosmeticsAsync();
            var shopTask = _shopServices.GetShopAsync();
            
            // Aguardar todas as tarefas em paralelo
            await Task.WhenAll(cosmeticsTask, newCosmeticsTask, shopTask);
            
            var cosmetics = await cosmeticsTask;
            var newCosmeticsResponse = await newCosmeticsTask;
            var shopResponse = await shopTask;
            
            var newCosmeticIds = newCosmeticsResponse.Data?.Items?.Br?
                .Select(c => c.Id)
                .ToHashSet() ?? new HashSet<string>();
            var shopCosmeticIds = shopResponse.Data?.Entries?
                .SelectMany(e => e.BrItems?.Select(i => i.Id) ?? new List<string>())
                .ToHashSet() ?? new HashSet<string>();

            // Criar dicionário que mapeia cosmeticId -> ShopEntry (que contém os preços)
            var shopCosmetics = new Dictionary<string, ShopEntry>();
            var bundleEntries = new Dictionary<string, ShopEntry>(); // Para identificar bundles
            
            if (shopResponse.Data?.Entries != null)
            {
                foreach (var entry in shopResponse.Data.Entries)
                {
                    if (entry.BrItems != null && entry.BrItems.Count > 0)
                    {
                        // Se tem mais de um item, é um bundle
                        if (entry.BrItems.Count > 1)
                        {
                            // Para cada item do bundle, marcar como bundle
                            foreach (var item in entry.BrItems)
                            {
                                if (!shopCosmetics.ContainsKey(item.Id))
                                {
                                    shopCosmetics[item.Id] = entry;
                                    bundleEntries[item.Id] = entry;
                                }
                            }
                        }
                        else
                        {
                            // Item único
                        foreach (var item in entry.BrItems)
                        {
                            if (!shopCosmetics.ContainsKey(item.Id))
                            {
                                shopCosmetics[item.Id] = entry;
                            }
                        }
                    }
                }
                }
            }

            // Buscar cosméticos possuídos pelo usuário (se logado)
            HashSet<string> ownedCosmeticIds = new HashSet<string>();
            if (userId.HasValue)
            {
                var owned = await _inventoryService.GetOwnedCosmeticsAsync(userId.Value);
                ownedCosmeticIds = owned.ToHashSet();
            }

            // Enriquecer cosméticos
            var enriched = cosmetics.Select(c => {
                var isBundle = bundleEntries.ContainsKey(c.Id);
                var bundleItems = new List<BundleItem>();
                
                if (isBundle && bundleEntries[c.Id].BrItems != null)
                {
                    // Calcular preço por item (preço total dividido pelo número de itens)
                    var entry = bundleEntries[c.Id];
                    var pricePerItem = entry.BrItems.Count > 0 ? entry.FinalPrice / entry.BrItems.Count : 0;
                    
                    bundleItems = entry.BrItems.Select(item => new BundleItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = pricePerItem
                    }).ToList();
                }
                
                // Determinar preço: usar do shop se disponível, senão gerar aleatório
                int? finalPrice = null;
                int? regularPrice = null;
                
                if (shopCosmetics.ContainsKey(c.Id))
                {
                    finalPrice = shopCosmetics[c.Id].FinalPrice;
                    regularPrice = shopCosmetics[c.Id].RegularPrice;
                }
                else
                {
                    // Gerar preço aleatório determinístico
                    finalPrice = GenerateRandomPrice(c.Id);
                    regularPrice = finalPrice; // Para itens sem shop, regularPrice = finalPrice
                }
                
                return new EnrichedCosmetic
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                Rarity = c.Rarity,
                Images = c.Images,
                Added = c.Added,
                IsNew = newCosmeticIds.Contains(c.Id),
                IsInShop = shopCosmeticIds.Contains(c.Id),
                    IsOwned = userId.HasValue && ownedCosmeticIds.Contains(c.Id),
                IsOnSale = shopCosmetics.ContainsKey(c.Id) && 
                          shopCosmetics[c.Id].FinalPrice < shopCosmetics[c.Id].RegularPrice,
                    Price = finalPrice,
                    RegularPrice = regularPrice,
                    IsBundle = isBundle,
                    BundleItems = isBundle ? bundleItems : null
                };
            });

            // Aplicar ordenação
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                enriched = QueryableExtensions.ApplySorting(enriched, sortBy);
            }

            return enriched.ToList();
        }

        public async Task<List<EnrichedCosmetic>> SearchCosmeticsAsync(CosmeticSearchFilter filter, int? userId = null)
        {
            // Se não há filtros complexos, usar método otimizado
            bool needsFullSearch = !string.IsNullOrWhiteSpace(filter.Name) ||
                                   !string.IsNullOrWhiteSpace(filter.Type) ||
                                   !string.IsNullOrWhiteSpace(filter.Rarity) ||
                                   filter.DateFrom.HasValue ||
                                   filter.DateTo.HasValue ||
                                   filter.MinPrice.HasValue ||
                                   filter.MaxPrice.HasValue ||
                                   filter.OnlyNew == true ||
                                   filter.OnlyInShop == true ||
                                   filter.OnlyOnSale == true ||
                                   filter.OnlyOwned == true ||
                                   filter.OnlyBundle == true;

            List<EnrichedCosmetic> allCosmetics;
            
            if (needsFullSearch)
            {
                // Busca completa necessária - buscar todos
                allCosmetics = await GetAllCosmeticsAsync(userId);
            }
            else
            {
                // Sem filtros complexos, buscar apenas a página necessária
                allCosmetics = await GetEnrichedCosmeticsAsync(filter.Page, filter.PageSize, filter.SortBy, userId);
            }
            
            var query = allCosmetics.AsQueryable();

            // Filtro por nome
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(c => c.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            }

            // Filtro por tipo
            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(c => c.Type.Value.Equals(filter.Type, StringComparison.OrdinalIgnoreCase));
            }

            // Filtro por raridade
            if (!string.IsNullOrWhiteSpace(filter.Rarity))
            {
                query = query.Where(c => c.Rarity.Value.Equals(filter.Rarity, StringComparison.OrdinalIgnoreCase));
            }

            // Filtro por data de inclusão
            if (filter.DateFrom.HasValue)
            {
                query = query.Where(c => c.Added >= filter.DateFrom.Value);
            }
            if (filter.DateTo.HasValue)
            {
                query = query.Where(c => c.Added <= filter.DateTo.Value);
            }

            // Filtro apenas novos
            if (filter.OnlyNew == true)
            {
                query = query.Where(c => c.IsNew);
            }

            // Filtro apenas à venda
            if (filter.OnlyInShop == true)
            {
                query = query.Where(c => c.IsInShop);
            }

            // Filtro apenas em promoção
            if (filter.OnlyOnSale == true)
            {
                query = query.Where(c => c.IsOnSale);
            }

            // Filtro apenas possuídos
            if (filter.OnlyOwned == true)
            {
                query = query.Where(c => c.IsOwned);
            }

            // Filtro apenas bundles
            if (filter.OnlyBundle == true)
            {
                query = query.Where(c => c.IsBundle);
            }

            // Filtro por preço mínimo
            if (filter.MinPrice.HasValue)
            {
                query = query.Where(c => c.Price.HasValue && c.Price >= filter.MinPrice.Value);
            }

            // Filtro por preço máximo
            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(c => c.Price.HasValue && c.Price <= filter.MaxPrice.Value);
            }

            // Aplicar ordenação
            if (!string.IsNullOrWhiteSpace(filter.SortBy))
            {
                query = QueryableExtensions.ApplySorting(query, filter.SortBy);
            }

            // Contar total antes de paginar
            var totalCount = query.Count();
            
            var cosmetics = query.Skip((filter.Page - 1) * filter.PageSize)
                       .Take(filter.PageSize)
                       .ToList();

            return cosmetics;
        }

        public async Task<int> GetTotalCountAsync(CosmeticSearchFilter filter, int? userId = null)
        {
            // Reutilizar a mesma lógica de busca, mas sem paginação
            bool needsFullSearch = !string.IsNullOrWhiteSpace(filter.Name) ||
                                   !string.IsNullOrWhiteSpace(filter.Type) ||
                                   !string.IsNullOrWhiteSpace(filter.Rarity) ||
                                   filter.DateFrom.HasValue ||
                                   filter.DateTo.HasValue ||
                                   filter.MinPrice.HasValue ||
                                   filter.MaxPrice.HasValue ||
                                   filter.OnlyNew == true ||
                                   filter.OnlyInShop == true ||
                                   filter.OnlyOnSale == true ||
                                   filter.OnlyOwned == true ||
                                   filter.OnlyBundle == true;

            List<EnrichedCosmetic> allCosmetics;
            
            if (needsFullSearch)
            {
                allCosmetics = await GetAllCosmeticsAsync(userId);
            }
            else
            {
                // Sem filtros, usar método otimizado para obter contagem total
                return await GetTotalCosmeticsCountAsync(userId);
            }
            
            var query = allCosmetics.AsQueryable();

            // Aplicar os mesmos filtros
            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                query = query.Where(c => c.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Type))
            {
                query = query.Where(c => c.Type.Value.Equals(filter.Type, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(filter.Rarity))
            {
                query = query.Where(c => c.Rarity.Value.Equals(filter.Rarity, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.DateFrom.HasValue)
            {
                query = query.Where(c => c.Added >= filter.DateFrom.Value);
            }
            if (filter.DateTo.HasValue)
            {
                query = query.Where(c => c.Added <= filter.DateTo.Value);
            }

            if (filter.OnlyNew == true)
            {
                query = query.Where(c => c.IsNew);
            }

            if (filter.OnlyInShop == true)
            {
                query = query.Where(c => c.IsInShop);
            }

            if (filter.OnlyOnSale == true)
            {
                query = query.Where(c => c.IsOnSale);
            }

            if (filter.OnlyOwned == true)
            {
                query = query.Where(c => c.IsOwned);
            }

            if (filter.OnlyBundle == true)
            {
                query = query.Where(c => c.IsBundle);
            }

            if (filter.MinPrice.HasValue)
            {
                query = query.Where(c => c.Price.HasValue && c.Price >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                query = query.Where(c => c.Price.HasValue && c.Price <= filter.MaxPrice.Value);
            }

            return query.Count();
        }

        private async Task<List<EnrichedCosmetic>> GetAllCosmeticsAsync(int? userId = null)
        {
            // Buscar todos os cosméticos de uma vez (com cache no CosmeticsServices)
            var allCosmeticData = await _cosmeticsServices.GetAllCosmeticsAsync();
            
            // Buscar shop e new uma vez (com cache)
            var newCosmeticsTask = _newServices.GetNewCosmeticsAsync();
            var shopTask = _shopServices.GetShopAsync();
            await Task.WhenAll(newCosmeticsTask, shopTask);
            
            var newCosmeticsResponse = await newCosmeticsTask;
            var shopResponse = await shopTask;
            
            var newCosmeticIds = newCosmeticsResponse.Data?.Items?.Br?
                .Select(c => c.Id)
                .ToHashSet() ?? new HashSet<string>();
            
            var shopCosmeticIds = shopResponse.Data?.Entries?
                .SelectMany(e => e.BrItems?.Select(i => i.Id) ?? new List<string>())
                .ToHashSet() ?? new HashSet<string>();

            var shopCosmetics = new Dictionary<string, ShopEntry>();
            var bundleEntries = new Dictionary<string, ShopEntry>();
            
            if (shopResponse.Data?.Entries != null)
            {
                foreach (var entry in shopResponse.Data.Entries)
                {
                    if (entry.BrItems != null && entry.BrItems.Count > 0)
                    {
                        if (entry.BrItems.Count > 1)
                        {
                            foreach (var item in entry.BrItems)
                            {
                                if (!shopCosmetics.ContainsKey(item.Id))
                                {
                                    shopCosmetics[item.Id] = entry;
                                    bundleEntries[item.Id] = entry;
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in entry.BrItems)
                            {
                                if (!shopCosmetics.ContainsKey(item.Id))
                                {
                                    shopCosmetics[item.Id] = entry;
                                }
                            }
                        }
                    }
                }
            }

            HashSet<string> ownedCosmeticIds = new HashSet<string>();
            if (userId.HasValue)
            {
                var owned = await _inventoryService.GetOwnedCosmeticsAsync(userId.Value);
                ownedCosmeticIds = owned.ToHashSet();
            }

            // Enriquecer todos os cosméticos de uma vez
            var enriched = allCosmeticData.Select(c => {
                var isBundle = bundleEntries.ContainsKey(c.Id);
                var bundleItems = new List<BundleItem>();
                
                if (isBundle && bundleEntries[c.Id].BrItems != null)
                {
                    var entry = bundleEntries[c.Id];
                    var pricePerItem = entry.BrItems.Count > 0 ? entry.FinalPrice / entry.BrItems.Count : 0;
                    
                    bundleItems = entry.BrItems.Select(item => new BundleItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = pricePerItem
                    }).ToList();
                }
                
                int? finalPrice = null;
                int? regularPrice = null;
                
                if (shopCosmetics.ContainsKey(c.Id))
                {
                    finalPrice = shopCosmetics[c.Id].FinalPrice;
                    regularPrice = shopCosmetics[c.Id].RegularPrice;
                }
                else
                {
                    finalPrice = GenerateRandomPrice(c.Id);
                    regularPrice = finalPrice;
                }
                
                return new EnrichedCosmetic
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type,
                    Rarity = c.Rarity,
                    Images = c.Images,
                    Added = c.Added,
                    IsNew = newCosmeticIds.Contains(c.Id),
                    IsInShop = shopCosmeticIds.Contains(c.Id),
                    IsOwned = userId.HasValue && ownedCosmeticIds.Contains(c.Id),
                    IsOnSale = shopCosmetics.ContainsKey(c.Id) && 
                              shopCosmetics[c.Id].FinalPrice < shopCosmetics[c.Id].RegularPrice,
                    Price = finalPrice,
                    RegularPrice = regularPrice,
                    IsBundle = isBundle,
                    BundleItems = isBundle ? bundleItems : null
                };
            });

            return enriched.ToList();
        }

        public async Task<int> GetTotalCosmeticsCountAsync(int? userId = null)
        {
            // Usar cache do CosmeticsServices para obter contagem total sem buscar todos
            var allCosmetics = await _cosmeticsServices.GetAllCosmeticsAsync();
            return allCosmetics.Count;
        }

        public async Task<object> GetNewCosmeticsDataAsync()
        {
            var newCosmetics = await _newServices.GetNewCosmeticsAsync();
            return newCosmetics;
        }

        public async Task<object> GetShopDataAsync()
        {
            var shop = await _shopServices.GetShopAsync();
            return shop;
        }

        public async Task<object> GetFilterOptionsAsync(
            string name = "",
            string type = "",
            string rarity = "",
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            bool? onlyNew = null,
            bool? onlyInShop = null,
            bool? onlyOnSale = null,
            bool? onlyOwned = null,
            bool? onlyBundle = null,
            int? minPrice = null,
            int? maxPrice = null,
            int? userId = null)
        {
            // Buscar todos os cosméticos enriquecidos
            var allCosmetics = await GetAllCosmeticsAsync(userId);
            
            // Primeiro, obter TODAS as categorias e raridades únicas de TODOS os cosméticos
            // Normalizar para garantir que tipos/raridades com mesmo nome (case-insensitive) sejam agrupados
            var allTypes = allCosmetics
                .Where(c => c.Type != null && !string.IsNullOrWhiteSpace(c.Type.Value))
                .GroupBy(c => c.Type.Value, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { 
                    value = g.Key, // Usar a primeira ocorrência como valor canônico
                    label = g.First().Type.DisplayValue ?? g.Key 
                })
                .OrderBy(x => x.value)
                .ToList();
            
            var allRarities = allCosmetics
                .Where(c => c.Rarity != null && !string.IsNullOrWhiteSpace(c.Rarity.Value))
                .GroupBy(c => c.Rarity.Value, StringComparer.OrdinalIgnoreCase)
                .Select(g => new { 
                    value = g.Key, // Usar a primeira ocorrência como valor canônico
                    label = g.First().Rarity.DisplayValue ?? g.Key 
                })
                .OrderBy(x => x.value)
                .ToList();
            
            // Aplicar filtros (exceto type e rarity que queremos contar)
            var query = allCosmetics.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(c => c.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }
            
            if (dateFrom.HasValue)
            {
                query = query.Where(c => c.Added >= dateFrom.Value);
            }
            if (dateTo.HasValue)
            {
                query = query.Where(c => c.Added <= dateTo.Value);
            }
            
            if (onlyNew == true)
            {
                query = query.Where(c => c.IsNew);
            }
            
            if (onlyInShop == true)
            {
                query = query.Where(c => c.IsInShop);
            }
            
            if (onlyOnSale == true)
            {
                query = query.Where(c => c.IsOnSale);
            }
            
            if (onlyOwned == true)
            {
                query = query.Where(c => c.IsOwned);
            }
            
            if (onlyBundle == true)
            {
                query = query.Where(c => c.IsBundle);
            }
            
            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price.HasValue && c.Price >= minPrice.Value);
            }
            
            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price.HasValue && c.Price <= maxPrice.Value);
            }
            
            var filteredCosmetics = query.ToList();
            
            // Contar tipos nos cosméticos filtrados (usando comparação case-insensitive)
            var typeCountsDict = filteredCosmetics
                .Where(c => c.Type != null && !string.IsNullOrWhiteSpace(c.Type.Value))
                .GroupBy(c => c.Type.Value, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key.ToLowerInvariant(), g => g.Count(), StringComparer.OrdinalIgnoreCase);
            
            // Criar lista de tipos com contagens (incluindo os que têm count 0)
            var typeCounts = allTypes.Select(t => new 
            { 
                value = t.value, 
                label = t.label, 
                count = typeCountsDict.ContainsKey(t.value.ToLowerInvariant()) ? typeCountsDict[t.value.ToLowerInvariant()] : 0 
            }).ToList();
            
            // Contar raridades nos cosméticos filtrados (usando comparação case-insensitive)
            var rarityCountsDict = filteredCosmetics
                .Where(c => c.Rarity != null && !string.IsNullOrWhiteSpace(c.Rarity.Value))
                .GroupBy(c => c.Rarity.Value, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key.ToLowerInvariant(), g => g.Count(), StringComparer.OrdinalIgnoreCase);
            
            // Criar lista de raridades com contagens (incluindo as que têm count 0)
            var rarityCounts = allRarities.Select(r => new 
            { 
                value = r.value, 
                label = r.label, 
                count = rarityCountsDict.ContainsKey(r.value.ToLowerInvariant()) ? rarityCountsDict[r.value.ToLowerInvariant()] : 0 
            }).ToList();
            
            return new
            {
                types = typeCounts,
                rarities = rarityCounts
            };
        }
    }

    public class EnrichedCosmetic
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public TypeInfo Type { get; set; } = new();
        public RarityInfo Rarity { get; set; } = new();
        public Images Images { get; set; } = new();
        public DateTime Added { get; set; }
        public bool IsNew { get; set; }
        public bool IsInShop { get; set; }
        public bool IsOwned { get; set; }
        public bool IsOnSale { get; set; }
        public int? Price { get; set; }
        public int? RegularPrice { get; set; }
        public bool IsBundle { get; set; }
        public List<BundleItem>? BundleItems { get; set; }
    }

    public class BundleItem
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int Price { get; set; }
    }

    public class CosmeticSearchFilter
    {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Rarity { get; set; } = "";
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? OnlyNew { get; set; }
        public bool? OnlyInShop { get; set; }
        public bool? OnlyOnSale { get; set; }
        public bool? OnlyOwned { get; set; }
        public bool? OnlyBundle { get; set; }
        public int? MinPrice { get; set; }
        public int? MaxPrice { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
        public string SortBy { get; set; } = "";
    }

    public static class QueryableExtensions
    {
        public static IQueryable<EnrichedCosmetic> ApplySorting(IQueryable<EnrichedCosmetic> query, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "newest" => query.OrderByDescending(c => c.Added),
                "oldest" => query.OrderBy(c => c.Added),
                "name_asc" => query.OrderBy(c => c.Name),
                "name_desc" => query.OrderByDescending(c => c.Name),
                "price_asc" => query.OrderBy(c => c.Price ?? int.MaxValue),
                "price_desc" => query.OrderByDescending(c => c.Price ?? int.MinValue),
                _ => query.OrderByDescending(c => c.Added) // padrão: mais recentes primeiro
            };
        }

        public static IEnumerable<EnrichedCosmetic> ApplySorting(IEnumerable<EnrichedCosmetic> query, string sortBy)
        {
            return sortBy.ToLower() switch
            {
                "newest" => query.OrderByDescending(c => c.Added),
                "oldest" => query.OrderBy(c => c.Added),
                "name_asc" => query.OrderBy(c => c.Name),
                "name_desc" => query.OrderByDescending(c => c.Name),
                "price_asc" => query.OrderBy(c => c.Price ?? int.MaxValue),
                "price_desc" => query.OrderByDescending(c => c.Price ?? int.MinValue),
                _ => query.OrderByDescending(c => c.Added) // padrão: mais recentes primeiro
            };
        }
    }
}
