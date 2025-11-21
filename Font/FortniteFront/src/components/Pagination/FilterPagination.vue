<template>
  <div class="pagination-container">
    <button 
      class="nav-button prev-button" 
      :disabled="currentPage === 1"
      @click="$emit('change', currentPage - 1)"
    >
      Anterior
    </button>

    <span class="page-info">{{ currentPage }} / {{ totalPages }}</span>
    
    <button 
      class="nav-button next-button" 
      :disabled="currentPage >= totalPages || totalPages === 0"
      @click="$emit('change', currentPage + 1)"
    >
      Próxima
    </button>
  </div>
</template>

<script setup>
const props = defineProps({
  currentPage: { type: Number, default: 1 },
  totalPages: { type: Number, default: 1 } 
});

defineEmits(['change']);
</script>

<style scoped>
.pagination-container {
  display: flex;
  align-items: center;
  gap: 12px;
  font-family: sans-serif;
}

.nav-button {
  padding: 8px 16px;
  border-radius: 8px;
  border: 2px solid #212554;
  background: linear-gradient(162deg, #161A42 22.61%, rgba(22, 26, 66, 0) 118.29%);
  color: #fff;
  font-size: 0.95rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  min-width: 100px;
}

.nav-button:hover:not(:disabled) {
  border-color: #00E0FF;
  background: linear-gradient(162deg, #1fa2ff 22.61%, #12d8fa 118.29%);
  color: #000;
  transform: translateY(-1px);
}

.nav-button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  border-color: #555;
}

.page-info {
  color: white;
  font-weight: bold;
  font-size: 1rem;
  padding: 0 12px;
  min-width: 80px;
  text-align: center;
}
</style>