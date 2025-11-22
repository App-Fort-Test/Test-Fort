<template>
  <div class="cosmetic-details-modal">
    <div v-if="cosmetic" class="details-content">
      <!-- Header com imagem e nome -->
      <div class="details-header">
        <div class="details-image-container">
          <img 
            :src="getImageSrc()" 
            :alt="cosmetic.name"
            class="details-image"
            @error="handleImageError"
          />
          <div class="image-badges">
            <span v-if="cosmetic.isNew" class="badge new-badge">NOVO</span>
            <span v-if="cosmetic.isOnSale" class="badge sale-badge">PROMOÇÃO</span>
            <span v-if="cosmetic.isOwned" class="badge owned-badge">ADQUIRIDO</span>
            <span v-if="cosmetic.isBundle" class="badge bundle-badge">BUNDLE</span>
          </div>
        </div>
        <div class="details-title">
          <h2 class="cosmetic-name">{{ cosmetic.name }}</h2>
          <div class="cosmetic-id">ID: {{ cosmetic.id }}</div>
        </div>
      </div>

      <!-- Informações principais -->
      <div class="details-section">
        <h3 class="section-title">Informações</h3>
        <div class="info-grid">
          <div class="info-item">
            <span class="info-label">Tipo:</span>
            <span class="info-value">{{ getTypeDisplay() }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">Raridade:</span>
            <span class="info-value rarity-value" :class="`rarity-${getRarityValue()}`">
              {{ getRarityDisplay() }}
            </span>
          </div>
          <div class="info-item">
            <span class="info-label">Data de Adição:</span>
            <span class="info-value">{{ formatDate(cosmetic.added) }}</span>
          </div>
          <div class="info-item">
            <span class="info-label">Status:</span>
            <span class="info-value">
              <span v-if="cosmetic.isNew" class="status-tag new">Novo</span>
              <span v-if="cosmetic.isOnSale" class="status-tag sale">Em Promoção</span>
              <span v-if="cosmetic.isOwned" class="status-tag owned">Adquirido</span>
              <span v-if="!cosmetic.isInShop && !cosmetic.isOwned" class="status-tag unavailable">Indisponível</span>
            </span>
          </div>
        </div>
      </div>

      <!-- Preço -->
      <div v-if="cosmetic.price !== null && cosmetic.price !== undefined" class="details-section">
        <h3 class="section-title">Preço</h3>
        <div class="price-section">
          <div class="price-display">
            <img :src="logoMoeda" alt="V-Bucks" class="vbucks-icon-large" />
            <div class="price-values">
              <span v-if="cosmetic.isOnSale && cosmetic.regularPrice" class="original-price-large">
                {{ formatCurrency(cosmetic.regularPrice) }} V-Bucks
              </span>
              <span class="current-price-large">
                {{ formatCurrency(cosmetic.price) }} V-Bucks
              </span>
            </div>
          </div>
          <div v-if="cosmetic.isOnSale && cosmetic.regularPrice" class="discount-info">
            Economia de {{ formatCurrency(cosmetic.regularPrice - cosmetic.price) }} V-Bucks
          </div>
        </div>
      </div>

      <!-- Bundle Items -->
      <div v-if="cosmetic.isBundle && cosmetic.bundleItems && cosmetic.bundleItems.length > 0" class="details-section">
        <h3 class="section-title">Itens do Bundle</h3>
        <div class="bundle-items-list">
          <div 
            v-for="(item, index) in cosmetic.bundleItems" 
            :key="item.id || index"
            class="bundle-item"
          >
            <div class="bundle-item-info">
              <span class="bundle-item-name">{{ item.name || item.id }}</span>
              <span class="bundle-item-price">{{ formatCurrency(item.price) }} V-Bucks</span>
            </div>
          </div>
          <div class="bundle-total">
            <strong>Total: {{ formatCurrency(cosmetic.price || 0) }} V-Bucks</strong>
          </div>
        </div>
      </div>

      <!-- Imagens adicionais -->
      <div v-if="hasAdditionalImages" class="details-section">
        <h3 class="section-title">Imagens Adicionais</h3>
        <div class="images-gallery">
          <img 
            v-if="cosmetic.images?.icon && cosmetic.images.icon !== cosmetic.images?.smallIcon" 
            :src="cosmetic.images.icon" 
            alt="Icon"
            class="gallery-image"
            @error="handleImageError"
          />
          <img 
            v-if="cosmetic.images?.featured" 
            :src="cosmetic.images.featured" 
            alt="Featured"
            class="gallery-image"
            @error="handleImageError"
          />
        </div>
      </div>

      <!-- Botões de Ação -->
      <div class="details-actions">
        <button 
          v-if="canPurchase && user"
          class="action-button buy-button"
          @click="handlePurchase"
          :disabled="isPurchasing"
        >
          {{ isPurchasing ? 'Comprando...' : 'Comprar' }}
        </button>
        <button 
          v-else-if="canPurchase && !user"
          class="action-button buy-button disabled-button"
          disabled
          title="Login para comprar"
        >
          Login para comprar
        </button>
        <button 
          v-else-if="cosmetic.isOwned && !isRefunding"
          class="action-button refund-button"
          @click="handleRefund"
          :disabled="isRefunding"
        >
          Devolver
        </button>
        <button 
          v-else-if="cosmetic.isOwned && isRefunding"
          class="action-button refund-button"
          disabled
        >
          Devolvendo...
        </button>
        <button v-else class="action-button disabled-button" disabled>
          Indisponível
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, ref } from 'vue';
import logoMoeda from '../../../assets/svg/logomoeda.svg';
import { useCosmetics } from '../../../composables/useCosmetics';
import { useAuth } from '../../../composables/useAuth';

