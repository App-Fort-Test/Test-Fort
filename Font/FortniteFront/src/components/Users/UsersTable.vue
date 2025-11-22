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
          <tr v-if="users.length === 0 && !loading" class="no-users-row">
            <td colspan="6" class="no-users-message">
              Nenhum usuário encontrado.
            </td>
          </tr>
          <template v-for="user in users" :key="user.id">
            <tr 
              :class="{ 'is-expanded': expandedUser === user.id }"
              class="user-row"
              @click="toggleUser(user.id)"
            >
              <td class="user-name">{{ user.name }}</td>
              <td class="user-email">{{ formatDate(user.createdAt) }}</td>
              <td class="transaction-count">{{ user.totalTransactions || 0 }}</td>
              <td class="items-cell" @click.stop>
                <button 
                  class="items-button"
                  @click="openItemsModal(user)"
                >
                  {{ user.totalCosmetics || 0 }} itens
                </button>
              </td>
              <td class="total-value">{{ formatCurrency(user.totalValue || 0) }}</td>
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
                  <div v-if="!user.transactions || user.transactions.length === 0" class="no-transactions">
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
                          {{ transaction.type === 'purchase' ? 'Compra' : 'Devolução' }}
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
                  <span class="stat-value">{{ user.totalTransactions || 0 }}</span>
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
                  <div v-if="!user.transactions || user.transactions.length === 0" class="no-transactions">
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
                  {{ transaction.type === 'purchase' ? 'Compra' : 'Devolução' }}
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
        <div v-else class="items-table-wrapper">
          <table class="items-table">
            <thead>
              <tr>
                <th class="col-image">Imagem</th>
                <th class="col-name">Nome</th>
                <th class="col-type">Tipo</th>
                <th class="col-rarity">Raridade</th>
                <th class="col-date">Data</th>
                <th class="col-price">Preço</th>
              </tr>
            </thead>
            <tbody>
              <tr 
                v-for="item in filteredItems" 
                :key="item.id"
                class="item-row"
                @click="openCosmeticDetails(item)"
              >
                <td class="col-image">
                  <img 
                    :src="item.image || '/src/assets/png/amostracard.png'" 
                    :alt="item.name"
                    class="item-image-table"
                    @error="handleImageError"
                  />
                </td>
                <td class="col-name">
                  <span class="item-name-link">{{ item.name }}</span>
                </td>
                <td class="col-type">{{ item.type || '-' }}</td>
                <td class="col-rarity">
                  <span class="rarity-badge" :class="`rarity-${item.rarity}`">
                    {{ getRarityLabel(item.rarity) }}
                  </span>
                </td>
                <td class="col-date">{{ formatDate(item.dateAdded || '2024-01-15') }}</td>
                <td class="col-price">{{ formatCurrency(item.purchasePrice || 0) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </BaseModal>

    <!-- Modal de Detalhes do Cosmético -->
    <BaseModal v-if="selectedCosmetic" @close="closeCosmeticDetails">
      <CosmeticDetailsModal 
        :cosmetic="selectedCosmetic" 
        @close="closeCosmeticDetails"
      />
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onActivated, watch, nextTick } from 'vue';
import { usersService } from '../../services/users';
import { transactionsService } from '../../services/transactions';
import BaseModal from '../Modal/User/BaseModal.vue';
import CosmeticDetailsModal from '../Modal/Cosmetics/CosmeticDetailsModal.vue';
import { fortniteExternalAPI } from '../../services/fortniteApi';
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
const userTotalValues = ref({}); // Cache de valores totais por usuário
const selectedCosmetic = ref(null);

// Carregar usuários (com cache automático)
const loadUsers = async (forceRefresh = false) => {
  // Evitar múltiplas chamadas simultâneas
  if (isLoadingUsers.value && !forceRefresh) {
    console.log('Já está carregando usuários, aguardando...');
    return;
  }
  
  isLoadingUsers.value = true;
  
  // Tentar carregar do cache primeiro para mostrar imediatamente
  if (!forceRefresh) {
    const cacheKey = `fortnite_users_${currentPage.value}-${pageSize.value}`;
    try {
      const cached = localStorage.getItem(cacheKey);
      if (cached) {
        const cacheData = JSON.parse(cached);
        const now = Date.now();
        if (now - cacheData.timestamp < 10 * 60 * 1000) {
          const data = cacheData.data;
          if (data && data.users && Array.isArray(data.users)) {
            users.value = data.users.map(u => ({
              id: u.id,
              name: u.username,
              email: '',
              totalCosmetics: u.totalCosmetics || 0,
              totalTransactions: u.totalTransactions || 0, // Total de transações do backend
              createdAt: u.createdAt,
              items: [],
              transactions: [], // Inicializar como array vazio (será carregado quando expandir)
              totalValue: 0 // Será calculado depois
            }));
            
            // Calcular valores totais para cada usuário em paralelo
            await Promise.all(users.value.map(async (user) => {
              user.totalValue = await calculateUserTotalValue(user.id);
            }));
            totalPages.value = data.totalPages || 1;
            loading.value = false; // Mostrar dados do cache imediatamente
            console.log('Usuários carregados do cache:', users.value.length);
          }
        }
      }
    } catch (e) {
      console.warn('Erro ao carregar do cache:', e);
    }
  }
  
  // Buscar dados atualizados em background
  loading.value = true;
  try {
    console.log('Buscando usuários da API...', { page: currentPage.value, pageSize: pageSize.value, forceRefresh });
    const data = await usersService.getUsers(currentPage.value, pageSize.value, forceRefresh);
    console.log('Dados recebidos da API:', data);
    
    if (data && data.users && Array.isArray(data.users)) {
          users.value = data.users.map(u => ({
            id: u.id,
            name: u.username,
            email: '', // Não expor email por segurança
            totalCosmetics: u.totalCosmetics || 0,
            totalTransactions: u.totalTransactions || 0, // Total de transações do backend
            createdAt: u.createdAt,
            items: [],
            transactions: [], // Inicializar como array vazio (será carregado quando expandir)
            totalValue: 0 // Será calculado depois
          }));
      totalPages.value = data.totalPages || 1;
      console.log('Usuários processados:', users.value.length, 'Total de páginas:', totalPages.value);
      
      // Calcular valores totais para cada usuário em paralelo
      await Promise.all(users.value.map(async (user) => {
        user.totalValue = await calculateUserTotalValue(user.id);
      }));
    } else {
      console.warn('Dados inválidos recebidos:', data);
      users.value = [];
      totalPages.value = 1;
    }
  } catch (error) {
    console.error('Erro ao carregar usuários:', error);
    console.error('Detalhes do erro:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status
    });
    // Manter dados do cache se houver erro
    if (users.value.length === 0) {
      users.value = [];
      totalPages.value = 1;
    }
  } finally {
    loading.value = false;
    isLoadingUsers.value = false;
  }
};

