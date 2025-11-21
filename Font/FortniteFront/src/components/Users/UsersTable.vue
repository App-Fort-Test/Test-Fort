<template>
  <div class="users-table-container">
    <div class="table-header">
      <h2 class="table-title">Lista de Usuários</h2>
      <div v-if="loading" class="loading">Carregando...</div>
    </div>
    
    <div v-if="totalPages > 1" class="pagination-controls">
      <button @click="currentPage > 1 && (currentPage--, loadUsers())" :disabled="currentPage === 1">
        Anterior
      </button>
      <span>Página {{ currentPage }} de {{ totalPages }}</span>
      <button @click="currentPage < totalPages && (currentPage++, loadUsers())" :disabled="currentPage === totalPages">
        Próxima
      </button>
    </div>

    <!-- Versão Desktop: Tabela -->
    <div class="table-wrapper desktop-view">
      <table class="users-table">
        <thead>
          <tr>
            <th>Nome</th>
            <th>Email</th>
            <th>Transações</th>
            <th>Itens</th>
            <th>Valor Total</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <template v-for="user in users" :key="user.id">
            <tr 
              :class="{ 'is-expanded': expandedUser === user.id }"
              class="user-row"
              @click="toggleUser(user.id)"
            >
              <td class="user-name">{{ user.name }}</td>
              <td class="user-email">{{ formatDate(user.createdAt) }}</td>
              <td class="transaction-count">{{ user.transactions?.length || 0 }}</td>
              <td class="items-cell" @click.stop>
                <button 
                  class="items-button"
                  @click="openItemsModal(user)"
                >
                  {{ user.totalCosmetics || 0 }} itens
                </button>
              </td>
              <td class="total-value">-</td>
              <td class="expand-cell" @click.stop>
                <button 
                  class="expand-button"
                  @click="toggleUser(user.id)"
                  :aria-expanded="expandedUser === user.id"
                >
                  <svg 
                    class="expand-icon" 
                    :class="{ 'is-expanded': expandedUser === user.id }"
                    width="20" 
                    height="20" 
                    viewBox="0 0 24 24" 
                    fill="none" 
                    stroke="currentColor" 
                    stroke-width="2"
                  >
                    <polyline points="6 9 12 15 18 9"></polyline>
                  </svg>
                </button>
              </td>
            </tr>
            
            <tr 
              v-if="expandedUser === user.id"
              class="transactions-row"
            >
              <td colspan="6" class="transactions-cell">
                <div class="transactions-container">
                  <h3 class="transactions-title">Transações de {{ user.name }}</h3>
                  <div v-if="user.transactions.length === 0" class="no-transactions">
                    Nenhuma transação encontrada.
                  </div>
                  <div v-else class="transactions-list">
                    <div 
                      v-for="transaction in user.transactions" 
                      :key="transaction.id"
                      class="transaction-item"
                    >
                      <div class="transaction-info">
                        <span class="transaction-date">{{ formatDate(transaction.date) }}</span>
                        <span class="transaction-type" :class="`type-${transaction.type}`">
                          {{ transaction.type === 'purchase' ? 'Compra' : 'Venda' }}
                        </span>
                        <span class="transaction-item-name">{{ transaction.itemName }}</span>
                      </div>
                      <div class="transaction-value">
                        <span :class="transaction.type === 'purchase' ? 'negative' : 'positive'">
                          {{ transaction.type === 'purchase' ? '-' : '+' }}{{ formatCurrency(transaction.value) }}
                        </span>
                      </div>
                    </div>
                  </div>
                </div>
              </td>
            </tr>
          </template>
        </tbody>
      </table>
    </div>

    <div class="mobile-view">
      <div 
        v-for="user in users" 
        :key="user.id"
        class="user-card"
        :class="{ 'is-expanded': expandedUser === user.id }"
      >
        <div class="user-card-header" @click="toggleUser(user.id)">
          <div class="user-card-info">
            <h3 class="user-card-name">{{ user.name }}</h3>
            <p class="user-card-email">{{ user.email }}</p>
          </div>
          <div class="user-card-stats">
            <div class="stat-item">
              <span class="stat-label">Transações</span>
              <span class="stat-value">{{ user.transactions.length }}</span>
            </div>
            <div class="stat-item">
              <span class="stat-label">Itens</span>
              <button class="items-button-mobile" @click.stop="openItemsModal(user)">
                {{ user.items.length }}
              </button>
            </div>
            <div class="stat-item">
              <span class="stat-label">Total</span>
              <span class="stat-value">{{ formatCurrency(user.totalValue) }}</span>
            </div>
          </div>
          <button class="expand-button-mobile">
            <svg 
              class="expand-icon" 
              :class="{ 'is-expanded': expandedUser === user.id }"
              width="20" 
              height="20" 
              viewBox="0 0 24 24" 
              fill="none" 
              stroke="currentColor" 
              stroke-width="2"
            >
              <polyline points="6 9 12 15 18 9"></polyline>
            </svg>
          </button>
        </div>

        <div v-if="expandedUser === user.id" class="user-card-transactions">
          <h4 class="transactions-title">Transações</h4>
          <div v-if="user.transactions.length === 0" class="no-transactions">
            Nenhuma transação encontrada.
          </div>
          <div v-else class="transactions-list">
            <div 
              v-for="transaction in user.transactions" 
              :key="transaction.id"
              class="transaction-item"
            >
              <div class="transaction-info">
                <span class="transaction-date">{{ formatDate(transaction.date) }}</span>
                <span class="transaction-type" :class="`type-${transaction.type}`">
                  {{ transaction.type === 'purchase' ? 'Compra' : 'Venda' }}
                </span>
                <span class="transaction-item-name">{{ transaction.itemName }}</span>
              </div>
              <div class="transaction-value">
                <span :class="transaction.type === 'purchase' ? 'negative' : 'positive'">
                  {{ transaction.type === 'purchase' ? '-' : '+' }}{{ formatCurrency(transaction.value) }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <BaseModal v-if="selectedUser" @close="closeItemsModal">
      <div class="items-modal-content">
        <h2 class="items-modal-title">Itens de {{ selectedUser.name }}</h2>
        
        <!-- Filtros de Busca -->
        <div class="items-filters">
          <div class="search-input-wrapper">
            <svg class="search-icon" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="11" cy="11" r="8"></circle>
              <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
            </svg>
            <input 
              type="text" 
              v-model="searchQuery"
              placeholder="Pesquisar por nome..."
              class="search-input"
            />
          </div>

          <div class="filters-row">
            <select v-model="filterType" class="filter-select">
              <option value="">Todos os Tipos</option>
              <option value="Outfit">Outfit</option>
              <option value="Emote">Emote</option>
              <option value="Harvesting Tool">Harvesting Tool</option>
              <option value="Backpack">Backpack</option>
              <option value="Glider">Glider</option>
              <option value="Wrap">Wrap</option>
            </select>

            <select v-model="filterRarity" class="filter-select">
              <option value="">Todas as Raridades</option>
              <option value="common">Comum</option>
              <option value="uncommon">Incomum</option>
              <option value="rare">Raro</option>
              <option value="epic">Épico</option>
              <option value="legendary">Lendário</option>
              <option value="mythic">Mítico</option>
            </select>
          </div>
        </div>

        <div v-if="filteredItems.length === 0" class="no-items">
          Nenhum item encontrado com os filtros aplicados.
        </div>
        <div v-else class="items-grid">
          <div 
            v-for="item in filteredItems" 
            :key="item.id"
            class="item-card-mini"
            :class="`rarity-${item.rarity}`"
          >
            <div class="card-header-mini">
              <div class="date-tag-mini" :style="{ backgroundImage: `url(${getFundoImage(item.rarity)})` }">
                {{ formatDate(item.dateAdded || '2024-01-15') }}
              </div>
              <div class="rarity-tag-mini">{{ getRarityLabel(item.rarity) }}</div>
            </div>

            <div class="image-container-mini">
              <img 
                :src="item.image || '/src/assets/png/amostracard.png'" 
                :alt="item.name"
                class="item-image-mini"
                @error="handleImageError"
              />
              <div class="overlay-tags-mini">
                <div class="price-badge-mini">{{ item.type }}</div>
              </div>
            </div>

            <div class="info-section-mini">
              <div class="item-name-mini">{{ item.name }}</div>
            </div>
          </div>
        </div>
      </div>
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue';
import { usersService } from '../../services/users';
import { transactionsService } from '../../services/transactions';
import BaseModal from '../Modal/User/BaseModal.vue';
import fundoComum from '../../assets/svg/fundocomum.svg';
import fundoIncomum from '../../assets/svg/fundoincomum.svg';
import fundoRaro from '../../assets/svg/fundoraro.svg';
import fundoEpico from '../../assets/svg/fundoepico.svg';
import fundoLegendario from '../../assets/svg/fundolegendario.svg';
import fundoMystico from '../../assets/svg/fundomystico.svg';

const expandedUser = ref(null);
const selectedUser = ref(null);
const searchQuery = ref('');
const filterType = ref('');
const filterRarity = ref('');
const users = ref([]);
const loading = ref(false);
const currentPage = ref(1);
const pageSize = ref(20);
const totalPages = ref(1);
const userCosmetics = ref({}); // Cache de cosméticos por usuário
const userTransactions = ref({}); // Cache de transações por usuário

// Carregar usuários (com cache automático)
const loadUsers = async (forceRefresh = false) => {
  loading.value = true;
  try {
    const data = await usersService.getUsers(currentPage.value, pageSize.value, forceRefresh);
    users.value = data.users.map(u => ({
      id: u.id,
      name: u.username,
      email: '', // Não expor email por segurança
      totalCosmetics: u.totalCosmetics,
      createdAt: u.createdAt,
      items: []
    }));
    totalPages.value = data.totalPages;
  } catch (error) {
    console.error('Erro ao carregar usuários:', error);
  } finally {
    loading.value = false;
  }
};

// Carregar cosméticos de um usuário (com cache automático)
const loadUserCosmetics = async (userId, forceRefresh = false) => {
  // Verificar cache em memória primeiro
  if (!forceRefresh && userCosmetics.value[userId]) {
    return userCosmetics.value[userId];
  }
  
  try {
    const data = await usersService.getUserCosmetics(userId, 1, 100, forceRefresh);
    userCosmetics.value[userId] = data.cosmetics.map(c => ({
      id: c.cosmeticId,
      name: c.cosmeticId, // Será substituído quando tivermos detalhes do cosmético
      cosmeticId: c.cosmeticId,
      dateAdded: c.acquiredAt,
      purchasePrice: c.purchasePrice
    }));
    return userCosmetics.value[userId];
  } catch (error) {
    console.error('Erro ao carregar cosméticos do usuário:', error);
    return [];
  }
};

// Carregar transações de um usuário
const loadUserTransactions = async (userId) => {
  if (userTransactions.value[userId]) {
    return userTransactions.value[userId];
  }
  
  try {
    const data = await transactionsService.getHistory(userId);
    userTransactions.value[userId] = data.transactions.map(t => ({
      id: t.id,
      date: t.createdAt,
      type: t.type.toLowerCase(),
      itemName: t.cosmeticName,
      value: Math.abs(t.amount)
    }));
    return userTransactions.value[userId];
  } catch (error) {
    console.error('Erro ao carregar transações do usuário:', error);
    return [];
  }
};

onMounted(() => {
  loadUsers();
});

const toggleUser = async (userId) => {
  if (expandedUser.value === userId) {
    expandedUser.value = null;
  } else {
    expandedUser.value = userId;
    // Carregar transações quando expandir
    const user = users.value.find(u => u.id === userId);
    if (user) {
      user.transactions = await loadUserTransactions(userId);
    }
  }
};

const openItemsModal = async (user) => {
  selectedUser.value = { ...user };
  // Carregar cosméticos do usuário
  selectedUser.value.items = await loadUserCosmetics(user.id);
  // Resetar filtros ao abrir modal
  searchQuery.value = '';
  filterType.value = '';
  filterRarity.value = '';
};

const closeItemsModal = () => {
  selectedUser.value = null;
  searchQuery.value = '';
  filterType.value = '';
  filterRarity.value = '';
};

// Computed para filtrar itens
const filteredItems = computed(() => {
  if (!selectedUser.value || !selectedUser.value.items) {
    return [];
  }

  return selectedUser.value.items.filter(item => {
    // Filtro por nome (busca)
    const matchesSearch = !searchQuery.value || 
      item.name.toLowerCase().includes(searchQuery.value.toLowerCase());

    // Filtro por tipo
    const matchesType = !filterType.value || item.type === filterType.value;

    // Filtro por raridade
    const matchesRarity = !filterRarity.value || item.rarity === filterRarity.value;

    return matchesSearch && matchesType && matchesRarity;
  });
});

const getRarityLabel = (rarity) => {
  const labels = {
    common: 'COMUM',
    uncommon: 'INCOMUM',
    rare: 'RARO',
    epic: 'ÉPICO',
    legendary: 'LENDÁRIO',
    mythic: 'MÍTICO'
  };
  return labels[rarity] || rarity.toUpperCase();
};

const formatCurrency = (value) => {
  return new Intl.NumberFormat('pt-BR', {
    style: 'currency',
    currency: 'BRL'
  }).format(value);
};

const formatDate = (dateString) => {
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(date);
};

const handleImageError = (event) => {
  event.target.src = '/src/assets/png/amostracard.png';
};

const getFundoImage = (rarity) => {
  const fundos = {
    common: fundoComum,
    uncommon: fundoIncomum,
    rare: fundoRaro,
    epic: fundoEpico,
    legendary: fundoLegendario,
    mythic: fundoMystico
  };
  return fundos[rarity] || fundoComum;
};
</script>

<style scoped>
.users-table-container {
  width: 100%;
  padding: 24px 0 24px 0; /* Removido padding-left e padding-right, mantendo apenas top e bottom */
  box-sizing: border-box;
}

.table-header {
  margin-bottom: 24px;
  padding-left: 0; /* Garantir que não tenha padding extra */
}

.table-title {
  font-size: 24px;
  font-weight: 700;
  color: #fff;
  margin: 0;
  padding-left: 0; /* Alinhar com o início do conteúdo */
}


.desktop-view {
  display: block;
}

.mobile-view {
  display: none;
}

.table-wrapper {
  overflow-x: auto;
  border-radius: 12px;
  background: #1a1a1a;
  border: 1px solid #312E81;
  width: 100%;
}

.users-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 600px;
}

