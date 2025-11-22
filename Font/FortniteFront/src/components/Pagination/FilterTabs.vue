<template>
  <div class="tabs-container">
    <div 
      v-for="tab in availableTabs" 
      :key="tab.value"
      class="tab-item"
      :class="{ 'is-active': modelValue === tab.value }"
      @click="$emit('update:modelValue', tab.value)"
    >
      {{ tab.label }}
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue';
import { useAuth } from '../../composables/useAuth';

defineProps({
  modelValue: {
    type: String,
    default: 'todos'
  }
});

defineEmits(['update:modelValue']);

const { user } = useAuth();

const allTabs = [
  { label: 'Todos', value: 'todos' },
  { label: 'Novos', value: 'novos' },
  { label: 'Promoção', value: 'promocao' },
  { label: 'Bundle', value: 'bundle' },
  { label: 'Possuido', value: 'possuido' }
];

const availableTabs = computed(() => {
  if (user.value) {
    return allTabs;
  }
  return allTabs.filter(tab => tab.value !== 'possuido');
});
</script>

<style scoped>
.tabs-container {
  display: inline-flex;
  background: linear-gradient(
    162deg,
    #161a42 22.61%,
    rgba(22, 26, 66, 0) 118.29%
  );
  border-radius: 40px; 
  border: 2px solid #212554;
  user-select: none;
  gap: 4px; 
  width: 100%;
  height: 48px;
  align-items: center;
}

.tab-item {
  padding: 8px 24px;
  border-radius: 999px;
  font-family: sans-serif;
  font-size: 0.95rem;
  font-weight: 600;
  color: #fff;
  cursor: pointer;
  transition: all 0.3s ease;
}

.tab-item:hover:not(.is-active) {
  color: #ccc;
  background-color: rgba(255, 255, 255, 0.05);
}

.tab-item.is-active {
  border-radius: 40px;
  border: 2px solid #00E0FF;
  padding: 8px 24px;
  background-color: #00E0FF;
  color: #000; 
  box-shadow: 0 2px 10px rgba(0, 224, 255, 0.3);
  justify-content: center;
  margin-left: 8px;
  gap: 16px;
}
</style>