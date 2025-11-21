<template>
  <div class="date-filter-container" ref="containerRef">
    <div class="filter-header" @click="isOpen = !isOpen">
      <h3>Data de Inclusão</h3>
      <span class="toggle-icon">{{ isOpen ? '−' : '+' }}</span>
    </div>


    <transition name="slide-fade">
      <div v-if="isOpen" class="filter-body">


        <div class="inputs-row">


          <div 
            class="date-box" 
            :class="{ 'is-active': showPicker && activeField === 'start' }"
            @click="openPicker('start')"
          >
            <svg class="calendar-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="16" y1="2" x2="16" y2="6"></line>
              <line x1="8" y1="2" x2="8" y2="6"></line>
              <line x1="3" y1="10" x2="21" y2="10"></line>
            </svg>
            <span class="placeholder">{{ formattedStartDate || 'Inicio' }}</span>
          </div>


          <span class="separator">-</span>


          <div 
            class="date-box"
            :class="{ 'is-active': showPicker && activeField === 'end' }"
            @click="openPicker('end')"
          >
            <svg class="calendar-icon" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="16" y1="2" x2="16" y2="6"></line>
              <line x1="8" y1="2" x2="8" y2="6"></line>
              <line x1="3" y1="10" x2="21" y2="10"></line>
            </svg>
            <span class="placeholder">{{ formattedEndDate || 'Fim' }}</span>
          </div>
        </div>


        <transition name="fade">
          <div v-if="showPicker" class="picker-popup">


            <div class="picker-header">
              <button @click.stop="changeMonth(-1)" class="nav-btn">&lt;</button>
              <span class="current-month">{{ monthNames[currentMonth] }} {{ currentYear }}</span>
              <button @click.stop="changeMonth(1)" class="nav-btn">&gt;</button>
            </div>


            <div class="weekdays-grid">
              <span v-for="day in weekDays" :key="day">{{ day }}</span>
            </div>


            <div class="days-grid">
              <div 
                v-for="n in startDayOffset" 
                :key="'empty-' + n" 
                class="day-cell empty"
              ></div>


              <div 
                v-for="day in daysInMonth" 
                :key="day"
                class="day-cell"
                :class="{ 
                  'is-selected': isSelected(day),
                  'is-today': isToday(day)
                }"
                @click.stop="selectDate(day)"
              >
                {{ day }}
              </div>
            </div>


          </div>
        </transition>


      </div>
    </transition>
  </div>
</template>


<script setup>
import { ref, computed, onMounted, onUnmounted, watch } from 'vue';

const props = defineProps({
  startDate: {
    type: Date,
    default: null
  },
  endDate: {
    type: Date,
    default: null
  }
});

const emit = defineEmits(['update:startDate', 'update:endDate']);

const isOpen = ref(false); // Alterado de true para false
const showPicker = ref(false);
const activeField = ref(null); 
const containerRef = ref(null);

const startDate = ref(props.startDate);
const endDate = ref(props.endDate);


const now = new Date();
const currentMonth = ref(now.getMonth());
const currentYear = ref(now.getFullYear());

const weekDays = ['D', 'S', 'T', 'Q', 'Q', 'S', 'S'];
const monthNames = ['Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho', 'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'];



const daysInMonth = computed(() => {
  return new Date(currentYear.value, currentMonth.value + 1, 0).getDate();
});


const startDayOffset = computed(() => {
  return new Date(currentYear.value, currentMonth.value, 1).getDay();
});


const openPicker = (field) => {

  if (showPicker.value && activeField.value === field) {
    showPicker.value = false;
    return;
  }
  activeField.value = field;
  showPicker.value = true;
  

  const dateToSync = field === 'start' ? startDate.value : endDate.value;
  if (dateToSync) {
    currentMonth.value = dateToSync.getMonth();
    currentYear.value = dateToSync.getFullYear();
  }
};


const changeMonth = (step) => {
  let nextMonth = currentMonth.value + step;
  if (nextMonth > 11) {
    currentMonth.value = 0;
    currentYear.value++;
  } else if (nextMonth < 0) {
    currentMonth.value = 11;
    currentYear.value--;
  } else {
    currentMonth.value = nextMonth;
  }
};