// Calcular valor total dos itens possuídos por um usuário
const calculateUserTotalValue = async (userId) => {
  // Verificar cache primeiro
  if (userTotalValues.value[userId] !== undefined) {
    return userTotalValues.value[userId];
  }
  
  try {
    // Buscar todos os cosméticos do usuário (sem paginação para calcular o total)
    const data = await usersService.getUserCosmetics(userId, 1, 10000, false);
    if (data && data.cosmetics && Array.isArray(data.cosmetics)) {
      const total = data.cosmetics.reduce((sum, c) => sum + (c.purchasePrice || 0), 0);
      userTotalValues.value[userId] = total;
      return total;
    }
    return 0;
  } catch (error) {
    console.error('Erro ao calcular valor total do usuário:', error);
    return 0;
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
    
    // Buscar detalhes completos dos cosméticos da API externa
    let cosmeticsDetails = new Map();
    try {
      const cosmeticsResponse = await fortniteExternalAPI.getCosmetics();
      if (cosmeticsResponse && cosmeticsResponse.data && cosmeticsResponse.data.br) {
        cosmeticsResponse.data.br.forEach(c => {
          cosmeticsDetails.set(c.id, c);
        });
      }
    } catch (err) {
      console.warn('Erro ao buscar detalhes dos cosméticos:', err);
    }
    
    userCosmetics.value[userId] = data.cosmetics.map(c => {
      const cosmeticDetail = cosmeticsDetails.get(c.cosmeticId);
      return {
        id: c.cosmeticId,
        name: cosmeticDetail?.name || c.cosmeticId,
        cosmeticId: c.cosmeticId,
        type: cosmeticDetail?.type?.displayValue || cosmeticDetail?.type?.value || '-',
        rarity: cosmeticDetail?.rarity?.value || 'common',
        image: cosmeticDetail?.images?.icon || cosmeticDetail?.images?.smallIcon || '/src/assets/png/amostracard.png',
        dateAdded: c.acquiredAt,
        purchasePrice: c.purchasePrice
      };
    });
    
    // Atualizar valor total do usuário
    const total = userCosmetics.value[userId].reduce((sum, c) => sum + (c.purchasePrice || 0), 0);
    userTotalValues.value[userId] = total;
    const user = users.value.find(u => u.id === userId);
    if (user) {
      user.totalValue = total;
    }
    
    return userCosmetics.value[userId];
  } catch (error) {
    console.error('Erro ao carregar cosméticos do usuário:', error);
    return [];
  }
};

