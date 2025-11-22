<template>
  <div class="header-login-container">
    <Wallet :amount="authUser?.vbucks || 0" />
    <User 
      :user-name="authUser?.username || 'Usuário'" 
      :user-email="authUser?.email || ''" 
    />
    <Logout @logout="$emit('logout')" />
  </div>
</template>

<script setup>
import { watch, computed } from 'vue';
import Wallet from './HeaderComp/Wallet.vue';
import User from './HeaderComp/User.vue';
import Logout from './HeaderComp/Logout.vue';
import { useAuth } from '../../composables/useAuth';

const props = defineProps({
  user: {
    type: Object,
    default: null
  }
});

defineEmits(['logout']);

// Usar diretamente o user do useAuth para garantir reatividade
const { user: authUser } = useAuth();

// Observar mudanças no user para atualizar a exibição
watch(() => authUser.value?.vbucks, (newVBucks) => {
  console.log('V-bucks atualizado no HeaderLogin:', newVBucks);
}, { immediate: true, deep: true });
</script>

<style scoped>
.header-login-container {
  display: flex;
  align-items: center;
  gap: 12px;
}
</style>
