import axios from 'axios';

const API_BASE_URL = 'http://localhost:5155/api/ControllerCosmeticsEnriched';

const api = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar userId nos headers quando disponível
api.interceptors.request.use(
  (config) => {
    const userId = localStorage.getItem('userId');
    if (userId) {
      config.headers['X-User-Id'] = userId;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Interceptor para tratamento de erros melhorado
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const errorDetails = {
      message: error.message,
      status: error.response?.status,
      statusText: error.response?.statusText,
      data: error.response?.data,
      url: error.config?.url,
      method: error.config?.method,
      params: error.config?.params,
    };
    console.error('API Error Details:', errorDetails);
    return Promise.reject(error);
  }
);

export const cosmeticsAPI = {
  // Listar cosméticos com paginação
  getCosmetics: async (page = 1, pageSize = 50, sortBy = '', userId = null) => {
    const params = {
      page: Number(page) || 1,
      pageSize: Number(pageSize) || 50,
    };
    if (sortBy && sortBy.trim() !== '') {
      params.sortBy = sortBy.trim();
    }
    const headers = userId ? { 'X-User-Id': userId } : {};
    const response = await api.get('', { params, headers });
    return response.data;
  },

  // Buscar com filtros, paginação e ordenação
  searchCosmetics: async (filters = {}, page = 1, pageSize = 50, sortBy = '') => {
    // Construir params conforme esperado pela API
    const params = {};
    
    // Parâmetros de string - sempre enviar como string vazia se não houver valor
    params.name = filters.name ? String(filters.name).trim() : '';
    params.type = filters.type ? String(filters.type).trim() : '';
    params.rarity = filters.rarity ? String(filters.rarity).trim() : '';
    
    // Parâmetros booleanos - converter para boolean explícito
    // A API pode esperar true/false ou "true"/"false" como string
            params.onlyNew = filters.onlyNew === true || filters.onlyNew === 'true' || filters.onlyNew === 1;
            params.onlyInShop = filters.onlyInShop === true || filters.onlyInShop === 'true' || filters.onlyInShop === 1;
            params.onlyOnSale = filters.onlyOnSale === true || filters.onlyOnSale === 'true' || filters.onlyOnSale === 1;
            params.onlyOwned = filters.onlyOwned === true || filters.onlyOwned === 'true' || filters.onlyOwned === 1;
            params.onlyBundle = filters.onlyBundle === true || filters.onlyBundle === 'true' || filters.onlyBundle === 1;
    
    // Parâmetros de paginação
    params.page = Number(page) || 1;
    params.pageSize = Number(pageSize) || 50;
    
    // Adicionar filtros de data apenas se existirem e forem válidos
    // Backend espera dateFrom e dateTo
    if (filters.startDate) {
      try {
        const date = filters.startDate instanceof Date 
          ? filters.startDate 
          : new Date(filters.startDate);
        if (!isNaN(date.getTime())) {
          // Formato pode ser ISO string ou apenas data
          params.dateFrom = date.toISOString().split('T')[0]; // YYYY-MM-DD
        }
      } catch (e) {
        console.warn('Data de início inválida, ignorando:', filters.startDate);
      }
    }
    
    if (filters.endDate) {
      try {
        const date = filters.endDate instanceof Date 
          ? filters.endDate 
          : new Date(filters.endDate);
        if (!isNaN(date.getTime())) {
          params.dateTo = date.toISOString().split('T')[0]; // YYYY-MM-DD
        }
      } catch (e) {
        console.warn('Data de fim inválida, ignorando:', filters.endDate);
      }
    }
    
    // Adicionar filtros de preço apenas se forem números válidos
    if (filters.minPrice !== null && filters.minPrice !== undefined && filters.minPrice !== '') {
      const minPrice = Number(filters.minPrice);
      if (!isNaN(minPrice) && minPrice >= 0) {
        params.minPrice = minPrice;
      }
    }
    
    if (filters.maxPrice !== null && filters.maxPrice !== undefined && filters.maxPrice !== '') {
      const maxPrice = Number(filters.maxPrice);
      if (!isNaN(maxPrice) && maxPrice >= 0) {
        params.maxPrice = maxPrice;
      }
    }
    
    // Adicionar ordenação apenas se não estiver vazia
    if (sortBy && sortBy.trim() !== '') {
      params.sortBy = sortBy.trim();
    }
    
    console.log('Requisição para API /search:', {
      url: `${API_BASE_URL}/search`,
      params: params,
      paramsString: new URLSearchParams(params).toString()
    });
    
    // Adicionar userId nos headers se disponível
    const headers = {};
    const userId = localStorage.getItem('userId');
    if (userId) {
      headers['X-User-Id'] = userId;
    }
    
    try {
      const response = await api.get('/search', { params, headers });
      
      console.log('Resposta da API:', {
        status: response.status,
        dataType: Array.isArray(response.data) ? 'array' : typeof response.data,
        dataLength: Array.isArray(response.data) ? response.data.length : response.data?.items?.length,
      });
      
      return response.data;
    } catch (error) {
      console.error('Erro na requisição searchCosmetics:', {
        message: error.message,
        status: error.response?.status,
        data: error.response?.data,
        params: params
      });
      throw error;
    }
  },

  // Ver inventário - com cache
  getInventory: async (userId, forceRefresh = false) => {
    if (!userId) {
      throw new Error('userId é obrigatório');
    }
    
    const cacheKey = `fortnite_inventory_${userId}`;
    const CACHE_EXPIRY_MS = 5 * 60 * 1000; // 5 minutos
    
    // Tentar carregar do localStorage primeiro
    if (!forceRefresh) {
      try {
        const stored = localStorage.getItem(cacheKey);
        if (stored) {
          const cacheData = JSON.parse(stored);
          const now = Date.now();
          
          if (now - cacheData.timestamp < CACHE_EXPIRY_MS) {
            console.log(`Inventário do usuário ${userId} carregado do localStorage`);
            return cacheData.data;
          } else {
            localStorage.removeItem(cacheKey);
          }
        }
      } catch (err) {
        console.warn('Erro ao carregar inventário do localStorage:', err);
      }
    }

    // Se não encontrou no cache ou forçou refresh, buscar da API
    try {
      const response = await api.get('/inventory', {
        headers: userId ? { 'X-User-Id': userId } : {}
      });
      const data = response.data;
      
      // Salvar no localStorage
      try {
        const cacheData = {
          data: data,
          timestamp: Date.now()
        };
        localStorage.setItem(cacheKey, JSON.stringify(cacheData));
        console.log(`Inventário do usuário ${userId} salvo no localStorage`);
      } catch (err) {
        console.warn('Erro ao salvar inventário no localStorage:', err);
      }
      
      return data;
    } catch (error) {
      console.error('Erro ao buscar inventário:', error);
      // Tentar retornar cache mesmo expirado em caso de erro
      try {
        const stored = localStorage.getItem(cacheKey);
        if (stored) {
          const cacheData = JSON.parse(stored);
          console.log('Usando cache expirado de inventário devido a erro');
          return cacheData.data;
        }
      } catch (e) {
        // Ignorar erro ao tentar usar cache expirado
      }
      throw error;
    }
  },

  // Ver créditos V-Bucks
  getVBucks: async (userId) => {
    try {
      const response = await api.get('/vbucks', {
        headers: userId ? { 'X-User-Id': userId } : {}
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao buscar V-Bucks:', error);
      throw error;
    }
  },

  // Comprar cosmético
  purchaseCosmetic: async (cosmeticId, price, cosmeticName, userId) => {
    try {
      const response = await api.post(`/purchase/${cosmeticId}`, { 
        price: Number(price) || 0,
        cosmeticName: cosmeticName || cosmeticId
      }, {
        headers: userId ? { 'X-User-Id': userId } : {}
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao comprar cosmético:', error);
      throw error;
    }
  },

  // Buscar novos cosméticos
  getNewCosmetics: async () => {
    try {
      const response = await api.get('/new');
      return response.data;
    } catch (error) {
      console.error('Erro ao buscar novos cosméticos:', error);
      throw error;
    }
  },

  // Buscar shop
  getShop: async () => {
    try {
      const response = await api.get('/shop');
      return response.data;
    } catch (error) {
      console.error('Erro ao buscar shop:', error);
      throw error;
    }
  },

  // Devolver cosmético
  refundCosmetic: async (cosmeticId, cosmeticName, userId) => {
    try {
      const response = await api.post(`http://localhost:5155/api/transactions/refund/${cosmeticId}`, {
        cosmeticName: cosmeticName || cosmeticId
      }, {
        headers: userId ? { 'X-User-Id': userId } : {}
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao devolver cosmético:', error);
      throw error;
    }
  },

  // Comprar bundle
  purchaseBundle: async (cosmetics, userId) => {
    try {
      const response = await axios.post('http://localhost:5155/api/bundles/purchase', {
        cosmetics: cosmetics
      }, {
        headers: {
          'Content-Type': 'application/json',
          'X-User-Id': userId
        }
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao comprar bundle:', error);
      throw error;
    }
  },
};

export default api;
