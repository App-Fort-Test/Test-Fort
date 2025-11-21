<template>
  <div class="login-form-container">
    <h2 class="form-title">LOGIN</h2>
    
    <div class="input-group with-icon">
      <svg class="input-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <path d="M4 4h16c1.1 0 2 .9 2 2v12c0 1.1-.9 2-2 2H4c-1.1 0-2-.9-2-2V6c0-1.1.9-2 2-2z"></path>
        <polyline points="22,6 12,13 2,6"></polyline>
      </svg>
      <input type="email" id="login-email" placeholder="example@email.com" v-model="formData.email" />
    </div>

    <div class="input-group with-icon">
      <svg class="input-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
        <rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect>
        <path d="M7 11V7a5 5 0 0 1 10 0v4"></path>
      </svg>
      <input type="password" id="login-password" placeholder="Insira a senha" v-model="formData.password" />
    </div>

    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <button class="primarybutton" @click="handleLogin" :disabled="isLoading">
      {{ isLoading ? 'Entrando...' : 'Login' }}
    </button>

    <p class="signup-prompt">
      Não possui conta? <a href="#" @click.prevent="$emit('switch-to-signup')">Criar minha conta</a>
    </p>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue';
import { useAuth } from '../../../composables/useAuth';

const emit = defineEmits(['switch-to-signup', 'login-success', 'close']);

const { login } = useAuth();

const formData = reactive({
  email: '',
  password: ''
});

const errorMessage = ref('');
const isLoading = ref(false);

const handleLogin = async () => {
  if (!formData.email || !formData.password) {
    errorMessage.value = 'Por favor, preencha todos os campos';
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';

  try {
    const result = await login(formData.email, formData.password);
    
    if (result.success) {
      emit('login-success', result.data);
      emit('close');
      // Limpar formulário
      formData.email = '';
      formData.password = '';
    } else {
      errorMessage.value = result.message || 'Erro ao fazer login';
    }
  } catch (error) {
    errorMessage.value = 'Erro ao fazer login. Tente novamente.';
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.login-form-container {
  background-color:#1a1a1a;
  padding: 40px;
  border-radius: 12px;
  max-width: 450px; 
  margin: 0 auto;
  font-family: 'Arial', sans-serif;
  color: white;
  text-align: center; 
}

.form-title {
  font-size: 2.2rem;
  font-weight: 700;
  margin-bottom: 40px;
  color: #fff;
}

.input-group {
  margin-bottom: 25px;
  position: relative; 
  text-align: left; 
}

.input-group.with-icon input {
  padding-left: 50px; 
}

.input-icon {
  position: absolute;
  left: 18px;
  top: 50%;
  transform: translateY(-50%);
  color: #666; 
}

input[type="email"],
input[type="password"] {
  width: 100%;
  padding: 14px 18px;
  background-color: #2a2a2a;
  border: 1px solid #3a3a3a;
  border-radius: 8px;
  color: white;
  font-size: 1rem;
  outline: none;
  box-sizing: border-box;
  transition: border-color 0.2s, box-shadow 0.2s;
}

input[type="email"]::placeholder,
input[type="password"]::placeholder {
  color: #666;
}

input[type="email"]:focus,
input[type="password"]:focus {
  border-color: #00E0FF;
  box-shadow: 0 0 0 2px rgba(0, 224, 255, 0.2);
}

.submit-button {
  width: 100%;
  padding: 16px;
  background: linear-gradient(to right, #0a0e21, #12d8fa);
  border: none;
  border-radius: 8px;
  color: white;
  font-size: 1.1rem;
  font-weight: 700;
  cursor: pointer;
  transition: opacity 0.2s, transform 0.1s;
  box-shadow: 0 4px 15px rgba(0, 224, 255, 0.3);
  margin-bottom: 30px;
}

.submit-button:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.submit-button:active {
  transform: translateY(0);
}

.signup-prompt {
  color: #aaa;
  font-size: 0.9rem;
}

.signup-prompt a {
  color: #00E0FF; /* Cor de destaque para o link */
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s;
}

.signup-prompt a:hover {
  color: #00b0d3;
  text-decoration: underline;
}

.error-message {
  color: #ff4444;
  background-color: rgba(255, 68, 68, 0.1);
  border: 1px solid #ff4444;
  border-radius: 8px;
  padding: 12px;
  margin-bottom: 20px;
  font-size: 0.9rem;
  text-align: left;
}

.primarybutton:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>