<template>
  <div class="transaction-history-container">
    <div class="history-header">
      <h2 class="history-title">Histórico de Transações</h2>
    </div>

    <div v-if="loading" class="loading-container">
      <p>Carregando histórico...</p>
    </div>

    <div v-else-if="error" class="error-container">
      <p>Erro: {{ error }}</p>
    </div>

    <div v-else-if="transactions.length === 0" class="no-transactions">
      <p>Nenhuma transação encontrada.</p>
    </div>

    <div v-else class="transactions-list">
      <div 
        v-for="transaction in transactions" 
        :key="transaction.id"
        class="transaction-item"
        :class="transaction.type"
        @click="openCosmeticDetails(transaction)"
        style="cursor: pointer;"
      >
        <div class="transaction-icon">
          <svg v-if="isPurchase(transaction)" width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M7 18C5.9 18 5.01 18.9 5.01 20S5.9 22 7 22 8.99 21.1 8.99 20 8.1 18 7 18ZM1 2V4H3L6.6 11.59L5.25 14.04C5.09 14.32 5 14.65 5 15C5 16.1 5.9 17 7 17H19V15H7.42C7.28 15 7.17 14.89 7.17 14.75L7.2 14.63L8.1 13H15.55C16.3 13 16.96 12.59 17.3 11.97L20.88 5.48C20.96 5.34 21 5.17 21 5C21 4.45 20.55 4 20 4H5.21L4.27 2H1ZM17 18C15.9 18 15.01 18.9 15.01 20S15.9 22 17 22 18.99 21.1 18.99 20 18.1 18 17 18Z" fill="currentColor"/>
          </svg>
          <svg v-else width="24" height="24" viewBox="0 0 24 24" fill="none">
            <path d="M19 7H18V6C18 3.24 15.76 1 13 1S8 3.24 8 6V7H7C5.9 7 5 7.9 5 9V20C5 21.1 5.9 22 7 22H19C20.1 22 21 21.1 21 20V9C21 7.9 20.1 7 19 7ZM10 6C10 4.34 11.34 3 13 3S16 4.34 16 6V7H10V6ZM19 20H7V9H19V20Z" fill="currentColor"/>
            <path d="M12 12C10.9 12 10 12.9 10 14S10.9 16 12 16 14 15.1 14 14 13.1 12 12 12Z" fill="currentColor"/>
          </svg>
        </div>

        <div class="transaction-details">
          <div class="transaction-name">{{ transaction.cosmeticName }}</div>
          <div class="transaction-date">{{ formatDate(transaction.createdAt || transaction.CreatedAt) }}</div>
        </div>

        <div class="transaction-amount" :class="getTransactionTypeClass(transaction)">
          <span v-if="isPurchase(transaction)" class="amount-negative">
            -{{ formatCurrency(Math.abs(transaction.amount || transaction.Amount)) }}
          </span>
          <span v-else class="amount-positive">
            +{{ formatCurrency(transaction.amount || transaction.Amount) }}
          </span>
        </div>

        <div class="transaction-type-badge" :class="getTransactionTypeClass(transaction)">
          {{ isPurchase(transaction) ? 'Compra' : 'Devolução' }}
        </div>
      </div>
    </div>


    <BaseModal v-if="selectedCosmetic" @close="closeCosmeticDetails">
      <CosmeticDetailsModal 
        :cosmetic="selectedCosmetic" 
        @close="closeCosmeticDetails"
        @refund-success="handleRefundSuccess"
      />
    </BaseModal>
  </div>
</template>

<script setup>
import { ref, onMounted, onActivated, watch, onUnmounted } from 'vue';
import { transactionsService } from '../../services/transactions';
import { useAuth } from '../../composables/useAuth';
import { useTransactions } from '../../composables/useTransactions';
import BaseModal from '../Modal/User/BaseModal.vue';
import CosmeticDetailsModal from '../Modal/Cosmetics/CosmeticDetailsModal.vue';
import { fortniteExternalAPI } from '../../services/fortniteApi';

const { user } = useAuth();
const { transactionUpdateEvent } = useTransactions();
const transactions = ref([]);
const loading = ref(false);
const error = ref(null);
const selectedCosmetic = ref(null);

