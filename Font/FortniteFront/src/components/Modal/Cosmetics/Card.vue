<template>
    <div 
        class="card" 
        :class="`rarity-${normalizedRarity}`"
        :style="{
            background: rarityColors.gradient,
            borderColor: rarityColors.border
        }"
    >
        <div class="card-header">
            <div class="date-tag" :style="{ backgroundImage: `url(${fundoImage})` }">{{ formattedDate }}</div>
            <div class="rarity-tag">{{ rarityLabel }}</div>
        </div>

        <div class="image-container">
            <img 
                :src="imageSrc" 
                :alt="itemNameFull"
                class="item-image" 
                width="256" 
                height="256"
                @error="handleImageError"
            >

            <div class="overlay-tags">
                <div class="price-badge">{{ typeDisplay }}</div>
                <div v-if="cosmetic?.isNew" class="discont-badge new-badge">NOVO</div>
                <div v-else-if="cosmetic?.isOnSale" class="discont-badge">PROMOÇÃO</div>
                <div v-if="cosmetic?.isOwned" class="discont-badge owned-badge">ADQUIRIDO</div>
                <div v-if="cosmetic?.isInShop && !cosmetic?.isOwned" class="discont-badge shop-badge">PROMOÇÃO</div>
            </div>
        </div>

        <div class="info-section">
            <div 
                class="item-name" 
                :title="showTooltip ? itemNameFull : ''"
                :class="{ 'has-tooltip': showTooltip }"
            >
                {{ itemName }}
            </div>

            <div class="price-row">
                <span class="vbucks-icon">
                    <img 
                        :src="logoMoeda" 
                        alt="V-Bucks Icon" 
                        class="vbucks-image" 
                        width="30" 
                        height="36" 
                    />
                </span>
                <div class="price-amount">
                    <span v-if="hasDiscount" class="original-price">{{ regularPrice }} V-Bucks</span>
                    <span class="discounted-price">{{ displayPrice }} V-Bucks</span>
                </div>
            </div>
        </div>

        <div class="bottons-actions">
            <button 
                v-if="canPurchase && user"
                class="buy-button"
                @click="handlePurchase"
                :disabled="isPurchasing"
            >
                {{ isPurchasing ? 'Comprando...' : 'Comprar' }}
            </button>
            <button 
                v-else-if="canPurchase && !user"
                class="buy-button"
                disabled
                title="Login para comprar"
            >
                Login para comprar
            </button>
            <button 
                v-else-if="cosmetic?.isOwned && !isRefunding"
                class="buy-button refund-button"
                @click="handleRefund"
            >
                Devolver
            </button>
            <button 
                v-else-if="cosmetic?.isOwned && isRefunding"
                class="buy-button refund-button"
                disabled
            >
                Devolvendo...
            </button>
            <button v-else class="buy-button" disabled>
                Indisponível
            </button>
            <button class="details-button" @click="handleShowDetails">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">
                    <path
                        d="M12 16V12M12 8H12.01M22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2C17.5228 2 22 6.47715 22 12Z"
                        stroke="white" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
            </button>
        </div>
    </div>
</template>

<script setup>
import { computed } from 'vue';
import { useAuth } from '../../../composables/useAuth';
import logoMoeda from '../../../assets/svg/logomoeda.svg';
import fundoComum from '../../../assets/svg/fundocomum.svg';
import fundoIncomum from '../../../assets/svg/fundoincomum.svg';
import fundoRaro from '../../../assets/svg/fundoraro.svg';
import fundoEpico from '../../../assets/svg/fundoepico.svg';
import fundoLegendario from '../../../assets/svg/fundolegendario.svg';
import fundoMystico from '../../../assets/svg/fundomystico.svg';
import amostraCard from '../../../assets/png/amostracard.png';

const { user } = useAuth();

