<template>
    <div class="page-size-container" ref="containerRef">
        <div class="dropdown-trigger" @click="isOpen = !isOpen">
            <span class="highlight">{{ modelValue }}</span>
            <span class="label-text">Itens por página</span>

            <svg class="arrow-icon" :class="{ 'is-open': isOpen }" width="16" height="16" viewBox="0 0 24 24"
                fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                <polyline points="6 9 12 15 18 9"></polyline>
            </svg>
        </div>

        <transition name="fade">
            <ul v-if="isOpen" class="dropdown-menu">
                <li v-for="option in options" :key="option" class="dropdown-item"
                    :class="{ 'is-selected': modelValue === option }" @click="selectOption(option)">
                    {{ option }} Itens
                </li>
            </ul>
        </transition>
    </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue';

const props = defineProps({
  modelValue: { type: Number, default: 50 }
});
const emit = defineEmits(['update:modelValue']);

const isOpen = ref(false);
const containerRef = ref(null);
const options = [10, 20, 50, 100];

const selectOption = (val) => {
  emit('update:modelValue', val);
  isOpen.value = false;
};

const handleClickOutside = (e) => {
  if (containerRef.value && !containerRef.value.contains(e.target)) {
    isOpen.value = false;
  }
};

onMounted(() => document.addEventListener('click', handleClickOutside));
onUnmounted(() => document.removeEventListener('click', handleClickOutside));
</script>

<style scoped>
.page-size-container {
    position: relative;
    display: inline-block;
    font-family: sans-serif;
    user-select: none;
}

.dropdown-trigger {
    background-color: #0c0e24;
    color: #aaa;
    padding: 10px 20px;
    border-radius: 999px;
    align-items: center;
    gap: 6px;
    cursor: pointer;
    transition: background 0.2s;
    border: 1px solid transparent;
}

.dropdown-trigger:hover {
    border-color: #333;
}

.highlight {
    color: white;
    font-weight: 700;
    font-size: 1rem;
    gap: 8px;
}

.label-text {
    margin-left: 8px;
    font-size: 0.9rem;
    font-weight: 400;
}

.arrow-icon {
    margin-left: 4px;
    transition: transform 0.3s;
}

.arrow-icon.is-open {
    transform: rotate(180deg);
}

.dropdown-menu {
    position: absolute;
    top: 110%;
    left: 0;
    width: 100%;
    background-color: #1e1e1e;
    border: 1px solid #333;
    border-radius: 12px;
    padding: 8px 0;
    list-style: none;
    z-index: 50;
    box-shadow: 0 4px 15px rgba(0, 0, 0, 0.5);
}

.dropdown-item {
    padding: 8px 16px;
    color: #ccc;
    cursor: pointer;
    font-size: 0.9rem;
    transition: all 0.2s;
}

.dropdown-item:hover {
    background-color: #333;
    color: white;
}

.dropdown-item.is-selected {
    color: #00E0FF;
    font-weight: bold;
}

.fade-enter-active,
.fade-leave-active {
    transition: opacity 0.2s;
}

.fade-enter-from,
.fade-leave-to {
    opacity: 0;
}
</style>