import axios from 'axios';

// Usar variável de ambiente se disponível, senão usar localhost
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL?.replace('/ControllerCosmeticsEnriched', '') || 'http://localhost:5155/api';

const transactionsAPI = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

transactionsAPI.interceptors.request.use(
  (config) => {
    const existingUserId = config.headers['X-User-Id'];
    console.log('Interceptor transactionsAPI - URL:', config.url, 'X-User-Id existente:', existingUserId);
    
    if (!existingUserId) {
      const userId = localStorage.getItem('userId');
      if (userId) {
        config.headers['X-User-Id'] = userId;
        console.log('Interceptor adicionou userId do localStorage:', userId);
      }
    } else {
      console.log('Interceptor não adicionou userId - já existe:', existingUserId);
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

export const transactionsService = {
  getHistory: async (userId) => {
    try {
      const userIdNum = userId ? (typeof userId === 'string' ? parseInt(userId, 10) : userId) : null;
      
      let finalUserId = userIdNum;
      if (!finalUserId) {
        const storedUserId = localStorage.getItem('userId');
        if (storedUserId) {
          finalUserId = parseInt(storedUserId, 10);
          console.log('UserId obtido do localStorage para histórico:', finalUserId);
        }
      }
      
      if (!finalUserId) {
        throw new Error('UserId não encontrado. É necessário especificar um userId ou estar logado para ver o histórico.');
      }
      
      console.log('Buscando histórico com userId:', finalUserId, 'tipo:', typeof finalUserId);
      console.log('UserId do localStorage (não deve ser usado aqui):', localStorage.getItem('userId'));
      
      const config = {
        headers: { 
          'X-User-Id': finalUserId.toString(),
          'Content-Type': 'application/json'
        }
      };
      
      config.headers['X-User-Id'] = finalUserId.toString();
      
      console.log('Headers que serão enviados:', config.headers);
      const response = await transactionsAPI.get('/transactions', config);
      
      console.log('Resposta do histórico recebida:', response.data);
      console.log('Número de transações retornadas:', response.data?.transactions?.length || 0);
      
      if (response.data?.transactions && response.data.transactions.length > 0) {
        console.log('Primeira transação:', response.data.transactions[0]);
      }
      return response.data;
    } catch (error) {
      console.error('Erro ao buscar histórico:', error);
      console.error('Detalhes do erro:', {
        message: error.message,
        response: error.response?.data,
        status: error.response?.status
      });
      throw error;
    }
  },

  getOwnedCosmetics: async (userId) => {
    try {
      const userIdNum = userId ? (typeof userId === 'string' ? parseInt(userId, 10) : userId) : null;
      let finalUserId = userIdNum;
      
      if (!finalUserId) {
        const storedUserId = localStorage.getItem('userId');
        if (storedUserId) {
          finalUserId = parseInt(storedUserId, 10);
        }
      }
      
      if (!finalUserId) {
        throw new Error('UserId não encontrado');
      }
      
      const config = {
        headers: { 
          'X-User-Id': finalUserId.toString(),
          'Content-Type': 'application/json'
        }
      };
      
      const response = await transactionsAPI.get('/transactions/owned', config);
      return response.data;
    } catch (error) {
      console.error('Erro ao buscar itens possuídos das transações:', error);
      throw error;
    }
  },

  refundCosmetic: async (cosmeticId, cosmeticName, userId) => {
    try {
      const response = await transactionsAPI.post(`/transactions/refund/${cosmeticId}`, {
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
};

export default transactionsAPI;

