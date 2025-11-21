<template>
  <div class="signup-form-container">
    <h2 class="form-title">CRIE SUA CONTA PARA COMPRAR OS COSMETICOS</h2>
    
    <div class="input-group">
      <label for="username">Nome de Usuário</label>
      <input type="text" id="username" placeholder="Insira seu nome de usuário" v-model="formData.username" />
    </div>

    <div class="input-group">
      <label for="email">E-mail</label>
      <input type="email" id="email" placeholder="Insira seu e-mail" v-model="formData.email" />
    </div>

    <div class="input-group">
      <label for="password">Senha</label>
      <input type="password" id="password" placeholder="Insira a sua senha" v-model="formData.password" />
    </div>

    <div v-if="errorMessage" class="error-message">
      {{ errorMessage }}
    </div>

    <div v-if="successMessage" class="success-message">
      {{ successMessage }}
    </div>

    <button class="primarybutton" @click="handleSignup" :disabled="isLoading">
      {{ isLoading ? 'Criando conta...' : 'Crie a sua conta' }}
    </button>
  </div>
</template>

<script setup>
import { reactive, ref } from 'vue';
import { useAuth } from '../../../composables/useAuth';

const emit = defineEmits(['signup-success', 'close']);

const { register } = useAuth();

const formData = reactive({
  username: '',
  email: '',
  password: ''
});

const errorMessage = ref('');
const successMessage = ref('');
const isLoading = ref(false);

const handleSignup = async () => {
  // Validações
  if (!formData.username || !formData.email || !formData.password) {
    errorMessage.value = 'Por favor, preencha todos os campos';
    return;
  }

  if (formData.username.length < 3) {
    errorMessage.value = 'O nome de usuário deve ter pelo menos 3 caracteres';
    return;
  }

  if (formData.password.length < 6) {
    errorMessage.value = 'A senha deve ter pelo menos 6 caracteres';
    return;
  }

  isLoading.value = true;
  errorMessage.value = '';
  successMessage.value = '';

  try {
    const result = await register(formData.email, formData.password, formData.username);
    
    if (result.success) {
      successMessage.value = `Conta criada com sucesso! Você recebeu ${result.data.vbucks} v-bucks.`;
      emit('signup-success', result.data);
      // Limpar formulário após 2 segundos e fechar modal
      setTimeout(() => {
        formData.username = '';
        formData.email = '';
        formData.password = '';
        emit('close');
      }, 2000);
    } else {
      errorMessage.value = result.message || 'Erro ao criar conta';
    }
  } catch (error) {
    errorMessage.value = 'Erro ao criar conta. Tente novamente.';
  } finally {
    isLoading.value = false;
  }
};
</script>

<style scoped>
.signup-form-container {
  background-color:  #1a1a1a;
  padding: 40px;
  border-radius: 12px;
  max-width: 600px; 
  margin: 0 auto;
  font-family: 'Arial', sans-serif;
  color: white;
}

.form-title {
  font-size: 1.8rem;
  font-weight: 700;
  text-align: center;
  margin-bottom: 40px;
  line-height: 1.4;
  color: #fff;
}

.input-group {
  margin-bottom: 20px;
}

.input-group label {
  display: block;
  margin-bottom: 8px;
  font-size: 0.95rem;
  color: #aaa;
}

input[type="text"],
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
}

input[type="text"]::placeholder,
input[type="email"]::placeholder,
input[type="password"]::placeholder {
  color: #666;
}

input[type="text"]:focus,
input[type="email"]:focus,
input[type="password"]:focus {
  border-color: #00E0FF; 
  box-shadow: 0 0 0 2px rgba(0, 224, 255, 0.2);
}


.submit-button {
  width: 100%;
  padding: 16px;
  background: linear-gradient(to right, #0a0e21, #12d8fa); /* Gradiente da imagem */
  border: none;
  border-radius: 8px;
  color: white;
  font-size: 1.1rem;
  font-weight: 700;
  cursor: pointer;
  transition:5 opacity 0.2s, transform 0.1s;
  box-shadow: 0 4px 15px rgba(0, 224, 255, 0.3);
}

.submit-button:hover {
  opacity: 0.9;
  transform: translateY(-1px);
}

.submit-button:active {
  transform: translateY(0);
}

.primarybutton{
   width: 100%;
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

.success-message {
  color: #44ff44;
  background-color: rgba(68, 255, 68, 0.1);
  border: 1px solid #44ff44;
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