const props = defineProps({
  cosmetic: {
    type: Object,
    default: null
  }
});

const emit = defineEmits(['close', 'purchase-success', 'refund-success']);

const { purchaseCosmetic, refundCosmetic, loadVBucks, searchCosmetics, error: cosmeticError } = useCosmetics();
const { user, updateVBucks } = useAuth();

const isPurchasing = ref(false);
const isRefunding = ref(false);

const getTypeDisplay = () => {
  if (!props.cosmetic?.type) return 'N/A';
  if (typeof props.cosmetic.type === 'object') {
    return props.cosmetic.type.displayValue || props.cosmetic.type.value || 'N/A';
  }
  return String(props.cosmetic.type);
};

const getRarityValue = () => {
  if (!props.cosmetic?.rarity) return 'common';
  if (typeof props.cosmetic.rarity === 'object') {
    return props.cosmetic.rarity.value?.toLowerCase() || 'common';
  }
  return String(props.cosmetic.rarity).toLowerCase();
};

const getRarityDisplay = () => {
  if (!props.cosmetic?.rarity) return 'COMMON';
  if (typeof props.cosmetic.rarity === 'object') {
    return props.cosmetic.rarity.displayValue?.toUpperCase() || props.cosmetic.rarity.value?.toUpperCase() || 'COMMON';
  }
  return String(props.cosmetic.rarity).toUpperCase();
};

const formatDate = (dateString) => {
  if (!dateString) return 'N/A';
  const date = new Date(dateString);
  return new Intl.DateTimeFormat('pt-BR', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
  }).format(date);
};

const formatCurrency = (value) => {
  if (value === null || value === undefined) return '0';
  return new Intl.NumberFormat('pt-BR').format(value);
};

const hasAdditionalImages = computed(() => {
  return props.cosmetic?.images?.icon || props.cosmetic?.images?.featured;
});

const getImageSrc = () => {
  if (!props.cosmetic?.images) return '/placeholder.png';
  // Tentar diferentes campos de imagem
  return props.cosmetic.images.smallIcon || 
         props.cosmetic.images.icon || 
         props.cosmetic.images.featured || 
         '/placeholder.png';
};

const handleImageError = (event) => {
  event.target.src = '/placeholder.png';
};

const canPurchase = computed(() => {
  if (!props.cosmetic) return false;
  
  // Verificar se tem preço (pode ser do shop ou aleatório)
  const price = props.cosmetic.price || props.cosmetic.regularPrice || 0;
  const hasPrice = price > 0;
  
  // Pode comprar se tem preço e não está possuído (não precisa estar no shop)
  return !props.cosmetic.isOwned && hasPrice;
});

