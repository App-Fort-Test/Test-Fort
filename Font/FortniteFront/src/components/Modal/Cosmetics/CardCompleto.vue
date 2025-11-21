<script setup>
import { ref, onMounted, watch } from 'vue';
import Card from './Card.vue';
import PaginationMaster from '../../Pagination/PaginationMaster.vue';
import CosmeticDetailsModal from './CosmeticDetailsModal.vue';
import BaseModal from '../User/BaseModal.vue';
import { useCosmetics } from '../../../composables/useCosmetics';

const { 
    cosmetics, 
    loading, 
    error, 
    vbucks, 
    currentPage, 
    pageSize, 
    totalItems, 
    sortBy,
    filters, 
    searchCosmetics, 
    purchaseCosmetic, 
    refundCosmetic,
    loadVBucks,
    prefetchNextPage,
    clearCache
} = useCosmetics();

const selectedTab = ref('todos');

const isPurchasing = ref(false);
const purchasingId = ref(null);
const isRefunding = ref(false);
const refundingId = ref(null);
const showDetailsModal = ref(false);
const selectedCosmetic = ref(null);

// Flag para evitar loop infinito entre watch de filtros e tabs
let isTabChanging = false;

onMounted(async () => {
    await loadVBucks();
    await searchCosmetics();
    // O pré-carregamento múltiplo será feito automaticamente após searchCosmetics
    // quando estiver na página 1
});

// Observar mudanças nos filtros (exceto quando mudança vem da tab)
watch(() => filters, async () => {
    // Não buscar se a mudança veio da tab
    if (!isTabChanging) {
        currentPage.value = 1;
        // Limpar cache quando filtros mudarem
        clearCache();
        // Se filtros foram alterados manualmente, resetar tab para 'todos'
        if (selectedTab.value !== 'todos') {
            selectedTab.value = 'todos';
        } else {
            await searchCosmetics(true); // Forçar refresh
        }
    }
    // Não resetar isTabChanging aqui, pois ele é controlado pelo watch da tab
}, { deep: true });

// Observar mudanças na página - IMPORTANTE: sempre buscar novos dados
watch(() => currentPage.value, async (newPage, oldPage) => {
    if (newPage !== oldPage) {
        console.log(`Mudando de página ${oldPage} para ${newPage}`);
        // Não forçar refresh, usar cache se disponível
        await searchCosmetics(false);
        // Pré-carregar próxima página após mudança
        prefetchNextPage();
    }
}, { immediate: false });

// Observar mudanças no tamanho da página
watch(() => pageSize.value, async (newSize, oldSize) => {
    if (newSize !== oldSize) {
        currentPage.value = 1;
        clearCache(); // Limpar cache quando tamanho da página mudar
        await searchCosmetics(true); // Forçar refresh
    }
});

// Observar mudanças na ordenação
watch(() => sortBy.value, async (newSort, oldSort) => {
    if (newSort !== oldSort) {
        currentPage.value = 1;
        clearCache(); // Limpar cache quando ordenação mudar
        await searchCosmetics(true); // Forçar refresh
    }
});

// Observar mudanças na tab selecionada
watch(() => selectedTab.value, async (newTab, oldTab) => {
    if (newTab !== oldTab) {
        isTabChanging = true;
        currentPage.value = 1;
        clearCache(); // Limpar cache quando tab mudar
        // Atualizar filtros baseado na tab
        if (newTab === 'novos') {
            filters.onlyNew = true;
            filters.onlyInShop = false;
            filters.onlyOnSale = false;
            filters.onlyOwned = false;
            filters.onlyBundle = false;
        } else if (newTab === 'promocao') {
            filters.onlyNew = false;
            filters.onlyInShop = false;
            filters.onlyOnSale = true;
            filters.onlyOwned = false;
            filters.onlyBundle = false;
        } else if (newTab === 'bundle') {
            filters.onlyNew = false;
            filters.onlyInShop = false;
            filters.onlyOnSale = false;
            filters.onlyOwned = false;
            filters.onlyBundle = true;
        } else if (newTab === 'possuido') {
            filters.onlyNew = false;
            filters.onlyInShop = false;
            filters.onlyOnSale = false;
            filters.onlyOwned = true;
            filters.onlyBundle = false;
        } else {
            // Tab "todos" - limpar todos os filtros de tab
            filters.onlyNew = false;
            filters.onlyInShop = false;
            filters.onlyOnSale = false;
            filters.onlyOwned = false;
            filters.onlyBundle = false;
        }
        // Aguardar um tick para garantir que os filtros foram atualizados
        await searchCosmetics(true); // Forçar refresh
        isTabChanging = false;
    }
});

const handlePurchase = async (cosmeticId, price, cosmeticName, isBundle = false, bundleItems = null) => {
    // Se for bundle, usar endpoint específico
    if (isBundle && bundleItems && bundleItems.length > 0) {
        const { useAuth } = await import('../../../composables/useAuth');
        const { user } = useAuth();
        
        if (!user.value) {
            alert('É necessário estar logado para comprar bundles');
            return;
        }
        
        isPurchasing.value = true;
        purchasingId.value = cosmeticId;
        
        try {
            const { cosmeticsAPI } = await import('../../../services/api');
            const cosmetics = bundleItems.map(item => ({
                cosmeticId: item.id,
                cosmeticName: item.name || item.id,
                price: item.price || (price / bundleItems.length)
            }));
            
            const result = await cosmeticsAPI.purchaseBundle(cosmetics, user.value.id);
            
            if (result.success) {
                await searchCosmetics();
                await loadVBucks();
                // Atualizar v-bucks no header também
                const { updateVBucks } = useAuth();
                if (result.vbucks !== undefined) {
                    updateVBucks(result.vbucks);
                }
                alert('Bundle adquirido com sucesso!');
            } else {
                alert(result.message || 'Erro ao comprar bundle');
            }
        } catch (error) {
            console.error('Erro ao comprar bundle:', error);
            alert(error.response?.data?.message || 'Erro ao comprar bundle');
        } finally {
            isPurchasing.value = false;
            purchasingId.value = null;
        }
        return;
    }
    
    // Compra normal de item único
    isPurchasing.value = true;
    purchasingId.value = cosmeticId;
    try {
        const success = await purchaseCosmetic(cosmeticId, price, cosmeticName);
        if (success) {
            await searchCosmetics();
            await loadVBucks();
        }
    } finally {
        isPurchasing.value = false;
        purchasingId.value = null;
    }
};