// Carregar transações de um usuário
const loadUserTransactions = async (userId) => {
  // Sempre buscar transações do usuário específico, não usar cache para garantir dados corretos
  // Remover do cache se existir para forçar nova busca
  if (userTransactions.value[userId]) {
    delete userTransactions.value[userId];
  }
  
  try {
    console.log(`Carregando transações para usuário ${userId} (tipo: ${typeof userId})`);
    const data = await transactionsService.getHistory(userId);
    console.log(`Transações recebidas para usuário ${userId}:`, data.transactions?.length || 0);
    
    if (data && data.transactions && Array.isArray(data.transactions)) {
      userTransactions.value[userId] = data.transactions.map(t => ({
        id: t.id,
        date: t.createdAt,
        type: t.type.toLowerCase(),
        itemName: t.cosmeticName,
        value: Math.abs(t.amount)
      }));
      console.log(`Transações processadas para usuário ${userId}:`, userTransactions.value[userId].length);
      return userTransactions.value[userId];
    } else {
      console.warn(`Nenhuma transação encontrada para usuário ${userId}`);
      userTransactions.value[userId] = [];
      return [];
    }
  } catch (error) {
    console.error(`Erro ao carregar transações do usuário ${userId}:`, error);
    console.error('Detalhes do erro:', {
      message: error.message,
      response: error.response?.data,
      status: error.response?.status
    });
    userTransactions.value[userId] = [];
    return [];
  }
};

// Flag para evitar múltiplas chamadas simultâneas
const isLoadingUsers = ref(false);

onMounted(async () => {
  console.log('UsersTable montado, carregando usuários...');
  // Sempre carregar quando o componente for montado
  if (!isLoadingUsers.value) {
    isLoadingUsers.value = true;
    try {
      await loadUsers(true); // Forçar refresh para garantir que carregue
    } catch (error) {
      console.error('Erro ao carregar usuários no onMounted:', error);
    } finally {
      isLoadingUsers.value = false;
    }
  }
});

