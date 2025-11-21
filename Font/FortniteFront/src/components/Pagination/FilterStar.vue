<template>
  <div class="sort-container" ref="containerRef">
    
    <div 
      class="sort-trigger" 
      :class="{ 'is-active': isOpen }"
      @click="isOpen = !isOpen"
    >
      <span class="label-text">Ordenar por</span>
      
      <svg 
        class="arrow-icon" 
        :class="{ 'is-open': isOpen }"
        width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
      >
        <polyline points="6 9 12 15 18 9"></polyline>
      </svg>
    </div>

    <transition name="fade">
      <ul v-if="isOpen" class="dropdown-menu">
        <li 
          v-for="option in options" 
          :key="option.value" 
          class="dropdown-item"
          :class="{ 'is-selected': modelValue === option.value }"
          @click="selectOption(option)"
        >
          {{ option.label }}
          
          <span v-if="modelValue === option.value" class="check-icon">✓</span>
        </li>
      </ul>
    </transition>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';

const props = defineProps({
  modelValue: { type: String, default: '' }
});
const emit = defineEmits(['update:modelValue']);

const isOpen = ref(false);
const containerRef = ref(null);

// Opções de Ordenação (Edite conforme sua necessidade)
const options = [
  { label: 'Relevância', value: 'relevance' },
  { label: 'Menor Preço', value: 'price_asc' },
  { label: 'Maior Preço', value: 'price_desc' },
  { label: 'Mais Recentes', value: 'newest' },
  { label: 'Nome (A-Z)', value: 'name_asc' },
];

// Helper para pegar o label do item selecionado (caso queira exibir no botão)
const selectedLabel = computed(() => {
  const found = options.find(o => o.value === props.modelValue);
  return found ? found.label : '';
});

const selectOption = (option) => {
  emit('update:modelValue', option.value);
  isOpen.value = false;
};

// Fechar ao clicar fora
const handleClickOutside = (e) => {
  if (containerRef.value && !containerRef.value.contains(e.target)) {
    isOpen.value = false;
  }
};

onMounted(() => document.addEventListener('click', handleClickOutside));
onUnmounted(() => document.removeEventListener('click', handleClickOutside));
</script>

<style scoped>
.sort-container {
  position: relative;
  display: inline-block;
  font-family: sans-serif;
  user-select: none;
  z-index: 40; /* Garante que o dropdown fique acima de outros elementos */
}

/* --- Botão Principal --- */
.sort-trigger {
  background-color: #0c0e24; /* Azul profundo da imagem */
  color: #ccc; /* Texto cinza claro */
  padding: 10px 24px; /* Um pouco mais largo nas laterais */
  border-radius: 999px; /* Formato Pílula */
  display: flex;
  align-items: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid transparent;
}

.sort-trigger:hover {
  border-color: #333;
  color: white;
}

.sort-trigger.is-active {
  border-color: #555;
  color: white;
}

.label-text {
  font-size: 1rem;
  font-weight: 400;
}

/* Ícone Seta */
.arrow-icon {
  transition: transform 0.3s ease;
  color: #888;
}
.arrow-icon.is-open {
  transform: rotate(180deg);
  color: white;
}

/* --- Dropdown Menu --- */
.dropdown-menu {
  position: absolute;
  top: 120%; /* Espaço entre o botão e o menu */
  right: 0; /* Alinha à direita do botão (ou left: 0 se preferir) */
  min-width: 180px;
  background-color: #1e1e1e; /* Fundo do card */
  border: 1px solid #333;
  border-radius: 12px;
  padding: 8px 0;
  list-style: none;
  box-shadow: 0 8px 20px rgba(0,0,0,0.6);
  margin: 0;
}

.dropdown-item {
  padding: 10px 16px;
  color: #ccc;
  cursor: pointer;
  font-size: 0.95rem;
  transition: all 0.2s;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.dropdown-item:hover {
  background-color: #2c2c2c;
  color: white;
}

/* Item Selecionado (Ciano) */
.dropdown-item.is-selected {
  color: #00E0FF;
  font-weight: 600;
  background-color: rgba(0, 224, 255, 0.05);
}

.check-icon {
  font-size: 0.9rem;
  color: #00E0FF;
}

/* Animação de entrada */
.fade-enter-active, .fade-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(-10px);
}
</style>