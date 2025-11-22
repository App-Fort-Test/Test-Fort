import { ref, reactive, computed } from 'vue';
import { cosmeticsAPI } from '../services/api';
import { fortniteExternalAPI } from '../services/fortniteApi';
import { useAuth } from './useAuth';

const cosmetics = ref([]);
const loading = ref(false);
const error = ref(null);
const vbucks = ref(0);
const currentPage = ref(1);
const pageSize = ref(20);
const totalItems = ref(0);
const sortBy = ref('');

const filters = reactive({
  name: '',
  type: '',
  rarity: '',
  onlyNew: false,
  onlyInShop: false,
  onlyOnSale: false,
  onlyOwned: false,
  onlyBundle: false,
  minPrice: null,
  maxPrice: null,
  startDate: null,
  endDate: null,
});

// Cache de páginas pré-carregadas (memória)
const pageCache = new Map();
const prefetchingPages = new Set(); // Páginas que estão sendo pré-carregadas

// Configurações do localStorage
const STORAGE_PREFIX = 'fortnite_cosmetics_';
const STORAGE_PREFIX_NEW = 'fortnite_new_';
const STORAGE_PREFIX_SHOP = 'fortnite_shop_';
const CACHE_EXPIRY_MS = 30 * 60 * 1000; // 30 minutos
const CACHE_EXPIRY_NEW_MS = 10 * 60 * 1000; // 10 minutos para novos (muda menos)
const CACHE_EXPIRY_SHOP_MS = 5 * 60 * 1000; // 5 minutos para shop (muda mais frequentemente)
const MAX_STORAGE_ITEMS = 50; // Máximo de páginas em cache

