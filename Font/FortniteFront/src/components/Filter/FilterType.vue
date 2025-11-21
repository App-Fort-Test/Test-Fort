<template>
  <div class="filter-container">
    <div class="filter-header" @click="isOpen = !isOpen">
      <h3>Raridade</h3>
      <span class="toggle-icon">{{ isOpen ? '−' : '+' }}</span>
    </div>

    <transition name="slide-fade">
      <ul v-if="isOpen" class="filter-list">
        <li 
          v-for="item in rarities" 
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
    </transition>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';

const props = defineProps({
  modelValue: {
    type: String,
    default: ''
  }
});

const emit = defineEmits(['update:modelValue']);

const isOpen = ref(false);
const selectedValue = ref(props.modelValue || '');

// Raridades (ajustar conforme API)
const rarities = ref([
  { value: 'common', label: 'Comum', count: 0 },
  { value: 'uncommon', label: 'Incomum', count: 0 },
  { value: 'rare', label: 'Raro', count: 0 },
  { value: 'epic', label: 'Épico', count: 0 },
  { value: 'legendary', label: 'Lendário', count: 0 },
  { value: 'mythic', label: 'Mítico', count: 0 },
]);

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
</style>
