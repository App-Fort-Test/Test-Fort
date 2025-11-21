<template>
  <div class="modal-overlay" @click="emit('close')">
    <div class="modal-content" @click.stop>
      
      <button class="close-button" @click="emit('close')">
        <svg class="close-icon" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="18" y1="6" x2="6" y2="18"></line>
          <line x1="6" y1="6" x2="18" y2="18"></line>
        </svg>
      </button>
      
      <slot></slot>
      
    </div>
  </div>
</template>

<script setup>
const emit = defineEmits(['close']);
</script>

<style scoped>

.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: rgba(0, 0, 0, 0.7); 
  display: flex;
  justify-content: center;
  align-items: center;
  z-index: 1000; 
  outline: none; 
  padding: 20px; /* Adicionado padding para não colar nas bordas */
  box-sizing: border-box;
}


.modal-content {
  background-color: #1a1a1a;
  padding: 40px; 
  border-radius: 40px;
  border: 1px solid #12D8FA;
  min-width: 450px;
  max-width: 95vw; /* Adicionado para não ultrapassar a tela */
  max-height: 90vh; /* Adicionado para não ultrapassar a tela */
  position: relative;
  outline: none; 
  box-shadow: 0 0 1px rgba(0, 224, 255, 0.3);
  overflow-y: auto; /* Adicionado para scroll se necessário */
}

.close-button {
  position: absolute;
  top: 20px; 
  right: 20px;
  background: none;
  border: none;
  padding: 0;
  
  cursor: pointer;
  z-index: 1010; 
  transition: transform 0.2s;
  outline: none;
}

.close-button:hover {
  transform: rotate(90deg); 
}

.close-icon {
  width: 24px;
  height: 24px;
  color: #12D8FA; 
}


</style>