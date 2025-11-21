<template>
  <div class="filter-bundle">
    <label class="filter-label">
      <input 
        type="checkbox" 
        :checked="modelValue"
        @change="$emit('update:modelValue', $event.target.checked)"
        class="checkbox-input"
      />
      <span class="checkbox-label">Apenas Bundles</span>
    </label>
  </div>
</template>

<script setup>
defineProps({
  modelValue: {
    type: Boolean,
    default: false
  }
});

defineEmits(['update:modelValue']);
</script>

<style scoped>
.filter-bundle {
  display: flex;
  align-items: center;
  padding: 12px 16px;
  background: linear-gradient(162deg, #161A42 22.61%, rgba(22, 26, 66, 0) 118.29%);
  border: 2px solid #212554;
  border-radius: 12px;
  transition: all 0.2s;
}

.filter-bundle:hover {
  border-color: #00E0FF;
}

.filter-label {
  display: flex;
  align-items: center;
  gap: 12px;
  cursor: pointer;
  width: 100%;
  user-select: none;
}

.checkbox-input {
  width: 20px;
  height: 20px;
  cursor: pointer;
  accent-color: #00E0FF;
}

.checkbox-label {
  color: #fff;
  font-size: 0.95rem;
  font-weight: 500;
  font-family: sans-serif;
}

.filter-label:hover .checkbox-label {
  color: #00E0FF;
}
</style>

