<template>
  <div class="v-bucks">
    <img src="C:\Users\marce\OneDrive\Documentos\GitHub\Privado\Font\FortniteFront\src\assets\svg\logomoeda.svg" alt="V-Bucks Icon" class="vbucks-icon" />
    <span class="vbucks-amount">{{ formatNumber(amount) }} V-Bucks</span>
  </div>
</template>

<script setup>
const props = defineProps({
  amount: {
    type: Number,
    default: 0
  }
});

const formatNumber = (num) => {
  return new Intl.NumberFormat('pt-BR').format(num);
};
</script>

<style scoped>

.v-bucks{
    margin-top:8px;
  display: flex;
  width: 200px;
  height: 36px;
  justify-content: center;
  align-items: center;
  background: linear-gradient(162deg, #1fa2ff 22.61%, #12d8fa 118.29%);
  border: 2px solid #12d8fa;
  box-shadow: 0 13px 17px 0 rgba(18, 216, 250, 0.1);
  border-radius: 40px;
}

.vbucks-amount{
  color: black;
  font-weight: 600;
}

</style>