const handleRefund = async (cosmeticId, cosmeticName) => {
    if (!confirm(`Tem certeza que deseja devolver "${cosmeticName}"? Você receberá o valor pago em v-bucks.`)) {
        return;
    }
    
    isRefunding.value = true;
    refundingId.value = cosmeticId;
    try {
        const success = await refundCosmetic(cosmeticId, cosmeticName);
        
        if (success) {
            // Atualizar v-bucks no header também
            const { useAuth } = await import('../../../composables/useAuth');
            const { updateVBucks } = useAuth();
            await loadVBucks();
            if (vbucks.value) {
                updateVBucks(vbucks.value);
            }
            await searchCosmetics();
            alert('Cosmético devolvido com sucesso!');
        } else {
            alert(error.value || 'Erro ao devolver cosmético');
        }
    } catch (err) {
        console.error('Erro ao devolver cosmético:', err);
        alert(err.response?.data?.message || 'Erro ao devolver cosmético');
    } finally {
        isRefunding.value = false;
        refundingId.value = null;
    }
};

const handlePageChange = (page) => {
    console.log(`handlePageChange chamado: página ${page}`);
    currentPage.value = page;
    // A busca será feita pelo watch acima
};

const handlePageSizeChange = (size) => {
    console.log(`handlePageSizeChange chamado: tamanho ${size}`);
    pageSize.value = size;
    // A busca será feita pelo watch acima
};

const handleSortChange = (sort) => {
    console.log(`handleSortChange chamado: ordenação ${sort}`);
    sortBy.value = sort;
    // A busca será feita pelo watch acima
};

const handleTabChange = (tab) => {
    console.log(`handleTabChange chamado: tab ${tab}`);
    selectedTab.value = tab;
    // A busca será feita pelo watch acima
};

const handleShowDetails = (cosmetic) => {
    selectedCosmetic.value = cosmetic;
    showDetailsModal.value = true;
};

const handleCloseDetails = () => {
    showDetailsModal.value = false;
    selectedCosmetic.value = null;
};
</script>

<template>
    <div class="filter-master">
        <div class="content-wrapper">
            <PaginationMaster 
                :currentPage="currentPage"
                :pageSize="pageSize"
                :totalItems="totalItems"
                :sortBy="sortBy"
                :selectedTab="selectedTab"
                @update:currentPage="handlePageChange"
                @update:pageSize="handlePageSizeChange"
                @update:sortBy="handleSortChange"
                @update:selectedTab="handleTabChange"
            />
            
            <div v-if="loading" class="loading-container">
                <p>Carregando...</p>
            </div>
            
            <div v-else-if="error" class="error-container">
                <p>Erro: {{ error }}</p>
            </div>
            
            <div v-else class="cards-grid">
                <!-- Exibir apenas os itens retornados pela API para esta página -->
                <Card 
                    v-for="cosmetic in cosmetics" 
                    :key="cosmetic.id" 
                    :cosmetic="cosmetic"
                    :is-purchasing="isPurchasing && purchasingId === cosmetic.id"
                    :is-refunding="isRefunding && refundingId === cosmetic.id"
                    @purchase="(id, price, name, isBundle, bundleItems) => handlePurchase(id, price, name, isBundle, bundleItems)"
                    @refund="(id, name) => handleRefund(id, name)"
                    @show-details="handleShowDetails"
                />
            </div>
            
            <div v-if="!loading && !error && cosmetics.length === 0" class="no-results">
                <p>Nenhum cosmético encontrado.</p>
            </div>
            
            <!-- Debug: mostrar informações da paginação -->
            <div v-if="!loading" class="debug-info" style="color: #888; font-size: 12px; margin-top: 16px;">
                Página: {{ currentPage }} | Itens por página: {{ pageSize }} | 
                Itens exibidos: {{ cosmetics.length }} | Total de itens: {{ totalItems }}
            </div>
        </div>

        <!-- Modal de Detalhes -->
        <BaseModal v-if="showDetailsModal" @close="handleCloseDetails">
            <CosmeticDetailsModal 
                :cosmetic="selectedCosmetic" 
                @purchase-success="handleCloseDetails"
                @refund-success="handleCloseDetails"
            />
        </BaseModal>
    </div>
</template>

<style scoped>
.filter-master {
    width: 100%;
}

.content-wrapper {
    display: flex;
    flex-direction: column;
    gap: 24px;
}


.cards-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(223px, 1fr));
    gap: 39px; 
    width: 100%;
    justify-items: stretch; 
}

.loading-container,
.error-container,
.no-results {
    text-align: center;
    padding: 40px;
    color: #fff;
}

.no-results p {
    font-size: 18px;
    color: #888;
}

@media (max-width: 768px) {
    .cards-grid {
        grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
        gap: 16px;
    }
}
</style>