import type {
  ApiResponse,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  User,
  BlogPost,
  CreatePostRequest,
  UpdatePostRequest,
  Comment,
  CreateCommentRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ConfirmEmailRequest,
  PaginatedResponse
} from '../types';

/**
 * Сервіс для роботи з API
 */
export class ApiService {
  private baseUrl: string;
  private token: string | null = null;

  constructor() {
    this.baseUrl = 'http://localhost:8081';
    this.token = localStorage.getItem('authToken');
  }

  /**
   * Встановлення токена аутентифікації
   */
  setToken(token: string): void {
    this.token = token;
    localStorage.setItem('authToken', token);
  }

  /**
   * Видалення токена аутентифікації
   */
  clearToken(): void {
    this.token = null;
    localStorage.removeItem('authToken');
  }

  /**
   * Отримання заголовків для запиту
   */
  private getHeaders(): HeadersInit {
    const headers: HeadersInit = {
      'Content-Type': 'application/json',
    };

    if (this.token) {
      headers['Authorization'] = `Bearer ${this.token}`;
    }

    return headers;
  }

  /**
   * Базовий метод для HTTP запитів
   */
  private async request<T>(
    endpoint: string, 
    options: RequestInit = {}
  ): Promise<ApiResponse<T>> {
    const url = `${this.baseUrl}${endpoint}`;
    
    const config: RequestInit = {
      ...options,
      headers: {
        ...this.getHeaders(),
        ...options.headers,
      },
    };

    try {
      console.log(`API Request: ${config.method || 'GET'} ${url}`);
      
      const response = await fetch(url, config);
      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || `HTTP error! status: ${response.status}`);
      }

