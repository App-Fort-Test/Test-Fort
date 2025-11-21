<script setup>
import { ref, computed } from 'vue';
import CardCompleto from './components/Modal/Cosmetics/CardCompleto.vue';
import Header from './components/Header/Header.vue';
import FilterMaster from './components/Filter/FilterMaster.vue';
import Title from './components/Title.vue';
import UsersTable from './components/Users/UsersTable.vue';
import TransactionHistory from './components/Transactions/TransactionHistory.vue';

const rarities = ['common', 'uncommon', 'rare', 'epic', 'legendary'];
const activeRoute = ref('loja');


const setActiveRoute = (route) => {
  activeRoute.value = route;
};


defineExpose({ setActiveRoute });
</script>

<template>
  <Header :active-route="activeRoute" @route-change="setActiveRoute" />
  <Title />

  <div v-if="activeRoute === 'loja'" class="main-container">
    <aside class="filters-sidebar">
      <FilterMaster />
    </aside>
    
    <main class="content-area">
      <CardCompleto />
    </main>
  </div>

  <div v-else-if="activeRoute === 'usuarios'" class="users-container">
    <UsersTable />
  </div>

  <div v-else-if="activeRoute === 'historico'" class="history-container">
    <TransactionHistory />
  </div>
</template>

<style scoped>
.main-container {
  display: flex;
  gap: 24px;
  padding: 16px 64px 16px 40px; 
  box-sizing: border-box;
  width: 100%;
  min-height: calc(100vh - 200px);
}

.users-container,
.history-container {
  padding: 16px 64px 16px 40px; /* padding-left: 40px (mesmo do Title), padding-right: 64px (mesmo do Header) */
  width: 100%;
  min-height: calc(100vh - 200px);
  box-sizing: border-box;
}

.filters-sidebar {
  flex-shrink: 0;
  width: 320px;
  min-width: 280px;
}

.content-area {
  flex: 1;
  min-width: 0;
  width: 100%;
  margin-left: 16px;
}

@media (max-width: 1024px) {
  .main-container {
    flex-direction: column;
    padding: 16px 24px;
  }

  .filters-sidebar {
    width: 100%;
  }

  .users-container {
    padding: 16px 24px;
  }
}

@media (max-width: 768px) {
  .main-container {
    padding: 16px;
    gap: 16px;
  }

  .users-container {
    padding: 16px;
  }
}
</style>