.users-table thead {
  background: #161A42;
}

.users-table th {
  padding: 16px;
  text-align: left;
  font-weight: 600;
  font-size: 14px;
  color: #00E0FF;
  text-transform: uppercase;
  border-bottom: 2px solid #312E81;
  white-space: nowrap;
}

.user-row {
  cursor: pointer;
  transition: background-color 0.2s;
  border-bottom: 1px solid #2a2a2a;
}

.user-row:hover {
  background-color: #252525;
}

.user-row.is-expanded {
  background-color: #1e1e1e;
}

.users-table td {
  padding: 16px;
  color: #fff;
  font-size: 14px;
}

.user-name {
  font-weight: 600;
  color: #fff;
}

.user-email {
  color: #aaa;
  word-break: break-word;
}

.transaction-count {
  text-align: center;
  color: #00E0FF;
  font-weight: 600;
}

.total-value {
  font-weight: 700;
  color: #00E0FF;
  white-space: nowrap;
}

.expand-cell {
  text-align: center;
  width: 60px;
}

.expand-button {
  background: none;
  border: none;
  cursor: pointer;
  padding: 8px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: 4px;
  transition: background-color 0.2s;
}

.expand-button:hover {
  background-color: rgba(0, 224, 255, 0.1);
}

.expand-icon {
  color: #00E0FF;
  transition: transform 0.3s ease;
}

