<template>
  <Transition name="modal">
    <div v-if="isVisible" class="modal-overlay" @click.self="handleCancel">
      <div class="modal-container">
        <div class="modal-header">
          <h3 class="modal-title">{{ title }}</h3>
          <button class="modal-close" @click="handleCancel">
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <line x1="18" y1="6" x2="6" y2="18"></line>
              <line x1="6" y1="6" x2="18" y2="18"></line>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <p class="modal-message">{{ message }}</p>
        </div>
        <div class="modal-footer">
          <button class="btn btn-cancel" @click="handleCancel">{{ cancelText }}</button>
          <button class="btn btn-confirm" @click="handleConfirm">{{ confirmText }}</button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, watch } from 'vue';

const props = defineProps({
  isVisible: {
    type: Boolean,
    default: false
  },
  title: {
    type: String,
    default: 'Confirmar'
  },
  message: {
    type: String,
    required: true
  },
  confirmText: {
    type: String,
    default: 'Confirmar'
  },
  cancelText: {
    type: String,
    default: 'Cancelar'
  }
});

const emit = defineEmits(['confirm', 'cancel', 'update:isVisible']);

const handleConfirm = () => {
  emit('confirm');
  emit('update:isVisible', false);
};

const handleCancel = () => {
  emit('cancel');
  emit('update:isVisible', false);
};
</script>

<style scoped>
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.8);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10000;
  backdrop-filter: blur(8px);
  padding: 20px;
}

.modal-container {
  background: linear-gradient(135deg, #161A42 0%, #1a1a1a 100%);
  border-radius: 16px;
  padding: 0;
  max-width: 500px;
  width: 90%;
  box-shadow: 0 20px 60px rgba(0, 0, 0, 0.7);
  border: 2px solid #312E81;
  overflow: hidden;
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 2px solid #312E81;
  background: rgba(22, 26, 66, 0.5);
}

.modal-title {
  margin: 0;
  font-size: 20px;
  font-weight: 700;
  color: #00E0FF;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.modal-close {
  background: transparent;
  border: 2px solid transparent;
  border-radius: 6px;
  color: rgba(255, 255, 255, 0.6);
  cursor: pointer;
  padding: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
  width: 32px;
  height: 32px;
}

.modal-close:hover {
  color: #00E0FF;
  border-color: #312E81;
  background: rgba(0, 224, 255, 0.1);
}

.modal-close svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  padding: 24px;
  background: rgba(26, 26, 26, 0.3);
}

.modal-message {
  margin: 0;
  font-size: 16px;
  line-height: 1.6;
  color: #ffffff;
  text-align: center;
}

.modal-footer {
  display: flex;
  gap: 12px;
  padding: 20px 24px;
  border-top: 2px solid #312E81;
  justify-content: flex-end;
  background: rgba(22, 26, 66, 0.3);
}

.btn {
  padding: 12px 28px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.3s ease;
  border: 2px solid;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  min-width: 120px;
}

.btn-cancel {
  background: transparent;
  color: #ffffff;
  border-color: #312E81;
}

.btn-cancel:hover {
  background: rgba(49, 46, 129, 0.3);
  border-color: #00E0FF;
  color: #00E0FF;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 224, 255, 0.2);
}

.btn-confirm {
  background: linear-gradient(162deg, #161A42 22.61%, rgba(22, 26, 66, 0) 118.29%);
  border-color: #312E81;
  color: #00E0FF;
}

.btn-confirm:hover {
  background: linear-gradient(162deg, #1fa2ff 22.61%, #12d8fa 118.29%);
  border-color: #12D8FA;
  color: #000;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(18, 216, 250, 0.4);
}

.modal-enter-active,
.modal-leave-active {
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}

.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}

.modal-enter-from .modal-container,
.modal-leave-to .modal-container {
  transform: scale(0.9) translateY(-20px);
}

@media (max-width: 640px) {
  .modal-container {
    width: 95%;
    max-width: none;
  }
  
  .modal-header {
    padding: 16px 20px;
  }
  
  .modal-title {
    font-size: 18px;
  }
  
  .modal-body {
    padding: 20px;
  }
  
  .modal-message {
    font-size: 14px;
  }
  
  .modal-footer {
    flex-direction: column-reverse;
    padding: 16px 20px;
  }
  
  .btn {
    width: 100%;
    min-width: auto;
  }
}
</style>