const loadHistory = async (forceRefresh = false) => {
  if (!user.value) {
    error.value = 'É necessário estar logado para ver o histórico';
    transactions.value = [];
    return;
  }

  loading.value = true;
  error.value = null;

  try {
    console.log('Carregando histórico de transações para usuário:', user.value.id);
    const data = await transactionsService.getHistory(user.value.id);
    console.log('Dados recebidos do histórico:', data);
    
    // O backend retorna um objeto com transactions
    if (data && data.transactions && Array.isArray(data.transactions)) {
      transactions.value = data.transactions;
      console.log('Transações carregadas:', transactions.value.length);
    } else if (Array.isArray(data)) {
      transactions.value = data;
      console.log('Transações carregadas (array direto):', transactions.value.length);
    } else {
      console.warn('Formato de dados inesperado:', data);
      transactions.value = [];
    }
  } catch (err) {
    console.error('Erro ao carregar histórico:', err);
    error.value = err.response?.data?.message || err.message || 'Erro ao carregar histórico';
    transactions.value = [];
  } finally {
    loading.value = false;
  }
};

const formatDate = (dateString) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
};

const formatCurrency = (value) => {
  return new Intl.NumberFormat('pt-BR').format(value) + ' V-Bucks';
};

// Verificar se é compra (Purchase = 1 ou 'purchase' ou 'Purchase')
const isPurchase = (transaction) => {
  const type = transaction.type || transaction.Type;
  return type === 1 || type === 'purchase' || type === 'Purchase' || type?.toLowerCase() === 'purchase';
};

// Obter classe CSS baseada no tipo
const getTransactionTypeClass = (transaction) => {
  return isPurchase(transaction) ? 'purchase' : 'refund';
};

// Abrir modal de detalhes do cosmético
const openCosmeticDetails = async (transaction) => {
  try {
    const cosmeticId = transaction.cosmeticId || transaction.cosmeticName;
    if (!cosmeticId) return;
    
  
    const cosmeticsResponse = await fortniteExternalAPI.getCosmetics();
    if (cosmeticsResponse && cosmeticsResponse.data && cosmeticsResponse.data.br) {
      const cosmetic = cosmeticsResponse.data.br.find(c => c.id === cosmeticId);
      if (cosmetic) {
       
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
        

        const { cosmeticsAPI } = await import('../../services/api');
        let isOwned = false;
        if (user.value?.id) {
          try {
            const inventory = await cosmeticsAPI.getInventory(user.value.id, false);
            if (inventory && inventory.ownedCosmetics) {
              const ownedIds = new Set(
                inventory.ownedCosmetics.map(c => typeof c === 'string' ? c : (c.cosmeticId || c.id))
              );
              isOwned = ownedIds.has(cosmeticId);
            }
          } catch (err) {
            console.warn('Erro ao verificar se possui o cosmético:', err);
          }
        }
        
  
        const enrichedCosmetic = {
          id: cosmetic.id,
          name: cosmetic.name,
          type: cosmetic.type || { value: '', displayValue: '' },
          rarity: cosmetic.rarity || { value: 'common', displayValue: 'Common' },
          images: cosmetic.images || { smallIcon: '', icon: '', featured: '' },
          added: cosmetic.added ? new Date(cosmetic.added) : new Date(),
          isNew: newCosmeticIds.has(cosmetic.id),
          isInShop: shopCosmetics.has(cosmetic.id),
          isOwned: isOwned,
          isOnSale: shopCosmetics.has(cosmetic.id) && 
                   shopCosmetics.get(cosmetic.id).finalPrice < shopCosmetics.get(cosmetic.id).regularPrice,
          price: shopCosmetics.has(cosmetic.id) ? shopCosmetics.get(cosmetic.id).finalPrice : (Math.abs(transaction.amount || transaction.Amount) || 0),
          regularPrice: shopCosmetics.has(cosmetic.id) ? shopCosmetics.get(cosmetic.id).regularPrice : (Math.abs(transaction.amount || transaction.Amount) || 0),
          isBundle: false,
          bundleItems: null
        };
        
        selectedCosmetic.value = enrichedCosmetic;
      } else {
        // Se não encontrou na API externa, criar objeto básico
        selectedCosmetic.value = {
          id: cosmeticId,
          name: transaction.cosmeticName || cosmeticId,
          type: { value: '', displayValue: '' },
          rarity: { value: 'common', displayValue: 'Common' },
          images: { smallIcon: '', icon: '', featured: '' },
          added: new Date(),
          isNew: false,
          isInShop: false,
          isOwned: !isPurchase(transaction), // Se não é compra, pode não possuir mais
          isOnSale: false,
          price: Math.abs(transaction.amount || transaction.Amount) || 0,
          regularPrice: Math.abs(transaction.amount || transaction.Amount) || 0,
          isBundle: false,
          bundleItems: null
        };
      }
    }
  } catch (error) {
    console.error('Erro ao buscar detalhes do cosmético:', error);

    selectedCosmetic.value = {
      id: transaction.cosmeticId || transaction.cosmeticName,
      name: transaction.cosmeticName || transaction.cosmeticId,
      type: { value: '', displayValue: '' },
      rarity: { value: 'common', displayValue: 'Common' },
      images: { smallIcon: '', icon: '', featured: '' },
      added: new Date(),
      isNew: false,
      isInShop: false,
      isOwned: !isPurchase(transaction),
      isOnSale: false,
      price: Math.abs(transaction.amount || transaction.Amount) || 0,
      regularPrice: Math.abs(transaction.amount || transaction.Amount) || 0,
      isBundle: false,
      bundleItems: null
    };
  }
};