const handlePurchase = async () => {
  if (!user.value) {
    alert('É necessário estar logado para comprar cosméticos');
    return;
  }

  if (!props.cosmetic) return;

  isPurchasing.value = true;
  try {
    let success = false;
    if (props.cosmetic.isBundle && props.cosmetic.bundleItems && props.cosmetic.bundleItems.length > 0) {
      const cosmeticsToPurchase = props.cosmetic.bundleItems.map(item => ({
        cosmeticId: item.id,
        cosmeticName: item.name || item.id,
        price: item.price
      }));
      const { cosmeticsAPI } = await import('../../../services/api');
      const result = await cosmeticsAPI.purchaseBundle(cosmeticsToPurchase, user.value.id);
      success = result.success;
      if (success && result.vbucks !== undefined) {
        updateVBucks(result.vbucks);
      }
    } else {
      success = await purchaseCosmetic(props.cosmetic.id, props.cosmetic.price, props.cosmetic.name);
    }
    
    if (success) {
      await loadVBucks();
      await searchCosmetics();
      alert('Cosmético adquirido com sucesso!');
      emit('purchase-success');
    } else {
      alert(cosmeticError.value || 'Erro ao comprar cosmético');
    }
  } catch (err) {
    console.error('Erro ao comprar cosmético:', err);
    alert(err.response?.data?.message || 'Erro ao comprar cosmético');
  } finally {
    isPurchasing.value = false;
  }
};

const handleRefund = async () => {
  if (!user.value) {
    alert('É necessário estar logado para devolver cosméticos');
    return;
  }

  if (!props.cosmetic) return;

  if (!confirm(`Tem certeza que deseja devolver "${props.cosmetic.name}"? Você receberá o valor pago em v-bucks.`)) {
    return;
  }

  isRefunding.value = true;
  try {
    const success = await refundCosmetic(props.cosmetic.id, props.cosmetic.name);
    if (success) {
      // O refundCosmetic já atualiza os V-Bucks e a wallet, mas garantimos aqui também
      await loadVBucks();
      // Atualizar wallet no header (o refundCosmetic já faz isso, mas garantimos aqui)
      const { cosmeticsAPI } = await import('../../../services/api');
      try {
        if (user.value) {
          const data = await cosmeticsAPI.getVBucks(user.value.id);
          if (data.vbucks !== undefined) {
            updateVBucks(data.vbucks);
            console.log('Wallet atualizada no modal após devolução:', data.vbucks);
          }
        }
      } catch (e) {
        console.warn('Erro ao atualizar wallet no modal:', e);
      }
      await searchCosmetics();
      alert('Cosmético devolvido com sucesso!');
      emit('refund-success');
    } else {
      alert(cosmeticError.value || 'Erro ao devolver cosmético');
    }
  } catch (err) {
    console.error('Erro ao devolver cosmético:', err);
    alert(err.response?.data?.message || 'Erro ao devolver cosmético');
  } finally {
    isRefunding.value = false;
  }
};
</script>

<style scoped>
.cosmetic-details-modal {
  color: #fff;
  max-width: 800px;
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
}

.details-content {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding: 8px;
}

.details-header {
  display: flex;
  gap: 24px;
  align-items: flex-start;
  padding-bottom: 24px;
  border-bottom: 2px solid #212554;
}

.details-image-container {
  position: relative;
  flex-shrink: 0;
}

.details-image {
  width: 200px;
  height: 200px;
  object-fit: contain;
  background: rgba(22, 26, 66, 0.5);
  border-radius: 12px;
  padding: 16px;
  border: 2px solid #212554;
}

.image-badges {
  position: absolute;
  top: 8px;
  right: 8px;
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 0.75rem;
  font-weight: 700;
  text-transform: uppercase;
}