export function useCosmetics() {
  const { user } = useAuth();
  
  // Função para gerar preço aleatório determinístico baseado no ID
  const generateRandomPrice = (cosmeticId) => {
    let hash = 0;
    for (let i = 0; i < cosmeticId.length; i++) {
      const char = cosmeticId.charCodeAt(i);
      hash = ((hash << 5) - hash) + char;
      hash = hash & hash; // Convert to 32bit integer
    }
    return Math.abs(hash % 10000) + 1; // Entre 1 e 10000
  };
  
  // Função para enriquecer cosméticos usando API externa diretamente
  const enrichCosmeticsFromExternalAPI = async (page, pageSize, sortBy, filters, userId, forceRefreshInventory = false) => {
    try {
      // Buscar dados das APIs externas em paralelo (com cache)
      const [cosmeticsResponse, newCosmeticsResponse, shopResponse] = await Promise.all([
        fortniteExternalAPI.getCosmetics().catch(() => null),
        fortniteExternalAPI.getNewCosmetics().catch(() => null),
        fortniteExternalAPI.getShop().catch(() => null)
      ]);
      
      if (!cosmeticsResponse || !cosmeticsResponse.data || !cosmeticsResponse.data.br) {
        console.warn('Não foi possível buscar cosméticos da API externa');
        return null;
      }
      
          // Buscar inventário do usuário do backend (se logado)
          // Se forceRefreshInventory for true, buscar sempre do servidor (sem cache)
          let ownedCosmeticIds = new Set();
          if (userId) {
            try {
              // Se forceRefreshInventory for true, buscar sempre do servidor para garantir dados atualizados
              // Sempre forçar refresh após compra para garantir que o item aparece como possuído
              const inventory = await cosmeticsAPI.getInventory(userId, forceRefreshInventory || true); // Sempre buscar do servidor
              if (inventory && inventory.ownedCosmetics) {
                ownedCosmeticIds = new Set(inventory.ownedCosmetics.map(c => c.cosmeticId || c.id));
                console.log(`Inventário carregado: ${ownedCosmeticIds.size} itens possuídos (forceRefresh: ${forceRefreshInventory || true})`);
              }
            } catch (err) {
              // Se for 401, apenas não buscar inventário (usuário não autenticado)
              if (err.response?.status !== 401) {
                console.warn('Erro ao buscar inventário do usuário:', err);
              }
            }
          }
      
      // Processar novos cosméticos
      const newCosmeticIds = new Set();
      if (newCosmeticsResponse && newCosmeticsResponse.data && newCosmeticsResponse.data.items && newCosmeticsResponse.data.items.br) {
        newCosmeticsResponse.data.items.br.forEach(c => {
          if (c.id) newCosmeticIds.add(c.id);
        });
      }
      
      // Processar shop
      const shopCosmeticIds = new Set();
      const shopCosmetics = new Map();
      const bundleEntries = new Map();
      
      if (shopResponse && shopResponse.data && shopResponse.data.entries) {
        shopResponse.data.entries.forEach(entry => {
          if (entry.brItems && entry.brItems.length > 0) {
            const isBundle = entry.brItems.length > 1;
            entry.brItems.forEach(item => {
              if (item.id) {
                shopCosmeticIds.add(item.id);
                shopCosmetics.set(item.id, entry);
                if (isBundle) {
                  bundleEntries.set(item.id, entry);
                }
              }
            });
          }
        });
      }
      
      // Obter lista de cosméticos
      let allCosmetics = cosmeticsResponse.data.br || [];
      
      // Aplicar filtros básicos antes de enriquecer
      if (filters.name) {
        const nameLower = filters.name.toLowerCase();
        allCosmetics = allCosmetics.filter(c => 
          c.name && c.name.toLowerCase().includes(nameLower)
        );
      }
      
      if (filters.type) {
        allCosmetics = allCosmetics.filter(c => 
          c.type && c.type.value && c.type.value.toLowerCase() === filters.type.toLowerCase()
        );
      }
      
      if (filters.rarity) {
        allCosmetics = allCosmetics.filter(c => 
          c.rarity && c.rarity.value && c.rarity.value.toLowerCase() === filters.rarity.toLowerCase()
        );
      }
      
      if (filters.startDate) {
        const startDate = new Date(filters.startDate);
        allCosmetics = allCosmetics.filter(c => {
          const added = new Date(c.added);
          return added >= startDate;
        });
      }
      
      if (filters.endDate) {
        const endDate = new Date(filters.endDate);
        allCosmetics = allCosmetics.filter(c => {
          const added = new Date(c.added);
          return added <= endDate;
        });
      }
      
      // Enriquecer cosméticos
      let enriched = allCosmetics.map(c => {
        const isBundle = bundleEntries.has(c.id);
        let bundleItems = null;
        
        if (isBundle && bundleEntries.get(c.id)) {
          const entry = bundleEntries.get(c.id);
          const pricePerItem = entry.brItems && entry.brItems.length > 0 
            ? Math.floor(entry.finalPrice / entry.brItems.length) 
            : 0;
          
          bundleItems = (entry.brItems || []).map(item => ({
            id: item.id,
            name: item.name || item.id,
            price: pricePerItem
          }));
        }
        
        // Determinar preço
        let finalPrice = null;
        let regularPrice = null;
        
        if (shopCosmetics.has(c.id)) {
          const entry = shopCosmetics.get(c.id);
          finalPrice = entry.finalPrice;
          regularPrice = entry.regularPrice;
        } else {
          finalPrice = generateRandomPrice(c.id);
          regularPrice = finalPrice;
        }
        
        return {
          id: c.id,
          name: c.name,
          type: c.type || { value: '', displayValue: '' },
          rarity: c.rarity || { value: 'common', displayValue: 'Common' },
          images: c.images || { smallIcon: '', icon: '', featured: '' },
          added: c.added ? new Date(c.added) : new Date(),
          isNew: newCosmeticIds.has(c.id),
          isInShop: shopCosmeticIds.has(c.id),
          isOwned: ownedCosmeticIds.has(c.id),
          isOnSale: shopCosmetics.has(c.id) && 
                   shopCosmetics.get(c.id).finalPrice < shopCosmetics.get(c.id).regularPrice,
          price: finalPrice,
          regularPrice: regularPrice,
          isBundle: isBundle,
          bundleItems: bundleItems
        };
      });
      
      // Aplicar filtros booleanos
      if (filters.onlyNew) {
        enriched = enriched.filter(c => c.isNew);
      }
      
      if (filters.onlyInShop) {
        enriched = enriched.filter(c => c.isInShop);
      }
      
      if (filters.onlyOnSale) {
        enriched = enriched.filter(c => c.isOnSale);
      }
      
      if (filters.onlyOwned) {
        enriched = enriched.filter(c => c.isOwned);
      }
      
      if (filters.onlyBundle) {
        enriched = enriched.filter(c => c.isBundle);
      }
      
      if (filters.minPrice !== null && filters.minPrice !== undefined) {
        enriched = enriched.filter(c => c.price && c.price >= filters.minPrice);
      }
      
      if (filters.maxPrice !== null && filters.maxPrice !== undefined) {
        enriched = enriched.filter(c => c.price && c.price <= filters.maxPrice);
      }
      
      // Aplicar ordenação
      if (sortBy) {
        const sortLower = sortBy.toLowerCase();
        switch (sortLower) {
          case 'newest':
            enriched.sort((a, b) => new Date(b.added) - new Date(a.added));
            break;
          case 'oldest':
            enriched.sort((a, b) => new Date(a.added) - new Date(b.added));
            break;
          case 'name_asc':
            enriched.sort((a, b) => (a.name || '').localeCompare(b.name || ''));
            break;
          case 'name_desc':
            enriched.sort((a, b) => (b.name || '').localeCompare(a.name || ''));
            break;
          case 'price_asc':
            enriched.sort((a, b) => (a.price || 0) - (b.price || 0));
            break;
          case 'price_desc':
            enriched.sort((a, b) => (b.price || 0) - (a.price || 0));
            break;
          default:
            enriched.sort((a, b) => new Date(b.added) - new Date(a.added));
        }
      }
      
      // Contar total antes de paginar
      const totalCount = enriched.length;
      
      // Aplicar paginação
      const startIndex = (page - 1) * pageSize;
      const paginated = enriched.slice(startIndex, startIndex + pageSize);
      
      return {
        cosmetics: paginated,
        totalCount: totalCount
      };
      
    } catch (err) {
      console.error('Erro ao enriquecer cosméticos da API externa:', err);
      return null; // Retornar null para usar fallback do backend
    }
  };
  
  // Gerar chave única para o cache baseada nos filtros
  const getCacheKey = (page, pageSize, sortBy, filters, userId) => {
    return `${page}-${pageSize}-${sortBy}-${JSON.stringify(filters)}-${userId || 'guest'}`;
  };
  
  // Gerar chave para localStorage (mais curta)
  const getStorageKey = (page, pageSize, sortBy, filters, userId) => {
    const filterHash = btoa(JSON.stringify(filters)).substring(0, 20);
    return `${STORAGE_PREFIX}${page}-${pageSize}-${sortBy}-${filterHash}-${userId || 'guest'}`;
  };
  
  // Salvar no localStorage
  const saveToLocalStorage = (key, data) => {
    try {
      const cacheData = {
        data: data,
        timestamp: Date.now()
      };
      localStorage.setItem(key, JSON.stringify(cacheData));
      
      // Limpar itens antigos se exceder o limite
      cleanupOldStorageItems();
    } catch (err) {
      console.warn('Erro ao salvar no localStorage:', err);
      // Se exceder o limite de tamanho, limpar cache antigo
      if (err.name === 'QuotaExceededError') {
        clearOldestStorageItems();
      }
    }
  };
  
  // Carregar do localStorage
  const loadFromLocalStorage = (key) => {
    try {
      const stored = localStorage.getItem(key);
      if (!stored) return null;
      
      const cacheData = JSON.parse(stored);
      const now = Date.now();
      
      // Verificar se expirou
      if (now - cacheData.timestamp > CACHE_EXPIRY_MS) {
        localStorage.removeItem(key);
        return null;
      }
      
      return cacheData.data;
    } catch (err) {
      console.warn('Erro ao carregar do localStorage:', err);
      return null;
    }
  };
  
  // Limpar itens antigos do localStorage
  const cleanupOldStorageItems = () => {
    try {
      const keys = Object.keys(localStorage);
      const cosmeticKeys = keys.filter(k => k.startsWith(STORAGE_PREFIX));
      
      if (cosmeticKeys.length <= MAX_STORAGE_ITEMS) return;
      
      // Ordenar por timestamp e remover os mais antigos
      const itemsWithTime = cosmeticKeys.map(key => {
        try {
          const stored = localStorage.getItem(key);
          if (stored) {
            const data = JSON.parse(stored);
            return { key, timestamp: data.timestamp || 0 };
          }
        } catch (e) {
          return { key, timestamp: 0 };
        }
        return { key, timestamp: 0 };
      });
      
      itemsWithTime.sort((a, b) => a.timestamp - b.timestamp);
      const toRemove = itemsWithTime.slice(0, cosmeticKeys.length - MAX_STORAGE_ITEMS);
      
      toRemove.forEach(item => {
        localStorage.removeItem(item.key);
      });
      
      console.log(`Removidos ${toRemove.length} itens antigos do cache`);
    } catch (err) {
      console.warn('Erro ao limpar cache antigo:', err);
    }
  };
  
  // Limpar itens mais antigos quando exceder quota
  const clearOldestStorageItems = () => {
    try {
      const keys = Object.keys(localStorage);
      const cosmeticKeys = keys.filter(k => k.startsWith(STORAGE_PREFIX));
      
      // Remover metade dos itens mais antigos
      const itemsWithTime = cosmeticKeys.map(key => {
        try {
          const stored = localStorage.getItem(key);
          if (stored) {
            const data = JSON.parse(stored);
            return { key, timestamp: data.timestamp || 0 };
          }
        } catch (e) {
          return { key, timestamp: 0 };
        }
        return { key, timestamp: 0 };
      });
      
      itemsWithTime.sort((a, b) => a.timestamp - b.timestamp);
      const toRemove = itemsWithTime.slice(0, Math.floor(cosmeticKeys.length / 2));
      
      toRemove.forEach(item => {
        localStorage.removeItem(item.key);
      });
      
      console.log(`Removidos ${toRemove.length} itens do cache devido a quota excedida`);
    } catch (err) {
      console.warn('Erro ao limpar cache:', err);
    }
  };
  
  // Buscar página específica (usado para prefetch)
  const fetchPage = async (page, useCache = true, forceRefreshInventory = false) => {
    const cacheKey = getCacheKey(page, pageSize.value, sortBy.value, filters, user.value?.id);
    const storageKey = getStorageKey(page, pageSize.value, sortBy.value, filters, user.value?.id);
    
    // Se forceRefreshInventory for true, não usar cache e forçar atualização do inventário
    if (forceRefreshInventory) {
      useCache = false;
    }
    
    // Verificar cache em memória primeiro
    if (useCache && pageCache.has(cacheKey)) {
      console.log(`Página ${page} encontrada no cache em memória`);
      return pageCache.get(cacheKey);
    }
    
    // Verificar localStorage
    if (useCache) {
      const cachedData = loadFromLocalStorage(storageKey);
      if (cachedData) {
        console.log(`Página ${page} encontrada no localStorage`);
        // Armazenar também no cache em memória para acesso rápido
        pageCache.set(cacheKey, cachedData);
        return cachedData;
      }
    }
    
    // Se já está sendo pré-carregada, aguardar
    if (prefetchingPages.has(cacheKey)) {
      console.log(`Aguardando pré-carregamento da página ${page}...`);
      // Aguardar até 5 segundos
      for (let i = 0; i < 50; i++) {
        await new Promise(resolve => setTimeout(resolve, 100));
        if (pageCache.has(cacheKey)) {
          return pageCache.get(cacheKey);
        }
        if (!prefetchingPages.has(cacheKey)) {
          break; // Parou de pré-carregar, fazer requisição normal
        }
      }
    }
    
    // Marcar como pré-carregando
    prefetchingPages.add(cacheKey);
    
    try {
      // Tentar usar API externa diretamente para melhor performance
      // Se forceRefreshInventory for true, forçar atualização do inventário
      const enrichedData = await enrichCosmeticsFromExternalAPI(page, pageSize.value, sortBy.value, filters, user.value?.id, forceRefreshInventory);
      
      if (enrichedData) {
        // Armazenar no cache em memória
        pageCache.set(cacheKey, enrichedData);
        
        // Armazenar no localStorage para persistência
        saveToLocalStorage(storageKey, enrichedData);
        
        console.log(`Página ${page} enriquecida e salva no cache e localStorage`);
        
        return enrichedData;
      }
      
      // Fallback: usar API do backend se enriquecimento externo falhar
      const searchFilters = {
        name: filters.name || '',
        type: filters.type || '',
        rarity: filters.rarity || '',
        onlyNew: filters.onlyNew || false,
        onlyInShop: filters.onlyInShop || false,
        onlyOnSale: filters.onlyOnSale || false,
        onlyOwned: filters.onlyOwned || false,
        onlyBundle: filters.onlyBundle || false,
      };
      
      // Adicionar filtros de data se existirem
      if (filters.startDate) {
        searchFilters.startDate = filters.startDate.toISOString();
      }
      if (filters.endDate) {
        searchFilters.endDate = filters.endDate.toISOString();
      }
      
      // Adicionar filtros de preço se existirem
      if (filters.minPrice !== null && filters.minPrice !== undefined) {
        searchFilters.minPrice = filters.minPrice;
      }
      if (filters.maxPrice !== null && filters.maxPrice !== undefined) {
        searchFilters.maxPrice = filters.maxPrice;
      }
      
      const data = await cosmeticsAPI.searchCosmetics(
        searchFilters, 
        page, 
        pageSize.value, 
        sortBy.value,
        user.value?.id
      );
      
      let result = {
        cosmetics: [],
        totalCount: 0
      };
      
      // A API retorna um objeto com cosmetics e totalCount
      if (data && data.cosmetics && Array.isArray(data.cosmetics)) {
        result.cosmetics = data.cosmetics;
        result.totalCount = data.totalCount || 0;
      } else if (Array.isArray(data)) {
        result.cosmetics = data;
        result.totalCount = data.length;
      } else if (data && data.items && Array.isArray(data.items)) {
        result.cosmetics = data.items;
        result.totalCount = data.totalCount || data.total || data.items.length;
      } else {
        // Se não encontrou formato esperado, logar para debug
        console.warn('Formato de resposta inesperado da API:', data);
        result.cosmetics = [];
        result.totalCount = 0;
      }
      
      // Armazenar no cache em memória
      pageCache.set(cacheKey, result);
      
      // Armazenar no localStorage para persistência
      saveToLocalStorage(storageKey, result);
      
      console.log(`Página ${page} salva no cache e localStorage`);
      
      return result;
    } catch (err) {
      console.error(`Error fetching page ${page}:`, err);
      throw err;
    } finally {
      prefetchingPages.delete(cacheKey);
    }
  };
  
  // Pré-carregar próxima página em background
  const prefetchNextPage = async () => {
    const nextPage = currentPage.value + 1;
    const totalPages = Math.ceil(totalItems.value / pageSize.value) || 1;
    
    if (nextPage > totalPages) return;
    
    const cacheKey = getCacheKey(nextPage, pageSize.value, sortBy.value, filters, user.value?.id);
    
    // Se já está no cache ou sendo pré-carregada, não fazer nada
    if (pageCache.has(cacheKey) || prefetchingPages.has(cacheKey)) {
      return;
    }
    
    console.log(`Pré-carregando página ${nextPage}...`);
    // Fazer em background sem bloquear
    fetchPage(nextPage, false).catch(err => {
      console.warn(`Erro ao pré-carregar página ${nextPage}:`, err);
    });
  };

  // Pré-carregar múltiplas páginas sequencialmente em background
  const prefetchMultiplePages = async (startPage = 2, maxPages = 5) => {
    const totalPages = Math.ceil(totalItems.value / pageSize.value) || 1;
    
    if (startPage > totalPages) return;
    
    const pagesToPrefetch = Math.min(maxPages, totalPages - startPage + 1);
    console.log(`Iniciando pré-carregamento de ${pagesToPrefetch} páginas a partir da página ${startPage}...`);
    
    // Carregar páginas sequencialmente com pequeno delay entre elas para não sobrecarregar
    for (let i = 0; i < pagesToPrefetch; i++) {
      const page = startPage + i;
      if (page > totalPages) break;
      
      const cacheKey = getCacheKey(page, pageSize.value, sortBy.value, filters, user.value?.id);
      const storageKey = getStorageKey(page, pageSize.value, sortBy.value, filters, user.value?.id);
      
      // Verificar se já está no cache ou localStorage
      if (pageCache.has(cacheKey)) {
        console.log(`Página ${page} já está no cache em memória`);
        continue;
      }
      
      const cached = loadFromLocalStorage(storageKey);
      if (cached) {
        console.log(`Página ${page} já está no localStorage`);
        pageCache.set(cacheKey, cached);
        continue;
      }
      
      // Se já está sendo pré-carregada, pular
      if (prefetchingPages.has(cacheKey)) {
        console.log(`Página ${page} já está sendo pré-carregada`);
        continue;
      }
      
      // Aguardar um pouco antes de cada requisição para não sobrecarregar
      if (i > 0) {
        await new Promise(resolve => setTimeout(resolve, 300)); // 300ms entre requisições
      }
      
      // Carregar página em background
      fetchPage(page, false)
        .then(() => {
          console.log(`✓ Página ${page} pré-carregada e salva no localStorage`);
        })
        .catch(err => {
          console.warn(`✗ Erro ao pré-carregar página ${page}:`, err);
        });
    }
    
    console.log(`Pré-carregamento de páginas concluído`);
  };
  
  // Limpar cache quando filtros mudarem
  const clearCache = () => {
    pageCache.clear();
    prefetchingPages.clear();
    // Limpar localStorage também (opcional - pode manter para persistência)
    // clearLocalStorageCache();
  };
  
  // Limpar todo o cache do localStorage (opcional)
  const clearLocalStorageCache = () => {
    try {
      const keys = Object.keys(localStorage);
      const cosmeticKeys = keys.filter(k => k.startsWith(STORAGE_PREFIX));
      cosmeticKeys.forEach(key => localStorage.removeItem(key));
      console.log('Cache do localStorage limpo');
    } catch (err) {
      console.warn('Erro ao limpar cache do localStorage:', err);
    }
  };

  // Funções para gerenciar cache de novos cosméticos e shop
  const saveNewCosmeticsToStorage = (data) => {
    try {
      const cacheData = {
        data: data,
        timestamp: Date.now()
      };
      localStorage.setItem(`${STORAGE_PREFIX_NEW}data`, JSON.stringify(cacheData));
      console.log('Novos cosméticos salvos no localStorage');
    } catch (err) {
      console.warn('Erro ao salvar novos cosméticos no localStorage:', err);
    }
  };

  const loadNewCosmeticsFromStorage = () => {
    try {
      const stored = localStorage.getItem(`${STORAGE_PREFIX_NEW}data`);
      if (!stored) return null;
      
      const cacheData = JSON.parse(stored);
      const now = Date.now();
      
      if (now - cacheData.timestamp > CACHE_EXPIRY_NEW_MS) {
        localStorage.removeItem(`${STORAGE_PREFIX_NEW}data`);
        return null;
      }
      
      return cacheData.data;
    } catch (err) {
      console.warn('Erro ao carregar novos cosméticos do localStorage:', err);
      return null;
    }
  };

  const saveShopToStorage = (data) => {
    try {
      const cacheData = {
        data: data,
        timestamp: Date.now()
      };
      localStorage.setItem(`${STORAGE_PREFIX_SHOP}data`, JSON.stringify(cacheData));
      console.log('Shop salvo no localStorage');
    } catch (err) {
      console.warn('Erro ao salvar shop no localStorage:', err);
    }
  };

  const loadShopFromStorage = () => {
    try {
      const stored = localStorage.getItem(`${STORAGE_PREFIX_SHOP}data`);
      if (!stored) return null;
      
      const cacheData = JSON.parse(stored);
      const now = Date.now();
      
      if (now - cacheData.timestamp > CACHE_EXPIRY_SHOP_MS) {
        localStorage.removeItem(`${STORAGE_PREFIX_SHOP}data`);
        return null;
      }
      
      return cacheData.data;
    } catch (err) {
      console.warn('Erro ao carregar shop do localStorage:', err);
      return null;
    }
  };

  // Carregar novos cosméticos (com cache)
  const loadNewCosmetics = async (forceRefresh = false) => {
    // Tentar carregar do localStorage primeiro
    if (!forceRefresh) {
      const cached = loadNewCosmeticsFromStorage();
      if (cached) {
        console.log('Novos cosméticos carregados do localStorage');
        return cached;
      }
    }

    // Se não encontrou no cache ou forçou refresh, buscar da API
    try {
      const data = await cosmeticsAPI.getNewCosmetics();
      saveNewCosmeticsToStorage(data);
      return data;
    } catch (err) {
      console.error('Erro ao carregar novos cosméticos:', err);
      // Tentar retornar cache mesmo expirado em caso de erro
      const cached = loadNewCosmeticsFromStorage();
      if (cached) {
        console.log('Usando cache expirado de novos cosméticos devido a erro');
        return cached;
      }
      throw err;
    }
  };

  // Carregar shop (com cache)
  const loadShop = async (forceRefresh = false) => {
    // Tentar carregar do localStorage primeiro
    if (!forceRefresh) {
      const cached = loadShopFromStorage();
      if (cached) {
        console.log('Shop carregado do localStorage');
        return cached;
      }
    }

    // Se não encontrou no cache ou forçou refresh, buscar da API
    try {
      const data = await cosmeticsAPI.getShop();
      saveShopToStorage(data);
      return data;
    } catch (err) {
      console.error('Erro ao carregar shop:', err);
      // Tentar retornar cache mesmo expirado em caso de erro
      const cached = loadShopFromStorage();
      if (cached) {
        console.log('Usando cache expirado de shop devido a erro');
        return cached;
      }
      throw err;
    }
  };
  
  // Buscar com filtros, paginação e ordenação
  const searchCosmetics = async (forceRefresh = false) => {
    // Se forçar refresh, limpar cache
    if (forceRefresh) {
      clearCache();
    }
    
    loading.value = true;
    error.value = null;
    
    try {
      // Tentar buscar do cache primeiro (mas forçar refresh do inventário se necessário)
      // Se forceRefresh for true, sempre buscar inventário atualizado do servidor
      const cached = await fetchPage(currentPage.value, !forceRefresh, forceRefresh);
      
      if (cached) {
        cosmetics.value = cached.cosmetics;
        totalItems.value = cached.totalCount;
        loading.value = false;
        
        // Se estiver na página 1, pré-carregar múltiplas páginas automaticamente
        if (currentPage.value === 1) {
          setTimeout(() => {
            prefetchMultiplePages(2, 5); // Pré-carregar páginas 2-6 em background
          }, 500);
        } else {
          // Para outras páginas, pré-carregar apenas a próxima
          prefetchNextPage();
        }
        return;
      }
    } catch (err) {
      console.warn('Erro ao buscar do cache, fazendo requisição normal:', err);
    }
    
    // Se não encontrou no cache ou deu erro, fazer requisição normal
    try {
      const result = await fetchPage(currentPage.value, false, forceRefresh);
      cosmetics.value = result.cosmetics;
      totalItems.value = result.totalCount;
      
      // Se estiver na página 1, pré-carregar múltiplas páginas automaticamente
      if (currentPage.value === 1) {
        setTimeout(() => {
          prefetchMultiplePages(2, 5); // Pré-carregar páginas 2-6 em background
        }, 500);
      } else {
        // Para outras páginas, pré-carregar apenas a próxima
        prefetchNextPage();
      }
      
    } catch (err) {
      error.value = err.message;
      console.error('Error searching cosmetics:', err);
      cosmetics.value = [];
      totalItems.value = 0;
    } finally {
      loading.value = false;
    }
  };

  // Carregar cosméticos com paginação e ordenação (sem filtros)
  const loadCosmetics = async () => {
    loading.value = true;
    error.value = null;
    cosmetics.value = [];
    
    try {
      console.log('Carregando cosméticos:', {
        page: currentPage.value,
        pageSize: pageSize.value,
        sortBy: sortBy.value,
        userId: user.value?.id
      });
      
      const data = await cosmeticsAPI.getCosmetics(currentPage.value, pageSize.value, sortBy.value, user.value?.id);
      
      if (Array.isArray(data)) {
        cosmetics.value = data;
        totalItems.value = data.length;
      } else if (data.items && Array.isArray(data.items)) {
        cosmetics.value = data.items;
        totalItems.value = data.totalCount || data.total || data.items.length;
      } else {
        cosmetics.value = [];
        totalItems.value = 0;
      }
      
      console.log('Cosméticos carregados:', {
        quantidade: cosmetics.value.length,
        totalItems: totalItems.value
      });
      
    } catch (err) {
      error.value = err.message;
      console.error('Error loading cosmetics:', err);
      cosmetics.value = [];
      totalItems.value = 0;
    } finally {
      loading.value = false;
    }
  };

      // Carregar V-Bucks
      const loadVBucks = async () => {
        if (!user.value) {
          vbucks.value = 0;
          return;
        }
        try {
          const data = await cosmeticsAPI.getVBucks(user.value.id);
          vbucks.value = data.vbucks || data || 0;
        } catch (err) {
          // Se for 401, apenas não atualizar (usuário não logado)
          if (err.response?.status === 401) {
            console.log('Usuário não autenticado, V-Bucks não disponíveis');
            vbucks.value = 0;
            return;
          }
          console.error('Error loading V-Bucks:', err);
          vbucks.value = 0;
        }
      };

  // Comprar cosmético
  const purchaseCosmetic = async (cosmeticId, price, cosmeticName) => {
    // Verificar se o usuário está logado (tentar carregar do localStorage se necessário)
    let currentUser = user.value;
    if (!currentUser) {
      // Tentar carregar do localStorage
      try {
        const storedUser = localStorage.getItem('user');
        const userId = localStorage.getItem('userId');
        if (storedUser && userId) {
          currentUser = JSON.parse(storedUser);
          // Atualizar o user.value também
          user.value = currentUser;
          console.log('Usuário carregado do localStorage:', currentUser);
        }
      } catch (e) {
        console.error('Erro ao carregar usuário do localStorage:', e);
      }
    }
    
    if (!currentUser || !currentUser.id) {
      error.value = 'É necessário estar logado para comprar cosméticos';
      console.error('Tentativa de compra sem usuário logado', { currentUser, userValue: user.value });
      return false;
    }
    
    const userId = currentUser.id;
    console.log('Iniciando compra:', { cosmeticId, price, cosmeticName, userId });
    
    try {
      const result = await cosmeticsAPI.purchaseCosmetic(cosmeticId, price, cosmeticName, userId);
      
      console.log('Resultado da compra:', result);
      
      // Verificar se a compra foi bem-sucedida
      if (!result || !result.success) {
        error.value = result?.message || 'Erro ao comprar cosmético';
        console.error('Compra falhou:', result);
        return false;
      }
      
      // Limpar cache do inventário ANTES de atualizar (importante!)
      try {
        localStorage.removeItem(`fortnite_inventory_${userId}`);
        const { usersService } = await import('../services/users');
        usersService.clearUserCache(userId);
        console.log('Cache do usuário limpo após compra');
      } catch (e) {
        console.warn('Erro ao limpar cache após compra:', e);
      }
      
      // Atualizar V-bucks SEMPRE após compra bem-sucedida
      console.log('Compra bem-sucedida, atualizando V-bucks...');
      
      // Primeiro, atualizar com o valor retornado (se disponível)
      if (result.vbucks !== undefined && result.vbucks !== null) {
        console.log('Atualizando V-bucks com valor da resposta:', result.vbucks);
        vbucks.value = result.vbucks;
        // Atualizar também no useAuth para sincronizar com o header
        const { useAuth } = await import('./useAuth');
        const { updateVBucks } = useAuth();
        updateVBucks(result.vbucks);
        console.log('V-bucks atualizado no useAuth:', result.vbucks);
      } else {
        // Se não retornou, buscar do servidor (forçar refresh)
        console.log('V-bucks não retornado na resposta, buscando do servidor...');
        await loadVBucks();
        const { useAuth } = await import('./useAuth');
        const { updateVBucks } = useAuth();
        if (vbucks.value !== undefined && vbucks.value !== null) {
          updateVBucks(vbucks.value);
          console.log('V-bucks atualizado do servidor:', vbucks.value);
        }
      }
      
      // Garantir que o valor foi atualizado
      console.log('V-bucks final após compra:', vbucks.value);
      
      // Limpar cache de cosméticos para forçar recarregamento com inventário atualizado
      try {
        // Limpar todas as chaves de cache relacionadas a este usuário
        const keys = Object.keys(localStorage);
        keys.forEach(key => {
          if (key.startsWith(STORAGE_PREFIX) && key.includes(userId.toString())) {
            localStorage.removeItem(key);
          }
        });
        // Limpar cache em memória também
        pageCache.clear();
        prefetchingPages.clear();
        console.log('Cache de cosméticos limpo para forçar atualização do status "possui"');
      } catch (e) {
        console.warn('Erro ao limpar cache de cosméticos:', e);
      }
      
      // Aguardar um pouco para garantir que o backend processou a compra
      await new Promise(resolve => setTimeout(resolve, 200));
      
      // Forçar refresh completo dos cosméticos para atualizar status "possui"
      // Isso vai buscar o inventário atualizado e marcar o item como possuído
      await searchCosmetics(true);
      
      console.log('Compra concluída com sucesso - V-bucks atualizados e status "possui" atualizado');
      return true;
    } catch (err) {
      error.value = err.response?.data?.message || err.message || 'Erro ao comprar cosmético';
      console.error('Error purchasing cosmetic:', err);
      console.error('Detalhes do erro:', {
        message: err.message,
        response: err.response?.data,
        status: err.response?.status
      });
      return false;
    }
  };

  // Devolver cosmético
  const refundCosmetic = async (cosmeticId, cosmeticName) => {
    if (!user.value) {
      error.value = 'É necessário estar logado para devolver cosméticos';
      return false;
    }
    try {
      const { transactionsService } = await import('../services/transactions');
      const result = await transactionsService.refundCosmetic(cosmeticId, cosmeticName, user.value.id);
      if (result.success) {
        // Limpar cache do inventário e cosméticos do usuário após devolução
        try {
          localStorage.removeItem(`fortnite_inventory_${user.value.id}`);
          const { usersService } = await import('../services/users');
          usersService.clearUserCache(user.value.id);
          console.log('Cache do usuário limpo após devolução');
        } catch (e) {
          console.warn('Erro ao limpar cache após devolução:', e);
        }
        
        await loadVBucks();
        await searchCosmetics(true); // Forçar refresh para atualizar status "possui"
        return true;
      }
      error.value = result.message || 'Erro ao devolver cosmético';
      return false;
    } catch (err) {
      error.value = err.response?.data?.message || err.message || 'Erro ao devolver cosmético';
      console.error('Error refunding cosmetic:', err);
      return false;
    }
  };

  // Formatar data
  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('pt-BR', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
    }).format(date);
  };

  // Normalizar raridade para formato do componente
  const normalizeRarity = (rarity) => {
    if (typeof rarity === 'object' && rarity.value) {
      return rarity.value.toLowerCase();
    }
    return (rarity || 'common').toLowerCase();
  };

  // Função para limpar todos os filtros
  const clearFilters = () => {
    filters.name = '';
    filters.type = '';
    filters.rarity = '';
    filters.onlyNew = false;
    filters.onlyInShop = false;
    filters.onlyOnSale = false;
    filters.onlyOwned = false;
    filters.onlyBundle = false;
    filters.minPrice = null;
    filters.maxPrice = null;
    filters.startDate = null;
    filters.endDate = null;
    currentPage.value = 1;
    clearCache();
    searchCosmetics(true);
  };

  // Função para obter opções de filtros baseado nos cosméticos carregados
  const getFilterOptions = async () => {
    try {
      // Buscar todos os cosméticos da API externa (com cache)
      const cosmeticsResponse = await fortniteExternalAPI.getCosmetics().catch(() => null);
      
      if (!cosmeticsResponse || !cosmeticsResponse.data || !cosmeticsResponse.data.br) {
        console.warn('Não foi possível buscar cosméticos para opções de filtros');
        return { types: [], rarities: [] };
      }
      
      let allCosmetics = cosmeticsResponse.data.br || [];
      
      // Aplicar filtros (exceto type e rarity que queremos contar)
      if (filters.name) {
        const nameLower = filters.name.toLowerCase();
        allCosmetics = allCosmetics.filter(c => 
          c.name && c.name.toLowerCase().includes(nameLower)
        );
      }
      
      if (filters.startDate) {
        const startDate = new Date(filters.startDate);
        allCosmetics = allCosmetics.filter(c => {
          if (!c.added) return false;
          const addedDate = new Date(c.added);
          return addedDate >= startDate;
        });
      }
      
      if (filters.endDate) {
        const endDate = new Date(filters.endDate);
        allCosmetics = allCosmetics.filter(c => {
          if (!c.added) return false;
          const addedDate = new Date(c.added);
          return addedDate <= endDate;
        });
      }
      
      // Obter TODAS as categorias e raridades únicas de TODOS os cosméticos
      const allTypesMap = new Map();
      const allRaritiesMap = new Map();
      
      allCosmetics.forEach(c => {
        // Processar tipos
        if (c.type && c.type.value) {
          const typeKey = c.type.value.toLowerCase();
          if (!allTypesMap.has(typeKey)) {
            allTypesMap.set(typeKey, {
              value: c.type.value,
              label: c.type.displayValue || c.type.value
            });
          }
        }
        
        // Processar raridades
        if (c.rarity && c.rarity.value) {
          const rarityKey = c.rarity.value.toLowerCase();
          if (!allRaritiesMap.has(rarityKey)) {
            allRaritiesMap.set(rarityKey, {
              value: c.rarity.value,
              label: c.rarity.displayValue || c.rarity.value
            });
          }
        }
      });
      
      const allTypes = Array.from(allTypesMap.values()).sort((a, b) => a.value.localeCompare(b.value));
      const allRarities = Array.from(allRaritiesMap.values()).sort((a, b) => a.value.localeCompare(b.value));
      
      // Aplicar filtros adicionais para contar
      let filteredCosmetics = [...allCosmetics];
      
      if (filters.onlyNew) {
        // Buscar novos cosméticos
        const newCosmeticsResponse = await fortniteExternalAPI.getNewCosmetics().catch(() => null);
        if (newCosmeticsResponse && newCosmeticsResponse.data && newCosmeticsResponse.data.items && newCosmeticsResponse.data.items.br) {
          const newCosmeticIds = new Set(newCosmeticsResponse.data.items.br.map(c => c.id));
          filteredCosmetics = filteredCosmetics.filter(c => newCosmeticIds.has(c.id));
        } else {
          filteredCosmetics = [];
        }
      }
      
      if (filters.onlyInShop || filters.onlyOnSale) {
        // Buscar shop
        const shopResponse = await fortniteExternalAPI.getShop().catch(() => null);
        if (shopResponse && shopResponse.data && shopResponse.data.entries) {
          const shopCosmeticIds = new Set();
          shopResponse.data.entries.forEach(entry => {
            if (entry.brItems && entry.brItems.length > 0) {
              entry.brItems.forEach(item => {
                if (item.id) shopCosmeticIds.add(item.id);
              });
            }
          });
          
          if (filters.onlyInShop) {
            filteredCosmetics = filteredCosmetics.filter(c => shopCosmeticIds.has(c.id));
          }
          
          if (filters.onlyOnSale) {
            // Verificar se está em promoção (preço final < preço regular)
            const shopCosmeticsMap = new Map();
            shopResponse.data.entries.forEach(entry => {
              if (entry.brItems && entry.brItems.length > 0) {
                entry.brItems.forEach(item => {
                  if (item.id) {
                    shopCosmeticsMap.set(item.id, {
                      finalPrice: entry.finalPrice,
                      regularPrice: entry.regularPrice
                    });
                  }
                });
              }
            });
            
            filteredCosmetics = filteredCosmetics.filter(c => {
              const shopData = shopCosmeticsMap.get(c.id);
              return shopData && shopData.finalPrice < shopData.regularPrice;
            });
          }
        } else {
          if (filters.onlyInShop || filters.onlyOnSale) {
            filteredCosmetics = [];
          }
        }
      }
      
      if (filters.onlyOwned && user.value?.id) {
        try {
          const inventory = await cosmeticsAPI.getInventory(user.value.id, false);
          if (inventory && inventory.ownedCosmetics) {
            const ownedCosmeticIds = new Set(inventory.ownedCosmetics.map(c => c.cosmeticId || c.id));
            filteredCosmetics = filteredCosmetics.filter(c => ownedCosmeticIds.has(c.id));
          } else {
            filteredCosmetics = [];
          }
        } catch (err) {
          console.warn('Erro ao buscar inventário:', err);
          filteredCosmetics = [];
        }
      }
      
      if (filters.onlyBundle) {
        // Buscar shop para verificar bundles
        const shopResponse = await fortniteExternalAPI.getShop().catch(() => null);
        if (shopResponse && shopResponse.data && shopResponse.data.entries) {
          const bundleCosmeticIds = new Set();
          shopResponse.data.entries.forEach(entry => {
            if (entry.brItems && entry.brItems.length > 1) {
              entry.brItems.forEach(item => {
                if (item.id) bundleCosmeticIds.add(item.id);
              });
            }
          });
          filteredCosmetics = filteredCosmetics.filter(c => bundleCosmeticIds.has(c.id));
        } else {
          filteredCosmetics = [];
        }
      }
      
      if (filters.minPrice !== null && filters.minPrice !== undefined) {
        // Buscar shop para obter preços
        const shopResponse = await fortniteExternalAPI.getShop().catch(() => null);
        const shopCosmeticsMap = new Map();
        if (shopResponse && shopResponse.data && shopResponse.data.entries) {
          shopResponse.data.entries.forEach(entry => {
            if (entry.brItems && entry.brItems.length > 0) {
              entry.brItems.forEach(item => {
                if (item.id) {
                  shopCosmeticsMap.set(item.id, entry.finalPrice);
                }
              });
            }
          });
        }
        
        filteredCosmetics = filteredCosmetics.filter(c => {
          const price = shopCosmeticsMap.get(c.id) || generateRandomPrice(c.id);
          return price >= filters.minPrice;
        });
      }
      
      if (filters.maxPrice !== null && filters.maxPrice !== undefined) {
        // Buscar shop para obter preços
        const shopResponse = await fortniteExternalAPI.getShop().catch(() => null);
        const shopCosmeticsMap = new Map();
        if (shopResponse && shopResponse.data && shopResponse.data.entries) {
          shopResponse.data.entries.forEach(entry => {
            if (entry.brItems && entry.brItems.length > 0) {
              entry.brItems.forEach(item => {
                if (item.id) {
                  shopCosmeticsMap.set(item.id, entry.finalPrice);
                }
              });
            }
          });
        }
        
        filteredCosmetics = filteredCosmetics.filter(c => {
          const price = shopCosmeticsMap.get(c.id) || generateRandomPrice(c.id);
          return price <= filters.maxPrice;
        });
      }
      
      // Contar tipos nos cosméticos filtrados
      const typeCountsMap = new Map();
      filteredCosmetics.forEach(c => {
        if (c.type && c.type.value) {
          const typeKey = c.type.value.toLowerCase();
          typeCountsMap.set(typeKey, (typeCountsMap.get(typeKey) || 0) + 1);
        }
      });
      
      // Criar lista de tipos com contagens (incluindo os que têm count 0)
      const typeCounts = allTypes.map(t => ({
        value: t.value,
        label: t.label,
        count: typeCountsMap.get(t.value.toLowerCase()) || 0
      }));
      
      // Contar raridades nos cosméticos filtrados
      const rarityCountsMap = new Map();
      filteredCosmetics.forEach(c => {
        if (c.rarity && c.rarity.value) {
          const rarityKey = c.rarity.value.toLowerCase();
          rarityCountsMap.set(rarityKey, (rarityCountsMap.get(rarityKey) || 0) + 1);
        }
      });
      
      // Criar lista de raridades com contagens (incluindo as que têm count 0)
      const rarityCounts = allRarities.map(r => ({
        value: r.value,
        label: r.label,
        count: rarityCountsMap.get(r.value.toLowerCase()) || 0
      }));
      
      return {
        types: typeCounts,
        rarities: rarityCounts
      };
    } catch (error) {
      console.error('Erro ao obter opções de filtros:', error);
      return { types: [], rarities: [] };
    }
  };

  return {
    cosmetics,
    loading,
    error,
    vbucks,
    currentPage,
    pageSize,
    totalItems,
    sortBy,
    filters,
    loadCosmetics,
    searchCosmetics,
    loadVBucks,
    purchaseCosmetic,
    refundCosmetic,
    formatDate,
    normalizeRarity,
    prefetchNextPage,
    prefetchMultiplePages,
    clearCache,
    clearLocalStorageCache,
    loadNewCosmetics,
    loadShop,
    clearFilters,
    getFilterOptions,
  };
}