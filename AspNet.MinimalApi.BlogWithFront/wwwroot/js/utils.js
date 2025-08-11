/**
 * Утилітарні функції для роботи з додатком
 */

/**
 * Форматує дату у зручному для читання вигляді
 * @param {string|Date} date - Дата для форматування
 * @returns {string} Відформатована дата
 */
export function formatDate(date) {
  const d = new Date(date);
  const now = new Date();
  const diffMs = now - d;
  const diffMins = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMs / 3600000);
  const diffDays = Math.floor(diffMs / 86400000);

  if (diffMins < 1) return 'щойно';
  if (diffMins < 60) return `${diffMins} хв тому`;
  if (diffHours < 24) return `${diffHours} год тому`;
  if (diffDays < 7) return `${diffDays} дн тому`;
  
  return d.toLocaleDateString('uk-UA', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

/**
 * Скорочує текст до вказаної довжини
 * @param {string} text - Текст для скорочення
 * @param {number} maxLength - Максимальна довжина
 * @returns {string} Скорочений текст
 */
export function truncateText(text, maxLength = 150) {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength).trim() + '...';
}

/**
 * Екранує HTML символи
 * @param {string} text - Текст для екранування
 * @returns {string} Екранований текст
 */
export function escapeHtml(text) {
  const div = document.createElement('div');
  div.textContent = text;
  return div.innerHTML;
}

/**
 * Показує повідомлення користувачу
 * @param {string} message - Текст повідомлення
 * @param {string} type - Тип повідомлення (success, error, warning)
 */
export function showMessage(message, type = 'success') {
  // Видаляємо попередні повідомлення
  const existingAlert = document.querySelector('.alert');
  if (existingAlert) {
    existingAlert.remove();
  }

  const alert = document.createElement('div');
  alert.className = `alert alert-${type}`;
  alert.textContent = message;

  // Додаємо повідомлення на початок основного контенту
  const main = document.querySelector('.main');
  main.insertBefore(alert, main.firstChild);

  // Автоматично видаляємо повідомлення через 5 секунд
  setTimeout(() => {
    if (alert.parentNode) {
      alert.remove();
    }
  }, 5000);
}

/**
 * Показує індикатор завантаження
 * @param {HTMLElement} container - Контейнер для індикатора
 */
export function showLoading(container) {
  container.innerHTML = '<div class="loading">Завантаження</div>';
}

/**
 * Декодує JWT токен
 * @param {string} token - JWT токен
 * @returns {Object|null} Декодовані дані токена
 */
export function decodeJWT(token) {
  if (!token) return null;
  
  try {
    const parts = token.split('.');
    if (parts.length !== 3) return null;
    
    const payload = parts[1];
    const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
    return JSON.parse(decoded);
  } catch (error) {
    console.error('Error decoding JWT:', error);
    return null;
  }
}

/**
 * Перевіряє, чи токен ще дійсний
 * @param {string} token - JWT токен
 * @returns {boolean} true, якщо токен дійсний
 */
export function isTokenValid(token) {
  const decoded = decodeJWT(token);
  if (!decoded || !decoded.exp) return false;
  
  const now = Math.floor(Date.now() / 1000);
  return decoded.exp > now;
}

/**
 * Отримує ролі користувача з токена
 * @param {string} token - JWT токен
 * @returns {string[]} Масив ролей користувача
 */
export function getUserRoles(token) {
  const decoded = decodeJWT(token);
  if (!decoded) return [];
  
  const rolesClaim = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
  const roles = decoded[rolesClaim];
  
  if (Array.isArray(roles)) return roles;
  if (typeof roles === 'string') return [roles];
  return [];
}

/**
 * Отримує ім'я користувача з токена
 * @param {string} token - JWT токен
 * @returns {string|null} Ім'я користувача
 */
export function getUserName(token) {
  const decoded = decodeJWT(token);
  if (!decoded) return null;
  
  const nameClaim = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name';
  return decoded[nameClaim] || null;
}

/**
 * Перевіряє, чи користувач має вказану роль
 * @param {string} token - JWT токен
 * @param {string} role - Роль для перевірки
 * @returns {boolean} true, якщо користувач має роль
 */
export function hasRole(token, role) {
  const roles = getUserRoles(token);
  return roles.includes(role);
}

/**
 * Створює елемент з вказаними атрибутами
 * @param {string} tag - Тег елемента
 * @param {Object} attributes - Атрибути елемента
 * @param {string} content - Вміст елемента
 * @returns {HTMLElement} Створений елемент
 */
export function createElement(tag, attributes = {}, content = '') {
  const element = document.createElement(tag);
  
  Object.entries(attributes).forEach(([key, value]) => {
    if (key === 'className') {
      element.className = value;
    } else if (key === 'dataset') {
      Object.entries(value).forEach(([dataKey, dataValue]) => {
        element.dataset[dataKey] = dataValue;
      });
    } else {
      element.setAttribute(key, value);
    }
  });
  
  if (content) {
    element.innerHTML = content;
  }
  
  return element;
}

/**
 * Додає обробник подій з автоматичним видаленням
 * @param {HTMLElement} element - Елемент
 * @param {string} event - Тип події
 * @param {Function} handler - Обробник події
 * @returns {Function} Функція для видалення обробника
 */
export function addEventListenerWithCleanup(element, event, handler) {
  element.addEventListener(event, handler);
  return () => element.removeEventListener(event, handler);
}