.expand-icon.is-expanded {
  transform: rotate(180deg);
}

.transactions-row {
  background-color: #0f0f0f;
}

.transactions-cell {
  padding: 0;
  border-top: 2px solid #312E81;
}

.transactions-container {
  padding: 24px;
}

.transactions-title {
  font-size: 18px;
  font-weight: 600;
  color: #00E0FF;
  margin: 0 0 16px 0;
}

.no-transactions {
  color: #888;
  text-align: center;
  padding: 24px;
  font-style: italic;
}

.transactions-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.transaction-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 16px;
  background: #1a1a1a;
  border-radius: 8px;
  border: 1px solid #2a2a2a;
  transition: transform 0.2s, box-shadow 0.2s;
}

.transaction-item:hover {
  transform: translateX(4px);
  box-shadow: 0 2px 8px rgba(0, 224, 255, 0.1);
}

.transaction-info {
  display: flex;
  gap: 16px;
  align-items: center;
  flex: 1;
  flex-wrap: wrap;
}

.transaction-date {
  color: #888;
  font-size: 12px;
  min-width: 100px;
}

.transaction-type {
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  white-space: nowrap;
}

.transaction-type.type-purchase {
  background: rgba(255, 87, 87, 0.2);
  color: #ff5757;
}

.transaction-type.type-sale {
  background: rgba(76, 175, 80, 0.2);
  color: #4caf50;
}

