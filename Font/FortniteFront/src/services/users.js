import axios from 'axios';

const getApiBaseUrl = () => {
  const envUrl = import.meta.env.VITE_API_BASE_URL;
  if (envUrl) {
    if (envUrl.startsWith('/')) {
      return envUrl.replace('/ControllerCosmeticsEnriched', '');
    }
    return envUrl.replace('/ControllerCosmeticsEnriched', '');
  }
  return 'http://localhost:5155/api';
};

const API_BASE_URL = getApiBaseUrl();

const usersAPI = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Configurações do localStorage para usuários
const STORAGE_PREFIX_USERS = 'fortnite_users_';
const STORAGE_PREFIX_USER_PROFILE = 'fortnite_user_profile_';
const STORAGE_PREFIX_USER_COSMETICS = 'fortnite_user_cosmetics_';
const CACHE_EXPIRY_USERS_MS = 10 * 60 * 1000; // 10 minutos para lista de usuários
const CACHE_EXPIRY_PROFILE_MS = 15 * 60 * 1000; // 15 minutos para perfil
const CACHE_EXPIRY_COSMETICS_MS = 5 * 60 * 1000; // 5 minutos para cosméticos (muda mais)

// Funções auxiliares para localStorage
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
      // Limpar cache antigo se exceder quota
      clearOldestUsersCache();
    }
  }
};

