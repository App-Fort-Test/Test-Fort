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