const props = defineProps({
    // Prop original para compatibilidade
    rarity: {
        type: String,
        default: 'common',
        validator: (value) => ['common', 'uncommon', 'rare', 'epic', 'legendary', 'mythic'].includes(value)
    },
    hasDiscount: {
        type: Boolean,
        default: false
    },
    // Nova prop para dados da API
    cosmetic: {
        type: Object,
        default: null
    },
    isPurchasing: {
        type: Boolean,
        default: false
    },
    isRefunding: {
        type: Boolean,
        default: false
    }
});

const emit = defineEmits(['purchase', 'refund', 'show-details']);

const isRefunding = computed(() => props.isRefunding || false);

// Função para gerar cor de fundo baseada no nome da raridade
const generateRarityColor = (rarityName) => {
    const rarityLower = rarityName.toLowerCase();
    
    // Mapeamento de cores para raridades conhecidas
    const colorMap = {
        common: { gradient: 'linear-gradient(162deg, #B0B0B0 22.61%, #808080 118.29%)', border: '#808080' },
        uncommon: { gradient: 'linear-gradient(162deg, #00FF00 22.61%, #00CC00 118.29%)', border: '#00CC00' },
        rare: { gradient: 'linear-gradient(162deg, #0080FF 22.61%, #0066CC 118.29%)', border: '#0066CC' },
        epic: { gradient: 'linear-gradient(162deg, #8000FF 22.61%, #6600CC 118.29%)', border: '#6600CC' },
        legendary: { gradient: 'linear-gradient(162deg, #FF8000 22.61%, #CC6600 118.29%)', border: '#CC6600' },
        mythic: { gradient: 'linear-gradient(162deg, #FF0080 22.61%, #CC0066 118.29%)', border: '#CC0066' },
        gaminglegends: { gradient: 'linear-gradient(162deg, #FFD700 22.61%, #FFA500 118.29%)', border: '#FFA500' },
        'gaming legends': { gradient: 'linear-gradient(162deg, #FFD700 22.61%, #FFA500 118.29%)', border: '#FFA500' },
        marvel: { gradient: 'linear-gradient(162deg, #FF0000 22.61%, #CC0000 118.29%)', border: '#CC0000' },
        'marvel series': { gradient: 'linear-gradient(162deg, #FF0000 22.61%, #CC0000 118.29%)', border: '#CC0000' },
        icon: { gradient: 'linear-gradient(162deg, #00FFFF 22.61%, #00CCCC 118.29%)', border: '#00CCCC' },
        'icon series': { gradient: 'linear-gradient(162deg, #00FFFF 22.61%, #00CCCC 118.29%)', border: '#00CCCC' },
        dc: { gradient: 'linear-gradient(162deg, #0000FF 22.61%, #0000CC 118.29%)', border: '#0000CC' },
        'dc series': { gradient: 'linear-gradient(162deg, #0000FF 22.61%, #0000CC 118.29%)', border: '#0000CC' },
        starwars: { gradient: 'linear-gradient(162deg, #FFFF00 22.61%, #CCCC00 118.29%)', border: '#CCCC00' },
        'star wars': { gradient: 'linear-gradient(162deg, #FFFF00 22.61%, #CCCC00 118.29%)', border: '#CCCC00' },
        frozen: { gradient: 'linear-gradient(162deg, #00FFFF 22.61%, #00CCFF 118.29%)', border: '#00CCFF' },
        lava: { gradient: 'linear-gradient(162deg, #FF4500 22.61%, #CC3700 118.29%)', border: '#CC3700' },
        dark: { gradient: 'linear-gradient(162deg, #2C2C2C 22.61%, #1A1A1A 118.29%)', border: '#1A1A1A' },
        shadow: { gradient: 'linear-gradient(162deg, #4B0082 22.61%, #3D0066 118.29%)', border: '#3D0066' },
        slurp: { gradient: 'linear-gradient(162deg, #00D4FF 22.61%, #00A8CC 118.29%)', border: '#00A8CC' },
        slurpseries: { gradient: 'linear-gradient(162deg, #00D4FF 22.61%, #00A8CC 118.29%)', border: '#00A8CC' },
        'slurp series': { gradient: 'linear-gradient(162deg, #00D4FF 22.61%, #00A8CC 118.29%)', border: '#00A8CC' },
    };
    
    // Se já existe no mapa, retorna
    if (colorMap[rarityLower]) {
        return colorMap[rarityLower];
    }
    
    // Gera cor baseada no hash do nome (para consistência)
    const hash = rarityLower.split('').reduce((acc, char) => {
        return ((acc << 5) - acc) + char.charCodeAt(0);
    }, 0);
    
    // Gera cores baseadas no hash
    const hue = Math.abs(hash) % 360;
    const saturation = 60 + (Math.abs(hash) % 20); // 60-80%
    const lightness = 40 + (Math.abs(hash) % 20); // 40-60%
    
    const color1 = `hsl(${hue}, ${saturation}%, ${lightness}%)`;
    const color2 = `hsl(${hue}, ${saturation}%, ${lightness - 15}%)`;
    const borderColor = `hsl(${hue}, ${saturation}%, ${lightness - 10}%)`;
    
    return {
        gradient: `linear-gradient(162deg, ${color1} 22.61%, ${color2} 118.29%)`,
        border: borderColor
    };
};

