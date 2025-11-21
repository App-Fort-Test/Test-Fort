<template>
  <div class="promo-container" @click="toggle">
    <span class="label">Itens em Promoção</span>

    <div class="switch-track" :class="{ 'is-active': isActive }">
      <div class="switch-thumb"></div>
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: false
  }
});
const emit = defineEmits(['update:modelValue']);
const isActive = ref(props.modelValue);

const toggle = () => {
  isActive.value = !isActive.value;
  emit('update:modelValue', isActive.value);
};

watch(() => props.modelValue, (val) => {
  isActive.value = val;
});
</script>

<style scoped>
.promo-container {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background-color: #1e1e1e;
  width: 300px;
  padding: 16px;
  border-radius: 12px;
  cursor: pointer;
  user-select: none;
  font-family: sans-serif;
}
.label {
  color: white;
  font-weight: 600;
  font-size: 1rem;
}

.switch-track {
  width: 44px;
  height: 24px;
  background-color: #444; 
  border-radius: 20px;
  position: relative;
  transition: background-color 0.3s ease;
}

.switch-track.is-active {
  background-color: #00E0FF; 
}

.switch-thumb {
  width: 20px;
  height: 20px;
  background-color: white;
  border-radius: 50%;
  position: absolute;
  top: 2px;
  left: 2px;
  transition: transform 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
  box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}

.switch-track.is-active .switch-thumb {
  transform: translateX(20px); 
}
</style>