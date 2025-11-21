import axios from 'axios';

const API_BASE_URL = 'http://localhost:5155/api';

const transactionsAPI = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Interceptor para adicionar userId nos headers quando disponível
transactionsAPI.interceptors.request.use(
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

export const transactionsService = {
  // Obter histórico de transações
  getHistory: async (userId) => {
    try {
      const response = await transactionsAPI.get('/transactions', {
        headers: userId ? { 'X-User-Id': userId.toString() } : {}
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao buscar histórico:', error);
      throw error;
    }
  },

  // Devolver cosmético
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