      console.log(`API Response: ${response.status}`, data);
      return data;
    } catch (error) {
      console.error('API Error:', error);
      throw error;
    }
  }

  // === АУТЕНТИФІКАЦІЯ ===

  /**
   * Вхід в систему
   */
  async login(loginData: LoginRequest): Promise<AuthResponse> {
    const response = await fetch(`${this.baseUrl}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        Email: loginData.email,
        Password: loginData.password
      })
    });

    if (!response.ok) {
      throw new Error('Помилка входу в систему');
    }

    const data = await response.json();

    // Зберігаємо токен
    this.setToken(data.accessToken);

    return {
      token: data.accessToken,
      accessToken: data.accessToken,
      refreshToken: data.refreshToken,
      expiresAt: data.expiresAtUtc,
      user: data.user || { id: '', email: '', userName: '', roles: [] }
    };
  }

  /**
   * Реєстрація
   */
  async register(registerData: RegisterRequest): Promise<ApiResponse> {
    return await this.request('/auth/register', {
      method: 'POST',
      body: JSON.stringify({
        UserName: registerData.name,
        Email: registerData.email,
        Password: registerData.password
      }),
    });
  }

  /**
   * Підтвердження email
   */
  async confirmEmail(confirmData: ConfirmEmailRequest): Promise<ApiResponse> {
    return await this.request('/auth/confirm-email', {
      method: 'POST',
      body: JSON.stringify(confirmData),
    });
  }

  /**
   * Запит на відновлення пароля
   */
  async forgotPassword(forgotData: ForgotPasswordRequest): Promise<ApiResponse> {
    return await this.request('/auth/forgot-password', {
      method: 'POST',
      body: JSON.stringify(forgotData),
    });
  }

  /**
   * Скидання пароля
   */
  async resetPassword(resetData: ResetPasswordRequest): Promise<ApiResponse> {
    return await this.request('/auth/reset-password', {
      method: 'POST',
      body: JSON.stringify(resetData),
    });
  }

  /**
   * Отримання поточного користувача
   */
  async getCurrentUser(): Promise<User> {
    const response = await this.request<User>('/auth/me');
    return response.data!;
  }

  /**
   * Вихід з системи
   */
  async logout(): Promise<void> {
    try {
      await this.request('/auth/logout', {
        method: 'POST',
      });
    } finally {
      this.clearToken();
    }
  }

  // === ПОСТИ ===

  /**
   * Отримання списку постів
   */
  async getPosts(page: number = 1, pageSize: number = 10): Promise<PaginatedResponse<BlogPost>> {
    const response = await fetch(`${this.baseUrl}/posts?page=${page}&pageSize=${pageSize}`, {
      method: 'GET',
      headers: this.getHeaders()
    });

    if (!response.ok) {
      throw new Error('Помилка завантаження постів');
    }

    const data = await response.json();

    // Якщо API повертає новий формат з Success/Data
    if (data.success && data.data) {
      return data.data;
    }

    // Якщо API повертає старий формат (масив)
    if (Array.isArray(data)) {
      return {
        items: data.map((post: any) => ({
          id: post.id,
          title: post.title,
          content: post.content,
          summary: post.summary || '',
          createdAt: post.createdDate,
          updatedAt: post.updatedDate,
          isPublished: post.isPublished ?? true,
          tags: post.tags || [],
          authorId: post.authorId || '',
          authorName: post.authorName || 'Unknown',
          commentsCount: post.commentsCount || 0,
          likesCount: post.likesCount || 0
        })),
        totalCount: data.length,
        page: page,
        pageNumber: page,
        pageSize: pageSize,
        totalPages: 1
      };
    }

    return data;
  }

  /**
   * Отримання поста за ID
   */
  async getPost(id: string): Promise<BlogPost> {
    const response = await fetch(`${this.baseUrl}/posts/${id}`, {
      method: 'GET',
      headers: this.getHeaders()
    });

    if (!response.ok) {
      throw new Error(`Помилка завантаження поста: ${response.status}`);
    }

    const data = await response.json();
    console.log('API відповідь для поста:', data);

    if (!data.success) {
      throw new Error(data.message || 'Пост не знайдено');
    }

    return data.data;
  }

  /**
   * Створення нового поста
   */
  async createPost(postData: CreatePostRequest): Promise<BlogPost> {
    const response = await this.request<BlogPost>('/posts', {
      method: 'POST',
      body: JSON.stringify(postData),
    });
    return response.data!;
  }

  /**
   * Оновлення поста
   */
  async updatePost(id: string, postData: UpdatePostRequest): Promise<BlogPost> {
    const response = await this.request<BlogPost>(`/posts/${id}`, {
      method: 'PUT',
      body: JSON.stringify(postData),
    });
    return response.data!;
  }

  /**
   * Видалення поста
   */
  async deletePost(id: string): Promise<ApiResponse> {
    return await this.request(`/posts/${id}`, {
      method: 'DELETE',
    });
  }

  // === КОМЕНТАРІ ===

  /**
   * Отримання коментарів до поста
   */
  async getComments(postId: string): Promise<Comment[]> {
    const response = await this.request<Comment[]>(`/posts/${postId}/comments`);
    return response.data!;
  }

  /**
   * Створення коментаря
   */
  async createComment(commentData: CreateCommentRequest): Promise<Comment> {
    const response = await this.request<Comment>('/comments', {
      method: 'POST',
      body: JSON.stringify(commentData),
    });
    return response.data!;
  }

  /**
   * Видалення коментаря
   */
  async deleteComment(id: string): Promise<ApiResponse> {
    return await this.request(`/comments/${id}`, {
      method: 'DELETE',
    });
  }

  // === ПРОФІЛЬ ===

  /**
   * Оновлення профілю
   */
  async updateProfile(profileData: Partial<User>): Promise<User> {
    const response = await this.request<User>('/auth/profile', {
      method: 'PUT',
      body: JSON.stringify(profileData),
    });
    return response.data!;
  }

  /**
   * Зміна пароля
   */
  async changePassword(oldPassword: string, newPassword: string): Promise<ApiResponse> {
    return await this.request('/auth/change-password', {
      method: 'POST',
      body: JSON.stringify({ oldPassword, newPassword }),
    });
  }

  // === ДВОХФАКТОРНА АУТЕНТИФІКАЦІЯ ===

  /**
   * Налаштування 2FA
   */
  async setupTwoFactor(): Promise<{ qrCodeUri: string; manualEntryKey: string }> {
    const response = await this.request<{ qrCodeUri: string; manualEntryKey: string }>('/auth/2fa/setup');
    return response.data!;
  }

  /**
   * Увімкнення 2FA
   */
  async enableTwoFactor(code: string): Promise<ApiResponse> {
    return await this.request('/auth/2fa/enable', {
      method: 'POST',
      body: JSON.stringify({ code }),
    });
  }

  /**
   * Вимкнення 2FA
   */
  async disableTwoFactor(): Promise<ApiResponse> {
    return await this.request('/auth/2fa/disable', {
      method: 'POST',
    });
  }

  /**
   * Перевірка стану аутентифікації
   */
  isAuthenticated(): boolean {
    return !!this.token;
  }

  /**
   * Перевірка валідності токена
   */
  async validateToken(): Promise<boolean> {
    if (!this.token) return false;

    try {
      await this.getCurrentUser();
      return true;
    } catch {
      this.clearToken();
      return false;
    }
  }
}

// Створюємо глобальний екземпляр сервісу
export const apiService = new ApiService();
