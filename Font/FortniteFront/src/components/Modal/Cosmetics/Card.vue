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
            <div 
                class="rarity-tag" 
                :title="rarityLabelFull.length > 8 ? rarityLabelFull : ''"
                :style="{ 
                    background: solidRarityColor,
                    borderColor: darkRarityColor,
                    color: textColor
                }"
            >{{ rarityLabelDisplay }}</div>
        </div>

        <div 
            class="image-container"
            :style="{
                background: solidRarityColor
            }"
        >
            <img 
                :src="imageSrc" 
                :alt="itemNameFull"
                class="item-image" 
                width="256" 
                height="256"
                @error="handleImageError"
            >

            <div class="overlay-tags">
                <div 
                    class="price-badge" 
                    :title="typeDisplayFull.length > 12 ? typeDisplayFull : ''"
                    :style="{ 
                        background: solidRarityColor,
                        borderColor: darkRarityColor,
                        color: textColor
                    }"
                >{{ typeDisplay }}</div>
                <!-- Badges empilhados no canto inferior direito -->
                <div 
                    v-if="cosmetic?.isOwned" 
                    class="discont-badge owned-badge bottom-badge status-badge"
                    :style="{ bottom: getStatusBadgePosition('owned') + 'px' }"
                >ADQUIRIDO</div>
                <div 
                    v-if="cosmetic?.isOnSale" 
                    class="discont-badge bottom-badge status-badge"
                    :style="{ bottom: getStatusBadgePosition('promo') + 'px' }"
                >PROMOÇÃO</div>
                <div 
                    v-if="cosmetic?.isNew" 
                    class="discont-badge new-badge bottom-badge status-badge"
                    :style="{ bottom: getStatusBadgePosition('new') + 'px' }"
                >NOVO</div>
                <div 
                    v-if="cosmetic?.isBundle" 
                    class="discont-badge bundle-badge bottom-badge status-badge"
                    :style="{ bottom: getStatusBadgePosition('bundle') + 'px' }"
                >BUNDLE</div>
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
                :style="{ 
                    background: solidRarityColor,
                    borderColor: darkRarityColor,
                    color: textColor
                }"
            >
                {{ isPurchasing ? 'Comprando...' : 'Comprar' }}
            </button>
            <button 
                v-else-if="canPurchase && !user"
                class="buy-button"
                disabled
                title="Login para comprar"
                :style="{ 
                    background: solidRarityColor,
                    borderColor: darkRarityColor,
                    color: textColor
                }"
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
            <button 
                class="details-button" 
                @click="handleShowDetails"
                :style="{ 
                    background: solidRarityColor,
                    borderColor: darkRarityColor,
                    color: textColor
                }"
            >
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none">
                    <path
                        :stroke="textColor"
                        d="M12 16V12M12 8H12.01M22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2C17.5228 2 22 6.47715 22 12Z"
                        stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round" />
                </svg>
            </button>
        </div>
    </div>
</template>

