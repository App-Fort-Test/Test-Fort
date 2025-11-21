<template>
  <button class="logout-button" @click="$emit('logout')">
    <svg class="logout-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
      <path d="M10 3H6a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h4"></path>
      <polyline points="16 17 21 12 16 7"></polyline>
      <line x1="21" y1="12" x2="9" y2="12"></line>
    </svg>
  </button>
</template>

<script setup>
const emit = defineEmits(['logout']);
</script>

<style scoped>
.logout-button {
  border-radius: 40px;
  border: 2px solid #212554;
  background: linear-gradient(
    162deg,
    #161a42 22.61%,
    rgba(22, 26, 66, 0) 118.29%
  );
  color: white;
  width: 48px;
  height: 48px;
}

.logout-button:hover {
  background: #212554;
  border: 2px solid #1fa2ff;
  cursor: pointer;
  color: #1fa2ff;
}

.logout-icon {
  color: #00E0FF; /* Ciano de destaque */
}
</style>