const selectDate = (day) => {
  const newDate = new Date(currentYear.value, currentMonth.value, day);
  
  if (activeField.value === 'start') {
    startDate.value = newDate;
    emit('update:startDate', newDate);
    if (endDate.value && endDate.value < newDate) {
      endDate.value = null;
      emit('update:endDate', null);
    }
    activeField.value = 'end';
  } else {
    endDate.value = newDate;
    emit('update:endDate', newDate);
    if (startDate.value && newDate < startDate.value) {
      startDate.value = newDate;
      emit('update:startDate', newDate);
      endDate.value = null;
      emit('update:endDate', null);
    }
    showPicker.value = false;
  }
};



const isSelected = (day) => {
  const targetDate = activeField.value === 'start' ? startDate.value : endDate.value;
  if (!targetDate) return false;
  return targetDate.getDate() === day && 
         targetDate.getMonth() === currentMonth.value && 
         targetDate.getFullYear() === currentYear.value;
};

const isToday = (day) => {
  const today = new Date();
  return day === today.getDate() && 
         currentMonth.value === today.getMonth() && 
         currentYear.value === today.getFullYear();
};

const formatDate = (date) => {
  if (!date) return null;
  const d = String(date.getDate()).padStart(2, '0');
  const m = String(date.getMonth() + 1).padStart(2, '0');
  const y = date.getFullYear();
  return `${d}/${m}/${y}`;
};

const formattedStartDate = computed(() => formatDate(startDate.value));
const formattedEndDate = computed(() => formatDate(endDate.value));


const handleClickOutside = (e) => {
  if (containerRef.value && !containerRef.value.contains(e.target)) {
    showPicker.value = false;
    activeField.value = null;
  }
};

onMounted(() => document.addEventListener('click', handleClickOutside));
onUnmounted(() => document.removeEventListener('click', handleClickOutside));

watch(() => props.startDate, (newVal) => {
  startDate.value = newVal;
});

watch(() => props.endDate, (newVal) => {
  endDate.value = newVal;
});

</script>


<style scoped>

.date-filter-container {
  position: relative; 
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
  margin-bottom: 15px;
}

.filter-header h3 { margin: 0; font-size: 1rem; font-weight: 600; }
.toggle-icon { font-size: 1.5rem; color: #ccc; }


.inputs-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}

.date-box {
  flex: 1;
  height: 45px;
  background-color: #0c0e24; 
  border-radius: 12px;
  border: 1px solid transparent;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: inset 0 2px 4px rgba(0,0,0,0.5);
}

.date-box:hover { border-color: #333; }
.date-box.is-active { border-color: #00E0FF; box-shadow: 0 0 8px rgba(0, 224, 255, 0.15); }

.calendar-icon { color: #fff; }
.placeholder { font-size: 0.9rem; font-weight: 500; color: #fff; }
.separator { color: #888; font-size: 1.2rem; }


.picker-popup {
  position: absolute;
  top: 100%; 
  left: 0;
  right: 0;
  margin-top: 10px;
  background-color: #252525; 
  border: 1px solid #333;
  border-radius: 12px;
  padding: 16px;
  z-index: 50; 
  box-shadow: 0 10px 25px rgba(0,0,0,0.5);
}


.picker-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.current-month { font-weight: bold; color: white; }
.nav-btn {
  background: none;
  border: none;
  color: #aaa;
  font-size: 1.2rem;
  cursor: pointer;
  padding: 5px 10px;
  border-radius: 4px;
}
.nav-btn:hover { background-color: #333; color: white; }


.weekdays-grid, .days-grid {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  text-align: center;
}

.weekdays-grid {
  margin-bottom: 8px;
  font-size: 0.75rem;
  color: #888;
  font-weight: bold;
}


.day-cell {
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.9rem;
  border-radius: 6px;
  cursor: pointer;
  color: #ddd;
  transition: background 0.2s;
}

.day-cell:not(.empty):hover {
  background-color: #333;
}


.day-cell.is-selected {
  background-color: #00E0FF;
  color: #000; 
  font-weight: bold;
  box-shadow: 0 0 10px rgba(0, 224, 255, 0.4);
}

.day-cell.is-today:not(.is-selected) {
  border: 1px solid #00E0FF;
  color: #00E0FF;
}

.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>