.transaction-item-name {
  color: #fff;
  font-weight: 500;
  word-break: break-word;
}

.transaction-value {
  font-weight: 700;
  font-size: 16px;
  white-space: nowrap;
}

.transaction-value .positive {
  color: #4caf50;
}

.transaction-value .negative {
  color: #ff5757;
}

/* Mobile View - Cards */
.user-card {
  background: #1a1a1a;
  border: 1px solid #312E81;
  border-radius: 12px;
  margin-bottom: 16px;
  overflow: hidden;
  transition: all 0.3s ease;
}

.user-card.is-expanded {
  border-color: #00E0FF;
}

.user-card-header {
  padding: 16px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.user-card-info {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.user-card-name {
  font-size: 18px;
  font-weight: 600;
  color: #fff;
  margin: 0;
}

.user-card-email {
  font-size: 14px;
  color: #aaa;
  margin: 0;
}

.user-card-stats {
  display: flex;
  gap: 24px;
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-label {
  font-size: 12px;
  color: #888;
  text-transform: uppercase;
}

.stat-value {
  font-size: 16px;
  font-weight: 700;
  color: #00E0FF;
}

.expand-button-mobile {
  position: absolute;
  top: 16px;
  right: 16px;
  background: none;
  border: none;
  cursor: pointer;
  padding: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.user-card-transactions {
  padding: 16px;
  border-top: 2px solid #312E81;
  background: #0f0f0f;
}

.user-card-transactions .transactions-title {
  font-size: 16px;
  margin-bottom: 12px;
}

.user-card-transactions .transaction-item {
  flex-direction: column;
  align-items: flex-start;
  gap: 8px;
}

.user-card-transactions .transaction-info {
  width: 100%;
  flex-wrap: wrap;
}

.user-card-transactions .transaction-value {
  width: 100%;
  text-align: right;
}

/* Responsividade */
@media (max-width: 1024px) {
  .users-table-container {
    padding: 16px 24px;
  }

  .users-table th {
    padding: 12px;
    font-size: 12px;
  }

  .users-table td {
    padding: 12px;
    font-size: 13px;
  }
}

@media (max-width: 768px) {
  .users-table-container {
    padding: 16px;
  }

  .desktop-view {
    display: none;
  }

  .mobile-view {
    display: block;
  }

  .table-title {
    font-size: 20px;
  }

  .user-card-header {
    position: relative;
  }

  .transaction-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .transaction-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 8px;
  }

  .transaction-value {
    width: 100%;
    text-align: right;
  }
}

@media (max-width: 480px) {
  .users-table-container {
    padding: 12px;
  }

  .user-card-header {
    padding: 12px;
  }

  .user-card-transactions {
    padding: 12px;
  }

  .user-card-stats {
    flex-direction: column;
    gap: 12px;
  }
}

.items-cell {
  text-align: center;
}

.items-button {
  background: linear-gradient(162deg, #161A42 22.61%, rgba(22, 26, 66, 0) 118.29%);
  border: 2px solid #312E81;
  border-radius: 8px;
  color: #00E0FF;
  padding: 8px 16px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.2s;
}

.items-button:hover {
  background: linear-gradient(162deg, #1fa2ff 22.61%, #12d8fa 118.29%);
  border-color: #12D8FA;
  color: #000;
  transform: translateY(-2px);
  box-shadow: 0 4px 8px rgba(18, 216, 250, 0.3);
}

.items-button-mobile {
  background: transparent;
  border: none;
  color: #00E0FF;
  font-size: 16px;
  font-weight: 700;
  cursor: pointer;
  padding: 0;
}

.items-button-mobile:hover {
  color: #12D8FA;
}

/* Modal de Itens */
.items-modal-content {
  width: 100%;
  max-width: 1400px; /* Aumentado de 1200px para 1400px */
  max-height: 85vh; /* Aumentado de 80vh para 85vh */
  overflow-y: auto;
}

.items-modal-title {
  font-size: 24px;
  font-weight: 700;
  color: #fff;
  margin: 0 0 24px 0;
  text-align: center;
}

.no-items {
  color: #888;
  text-align: center;
  padding: 40px;
  font-style: italic;
}

/* Cards Mini - Versão menor dos cards da loja */
.items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
  gap: 16px;
  padding: 8px;
}

.item-card-mini {
  border-radius: 10px;
  width: 100%;
  height: 280px; /* Reduzido de 320px para 280px */
  padding: 10px; /* Reduzido de 12px para 10px */
  transition: transform 0.2s;
  display: flex;
  flex-direction: column;
}

.item-card-mini:hover {
  transform: translateY(-4px);
}

.rarity-common {
  background: linear-gradient(180deg, #828282 0%, #B0B0B0 50%, rgba(130, 130, 130, 0.70) 100%);
}

.rarity-uncommon {
  background: linear-gradient(180deg, #4CAF50 0%, #81C784 50%, rgba(76, 175, 80, 0.70) 100%);
}

.rarity-rare {
  background: linear-gradient(180deg, #137BBE 0%, #12D8FA 50%, rgba(19, 123, 190, 0.70) 100%);
}

.rarity-epic {
  background: linear-gradient(180deg, #9C27B0 0%, #BA68C8 50%, rgba(156, 39, 176, 0.70) 100%);
}

.rarity-legendary {
  background: linear-gradient(180deg, #FF9800 0%, #FFB74D 50%, rgba(255, 152, 0, 0.70) 100%);
}

.rarity-mythic {
  background: linear-gradient(180deg, #E91E63 0%, #F06292 50%, rgba(233, 30, 99, 0.70) 100%);
}

.card-header-mini {
  display: flex;
  justify-content: space-between;
  margin-bottom: 4px; /* Reduzido de 6px para 4px */
}

.date-tag-mini {
  background-size: cover;
  width: 75px; /* Reduzido de 80px para 75px */
  height: 28px; /* Reduzido de 32px para 28px */
  justify-content: start;
  align-items: center;
  display: flex;
  border-radius: 8px;
  padding-left: 8px; /* Reduzido de 10px para 8px */
  font-size: 9px; /* Reduzido de 10px para 9px */
  color: #fff;
  font-weight: 600;
}

.rarity-tag-mini {
  display: flex;
  border-radius: 10px;
  border: 1px solid #00458A;
  background: #00458A;
  box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.25);
  justify-content: center;
  align-items: center;
  width: 70px; /* Reduzido de 75px para 70px */
  height: 22px; /* Reduzido de 24px para 22px */
  color: #FFF;
  text-align: center;
  font-family: Poppins;
  font-size: 9px; /* Reduzido de 10px para 9px */
  font-style: italic;
  font-weight: 700;
  line-height: 14px; /* Reduzido de 16px para 14px */
}

.image-container-mini {
  margin-top: -6px; /* Reduzido de -8px para -6px */
  width: 100%;
  height: 160px; /* Reduzido de 180px para 160px */
  flex-shrink: 0;
  background: #FFFFFF;
  border-radius: 10px;
  position: relative;
}

.item-image-mini {
  width: 100%;
  height: 100%;
  object-fit: cover;
  border-radius: 10px;
}

.overlay-tags-mini {
  position: absolute;
  bottom: 6px;
  left: 6px;
}

.price-badge-mini {
  width: 60px; /* Reduzido de 65px para 60px */
  height: 24px; /* Reduzido de 26px para 24px */
  border-radius: 12px;
  border: 2px solid #312E81;
  background: linear-gradient(162deg, #161A42 22.61%, #161A42 118.29%);
  box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.25) inset;
  display: flex;
  justify-content: center;
  align-items: center;
  color: #FAFAFB;
  font-size: 10px; /* Reduzido de 11px para 10px */
  font-weight: 600;
  line-height: 18px; /* Reduzido de 20px para 18px */
}

.info-section-mini {
  display: flex;
  flex-direction: column;
  gap: 4px; /* Reduzido de 6px para 4px */
  margin-top: 6px; /* Reduzido de 8px para 6px */
}

.item-name-mini {
  display: flex;
  align-items: center;
  color: #FAFAFB;
  font-family: Poppins;
  font-size: 13px; /* Reduzido de 14px para 13px */
  font-weight: 600;
  line-height: 1.2;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

/* Filtros do Modal */
.items-filters {
  margin-bottom: 24px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.search-input-wrapper {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 16px;
  color: #888;
  pointer-events: none;
}

.search-input {
  width: 100%;
  padding: 12px 16px 12px 48px;
  background: #2a2a2a;
  border: 1px solid #3a3a3a;
  border-radius: 8px;
  color: #fff;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.search-input:focus {
  border-color: #00E0FF;
  box-shadow: 0 0 0 2px rgba(0, 224, 255, 0.2);
}

.search-input::placeholder {
  color: #666;
}

.filters-row {
  display: flex;
  gap: 12px;
}

.filter-select {
  flex: 1;
  padding: 12px 16px;
  background: #2a2a2a;
  border: 1px solid #3a3a3a;
  border-radius: 8px;
  color: #fff;
  font-size: 14px;
  outline: none;
  cursor: pointer;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.filter-select:focus {
  border-color: #00E0FF;
  box-shadow: 0 0 0 2px rgba(0, 224, 255, 0.2);
}

.filter-select option {
  background: #1a1a1a;
  color: #fff;
}

@media (max-width: 768px) {
  .filters-row {
    flex-direction: column;
  }

  .items-grid {
    grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); /* Aumentado de 150px para 180px */
    gap: 16px;
  }

  .item-image-container {
    height: 200px; /* Aumentado de 150px para 200px */
  }

  .item-info {
    padding: 16px;
  }
}

@media (max-width: 480px) {
  .items-grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
    gap: 12px;
  }

  .item-image-container {
    height: 180px;
  }
}
</style>