// Configuração base de raridades conhecidas
const baseRarityConfig = {
    common: { label: 'COMMON', fundo: fundoComum },
    uncommon: { label: 'UNCOMMON', fundo: fundoIncomum },
    rare: { label: 'RARE', fundo: fundoRaro },
    epic: { label: 'EPIC', fundo: fundoEpico },
    legendary: { label: 'LEGENDARY', fundo: fundoLegendario },
    mythic: { label: 'MYTHIC', fundo: fundoMystico },
    gaminglegends: { label: 'GAMING LEGENDS', fundo: fundoEpico },
    marvel: { label: 'MARVEL SERIES', fundo: fundoEpico },
    icon: { label: 'ICON SERIES', fundo: fundoRaro },
};

// Cache dinâmico para raridades descobertas
const dynamicRarityCache = new Map();

// Função para obter ou criar configuração de raridade
const getRarityConfig = (rarityKey) => {
    const key = rarityKey.toLowerCase();
    
    // Se já está no cache, retorna
    if (dynamicRarityCache.has(key)) {
        return dynamicRarityCache.get(key);
    }
    
    // Se está na config base, retorna
    if (baseRarityConfig[key]) {
        dynamicRarityCache.set(key, baseRarityConfig[key]);
        return baseRarityConfig[key];
    }
    
    // Cria nova configuração dinamicamente
    const displayValue = key.split(/(?=[A-Z])/).map(w => w.charAt(0).toUpperCase() + w.slice(1)).join(' ');
    const colors = generateRarityColor(key);
    
    const newConfig = {
        label: displayValue.toUpperCase(),
        fundo: fundoComum, // Fallback para imagem
        colors: colors // Cores dinâmicas
    };
    
    dynamicRarityCache.set(key, newConfig);
    return newConfig;
};

const rarityConfig = new Proxy(baseRarityConfig, {
    get(target, prop) {
        const key = String(prop).toLowerCase();
        return getRarityConfig(key);
    }
});

// Normalizar raridade - usa cosmetic se disponível, senão usa prop rarity
const normalizedRarity = computed(() => {
    let rarityValue = '';
    
    if (props.cosmetic?.rarity) {
        if (typeof props.cosmetic.rarity === 'object' && props.cosmetic.rarity.value) {
            rarityValue = props.cosmetic.rarity.value;
        } else {
            rarityValue = String(props.cosmetic.rarity);
        }
    } else {
        rarityValue = props.rarity || 'common';
    }
    
    // Normalizar: remover espaços, hífens, underscores e converter para lowercase
    return rarityValue
        .toLowerCase()
        .replace(/[\s\-_]/g, '')
        .trim();
});

