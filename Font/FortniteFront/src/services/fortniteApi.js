import axios from 'axios';

const FORTNITE_API_BASE_URL = 'https://fortnite-api.com/v2/';

const fortniteApi = axios.create({
  baseURL: FORTNITE_API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

const STORAGE_PREFIX_FORTNITE = 'fortnite_external_';
const CACHE_EXPIRY_COSMETICS_MS = 30 * 60 * 1000;
const CACHE_EXPIRY_NEW_MS = 10 * 60 * 1000;
const CACHE_EXPIRY_SHOP_MS = 5 * 60 * 1000;

const saveToStorage = (key, data, expiryMs) => {
  try {
    const cacheData = {
      data: data,
      timestamp: Date.now(),
      expiry: expiryMs
    };
    localStorage.setItem(key, JSON.stringify(cacheData));
  } catch (err) {
    console.warn('Erro ao salvar no localStorage:', err);
    if (err.name === 'QuotaExceededError') {
      clearOldestCache();
    }
  }
};

const loadFromStorage = (key, expiryMs) => {
  try {
    const stored = localStorage.getItem(key);
    if (!stored) return null;
    
    const cacheData = JSON.parse(stored);
    const now = Date.now();
    const expiry = expiryMs || cacheData.expiry || CACHE_EXPIRY_COSMETICS_MS;
    
    if (now - cacheData.timestamp > expiry) {
      localStorage.removeItem(key);
      return null;
    }
    
    return cacheData.data;
  } catch (err) {
    console.warn('Erro ao carregar do localStorage:', err);
    return null;
  }
};

const clearOldestCache = () => {
  try {
    const keys = Object.keys(localStorage);
    const fortniteKeys = keys.filter(k => k.startsWith(STORAGE_PREFIX_FORTNITE));
    
    if (fortniteKeys.length <= 20) return;
    
    const itemsWithTime = fortniteKeys.map(key => {
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
    const toRemove = itemsWithTime.slice(0, fortniteKeys.length - 20);
    
    toRemove.forEach(item => {
      localStorage.removeItem(item.key);
    });
  } catch (err) {
    console.warn('Erro ao limpar cache antigo:', err);
  }
};

export const fortniteExternalAPI = {
  getCosmetics: async (forceRefresh = false) => {
    const cacheKey = `${STORAGE_PREFIX_FORTNITE}cosmetics`;
    
    if (!forceRefresh) {
      const cached = loadFromStorage(cacheKey, CACHE_EXPIRY_COSMETICS_MS);
      if (cached) {
        console.log('Cosméticos carregados do localStorage (API externa)');
        return cached;
      }
    }

    try {
      const response = await fortniteApi.get('cosmetics');
      const data = response.data;
      
      saveToStorage(cacheKey, data, CACHE_EXPIRY_COSMETICS_MS);
      console.log('Cosméticos salvos no localStorage (API externa)');
      
      return data;
    } catch (error) {
      console.error('Erro ao buscar cosméticos da API externa:', error);
      const cached = loadFromStorage(cacheKey, Infinity);
      if (cached) {
        console.log('Usando cache expirado de cosméticos devido a erro');
        return cached;
      }
      throw error;
    }
  },

  getNewCosmetics: async (forceRefresh = false) => {
    const cacheKey = `${STORAGE_PREFIX_FORTNITE}new`;
    
    if (!forceRefresh) {
      const cached = loadFromStorage(cacheKey, CACHE_EXPIRY_NEW_MS);
      if (cached) {
        console.log('Novos cosméticos carregados do localStorage (API externa)');
        return cached;
      }
    }

    try {
      const response = await fortniteApi.get('cosmetics/new');
      const data = response.data;
      
      saveToStorage(cacheKey, data, CACHE_EXPIRY_NEW_MS);
      console.log('Novos cosméticos salvos no localStorage (API externa)');
      
      return data;
    } catch (error) {
      console.error('Erro ao buscar novos cosméticos da API externa:', error);
      const cached = loadFromStorage(cacheKey, Infinity);
      if (cached) {
        console.log('Usando cache expirado de novos cosméticos devido a erro');
        return cached;
      }
      throw error;
    }
  },

  getShop: async (forceRefresh = false) => {
    const cacheKey = `${STORAGE_PREFIX_FORTNITE}shop`;
    
    if (!forceRefresh) {
      const cached = loadFromStorage(cacheKey, CACHE_EXPIRY_SHOP_MS);
      if (cached) {
        console.log('Shop carregado do localStorage (API externa)');
        return cached;
      }
    }

    try {
      const response = await fortniteApi.get('shop');
      const data = response.data;
      
      saveToStorage(cacheKey, data, CACHE_EXPIRY_SHOP_MS);
      console.log('Shop salvo no localStorage (API externa)');
      
      return data;
    } catch (error) {
      console.error('Erro ao buscar shop da API externa:', error);
      const cached = loadFromStorage(cacheKey, Infinity);
      if (cached) {
        console.log('Usando cache expirado de shop devido a erro');
        return cached;
      }
      throw error;
    }
  },
};

export default fortniteApi;

