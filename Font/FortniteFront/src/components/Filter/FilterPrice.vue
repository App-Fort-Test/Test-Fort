<template>
  <div class="price-filter-container">
    <div class="filter-header" @click="isOpen = !isOpen">
      <h3>Preço</h3>
      <div class="icon-wrapper">
        <span v-if="isOpen" class="dash-icon"></span>
        <span v-else class="plus-icon">+</span>
      </div>
    </div>

    <transition name="fade">
      <div v-show="isOpen" class="filter-body">
        <div class="price-inputs">
          <div class="input-group">
            <label>Preço Inicial</label>
            <input 
              type="number" 
              v-model.number="localMinPrice" 
              placeholder="0"
              min="0"
              max="100000"
              class="price-input"
              @input="handleMinPriceChange"
              @blur="validateMinPrice"
            />
            <span class="vbucks-label">V-Bucks</span>
          </div>
          
          <div class="input-group">
            <label>Preço Final</label>
            <input 
              type="number" 
              v-model.number="localMaxPrice" 
              placeholder="10000"
              min="0"
              max="100000"
              class="price-input"
              @input="handleMaxPriceChange"
              @blur="validateMaxPrice"
            />
            <span class="vbucks-label">V-Bucks</span>
          </div>
        </div>
      </div>
    </transition>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue';

const props = defineProps({
  minPrice: {
    type: Number,
    default: null
  },
  maxPrice: {
    type: Number,
    default: null
  }
});

const emit = defineEmits(['update:minPrice', 'update:maxPrice']);

const isOpen = ref(false);
const localMinPrice = ref(props.minPrice ?? null);
const localMaxPrice = ref(props.maxPrice ?? null);

const handleMinPriceChange = () => {
  emit('update:minPrice', localMinPrice.value);
};

const handleMaxPriceChange = () => {
  emit('update:maxPrice', localMaxPrice.value);
};

const validateMinPrice = () => {
  if (localMinPrice.value !== null && localMinPrice.value < 0) {
    localMinPrice.value = 0;
    emit('update:minPrice', 0);
  }
  if (localMinPrice.value !== null && localMaxPrice.value !== null && localMinPrice.value > localMaxPrice.value) {
    localMinPrice.value = localMaxPrice.value;
    emit('update:minPrice', localMaxPrice.value);
  }
};

const validateMaxPrice = () => {
  if (localMaxPrice.value !== null && localMaxPrice.value < 0) {
    localMaxPrice.value = 0;
    emit('update:maxPrice', 0);
  }
  if (localMinPrice.value !== null && localMaxPrice.value !== null && localMaxPrice.value < localMinPrice.value) {
    localMaxPrice.value = localMinPrice.value;
    emit('update:maxPrice', localMinPrice.value);
  }
};

watch(() => props.minPrice, (newVal) => {
  if (newVal !== localMinPrice.value) {
    localMinPrice.value = newVal ?? null;
  }
});

watch(() => props.maxPrice, (newVal) => {
  if (newVal !== localMaxPrice.value) {
    localMaxPrice.value = newVal ?? null;
  }
});
</script>

<style scoped>
.price-filter-container {
  background-color: #1e1e1e;
  color: white;
  width: 300px;
  border-radius: 12px;
  padding: 16px;
  font-family: sans-serif;
  user-select: none;
  padding-top: 20px; 
}

.filter-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  cursor: pointer;
  margin-bottom: 15px; 
}

.filter-header h3 {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 600;
}

.dash-icon {
  display: block;
  width: 14px;
  height: 2px;
  background-color: white;
  border-radius: 1px;
}

.plus-icon {
  font-size: 1.2rem;
  font-weight: bold;
}

.price-inputs {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.input-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.input-group label {
  font-size: 0.9rem;
  font-weight: 500;
  color: #ccc;
}

.price-input {
  background-color: #2a2a2a;
  border: 1px solid #444;
  border-radius: 8px;
  padding: 10px 12px;
  color: white;
  font-size: 1rem;
  outline: none;
  transition: all 0.2s;
  width: 100%;
  box-sizing: border-box;
}

.price-input:focus {
  border-color: #00C2E0;
  box-shadow: 0 0 0 2px rgba(0, 194, 224, 0.2);
}

.price-input::placeholder {
  color: #666;
}

.price-input::-webkit-inner-spin-button,
.price-input::-webkit-outer-spin-button {
  -webkit-appearance: none;
  margin: 0;
}

.price-input[type=number] {
  -moz-appearance: textfield;
}

.vbucks-label {
  font-size: 0.85rem;
  color: #888;
  margin-top: -4px;
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s, max-height 0.3s;
  max-height: 500px;
  overflow: hidden;
}

.fade-enter-from, .fade-leave-to {
  opacity: 0;
  max-height: 0;
}
</style>