const rarityLabel = computed(() => {
    const config = getRarityConfig(normalizedRarity.value);
    if (config && config.label) return config.label;
    
    // Fallback para displayValue da API
    if (props.cosmetic?.rarity && typeof props.cosmetic.rarity === 'object' && props.cosmetic.rarity.displayValue) {
        return props.cosmetic.rarity.displayValue.toUpperCase();
    }
    
    // Se não tem displayValue, formata o próprio valor
    const formatted = normalizedRarity.value
        .split(/(?=[A-Z])|[-_\s]/)
        .map(w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase())
        .join(' ');
    
    return formatted.toUpperCase() || 'COMMON';
});

const fundoImage = computed(() => {
    const config = getRarityConfig(normalizedRarity.value);
    return config?.fundo || fundoComum;
});

// Cores dinâmicas para o fundo do card
const rarityColors = computed(() => {
    const config = getRarityConfig(normalizedRarity.value);
    return config?.colors || generateRarityColor(normalizedRarity.value);
});

// Imagem - usa da API se disponível
const imageSrc = computed(() => {
    if (props.cosmetic?.images?.smallIcon) {
        return props.cosmetic.images.smallIcon;
    }
    return amostraCard;
});

// Data formatada
const formattedDate = computed(() => {
    if (props.cosmetic?.added) {
        const date = new Date(props.cosmetic.added);
        return new Intl.DateTimeFormat('pt-BR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric'
        }).format(date);
    }
    return '19/11/2025';
});

// Nome do item completo
const itemNameFull = computed(() => {
    return props.cosmetic?.name || 'Item Name';
});

// Nome do item truncado (máximo 20 caracteres)
const itemName = computed(() => {
    const name = itemNameFull.value;
    if (name.length > 20) {
        return name.substring(0, 20) + '...';
    }
    return name;
});

// Verificar se precisa mostrar tooltip
const showTooltip = computed(() => {
    return itemNameFull.value.length > 20;
});

// Tipo do item - usar displayValue se disponível, senão value
const typeDisplay = computed(() => {
    if (props.cosmetic?.type) {
        if (typeof props.cosmetic.type === 'object') {
            // Se displayValue não for "null", usar ele, senão usar value
            if (props.cosmetic.type.displayValue && props.cosmetic.type.displayValue !== 'null') {
                return props.cosmetic.type.displayValue.toUpperCase();
            }
            return props.cosmetic.type.value.toUpperCase();
        }
        return String(props.cosmetic.type).toUpperCase();
    }
    return 'OUTFIT';
});

// Preço
const displayPrice = computed(() => {
    return props.cosmetic?.price || props.cosmetic?.regularPrice || 0;
});

const regularPrice = computed(() => {
    return props.cosmetic?.regularPrice || props.cosmetic?.price || 0;
});

const hasDiscount = computed(() => {
    if (props.cosmetic) {
        return props.cosmetic.isOnSale && 
               props.cosmetic.regularPrice && 
               props.cosmetic.price && 
               props.cosmetic.regularPrice > props.cosmetic.price;
    }
    return props.hasDiscount;
});

// Pode comprar?
const canPurchase = computed(() => {
    if (!props.cosmetic) return false;
    
    // Verificar se tem preço (pode ser do shop ou aleatório)
    const price = props.cosmetic.price || props.cosmetic.regularPrice || 0;
    const hasPrice = price > 0;
    
    // Pode comprar se tem preço e não está possuído (não precisa estar no shop)
    return !props.cosmetic.isOwned && hasPrice;
});

const handlePurchase = () => {
    if (canPurchase.value && !props.isPurchasing && props.cosmetic) {
        emit('purchase', 
            props.cosmetic.id, 
            props.cosmetic.price, 
            props.cosmetic.name,
            props.cosmetic.isBundle || false,
            props.cosmetic.bundleItems || null
        );
    }
};