const loadFromStorage = (key, expiryMs) => {
  try {
    const stored = localStorage.getItem(key);
    if (!stored) return null;
    
    const cacheData = JSON.parse(stored);
    const now = Date.now();
    const expiry = expiryMs || cacheData.expiry || CACHE_EXPIRY_USERS_MS;
    
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

const clearOldestUsersCache = () => {
  try {
    const keys = Object.keys(localStorage);
    const userKeys = keys.filter(k => 
      k.startsWith(STORAGE_PREFIX_USERS) || 
      k.startsWith(STORAGE_PREFIX_USER_PROFILE) || 
      k.startsWith(STORAGE_PREFIX_USER_COSMETICS)
    );
    
    if (userKeys.length <= 50) return; // Limite de 50 itens
    
    // Ordenar por timestamp e remover os mais antigos
    const itemsWithTime = userKeys.map(key => {
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
    const toRemove = itemsWithTime.slice(0, userKeys.length - 50);
    
    toRemove.forEach(item => {
      localStorage.removeItem(item.key);
    });
    
    console.log(`Removidos ${toRemove.length} itens antigos do cache de usuários`);
  } catch (err) {
    console.warn('Erro ao limpar cache antigo de usuários:', err);
  }
};

export const usersService = {
  // Listar usuários (público) - com cache
  getUsers: async (page = 1, pageSize = 20, forceRefresh = false) => {
    const cacheKey = `${STORAGE_PREFIX_USERS}${page}-${pageSize}`;
    
    // Tentar carregar do localStorage primeiro
    if (!forceRefresh) {
      const cached = loadFromStorage(cacheKey, CACHE_EXPIRY_USERS_MS);
      if (cached) {
        console.log(`Lista de usuários (página ${page}) carregada do localStorage`);
        return cached;
      }
    }

    // Se não encontrou no cache ou forçou refresh, buscar da API
    try {
      const response = await usersAPI.get('/users', {
        params: { page, pageSize }
      });
      const data = response.data;
      
      // Salvar no localStorage
      saveToStorage(cacheKey, data, CACHE_EXPIRY_USERS_MS);
      console.log(`Lista de usuários (página ${page}) salva no localStorage`);
      
      return data;
    } catch (error) {
      console.error('Erro ao buscar usuários:', error);
      // Tentar retornar cache mesmo expirado em caso de erro
      const cached = loadFromStorage(cacheKey, Infinity);
      if (cached) {
        console.log('Usando cache expirado de usuários devido a erro');
        return cached;
      }
      throw error;
    }
  },

  // Obter perfil de um usuário - com cache
  getUserProfile: async (userId, forceRefresh = false) => {
    const cacheKey = `${STORAGE_PREFIX_USER_PROFILE}${userId}`;
    
    // Tentar carregar do localStorage primeiro
    if (!forceRefresh) {
      const cached = loadFromStorage(cacheKey, CACHE_EXPIRY_PROFILE_MS);
      if (cached) {
        console.log(`Perfil do usuário ${userId} carregado do localStorage`);
        return cached;
      }
    }

    // Se não encontrou no cache ou forçou refresh, buscar da API
    try {
      const response = await usersAPI.get(`/users/${userId}`);
      const data = response.data;
      
      // Salvar no localStorage
      saveToStorage(cacheKey, data, CACHE_EXPIRY_PROFILE_MS);
      console.log(`Perfil do usuário ${userId} salvo no localStorage`);
      
      return data;
    } catch (error) {
      console.error('Erro ao buscar perfil do usuário:', error);
      // Tentar retornar cache mesmo expirado em caso de erro
      const cached = loadFromStorage(cacheKey, Infinity);
      if (cached) {
        console.log('Usando cache expirado de perfil devido a erro');
        return cached;
      }
      throw error;
    }
  },

  // Obter cosméticos de um usuário - com cache
  getUserCosmetics: async (userId, page = 1, pageSize = 50, forceRefresh = false) => {
    const cacheKey = `${STORAGE_PREFIX_USER_COSMETICS}${userId}-${page}-${pageSize}`;
    
    // Tentar carregar do localStorage primeiro
    if (!forceRefresh) {
      const cached = loadFromStorage(cacheKey, CACHE_EXPIRY_COSMETICS_MS);
      if (cached) {
        console.log(`Cosméticos do usuário ${userId} (página ${page}) carregados do localStorage`);
        return cached;
      }
    }

    // Se não encontrou no cache ou forçou refresh, buscar da API
    try {
      const response = await usersAPI.get(`/users/${userId}/cosmetics`, {
        params: { page, pageSize }
      });
      const data = response.data;
      
      // Salvar no localStorage
      saveToStorage(cacheKey, data, CACHE_EXPIRY_COSMETICS_MS);
      console.log(`Cosméticos do usuário ${userId} (página ${page}) salvos no localStorage`);
      
      return data;
    } catch (error) {
      console.error('Erro ao buscar cosméticos do usuário:', error);
      // Tentar retornar cache mesmo expirado em caso de erro
      const cached = loadFromStorage(cacheKey, Infinity);
      if (cached) {
        console.log('Usando cache expirado de cosméticos devido a erro');
        return cached;
      }
      throw error;
    }
  },

  // Limpar cache de um usuário específico (útil após compras/devoluções)
  clearUserCache: (userId) => {
    try {
      const keys = Object.keys(localStorage);
      const userKeys = keys.filter(k => 
        k.startsWith(`${STORAGE_PREFIX_USER_PROFILE}${userId}`) || 
        k.startsWith(`${STORAGE_PREFIX_USER_COSMETICS}${userId}`)
      );
      userKeys.forEach(key => localStorage.removeItem(key));
      console.log(`Cache do usuário ${userId} limpo`);
    } catch (err) {
      console.warn('Erro ao limpar cache do usuário:', err);
    }
  },

  // Limpar todo o cache de usuários
  clearAllUsersCache: () => {
    try {
      const keys = Object.keys(localStorage);
      const userKeys = keys.filter(k => 
        k.startsWith(STORAGE_PREFIX_USERS) || 
        k.startsWith(STORAGE_PREFIX_USER_PROFILE) || 
        k.startsWith(STORAGE_PREFIX_USER_COSMETICS)
      );
      userKeys.forEach(key => localStorage.removeItem(key));
      console.log('Todo o cache de usuários foi limpo');
    } catch (err) {
      console.warn('Erro ao limpar cache de usuários:', err);
    }
  },
};

export default usersAPI;

