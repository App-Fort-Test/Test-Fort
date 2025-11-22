import axios from 'axios';

// Função para normalizar a URL da API
const normalizeApiUrl = (url) => {
  if (!url) return 'http://localhost:5155/api';
  
  // Remover /ControllerCosmeticsEnriched se existir
  url = url.replace('/ControllerCosmeticsEnriched', '');
  
  // Se não começar com http:// ou https://, adicionar https://
  if (!url.startsWith('http://') && !url.startsWith('https://')) {
    url = 'https://' + url;
  }
  
  // Garantir que termina com /api
  if (!url.endsWith('/api')) {
    // Se terminar com /, remover antes de adicionar /api
    url = url.replace(/\/$/, '');
    url = url + '/api';
  }
  
  return url;
};

// Usar variável de ambiente se disponível, senão usar localhost
const API_BASE_URL = normalizeApiUrl(import.meta.env.VITE_API_BASE_URL) || 'http://localhost:5155/api';

console.log('API_BASE_URL configurada:', API_BASE_URL);

const authAPI = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

export const authService = {
  // Registrar novo usuário
  register: async (email, password, username) => {
    try {
      const response = await authAPI.post('/auth/register', {
        email,
        password,
        username,
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao registrar:', error);
      throw error;
    }
  },

  // Login
  login: async (email, password) => {
    try {
      const response = await authAPI.post('/auth/login', {
        email,
        password,
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao fazer login:', error);
      throw error;
    }
  },

  // Obter usuário atual
  getCurrentUser: async (userId) => {
    try {
      const response = await authAPI.get('/auth/me', {
        headers: {
          'X-User-Id': userId,
        },
      });
      return response.data;
    } catch (error) {
      console.error('Erro ao obter usuário:', error);
      throw error;
    }
  },
};

export default authAPI;