const handleRefund = () => {
    if (props.cosmetic?.isOwned && !isRefunding.value && props.cosmetic) {
        emit('refund', props.cosmetic.id, props.cosmetic.name);
    }
};

const handleShowDetails = () => {
    if (props.cosmetic) {
        emit('show-details', props.cosmetic);
    }
};

const handleImageError = (event) => {
    event.target.src = amostraCard;
};
</script>

<style scoped>
/* Manter TODOS os estilos existentes exatamente como estão */
.card {
    border-radius: 10px;
    width: 223px;
    height: 430px;
    padding: 16px;
    transition: transform 0.2s;
}

.card:hover {
    transform: translateY(-4px);
}

/* Estilos base para raridades conhecidas - podem ser sobrescritos pelo style inline */
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

/* Estilos dinâmicos para novas raridades serão aplicados via style inline */

.card-header {
    display: flex;
    justify-content: space-between;
    margin-bottom: 8px;
}

.date-tag {
    background-size: cover;
    width: 95px;
    height: 36px;
    justify-content: start;
    align-items: center;
    display: flex;
    border-radius: 8px;
    padding-left: 11px;
    font-size: 12px;
}

.rarity-tag {
    display: flex;
    border-radius: 10px;
    border: 1px solid #00458A;
    background: #00458A;
    box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.25);
    justify-content: center;
    align-items: center;
    width: 96px;
    height: 30px;
    color: #FFF;
    text-align: right;
    font-family: Poppins;
    font-size: 14px;
    font-style: italic;
    font-weight: 700;
    line-height: 24px;
}

.image-container {
    margin-top: -10px;
    width: 100%;
    height: 250px;
    flex-shrink: 0;
    background: #FFFFFF;
    border-radius: 10px;
    position: relative;
}

.item-image {
    width: 100%;
    height: 100%;
    object-fit: cover;
    border-radius: 10px;
}

.price-badge {
    position: absolute;
    bottom: 8px;
    left: 8px;
    width: 80px;
    height: 32px;
    border-radius: 16px;
    border: 2px solid #312E81;
    background: linear-gradient(162deg, #161A42 22.61%, #161A42 118.29%);
    box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.25) inset;
    display: flex;
    justify-content: center;
    align-items: center;
    color: #FAFAFB;
    font-size: 11px;
    font-style: bold;
    font-weight: 600;
    line-height: 1.2;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    padding: 0 6px;
}

.discont-badge {
    background: #DA22FF;
    border-radius: 16px;
    position: absolute;
    bottom: 8px;
    left: 8px;
    width: 80px;
    height: 32px;
    display: flex;
    margin-left: 120px;
    justify-content: center;
    align-items: center;
    border-radius: 16px;
    border: 2px solid #DA22FF;
    background: linear-gradient(180deg, #DA22FF 0%, #831499 100%);
    box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.25) inset;
    color: #fff;
    font-size: 12px;
    font-weight: 600;
}

