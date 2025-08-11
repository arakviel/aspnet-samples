/**
 * API клієнт для роботи з бекендом
 * Забезпечує централізовану роботу з HTTP запитами
 */
class ApiClient {
  constructor(baseUrl = '') {
    this.baseUrl = baseUrl;
    this.token = localStorage.getItem('authToken');
    this.refreshToken = localStorage.getItem('refreshToken');
    this.isRefreshing = false;
    this.refreshPromise = null;
  }

  /**
   * Встановлює токени авторизації
   * @param {string} token - JWT токен
   * @param {string} refreshToken - Рефреш токен
   */
  setTokens(token, refreshToken = null) {
    this.token = token;
    this.refreshToken = refreshToken;

    if (token) {
      localStorage.setItem('authToken', token);
    } else {
      localStorage.removeItem('authToken');
    }

    if (refreshToken) {
      localStorage.setItem('refreshToken', refreshToken);
    } else if (refreshToken === null && !token) {
      localStorage.removeItem('refreshToken');
    }
  }

  /**
   * Встановлює токен авторизації (для зворотної сумісності)
   * @param {string} token - JWT токен
   */
  setToken(token) {
    this.setTokens(token);
  }

  /**
   * Отримує токен авторизації
   * @returns {string|null} JWT токен
   */
  getToken() {
    return this.token;
  }

  /**
   * Оновлює токен доступу за допомогою рефреш токена
   * @returns {Promise<boolean>} true, якщо токен успішно оновлено
   */
  async refreshAccessToken() {
    if (!this.refreshToken || this.isRefreshing) {
      return false;
    }

    // Якщо вже йде процес оновлення, чекаємо його завершення
    if (this.refreshPromise) {
      return await this.refreshPromise;
    }

    this.isRefreshing = true;
    this.refreshPromise = this.performTokenRefresh();

    try {
      const result = await this.refreshPromise;
      return result;
    } finally {
      this.isRefreshing = false;
      this.refreshPromise = null;
    }
  }

  /**
   * Виконує фактичне оновлення токена
   * @returns {Promise<boolean>} true, якщо токен успішно оновлено
   */
  async performTokenRefresh() {
    try {
      const response = await fetch(this.baseUrl + '/auth/refresh', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken: this.refreshToken })
      });

      if (response.ok) {
        const data = await response.json();
        this.setTokens(data.accessToken, data.refreshToken || this.refreshToken);
        return true;
      } else {
        // Рефреш токен недійсний - очищуємо всі токени
        this.logout();
        return false;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      this.logout();
      return false;
    }
  }

  /**
   * Виконує HTTP запит з автоматичним оновленням токена
   * @param {string} path - Шлях до ендпоінту
   * @param {Object} options - Опції запиту
   * @returns {Promise<any>} Результат запиту
   */
  async request(path, options = {}) {
    const headers = {
      'Content-Type': 'application/json',
      ...options.headers
    };

    // Додаємо токен авторизації, якщо він є
    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`;
    }

    const config = {
      ...options,
      headers
    };

    try {
      const response = await fetch(this.baseUrl + path, config);

      // Якщо отримали 401 і є рефреш токен, спробуємо оновити токен
      if (response.status === 401 && this.refreshToken && !path.includes('/auth/')) {
        const refreshed = await this.refreshAccessToken();
        if (refreshed) {
          // Повторюємо запит з новим токеном
          headers['Authorization'] = `Bearer ${this.token}`;
          const retryResponse = await fetch(this.baseUrl + path, { ...config, headers });

          if (!retryResponse.ok) {
            const errorText = await retryResponse.text();
            throw new Error(`HTTP ${retryResponse.status}: ${errorText}`);
          }

          if (retryResponse.status === 204) {
            return null;
          }

          return await retryResponse.json();
        }
      }

      // Перевіряємо статус відповіді
      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(`HTTP ${response.status}: ${errorText}`);
      }

      // Повертаємо null для статусу 204 (No Content)
      if (response.status === 204) {
        return null;
      }

      // Парсимо JSON відповідь
      return await response.json();
    } catch (error) {
      console.error('API Request Error:', error);
      throw error;
    }
  }

  /**
   * GET запит
   * @param {string} path - Шлях до ендпоінту
   * @returns {Promise<any>} Результат запиту
   */
  get(path) {
    return this.request(path, { method: 'GET' });
  }

  /**
   * POST запит
   * @param {string} path - Шлях до ендпоінту
   * @param {Object} data - Дані для відправки
   * @returns {Promise<any>} Результат запиту
   */
  post(path, data) {
    return this.request(path, {
      method: 'POST',
      body: JSON.stringify(data)
    });
  }

  /**
   * PUT запит
   * @param {string} path - Шлях до ендпоінту
   * @param {Object} data - Дані для відправки
   * @returns {Promise<any>} Результат запиту
   */
  put(path, data) {
    return this.request(path, {
      method: 'PUT',
      body: JSON.stringify(data)
    });
  }

  /**
   * DELETE запит
   * @param {string} path - Шлях до ендпоінту
   * @returns {Promise<any>} Результат запиту
   */
  delete(path) {
    return this.request(path, { method: 'DELETE' });
  }

  // Методи для роботи з авторизацією
  async login(userName, password) {
    const response = await this.post('/auth/login', { userName, password });
    if (response.accessToken) {
      this.setTokens(response.accessToken, response.refreshToken);
    }
    return response;
  }

  async register(userName, email, password) {
    return await this.post('/auth/register', { userName, email, password });
  }

  logout() {
    this.setTokens(null, null);
  }

  // Методи для роботи з постами
  async getPosts() {
    return await this.get('/posts');
  }

  async getPost(id) {
    return await this.get(`/posts/${id}`);
  }

  async createPost(title, content) {
    return await this.post('/posts', { title, content });
  }

  async updatePost(id, title, content) {
    return await this.put(`/posts/${id}`, { title, content });
  }

  async deletePost(id) {
    return await this.delete(`/posts/${id}`);
  }

  // Методи для роботи з коментарями
  async createComment(postId, content) {
    return await this.post(`/posts/${postId}/comments`, { content });
  }

  async updateComment(id, content) {
    return await this.put(`/comments/${id}`, { content });
  }

  async deleteComment(id) {
    return await this.delete(`/comments/${id}`);
  }

  // Методи для роботи з лайками
  async toggleLike(postId) {
    return await this.post(`/posts/${postId}/likes/toggle`);
  }

  // Адмін методи
  async getUsers() {
    return await this.get('/admin/users');
  }

  async getAdminPosts() {
    return await this.get('/admin/posts');
  }

  async assignRole(userId, roleName) {
    return await this.post('/admin/users/assign-role', { userId, roleName });
  }

  async removeRole(userId, roleName) {
    return await this.post('/admin/users/remove-role', { userId, roleName });
  }
}

// Експортуємо екземпляр API клієнта
export const api = new ApiClient();