const closeCosmeticDetails = () => {
  selectedCosmetic.value = null;
};


const handleRefundSuccess = async () => {

  closeCosmeticDetails();

  await new Promise(resolve => setTimeout(resolve, 300));

  await loadHistory(true);
};

let pollingInterval = null;

onMounted(() => {
  console.log('TransactionHistory montado, carregando histórico...');
  loadHistory(true);
  
  if (user.value?.id) {
    pollingInterval = setInterval(() => {
      loadHistory(true);
    }, 5000);
  }
});

onActivated(() => {
  console.log('TransactionHistory ativado, recarregando histórico...');
  loadHistory(true);
  
  if (user.value?.id && !pollingInterval) {
    pollingInterval = setInterval(() => {
      loadHistory(true);
    }, 5000);
  }
});

onUnmounted(() => {
  if (pollingInterval) {
    clearInterval(pollingInterval);
    pollingInterval = null;
  }
});

watch(() => user.value?.id, (newUserId, oldUserId) => {
  if (newUserId && newUserId !== oldUserId) {
    console.log('Usuário mudou, recarregando histórico...');
    loadHistory(true);
    
    if (pollingInterval) {
      clearInterval(pollingInterval);
    }
    
    if (newUserId) {
      pollingInterval = setInterval(() => {
        loadHistory(true);
      }, 5000);
    }
  } else if (!newUserId && pollingInterval) {
    clearInterval(pollingInterval);
    pollingInterval = null;
  }
});

watch(() => transactionUpdateEvent.value, () => {
  if (user.value?.id) {
    console.log('Evento de atualização de transação detectado, recarregando histórico imediatamente...');
    loadHistory(true);
  }
});
</script>

<style scoped>
.transaction-history-container {
  padding: 24px;
  color: #fff;
  max-width: 1200px;
  margin: 0 auto;
}

.history-header {
  margin-bottom: 32px;
}

.history-title {
  font-size: 2rem;
  font-weight: 700;
  color: #fff;
  margin: 0;
}

.loading-container,
.error-container,
.no-transactions {
  text-align: center;
  padding: 40px;
  color: #aaa;
}

.transactions-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.transaction-item {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 20px;
  background: linear-gradient(162deg, #161A42 22.61%, rgba(22, 26, 66, 0) 118.29%);
  border: 2px solid #212554;
  border-radius: 12px;
  transition: all 0.3s ease;
}

.transaction-item:hover {
  border-color: #00E0FF;
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0, 224, 255, 0.2);
}

.transaction-icon {
  width: 48px;
  height: 48px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 224, 255, 0.1);
  border-radius: 8px;
  color: #00E0FF;
}

.transaction-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.transaction-name {
  font-size: 1.1rem;
  font-weight: 600;
  color: #fff;
}

.transaction-date {
  font-size: 0.9rem;
  color: #aaa;
}

.transaction-amount {
  font-size: 1.2rem;
  font-weight: 700;
  min-width: 120px;
  text-align: right;
}

.amount-negative {
  color: #ff4444;
}

.amount-positive {
  color: #44ff44;
}

.transaction-type-badge {
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 0.85rem;
  font-weight: 600;
  text-transform: uppercase;
}

.transaction-type-badge.purchase {
  background: rgba(255, 68, 68, 0.2);
  color: #ff4444;
  border: 1px solid #ff4444;
}

.transaction-type-badge.refund {
  background: rgba(68, 255, 68, 0.2);
  color: #44ff44;
  border: 1px solid #44ff44;
}

@media (max-width: 768px) {
  .transaction-item {
    flex-wrap: wrap;
  }

  .transaction-amount {
    width: 100%;
    text-align: left;
    margin-top: 8px;
  }
}
</style>

