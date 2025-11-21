<template>
  <div class="pagination-master">
    <FilterTabs :modelValue="props.selectedTab" @update:modelValue="handleTabChange" />

    <div class="pagination-controls">
      <div class="p-container">
        <p class="p">Quantidade de resultados: {{ totalItems }}</p>
      </div>
      <div class="controls-group">
        <FilterPagination 
          :currentPage="props.currentPage" 
          :totalPages="totalPages"
          @change="handlePageChange"
        />
      </div>
      <div class="right-controls">
        <FilterStar 
          :modelValue="sortBy" 
          @update:modelValue="updateSort" 
        />
        <FilterPageSize 
          :modelValue="pageSize" 
          @update:modelValue="updatePageSize" 
        />
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue';
import FilterTabs from './FilterTabs.vue';
import FilterPagination from './FilterPagination.vue';
import FilterPageSize from './FilterPageSize.vue';
import FilterStar from './FilterStar.vue';

const props = defineProps({
    currentPage: {
        type: Number,
        default: 1,
    },
    pageSize: {
        type: Number,
        default: 20,
    },
    totalItems: {
        type: Number,
        default: 0,
    },
    sortBy: {
        type: String,
        default: '',
    },
    selectedTab: {
        type: String,
        default: 'todos',
    },
});

const emit = defineEmits(['update:currentPage', 'update:pageSize', 'update:sortBy', 'update:selectedTab']);

const totalPages = computed(() => {
    return Math.ceil(props.totalItems / props.pageSize) || 1;
});

const handlePageChange = (page) => {
    emit('update:currentPage', page);
};

const updatePageSize = (size) => {
    emit('update:pageSize', size);
};

const updateSort = (sort) => {
    emit('update:sortBy', sort);
};

const handleTabChange = (tab) => {
    emit('update:selectedTab', tab);
};
</script>

<style scoped>
.pagination-master {
  display: flex;
  flex-direction: column;
  gap: 16px;
  width: 100%;
}

.pagination-controls {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  flex-wrap: nowrap;
  width: 100%;
}

.p-container {
  flex-shrink: 0;
}

.p {
  margin: 0;
  white-space: nowrap;
}

.controls-group {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  flex-wrap: wrap;
  flex: 1;
}

.right-controls {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 16px;
  flex-wrap: nowrap;
  flex-shrink: 0;
}

@media (max-width: 768px) {
  .pagination-controls {
    flex-direction: column;
    align-items: flex-start;
    flex-wrap: wrap;
  }

  .controls-group {
    width: 100%;
    justify-content: center;
  }
  
  .right-controls {
    width: 100%;
    justify-content: center;
  }
}
</style>

