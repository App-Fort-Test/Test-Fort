export function useToast() {
  const showToast = (message, type = 'info', duration = 4000) => {
    const event = new CustomEvent('show-toast', {
      detail: { message, type, duration }
    });
    window.dispatchEvent(event);
  };

  const success = (message, duration = 4000) => {
    showToast(message, 'success', duration);
  };

  const error = (message, duration = 5000) => {
    showToast(message, 'error', duration);
  };

  const warning = (message, duration = 4000) => {
    showToast(message, 'warning', duration);
  };

  const info = (message, duration = 4000) => {
    showToast(message, 'info', duration);
  };

  return {
    showToast,
    success,
    error,
    warning,
    info
  };
}

