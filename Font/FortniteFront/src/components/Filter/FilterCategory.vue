<template>
  <div class="filter-container">
    <div class="filter-header" @click="isOpen = !isOpen">
      <h3>Categoria</h3>
      <span class="toggle-icon">{{ isOpen ? '−' : '+' }}</span>
    </div>

    <transition name="slide-fade">
      <div v-if="isOpen">
        <div v-if="loading" class="loading-message">Carregando...</div>
        <ul v-else-if="categories.length > 0" class="filter-list">
          <li 
            v-for="item in categories" 
            :key="item.value" 
            class="filter-item"
            @click="toggleItem(item.value)"
          >
            <div class="left-content">
              <div 
                class="checkbox" 
                :class="{ 'is-active': selectedValue === item.value }"
              >
              </div>
              <span class="label">{{ item.label }}</span>
            </div>
            <span class="count">{{ item.count }}</span>
          </li>
        </ul>
        <div v-else class="empty-message">Nenhuma categoria encontrada</div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue';
import { useCosmetics } from '@/composables/useCosmetics';

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
});

const emit = defineEmits(['update:modelValue']);

const isOpen = ref(false);
const selectedValue = ref(props.modelValue || '');
const categories = ref([]);
const loading = ref(false);

const { filters, getFilterOptions } = useCosmetics();

const loadFilterOptions = async () => {
  try {
    loading.value = true;
    
    console.log('Buscando opções de categoria com filtros do frontend');
    const options = await getFilterOptions();
    console.log('Opções recebidas:', options);
    
    if (options && options.types && Array.isArray(options.types)) {
      // Garantir que todas as categorias sejam exibidas, mesmo com count 0
      categories.value = options.types
        .map(t => ({
          value: t.value,
          label: t.label || t.value,
          count: t.count !== undefined && t.count !== null ? t.count : 0
        }))
        .sort((a, b) => {
          // Ordenar por nome, mas manter ordem alfabética
          return a.label.localeCompare(b.label);
        });
      console.log('Categorias processadas:', categories.value);
    } else {
      console.warn('Nenhuma categoria encontrada ou formato inválido:', options);
      // Se não houver opções, manter lista vazia
      categories.value = [];
    }
  } catch (error) {
    console.error('Erro ao carregar opções de categoria:', error);
    console.error('Detalhes do erro:', error.response?.data || error.message);
    // Manter lista vazia em caso de erro - será recarregado quando os filtros mudarem
    categories.value = [];
  } finally {
    loading.value = false;
  }
};

const toggleItem = (value) => {
  if (selectedValue.value === value) {
    selectedValue.value = '';
  } else {
    selectedValue.value = value;
  }
  emit('update:modelValue', selectedValue.value);
};

watch(() => props.modelValue, (newVal) => {
  selectedValue.value = newVal || '';
});

// Recarregar opções quando os filtros mudarem (exceto type)
watch([
  () => filters.name,
  () => filters.rarity,
  () => filters.startDate,
  () => filters.endDate,
  () => filters.onlyNew,
  () => filters.onlyInShop,
  () => filters.onlyOnSale,
  () => filters.onlyOwned,
  () => filters.onlyBundle,
  () => filters.minPrice,
  () => filters.maxPrice,
], () => {
  loadFilterOptions();
}, { deep: true });

onMounted(() => {
  loadFilterOptions();
});
</script>

<style scoped>
.filter-container {
  background-color: #1e1e1e; 
  color: white;
  width: 300px; 
  border-radius: 12px;
  padding: 16px;
  font-family: sans-serif;
  user-select: none; 
}
.filter-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  margin-bottom: 12px;
}
.filter-header h3 {
  margin: 0;
  font-size: 1rem;
  font-weight: 600;
}
.toggle-icon {
  font-size: 1.5rem;
  line-height: 1;
  color: #cccccc;
}

.filter-list {
  list-style: none;
  padding: 0;
  margin: 0;
  max-height: 260px; 
  overflow-y: auto;  
  padding-right: 8px; 
}

.filter-list::-webkit-scrollbar {
  width: 4px;
}
.filter-list::-webkit-scrollbar-track {
  background: transparent;
}
.filter-list::-webkit-scrollbar-thumb {
  background-color: #444;
  border-radius: 4px;
}

.filter-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 6px 0;
  cursor: pointer;
}
.filter-item:hover .label {
  color: #fff;
}

.left-content {
  display: flex;
  align-items: center;
  gap: 12px;
}

.checkbox {
  width: 18px;
  height: 18px;
  border-radius: 4px;
  border: 1px solid #555; 
  background-color: transparent;
  transition: all 0.2s ease;
}

.checkbox.is-active {
  background-color: #00E0FF; 
  border-color: #00E0FF;
  box-shadow: 0 0 5px rgba(0, 224, 255, 0.4);
}

.label {
  font-size: 0.9rem;
  color: #cccccc;
}
.count {
  font-size: 0.9rem;
  color: #888; 
}

.checkbox.is-active + .label {
  color: white;
}

.loading-message,
.empty-message {
  padding: 12px;
  text-align: center;
  color: #888;
  font-size: 0.9rem;
}
</style>