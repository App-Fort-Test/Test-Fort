import axios from 'axios';

const API_BASE_URL = 'http://localhost:5155/api';

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

