<template>
  <div class="search-wrapper">
    <svg 
      class="search-icon" 
      width="20" 
      height="20" 
      viewBox="0 0 24 24" 
      fill="none" 
      stroke="currentColor" 
      stroke-width="2" 
      stroke-linecap="round" 
      stroke-linejoin="round"
    >
      <circle cx="11" cy="11" r="8"></circle>
      <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
    </svg>


    <input 
      type="text" 
      class="search-input"
      placeholder="Nome" 
      v-model="searchTerm"
      @input="$emit('update:modelValue', searchTerm)"
    />
  </div>
</template>


<script setup>
import { ref, watch } from 'vue';

const props = defineProps(['modelValue']);
const emit = defineEmits(['update:modelValue']);

const searchTerm = ref(props.modelValue || '');

watch(() => props.modelValue, (newVal) => {
  searchTerm.value = newVal;
});
</script>


<style scoped>
.search-wrapper {
  display: flex;
  align-items: center;
  background-color: #1e1e1e; 
  border-radius: 12px; 
  padding: 12px 16px;
  width: 300px; 
  transition: box-shadow 0.2s, border-color 0.2s;
  border: 1px solid transparent;
}

.search-wrapper:focus-within {
  border-color: #555;
  box-shadow: 0 0 0 2px rgba(255, 255, 255, 0.05);
}

.search-icon {
  color: #888;
  margin-right: 12px;
}

.search-input {
  flex: 1; 
  background: transparent;
  border: none;
  outline: none;
  color: white;
  font-size: 1rem;
  font-family: sans-serif;
}


.search-input::placeholder {
  color: #666;
  font-weight: 500;
}
</style>