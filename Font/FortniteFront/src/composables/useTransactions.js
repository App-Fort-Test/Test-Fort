import { ref } from 'vue';

const transactionUpdateEvent = ref(0);

export function useTransactions() {
  const triggerTransactionUpdate = () => {
    transactionUpdateEvent.value = Date.now();
  };

  return {
    transactionUpdateEvent,
    triggerTransactionUpdate
  };
}

