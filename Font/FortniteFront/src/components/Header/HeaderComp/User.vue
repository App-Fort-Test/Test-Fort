<template>
    <div class="user-profile-card">
        <div class="profile-info">
            <div v-if="!profileImage" class="profile-image-avatar">
                {{ avatarText }}
            </div>
            <img v-else :src="profileImage" alt="User Profile" class="profile-image" />
            <div class="text-info">
                <span class="user-name">{{ userName }}</span>
                <span v-if="userEmail" class="user-email">{{ userEmail }}</span>
            </div>
        </div>
    </div>
</template>

<script setup>
const props = defineProps({
    userName: {
        type: String,
        default: 'Usuário'
    },
    userEmail: {
        type: String,
        default: ''
    },
    profileImage: {
        type: String,
        default: ''
    }
});

// Gerar inicial do nome para avatar
const getInitials = (name) => {
    if (!name) return 'U';
    const parts = name.split(' ');
    if (parts.length >= 2) {
        return (parts[0][0] + parts[1][0]).toUpperCase();
    }
    return name.substring(0, 2).toUpperCase();
};

const avatarText = getInitials(props.userName);
</script>

<style scoped>
.user-profile-card {
    display: flex;
    align-items: center;
    justify-content: space-between;
    border-radius: 40px;
    border: 2px solid #212554;
    background: linear-gradient(162deg,
            #161a42 22.61%,
            rgba(22, 26, 66, 0) 118.29%);
    width: 250px;
    height: 48px;
    font-family: sans-serif;
    color: white;
    cursor: pointer;
    transition: background-color 0.2s;
    border: 1px solid transparent;
}

.user-profile-card:hover {
     border-radius: 40px;
  border: 2px solid #212554;
  background: linear-gradient(
    162deg,
    #161a42 22.61%,
    rgba(22, 26, 66, 0) 118.29%
  );
}

.profile-info {
    display: flex;
    align-items: center;
    gap: 12px;
}

.profile-image {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid #8A2BE2;
    background-color: #8A2BE2;
}

.profile-image-avatar {
    width: 36px;
    height: 36px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    border: 2px solid #8A2BE2;
    background: linear-gradient(162deg, #8A2BE2 22.61%, #12d8fa 118.29%);
    color: white;
    font-weight: 700;
    font-size: 14px;
}

.text-info {
    display: flex;
    flex-direction: column;
}

.user-name {
    font-weight: 600;
    font-size: 1rem;
}

.user-email {
    font-size: 0.85rem;
    color: #add8e6;
}

.dropdown-arrow {
    color: #aaa;
    transition: transform 0.2s;
}
</style>