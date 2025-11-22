<template>
    <div class="header-wrapper">
        <div class="navbar1">

            <div class="nav-top">
                <div class="logo">
                    <img :src="logo" alt="Fortnite Logo" class="fortnite-logo" />
                </div>

                <div class="action">
                    <HeaderLogin 
                        v-if="isAuthenticated" 
                        :user="user"
                        @logout="handleLogout"
                    />
                    <template v-else>
                        <button class="secondarybutton" @click="openModal('signup')">Criar Conta</button>
                        <button class="primarybutton" @click="openModal('login')">Entrar</button>
                    </template>
                </div>
            </div>

        </div>

        <div class="navbar2">
            <nav>
                <ul>
                    <li :class="{ 'is-active': props.activeRoute === 'loja' }">
                        <a href="#" @click.prevent="setActive('loja')">Loja</a>
                    </li>

                    <li :class="{ 'is-active': props.activeRoute === 'usuarios' }">
                        <a href="#" @click.prevent="setActive('usuarios')">Usuários</a>
                    </li>

                    <li v-if="isAuthenticated" :class="{ 'is-active': props.activeRoute === 'historico' }">
                        <a href="#" @click.prevent="setActive('historico')">Histórico</a>
                    </li>
                </ul>
            </nav>
        </div>

        <BaseModal v-if="activeModal === 'login'" @close="closeModal">
            <LoginModal 
                @switch-to-signup="switchToSignup"
                @login-success="handleLoginSuccess"
                @close="closeModal"
            />
        </BaseModal>

        <BaseModal v-if="activeModal === 'signup'" @close="closeModal">
            <SignupModal 
                @signup-success="handleSignupSuccess"
                @close="closeModal"
            />
        </BaseModal>

    </div>
</template>

<script setup>
import { ref, watch, computed } from 'vue';
import { useAuth } from '../../composables/useAuth';
import BaseModal from '../Modal/User/BaseModal.vue';
import LoginModal from '../Modal/User/LoginModal.vue';
import SignupModal from '../Modal/User/SignupModal.vue'; 
import HeaderLogin from './HeaderLogin.vue';
import logo from '../../assets/svg/logo.svg';

const props = defineProps({
  activeRoute: {
    type: String,
    default: 'loja'
  }
});

const { user, isAuthenticated, logout } = useAuth();
const activeModal = ref(null);

const emit = defineEmits(['route-change', 'user-changed']);

const setActive = (route) => {
    emit('route-change', route); 
};

const openModal = (type) => {
    activeModal.value = type;
};

const closeModal = () => {
    activeModal.value = null;
};

const switchToSignup = () => {
    closeModal();
    openModal('signup');
};

const handleLoginSuccess = async () => {
    closeModal();
    // Atualizar v-bucks após login
    try {
        const { cosmeticsAPI } = await import('../../services/api');
        if (user.value) {
            const data = await cosmeticsAPI.getVBucks(user.value.id);
            if (data.vbucks !== undefined) {
                user.value.vbucks = data.vbucks;
                localStorage.setItem('user', JSON.stringify(user.value));
            }
        }
    } catch (error) {
        console.error('Erro ao carregar v-bucks:', error);
    }
    emit('user-changed');
};

const handleSignupSuccess = () => {
    closeModal();
    emit('user-changed');
};

const handleLogout = () => {
    logout();
    emit('user-changed');
};

// Observar mudanças na autenticação
watch(isAuthenticated, () => {
    emit('user-changed');
});

</script>

<style scoped>

.header-wrapper {
    width: 100%;

}

.navbar1 {
    width: 100%;
    height: 48px;
    background: #0b0d21;
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 36px 64px;
    border-bottom: 2px solid #312E81;
    flex-direction: row;
    box-sizing: border-box;
}

.nav-top {
    width: 100%;
    display: flex;
    align-items: center;
    gap: 24px;
}

.action {
    width: auto;
    margin-left: auto;
    display: flex;
    gap: 12px;
}

.secondarybutton, .primarybutton {
    cursor: pointer;
    transition: background-color 0.2s;
   
}

.navbar2 {
    width: 100%;
    background: rgba(0,69, 138, 0.20);
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 0 64px;
    box-sizing: border-box;
    flex-direction: row;
}

.navbar2 ul {
    display: flex;
    gap: 32px;
    font-size: 12px;
    list-style: none;
    margin: 0;
    padding: 0;
}

.navbar2 ul li {
    padding: 12px 0; 
    transition: all 0.2s;
}

.navbar2 ul li a {
    text-decoration: none;
    color: #fff; 
    transition: color 0.2s;
}

.navbar2 ul li a:hover {
    color: #12D8FA;
}


.navbar2 ul li.is-active {
    border-bottom: 2px solid #12D8FA; 
}

.navbar2 ul li.is-active a {
    color: #12D8FA; 
    font-weight: bold;
}

</style>