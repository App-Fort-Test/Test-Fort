<template>
    <div class="FilterMaster">
        <FilterSearch :modelValue="filters.name" @update:modelValue="updateFilter('name', $event)" />
        <button class="clear-filters-button" @click="clearFilters">
            Limpar Filtros
        </button>
        <FilterPromotion :modelValue="filters.onlyOnSale" @update:modelValue="updateFilter('onlyOnSale', $event)" />
        <FilterDateWithPicker 
            :startDate="filters.startDate" 
            :endDate="filters.endDate"
            @update:startDate="updateFilter('startDate', $event)"
            @update:endDate="updateFilter('endDate', $event)"
        />
        <FilterPrice 
            :minPrice="filters.minPrice" 
            :maxPrice="filters.maxPrice"
            @update:minPrice="updateFilter('minPrice', $event)"
            @update:maxPrice="updateFilter('maxPrice', $event)"
        />
        <FilterCategory 
            :modelValue="filters.type" 
            @update:modelValue="updateFilter('type', $event)" 
        />
        <FilterType 
            :modelValue="filters.rarity" 
            @update:modelValue="updateFilter('rarity', $event)" 
        />
    </div>
</template>

<script setup>
import { watch } from 'vue';
import FilterCategory from '@/components/Filter/FilterCategory.vue';
import FilterPrice from '@/components/Filter/FilterPrice.vue';
import FilterDateWithPicker from '@/components/Filter/FilterDateWithPicker.vue';
import FilterPromotion from '@/components/Filter/FilterPromotion.vue';
import FilterSearch from '@/components/Filter/FilterSearch.vue';
import FilterType from '@/components/Filter/FilterType.vue';
import { useCosmetics } from '@/composables/useCosmetics';

const { filters, clearFilters } = useCosmetics();

const updateFilter = (key, value) => {
    filters[key] = value;
};
</script>

<style scoped>
.FilterMaster {
    display: flex;
    flex-direction: column;
    gap: 16px;
}

.clear-filters-button {
    background: linear-gradient(162deg, #f44336 22.61%, #e91e63 118.29%);
    border: 2px solid #f44336;
    border-radius: 12px;
    color: white;
    padding: 16px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    transition: all 0.2s;
    width: 336px;
    text-align: center;
    font-family: sans-serif;
}

.clear-filters-button:hover {
    background: linear-gradient(162deg, #d32f2f 22.61%, #c2185b 118.29%);
    transform: translateY(-1px);
    box-shadow: 0 4px 12px rgba(244, 67, 54, 0.3);
}

.clear-filters-button:active {
    transform: translateY(0);
}
</style>