.new-badge {
  background: linear-gradient(180deg, #4CAF50 0%, #81C784 100%);
  color: #fff;
}

.sale-badge {
  background: linear-gradient(180deg, #DA22FF 0%, #831499 100%);
  color: #fff;
}

.owned-badge {
  background: linear-gradient(180deg, #137BBE 0%, #12D8FA 100%);
  color: #fff;
}

.bundle-badge {
  background: linear-gradient(180deg, #9C27B0 0%, #E91E63 100%);
  color: #fff;
}

.details-title {
  flex: 1;
}

.cosmetic-name {
  font-size: 2rem;
  font-weight: 700;
  margin: 0 0 8px 0;
  color: #fff;
}

.cosmetic-id {
  font-size: 0.9rem;
  color: #aaa;
  font-family: monospace;
}

.details-section {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.section-title {
  font-size: 1.3rem;
  font-weight: 600;
  color: #00E0FF;
  margin: 0;
  border-bottom: 1px solid #212554;
  padding-bottom: 8px;
}

.info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
  gap: 16px;
}

.info-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.info-label {
  font-size: 0.9rem;
  color: #aaa;
  font-weight: 500;
}

.info-value {
  font-size: 1.1rem;
  color: #fff;
  font-weight: 600;
}

.rarity-value {
  text-transform: uppercase;
}

.status-tag {
  display: inline-block;
  padding: 4px 12px;
  border-radius: 12px;
  font-size: 0.85rem;
  font-weight: 600;
  margin-right: 8px;
}

.status-tag.new {
  background: rgba(76, 175, 80, 0.2);
  color: #4CAF50;
  border: 1px solid #4CAF50;
}

.status-tag.shop {
  background: rgba(255, 152, 0, 0.2);
  color: #FF9800;
  border: 1px solid #FF9800;
}

.status-tag.sale {
  background: rgba(218, 34, 255, 0.2);
  color: #DA22FF;
  border: 1px solid #DA22FF;
}

.status-tag.owned {
  background: rgba(19, 123, 190, 0.2);
  color: #12D8FA;
  border: 1px solid #12D8FA;
}

.status-tag.unavailable {
  background: rgba(128, 128, 128, 0.2);
  color: #808080;
  border: 1px solid #808080;
}

.price-section {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.price-display {
  display: flex;
  align-items: center;
  gap: 16px;
}

.vbucks-icon-large {
  width: 48px;
  height: 48px;
}

.price-values {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.original-price-large {
  font-size: 1.2rem;
  color: #aaa;
  text-decoration: line-through;
}

.current-price-large {
  font-size: 2rem;
  font-weight: 700;
  color: #00E0FF;
}

.discount-info {
  color: #4CAF50;
  font-weight: 600;
  font-size: 1rem;
}

.bundle-items-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.bundle-item {
  padding: 12px 16px;
  background: rgba(22, 26, 66, 0.5);
  border: 1px solid #212554;
  border-radius: 8px;
}

.bundle-item-info {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.bundle-item-name {
  color: #fff;
  font-weight: 500;
}

.bundle-item-price {
  color: #00E0FF;
  font-weight: 600;
}

.bundle-total {
  margin-top: 8px;
  padding-top: 12px;
  border-top: 2px solid #212554;
  color: #00E0FF;
  font-size: 1.2rem;
}

.images-gallery {
  display: flex;
  gap: 16px;
  flex-wrap: wrap;
}

.gallery-image {
  width: 150px;
  height: 150px;
  object-fit: contain;
  background: rgba(22, 26, 66, 0.5);
  border-radius: 8px;
  padding: 12px;
  border: 1px solid #212554;
}

.details-actions {
  display: flex;
  gap: 15px;
  margin-top: 20px;
  padding-top: 20px;
  border-top: 1px solid #212554;
}

.action-button {
  flex: 1;
  padding: 12px 20px;
  border-radius: 10px;
  font-size: 1rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  border: 2px solid;
}

.buy-button {
  background: linear-gradient(162deg, #1fa2ff 22.61%, #12d8fa 118.29%);
  border-color: #12d8fa;
  color: #000;
}

.buy-button:hover:not(:disabled) {
  opacity: 0.9;
  box-shadow: 0 5px 15px rgba(18, 216, 250, 0.4);
}

.refund-button {
  background: linear-gradient(180deg, #f44336 0%, #e91e63 100%);
  border-color: #f44336;
  color: #fff;
}

.refund-button:hover:not(:disabled) {
  opacity: 0.9;
  box-shadow: 0 5px 15px rgba(244, 67, 54, 0.4);
}

.disabled-button {
  background-color: #3a3a3a;
  border-color: #555;
  color: #888;
  cursor: not-allowed;
}

.action-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.buy-button.disabled-button {
  background: #2a2a2a !important;
  border-color: #555 !important;
  color: #888 !important;
  box-shadow: none !important;
}

.buy-button.disabled-button:hover {
  background: #2a2a2a !important;
  border-color: #555 !important;
  opacity: 0.6 !important;
}

@media (max-width: 768px) {
  .details-header {
    flex-direction: column;
  }

  .details-image {
    width: 150px;
    height: 150px;
  }

  .info-grid {
    grid-template-columns: 1fr;
  }

  .details-actions {
    flex-direction: column;
  }
}
</style>