<script setup>
import { computed, ref, watch, onMounted } from 'vue';
import { useAuth } from '../../../composables/useAuth';
import logoMoeda from '../../../assets/svg/logomoeda.svg';
import fundoComum from '../../../assets/svg/fundocomum.svg';
import fundoIncomum from '../../../assets/svg/fundoincomum.svg';
import fundoRaro from '../../../assets/svg/fundoraro.svg';
import fundoEpico from '../../../assets/svg/fundoepico.svg';
import fundoLegendario from '../../../assets/svg/fundolegendario.svg';
import fundoMystico from '../../../assets/svg/fundomystico.svg';

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
        common: { gradient: 'linear-gradient(180deg, #808080 0%, #B0B0B0 50%, rgba(128, 128, 128, 0.70) 100%)', border: '#808080' },
        uncommon: { gradient: 'linear-gradient(180deg, #00CC00 0%, #00FF00 50%, rgba(0, 204, 0, 0.70) 100%)', border: '#00CC00' },
        rare: { gradient: 'linear-gradient(180deg, #137BBE 0%, #12D8FA 50%, rgba(19, 123, 190, 0.70) 100%)', border: '#0066CC' },
        epic: { gradient: 'linear-gradient(180deg, #6600CC 0%, #BA68C8 50%, rgba(102, 0, 204, 0.70) 100%)', border: '#6600CC' },
        legendary: { gradient: 'linear-gradient(180deg, #CC6600 0%, #FFB74D 50%, rgba(204, 102, 0, 0.70) 100%)', border: '#CC6600' },
        mythic: { gradient: 'linear-gradient(180deg, #CC0066 0%, #F06292 50%, rgba(204, 0, 102, 0.70) 100%)', border: '#CC0066' },
        gaminglegends: { gradient: 'linear-gradient(180deg, #FFA500 0%, #FFD700 50%, rgba(255, 165, 0, 0.70) 100%)', border: '#FFA500' },
        'gaming legends': { gradient: 'linear-gradient(180deg, #FFA500 0%, #FFD700 50%, rgba(255, 165, 0, 0.70) 100%)', border: '#FFA500' },
        marvel: { gradient: 'linear-gradient(180deg, #CC0000 0%, #FF3333 50%, rgba(204, 0, 0, 0.70) 100%)', border: '#CC0000' },
        'marvel series': { gradient: 'linear-gradient(180deg, #CC0000 0%, #FF3333 50%, rgba(204, 0, 0, 0.70) 100%)', border: '#CC0000' },
        icon: { gradient: 'linear-gradient(180deg, #00CCCC 0%, #00FFFF 50%, rgba(0, 204, 204, 0.70) 100%)', border: '#00CCCC' },
        'icon series': { gradient: 'linear-gradient(180deg, #00CCCC 0%, #00FFFF 50%, rgba(0, 204, 204, 0.70) 100%)', border: '#00CCCC' },
        dc: { gradient: 'linear-gradient(180deg, #0000CC 0%, #3333FF 50%, rgba(0, 0, 204, 0.70) 100%)', border: '#0000CC' },
        'dc series': { gradient: 'linear-gradient(180deg, #0000CC 0%, #3333FF 50%, rgba(0, 0, 204, 0.70) 100%)', border: '#0000CC' },
        starwars: { gradient: 'linear-gradient(180deg, #CCCC00 0%, #FFFF00 50%, rgba(204, 204, 0, 0.70) 100%)', border: '#CCCC00' },
        'star wars': { gradient: 'linear-gradient(180deg, #CCCC00 0%, #FFFF00 50%, rgba(204, 204, 0, 0.70) 100%)', border: '#CCCC00' },
        frozen: { gradient: 'linear-gradient(180deg, #00CCFF 0%, #00FFFF 50%, rgba(0, 204, 255, 0.70) 100%)', border: '#00CCFF' },
        lava: { gradient: 'linear-gradient(180deg, #CC3700 0%, #FF6633 50%, rgba(204, 55, 0, 0.70) 100%)', border: '#CC3700' },
        dark: { gradient: 'linear-gradient(180deg, #1A1A1A 0%, #2C2C2C 50%, rgba(26, 26, 26, 0.70) 100%)', border: '#1A1A1A' },
        shadow: { gradient: 'linear-gradient(180deg, #3D0066 0%, #6B00B3 50%, rgba(61, 0, 102, 0.70) 100%)', border: '#3D0066' },
        slurp: { gradient: 'linear-gradient(180deg, #00A8CC 0%, #00D4FF 50%, rgba(0, 168, 204, 0.70) 100%)', border: '#00A8CC' },
        slurpseries: { gradient: 'linear-gradient(180deg, #00A8CC 0%, #00D4FF 50%, rgba(0, 168, 204, 0.70) 100%)', border: '#00A8CC' },
        'slurp series': { gradient: 'linear-gradient(180deg, #00A8CC 0%, #00D4FF 50%, rgba(0, 168, 204, 0.70) 100%)', border: '#00A8CC' },
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
    
    // Converter HSL para RGB para criar o gradiente no formato correto
    const h = hue / 360;
    const s = saturation / 100;
    const l1 = lightness / 100;
    const l2 = Math.min(1, (lightness + 20) / 100); // Cor intermediária mais clara
    
    // Função auxiliar para converter HSL para RGB
    const hslToRgb = (h, s, l) => {
        let r, g, b;
        if (s === 0) {
            r = g = b = l;
        } else {
            const hue2rgb = (p, q, t) => {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1/6) return p + (q - p) * 6 * t;
                if (t < 1/2) return q;
                if (t < 2/3) return p + (q - p) * (2/3 - t) * 6;
                return p;
            };
            const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            const p = 2 * l - q;
            r = Math.round(hue2rgb(p, q, h + 1/3) * 255);
            g = Math.round(hue2rgb(p, q, h) * 255);
            b = Math.round(hue2rgb(p, q, h - 1/3) * 255);
        }
        return { r, g, b };
    };
    
    const rgb1 = hslToRgb(h, s, l1);
    const rgb2 = hslToRgb(h, s, l2);
    const borderRgb = hslToRgb(h, s, Math.max(0, (lightness - 10) / 100));
    
    const color1 = `rgb(${rgb1.r}, ${rgb1.g}, ${rgb1.b})`;
    const color2 = `rgb(${rgb2.r}, ${rgb2.g}, ${rgb2.b})`;
    const borderColor = `rgb(${borderRgb.r}, ${borderRgb.g}, ${borderRgb.b})`;
    
    return {
        gradient: `linear-gradient(180deg, ${color1} 0%, ${color2} 50%, rgba(${rgb1.r}, ${rgb1.g}, ${rgb1.b}, 0.70) 100%)`,
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

const rarityLabelFull = computed(() => {
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

// Raridade truncada se tiver mais de 8 caracteres
const rarityLabelDisplay = computed(() => {
    const fullRarity = rarityLabelFull.value;
    if (fullRarity.length > 8) {
        return fullRarity.substring(0, 8) + '...';
    }
    return fullRarity;
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

// Função para extrair cor sólida principal do gradiente
const getSolidColor = (gradient) => {
    // Extrair a primeira cor do gradiente (hexadecimal)
    const hexMatch = gradient.match(/#[0-9A-Fa-f]{6}/);
    if (hexMatch) {
        return hexMatch[0];
    }
    // Tentar extrair cor RGB
    const rgbMatch = gradient.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
    if (rgbMatch) {
        return `rgb(${rgbMatch[1]}, ${rgbMatch[2]}, ${rgbMatch[3]})`;
    }
    // Tentar extrair cor HSL
    const hslMatch = gradient.match(/hsl\(([^)]+)\)/);
    if (hslMatch) {
        return `hsl(${hslMatch[1]})`;
    }
    // Se não encontrar, usar a cor da borda
    return rarityColors.value.border || '#808080';
};

// Cor sólida principal baseada na raridade
const solidRarityColor = computed(() => {
    return getSolidColor(rarityColors.value.gradient);
});

// Cor sólida mais escura para bordas e elementos escuros
const darkRarityColor = computed(() => {
    return rarityColors.value.border || '#000000';
});

// Cor sólida mais clara para textos e elementos claros (extrair do gradiente)
const lightRarityColor = computed(() => {
    const gradient = rarityColors.value.gradient;
    // Extrair a primeira cor do gradiente (mais clara)
    const hexMatch = gradient.match(/#[0-9A-Fa-f]{6}/);
    if (hexMatch) {
        return hexMatch[0];
    }
    // Tentar extrair cor RGB
    const rgbMatch = gradient.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
    if (rgbMatch) {
        return `rgb(${rgbMatch[1]}, ${rgbMatch[2]}, ${rgbMatch[3]})`;
    }
    // Tentar extrair cor HSL
    const hslMatch = gradient.match(/hsl\(([^)]+)\)/);
    if (hslMatch) {
        return `hsl(${hslMatch[1]})`;
    }
    return '#FFFFFF';
});

// Função para calcular o brilho (luminância) de uma cor
const getLuminance = (color) => {
    let r, g, b;
    
    // Converter hex para RGB
    if (color.startsWith('#')) {
        const hex = color.slice(1);
        r = parseInt(hex.slice(0, 2), 16);
        g = parseInt(hex.slice(2, 4), 16);
        b = parseInt(hex.slice(4, 6), 16);
    } 
    // Converter RGB para valores
    else if (color.startsWith('rgb')) {
        const match = color.match(/rgb\((\d+),\s*(\d+),\s*(\d+)\)/);
        if (match) {
            r = parseInt(match[1]);
            g = parseInt(match[2]);
            b = parseInt(match[3]);
        } else {
            return 0.5; // Fallback
        }
    }
    // Converter HSL para RGB (aproximação)
    else if (color.startsWith('hsl')) {
        const match = color.match(/hsl\((\d+),\s*(\d+)%,\s*(\d+)%\)/);
        if (match) {
            const h = parseInt(match[1]) / 360;
            const s = parseInt(match[2]) / 100;
            const l = parseInt(match[3]) / 100;
            // Converter HSL para RGB
            const c = (1 - Math.abs(2 * l - 1)) * s;
            const x = c * (1 - Math.abs((h * 6) % 2 - 1));
            const m = l - c / 2;
            if (h < 1/6) { r = c; g = x; b = 0; }
            else if (h < 2/6) { r = x; g = c; b = 0; }
            else if (h < 3/6) { r = 0; g = c; b = x; }
            else if (h < 4/6) { r = 0; g = x; b = c; }
            else if (h < 5/6) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }
            r = Math.round((r + m) * 255);
            g = Math.round((g + m) * 255);
            b = Math.round((b + m) * 255);
        } else {
            return 0.5; // Fallback
        }
    } else {
        return 0.5; // Fallback
    }
    
    // Calcular luminância relativa (fórmula WCAG)
    const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255;
    return luminance;
};

// Cor do texto baseada no contraste com o fundo
const textColor = computed(() => {
    const bgColor = solidRarityColor.value;
    const luminance = getLuminance(bgColor);
    // Se o fundo for claro (luminância > 0.5), usar texto escuro, senão usar texto claro
    return luminance > 0.5 ? '#000000' : '#FFFFFF';
});

// Cor do texto para elementos com fundo escuro (bordas)
const textColorDark = computed(() => {
    return '#FFFFFF'; // Sempre branco para fundos escuros
});

// Imagem - usa da API se disponível
const imageSrc = computed(() => {
    if (!props.cosmetic?.images) return '/placeholder.png';
    // Tentar diferentes campos de imagem
    return props.cosmetic.images.smallIcon || 
           props.cosmetic.images.icon || 
           props.cosmetic.images.featured || 
           '/placeholder.png';
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

// Nome do item - o CSS vai truncar automaticamente
const itemName = computed(() => {
    return itemNameFull.value;
});

// Verificar se precisa mostrar tooltip (se o nome foi truncado pelo CSS)
const showTooltip = computed(() => {
    // O tooltip será mostrado se o nome for longo o suficiente para ser truncado
    // O CSS com text-overflow: ellipsis vai fazer o truncamento visual
    return itemNameFull.value.length > 0;
});

// Tipo do item - usar displayValue se disponível, senão value
const typeDisplayFull = computed(() => {
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

// Tipo truncado se tiver mais de 12 caracteres
const typeDisplay = computed(() => {
    const fullType = typeDisplayFull.value;
    if (fullType.length > 12) {
        return fullType.substring(0, 12) + '...';
    }
    return fullType;
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

// Função para calcular a posição do badge de status (empilhamento de baixo para cima)
const getStatusBadgePosition = (badgeType) => {
    const statusBadges = [];
    
    // Coletar todos os badges de status que estão visíveis, na ordem de empilhamento
    // Badge "ADQUIRIDO" sempre fica na base (posição mais baixa)
    if (props.cosmetic?.isOwned) statusBadges.push('owned');
    if (props.cosmetic?.isOnSale) statusBadges.push('promo');
    if (props.cosmetic?.isNew) statusBadges.push('new');
    if (props.cosmetic?.isBundle) statusBadges.push('bundle');
    if (props.cosmetic?.isAdquirir) statusBadges.push('adquirido');
    
    // Encontrar o índice do badge atual na lista
    const index = statusBadges.indexOf(badgeType);
    
    if (index === -1) return 8; // Se não encontrado, retorna posição padrão
    
    return 8 + (index * 40); // 32px (altura) + 8px (gap) = 40px
};


const canPurchase = computed(() => {
    if (!props.cosmetic) return false;
    
    if (props.cosmetic.isOwned) return false;
    
    const price = props.cosmetic.price || props.cosmetic.regularPrice || 0;
    const hasPrice = price > 0;
    
    return hasPrice;
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
    event.target.src = '/placeholder.png';
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
    height: 40px;
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
    border: 1px solid;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 100%;
    cursor: help;
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
    border-radius: 10px;
    position: relative;
    /* Background será aplicado via style binding para usar o gradiente da raridade */
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
    height: 36px;
    border-radius: 16px;
    border: 2px solid;
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
    max-width: 80px;
    cursor: help;
}

.discont-badge {
    background: #DA22FF;
    border-radius: 16px;
    position: absolute;
    right: 8px;
    width: 80px;
    height: 32px;
    display: flex;
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

.discont-badge.bottom-badge {
    bottom: 8px;
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
    font-size: 14px;
    font-weight: 600;
    cursor: default;
    position: relative;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    max-width: 100%;
    line-height: 1.3;
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
    color: #001F3F; /* Azul escuro fixo */
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
    border: 2px solid;
    box-shadow: 0 13px 17px 0 rgba(0, 0, 0, 0.40);
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
    box-shadow: 0 13px 17px 0 rgba(0, 0, 0, 0.1);
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
    border: 2px solid;
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
    color: white;
}
</style>

