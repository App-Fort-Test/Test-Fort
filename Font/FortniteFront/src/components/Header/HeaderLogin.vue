<template>
  <div class="header-login-container">
    <Wallet :amount="user?.vbucks || 0" />
    <User 
      :user-name="user?.username || 'Usuário'" 
      :user-email="user?.email || ''" 
    />
    <Logout @logout="$emit('logout')" />
  </div>
</template>

<script setup>
import Wallet from './HeaderComp/Wallet.vue';
import User from './HeaderComp/User.vue';
import Logout from './HeaderComp/Logout.vue';

defineProps({
  user: {
    type: Object,
    default: null
  }
});

defineEmits(['logout']);
</script>

<style scoped>
.header-login-container {
  display: flex;
  align-items: center;
  gap: 12px;
}
</style>
