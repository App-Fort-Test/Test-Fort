import { ref, computed } from 'vue';
import { authService } from '../services/auth';

const user = ref(null);
const isAuthenticated = computed(() => user.value !== null);

export function useAuth() {
  // Carregar usuário do localStorage ao inicializar
  const loadUserFromStorage = () => {
    const storedUser = localStorage.getItem('user');
    const userId = localStorage.getItem('userId');
    if (storedUser && userId) {
      try {
        user.value = JSON.parse(storedUser);
      } catch (e) {
        console.error('Erro ao carregar usuário do storage:', e);
        localStorage.removeItem('user');
        localStorage.removeItem('userId');
      }
    }
  };

  // Registrar novo usuário
  const register = async (email, password, username) => {
    try {
      const data = await authService.register(email, password, username);
      user.value = {
        id: data.id,
        email: data.email,
        username: data.username,
        vbucks: data.vbucks,
      };
      localStorage.setItem('user', JSON.stringify(user.value));
      localStorage.setItem('userId', data.id.toString());
      return { success: true, data };
    } catch (error) {
      const message = error.response?.data?.message || 'Erro ao registrar usuário';
      return { success: false, message };
    }
  };

  // Login
  const login = async (email, password) => {
    try {
      const data = await authService.login(email, password);
      user.value = {
        id: data.id,
        email: data.email,
        username: data.username,
        vbucks: data.vbucks,
      };
      localStorage.setItem('user', JSON.stringify(user.value));
      localStorage.setItem('userId', data.id.toString());
      return { success: true, data };
    } catch (error) {
      const message = error.response?.data?.message || 'Erro ao fazer login';
      return { success: false, message };
    }
  };

  // Logout
  const logout = () => {
    user.value = null;
    localStorage.removeItem('user');
    localStorage.removeItem('userId');
  };

  // Atualizar v-bucks do usuário
  const updateVBucks = (newVBucks) => {
    if (user.value) {
      user.value.vbucks = newVBucks;
      localStorage.setItem('user', JSON.stringify(user.value));
    }
  };

  // Atualizar dados do usuário
  const updateUser = (userData) => {
    if (userData) {
      user.value = { ...user.value, ...userData };
      localStorage.setItem('user', JSON.stringify(user.value));
    }
  };

  // Carregar usuário do storage na inicialização
  loadUserFromStorage();

  return {
    user,
    isAuthenticated,
    register,
    login,
    logout,
    updateVBucks,
    updateUser,
    loadUserFromStorage,
  };
}