.discont-badge.new-badge {
    background: linear-gradient(180deg, #4CAF50 0%, #81C784 100%);
    border-color: #4CAF50;
}

.discont-badge.owned-badge {
    background: linear-gradient(180deg, #137BBE 0%, #12D8FA 100%);
    border-color: #12D8FA;
}

.discont-badge.shop-badge {
    background: linear-gradient(180deg, #FF9800 0%, #FFB74D 100%);
    border-color: #FF9800;
}

.discont-badge.bundle-badge {
    background: linear-gradient(180deg, #9C27B0 0%, #E91E63 100%);
    border-color: #9C27B0;
}

.buy-button.refund-button {
    background: linear-gradient(180deg, #f44336 0%, #e91e63 100%);
    border-color: #f44336;
}

.buy-button.refund-button:hover:not(:disabled) {
    background: linear-gradient(180deg, #d32f2f 0%, #c2185b 100%);
    transform: translateY(-1px);
}

.buy-button.refund-button:disabled {
    opacity: 0.6;
    cursor: not-allowed;
}

.info-section {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.item-name {
    display: flex;
    align-items: center;
    margin-top: 12px;
    color: #FAFAFB;
    font-family: Poppins;
    font-size: 16px;
    font-weight: 600;
    cursor: default;
    position: relative;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.item-name.has-tooltip {
    cursor: help;
}

.item-name.has-tooltip:hover::after {
    content: attr(title);
    position: absolute;
    bottom: calc(100% + 8px);
    left: 50%;
    transform: translateX(-50%);
    background-color: rgba(0, 0, 0, 0.95);
    color: white;
    padding: 8px 12px;
    border-radius: 6px;
    font-size: 0.85rem;
    white-space: nowrap;
    z-index: 1000;
    pointer-events: none;
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
    font-family: Poppins;
    font-weight: 500;
    max-width: 300px;
    word-wrap: break-word;
    white-space: normal;
    text-align: center;
}

.item-name.has-tooltip:hover::before {
    content: '';
    position: absolute;
    bottom: calc(100% - 1px);
    left: 50%;
    transform: translateX(-50%);
    border: 6px solid transparent;
    border-top-color: rgba(0, 0, 0, 0.95);
    z-index: 1001;
    pointer-events: none;
}

.vbucks-image {
    width: 30px;
    height: 36px;
}

.price-row {
    display: flex;
    align-items: center;
    gap: 8px;
}

.price-amount {
    display: flex;
    flex-direction: column;
    line-height: 1.2;
}

.original-price {
    text-decoration: line-through;
    color: #FAFAFB;
    font-size: 10px;
    font-weight: 400;
}

.discounted-price {
    background: darkblue;
    background-clip: text;
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    font-size: 22px;
    font-weight: 700;
    margin-bottom: 2px;
}

.bottons-actions {
    display: flex;
    gap: 8px;
    margin-top: 8px;
    align-items: center;
    justify-content: flex-start;
    white-space: nowrap;
}

.buy-button {
    flex: 0;
    border-radius: 12px;
    border: 2px solid #12D8FA;
    background: #161A42;
    box-shadow: 0 13px 17px 0 rgba(18, 216, 250, 0.40);
    display: flex;
    height: 48px;
    padding: 12px 13px;
    justify-content: center;
    align-items: center;
    gap: 10px;
    flex: 1 0 0;
    width: 100%;
    color: var(--Greys-Blue-Grey-300, #F9FAFB);
    text-align: center;
    font-size: 16px;
    font-style: normal;
    font-weight: 400;
    line-height: normal;
    cursor: pointer;
}

.buy-button:hover:not(:disabled) {
    background: linear-gradient(162deg, #1fa2ff 22.61%, #12d8fa 118.29%);
    border: 2px solid #12d8fa;
    box-shadow: 0 13px 17px 0 rgba(18, 216, 250, 0.1);
    color: #000000;
}

.buy-button:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    background: #2a2a2a !important;
    border-color: #555 !important;
    color: #888 !important;
    box-shadow: none !important;
}

.buy-button:disabled:hover {
    background: #2a2a2a !important;
    border-color: #555 !important;
    transform: none !important;
}

.buy-button.owned-button {
    background: #4CAF50 !important;
    border-color: #4CAF50 !important;
}

.details-button {
    width: 48px;
    height: 48px;
    border-radius: 12px;
    border: 2px solid #312E81;
    background: #161A42;
    box-shadow: 0 4px 4px 0 rgba(0, 0, 0, 0.25);
    display: flex;
    justify-content: center;
    align-items: center;
    color: var(--Greys-Blue-Grey-300, #F9FAFB);
    text-align: center;
    font-style: normal;
    font-weight: 400;
    line-height: normal;
    cursor: pointer;
}

.details-button:hover {
    border: 2px solid #12d8fa;
    background: linear-gradient(162deg, #1c27a1 22.61%, rgba(22, 26, 66, 0) 118.29%);
    color: white;
}
</style>