// Recarregar quando o componente for ativado (útil para navegação)
onActivated(async () => {
  console.log('UsersTable ativado, verificando se precisa recarregar...');
  // Sempre recarregar quando o componente for ativado
  if (!isLoadingUsers.value) {
    isLoadingUsers.value = true;
    try {
      await loadUsers(true);
    } catch (error) {
      console.error('Erro ao carregar usuários no onActivated:', error);
    } finally {
      isLoadingUsers.value = false;
    }
  }
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

// Abrir modal de detalhes do cosmético
const openCosmeticDetails = async (item) => {
  try {
    // Buscar detalhes completos do cosmético pela API externa
    const cosmeticsResponse = await fortniteExternalAPI.getCosmetics();
    if (cosmeticsResponse && cosmeticsResponse.data && cosmeticsResponse.data.br) {
      const cosmetic = cosmeticsResponse.data.br.find(c => c.id === item.cosmeticId || c.id === item.id);
      if (cosmetic) {
        // Enriquecer com dados do inventário e shop
        const [newCosmeticsResponse, shopResponse] = await Promise.all([
          fortniteExternalAPI.getNewCosmetics().catch(() => null),
          fortniteExternalAPI.getShop().catch(() => null)
        ]);
        
        const newCosmeticIds = new Set();
        if (newCosmeticsResponse?.data?.items?.br) {
          newCosmeticsResponse.data.items.br.forEach(c => {
            if (c.id) newCosmeticIds.add(c.id);
          });
        }
        
        const shopCosmetics = new Map();
        if (shopResponse?.data?.entries) {
          shopResponse.data.entries.forEach(entry => {
            if (entry.brItems) {
              entry.brItems.forEach(item => {
                if (item.id) {
                  shopCosmetics.set(item.id, entry);
                }
              });
            }
          });
        }
        
        // Criar objeto enriquecido
        const enrichedCosmetic = {
          id: cosmetic.id,
          name: cosmetic.name,
          type: cosmetic.type || { value: '', displayValue: '' },
          rarity: cosmetic.rarity || { value: 'common', displayValue: 'Common' },
          images: cosmetic.images || { smallIcon: '', icon: '', featured: '' },
          added: cosmetic.added ? new Date(cosmetic.added) : new Date(),
          isNew: newCosmeticIds.has(cosmetic.id),
          isInShop: shopCosmetics.has(cosmetic.id),
          isOwned: true, // Sempre true pois está no inventário do usuário
          isOnSale: shopCosmetics.has(cosmetic.id) && 
                   shopCosmetics.get(cosmetic.id).finalPrice < shopCosmetics.get(cosmetic.id).regularPrice,
          price: shopCosmetics.has(cosmetic.id) ? shopCosmetics.get(cosmetic.id).finalPrice : (item.purchasePrice || 0),
          regularPrice: shopCosmetics.has(cosmetic.id) ? shopCosmetics.get(cosmetic.id).regularPrice : (item.purchasePrice || 0),
          isBundle: false,
          bundleItems: null
        };
        
        selectedCosmetic.value = enrichedCosmetic;
      } else {
        // Se não encontrou na API externa, criar objeto básico com os dados do item
        selectedCosmetic.value = {
          id: item.cosmeticId || item.id,
          name: item.name || item.cosmeticId || item.id,
          type: { value: item.type || '', displayValue: item.type || '' },
          rarity: { value: item.rarity || 'common', displayValue: item.rarity || 'Common' },
          images: { smallIcon: item.image || '', icon: item.image || '', featured: item.image || '' },
          added: item.dateAdded ? new Date(item.dateAdded) : new Date(),
          isNew: false,
          isInShop: false,
          isOwned: true,
          isOnSale: false,
          price: item.purchasePrice || 0,
          regularPrice: item.purchasePrice || 0,
          isBundle: false,
          bundleItems: null
        };
      }
    }
  } catch (error) {
    console.error('Erro ao buscar detalhes do cosmético:', error);
    // Criar objeto básico em caso de erro
    selectedCosmetic.value = {
      id: item.cosmeticId || item.id,
      name: item.name || item.cosmeticId || item.id,
      type: { value: item.type || '', displayValue: item.type || '' },
      rarity: { value: item.rarity || 'common', displayValue: item.rarity || 'Common' },
      images: { smallIcon: item.image || '', icon: item.image || '', featured: item.image || '' },
      added: item.dateAdded ? new Date(item.dateAdded) : new Date(),
      isNew: false,
      isInShop: false,
      isOwned: true,
      isOnSale: false,
      price: item.purchasePrice || 0,
      regularPrice: item.purchasePrice || 0,
      isBundle: false,
      bundleItems: null
    };
  }
};

const closeCosmeticDetails = () => {
  selectedCosmetic.value = null;
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
  return new Intl.NumberFormat('pt-BR').format(value) + ' V-Bucks';
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

.transaction-type.type-sale,
.transaction-type.type-refund {
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

/* Tabela de Itens */
.items-table-wrapper {
  overflow-x: auto;
  border-radius: 12px;
  background: #1a1a1a;
  border: 1px solid #312E81;
}

.items-table {
  width: 100%;
  border-collapse: collapse;
  min-width: 800px;
}

.items-table thead {
  background: #161A42;
  position: sticky;
  top: 0;
  z-index: 10;
}

.items-table th {
  padding: 16px;
  text-align: left;
  font-weight: 600;
  font-size: 14px;
  color: #00E0FF;
  text-transform: uppercase;
  border-bottom: 2px solid #312E81;
  white-space: nowrap;
}

.items-table th.col-image {
  width: 80px;
  text-align: center;
}

.items-table th.col-name {
  min-width: 200px;
}

.items-table th.col-type {
  width: 120px;
}

.items-table th.col-rarity {
  width: 120px;
}

.items-table th.col-date {
  width: 120px;
}

.items-table th.col-price {
  width: 150px;
  text-align: right;
}

.items-table tbody tr {
  border-bottom: 1px solid #2a2a2a;
  transition: background-color 0.2s;
  cursor: pointer;
}

.items-table tbody tr:hover {
  background-color: #252525;
}

.items-table td {
  padding: 12px 16px;
  color: #fff;
  font-size: 14px;
  vertical-align: middle;
}

.items-table td.col-image {
  text-align: center;
  padding: 8px;
}

.items-table td.col-name {
  font-weight: 600;
}

.items-table td.col-price {
  text-align: right;
  color: #00E0FF;
  font-weight: 600;
}

.item-image-table {
  width: 60px;
  height: 60px;
  object-fit: cover;
  border-radius: 8px;
  background: #fff;
  border: 2px solid #312E81;
}

.item-name-link {
  color: #fff;
  text-decoration: none;
  transition: color 0.2s;
}

.item-name-link:hover {
  color: #00E0FF;
}

.rarity-badge {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
  border: 1px solid;
}

.rarity-badge.rarity-common {
  background: rgba(130, 130, 130, 0.2);
  color: #B0B0B0;
  border-color: #828282;
}

.rarity-badge.rarity-uncommon {
  background: rgba(76, 175, 80, 0.2);
  color: #81C784;
  border-color: #4CAF50;
}

.rarity-badge.rarity-rare {
  background: rgba(19, 123, 190, 0.2);
  color: #12D8FA;
  border-color: #137BBE;
}

.rarity-badge.rarity-epic {
  background: rgba(156, 39, 176, 0.2);
  color: #BA68C8;
  border-color: #9C27B0;
}

.rarity-badge.rarity-legendary {
  background: rgba(255, 152, 0, 0.2);
  color: #FFB74D;
  border-color: #FF9800;
}

.rarity-badge.rarity-mythic {
  background: rgba(233, 30, 99, 0.2);
  color: #F06292;
  border-color: #E91E63;
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
  background: #2a2a2a !important;
  border: 1px solid #3a3a3a;
  border-radius: 8px;
  color: #fff;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}

/* Remover fundo branco do autocomplete */
.search-input:-webkit-autofill,
.search-input:-webkit-autofill:hover,
.search-input:-webkit-autofill:focus,
.search-input:-webkit-autofill:active {
  -webkit-box-shadow: 0 0 0 30px #2a2a2a inset !important;
  -webkit-text-fill-color: #fff !important;
  background-color: #2a2a2a !important;
  background: #2a2a2a !important;
}

.search-input:focus {
  border-color: #00E0FF;
  box-shadow: 0 0 0 2px rgba(0, 224, 255, 0.2);
}

.search-input::placeholder {
  color: #666;
}

.no-users-row {
  background: transparent;
}

.no-users-message {
  text-align: center;
  padding: 40px 20px;
  color: #888;
  font-style: italic;
}

.filters-row {
  display: flex;
  gap: 12px;
}

.filter-select {
  flex: 1;
  padding: 12px 16px;
  background: #2a2a2a !important;
  border: 1px solid #3a3a3a;
  border-radius: 8px;
  color: #fff;
  font-size: 14px;
  outline: none;
  cursor: pointer;
  transition: border-color 0.2s, box-shadow 0.2s;
  appearance: none;
  -webkit-appearance: none;
  -moz-appearance: none;
}

.filter-select:focus {
  border-color: #00E0FF;
  box-shadow: 0 0 0 2px rgba(0, 224, 255, 0.2);
}

.filter-select option {
  background: #1a1a1a !important;
  color: #fff !important;
}

@media (max-width: 768px) {
  .filters-row {
    flex-direction: column;
  }

  .items-table-wrapper {
    overflow-x: auto;
  }

  .items-table {
    min-width: 600px;
  }

  .items-table th,
  .items-table td {
    padding: 10px 12px;
    font-size: 12px;
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
