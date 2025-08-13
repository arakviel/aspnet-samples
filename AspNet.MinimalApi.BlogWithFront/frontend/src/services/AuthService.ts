import type { User, AuthStateChangeEvent } from '../types';
import { apiService } from './ApiService';

/**
 * Сервіс для управління станом аутентифікації
 */
export class AuthService {
  private currentUser: User | null = null;
  private isAuthenticated = false;
  private listeners: Set<(event: AuthStateChangeEvent) => void> = new Set();

  constructor() {
    this.init();
  }

  /**
   * Ініціалізація сервісу
   */
  private async init(): Promise<void> {
    // Перевіряємо, чи є збережений токен
    if (apiService.isAuthenticated()) {
      try {
        await this.loadCurrentUser();
      } catch (error) {
        console.error('Помилка завантаження користувача:', error);
        this.logout();
      }
    }
  }

  /**
   * Завантаження поточного користувача
   */
  private async loadCurrentUser(): Promise<void> {
    try {
      this.currentUser = await apiService.getCurrentUser();
      this.isAuthenticated = true;
      this.notifyListeners();
    } catch (error) {
      this.currentUser = null;
      this.isAuthenticated = false;
      apiService.clearToken();
      this.notifyListeners();
      throw error;
    }
  }

  /**
   * Вхід в систему
   */
  async login(email: string, password: string, rememberMe?: boolean, twoFactorCode?: string): Promise<{ requiresTwoFactor?: boolean }> {
    try {
      const response = await apiService.login({
        email,
        password,
        rememberMe,
        twoFactorCode
      });

      if (response.requiresTwoFactor) {
        return { requiresTwoFactor: true };
      }

      this.currentUser = response.user;
      this.isAuthenticated = true;
      this.notifyListeners();

      return {};
    } catch (error) {
      console.error('Помилка входу:', error);
      throw error;
    }
  }

  /**
   * Реєстрація
   */
  async register(name: string, email: string, password: string, confirmPassword: string): Promise<void> {
    try {
      await apiService.register({
        name,
        email,
        password,
        confirmPassword
      });
    } catch (error) {
      console.error('Помилка реєстрації:', error);
      throw error;
    }
  }

  /**
   * Підтвердження email
   */
  async confirmEmail(userId: string, token: string): Promise<void> {
    try {
      await apiService.confirmEmail({ userId, token });
    } catch (error) {
      console.error('Помилка підтвердження email:', error);
      throw error;
    }
  }

  /**
   * Запит на відновлення пароля
   */
  async forgotPassword(email: string): Promise<void> {
    try {
      await apiService.forgotPassword({ email });
    } catch (error) {
      console.error('Помилка запиту відновлення пароля:', error);
      throw error;
    }
  }

  /**
   * Скидання пароля
   */
  async resetPassword(email: string, token: string, password: string, confirmPassword: string): Promise<void> {
    try {
      await apiService.resetPassword({
        email,
        token,
        password,
        confirmPassword
      });
    } catch (error) {
      console.error('Помилка скидання пароля:', error);
      throw error;
    }
  }

  /**
   * Вихід з системи
   */
  async logout(): Promise<void> {
    try {
      await apiService.logout();
    } catch (error) {
      console.error('Помилка виходу:', error);
    } finally {
      this.currentUser = null;
      this.isAuthenticated = false;
      this.notifyListeners();
    }
  }

  /**
   * Оновлення профілю
   */
  async updateProfile(profileData: Partial<User>): Promise<void> {
    try {
      this.currentUser = await apiService.updateProfile(profileData);
      this.notifyListeners();
    } catch (error) {
      console.error('Помилка оновлення профілю:', error);
      throw error;
    }
  }

  /**
   * Зміна пароля
   */
  async changePassword(oldPassword: string, newPassword: string): Promise<void> {
    try {
      await apiService.changePassword(oldPassword, newPassword);
    } catch (error) {
      console.error('Помилка зміни пароля:', error);
      throw error;
    }
  }

  /**
   * Налаштування двохфакторної аутентифікації
   */
  async setupTwoFactor(): Promise<{ qrCodeUri: string; manualEntryKey: string }> {
    try {
      return await apiService.setupTwoFactor();
    } catch (error) {
      console.error('Помилка налаштування 2FA:', error);
      throw error;
    }
  }

  /**
   * Увімкнення двохфакторної аутентифікації
   */
  async enableTwoFactor(code: string): Promise<void> {
    try {
      await apiService.enableTwoFactor(code);
      // Оновлюємо дані користувача
      await this.loadCurrentUser();
    } catch (error) {
      console.error('Помилка увімкнення 2FA:', error);
      throw error;
    }
  }

  /**
   * Вимкнення двохфакторної аутентифікації
   */
  async disableTwoFactor(): Promise<void> {
    try {
      await apiService.disableTwoFactor();
      // Оновлюємо дані користувача
      await this.loadCurrentUser();
    } catch (error) {
      console.error('Помилка вимкнення 2FA:', error);
      throw error;
    }
  }

  /**
   * Отримання поточного користувача
   */
  getCurrentUser(): User | null {
    return this.currentUser;
  }

  /**
   * Перевірка аутентифікації
   */
  getIsAuthenticated(): boolean {
    return this.isAuthenticated;
  }

  /**
   * Перевірка ролі користувача
   */
  hasRole(role: string): boolean {
    return this.currentUser?.roles.includes(role) || false;
  }

  /**
   * Перевірка, чи є користувач адміністратором
   */
  isAdmin(): boolean {
    return this.hasRole('Admin');
  }

  /**
   * Додавання слухача змін стану аутентифікації
   */
  addAuthStateListener(listener: (event: AuthStateChangeEvent) => void): void {
    this.listeners.add(listener);
  }

  /**
   * Видалення слухача змін стану аутентифікації
   */
  removeAuthStateListener(listener: (event: AuthStateChangeEvent) => void): void {
    this.listeners.delete(listener);
  }

  /**
   * Сповіщення слухачів про зміну стану
   */
  private notifyListeners(): void {
    const event = new CustomEvent('authstatechange', {
      detail: {
        isAuthenticated: this.isAuthenticated,
        user: this.currentUser
      }
    }) as AuthStateChangeEvent;

    this.listeners.forEach(listener => {
      try {
        listener(event);
      } catch (error) {
        console.error('Помилка в слухачі стану аутентифікації:', error);
      }
    });

    // Також відправляємо глобальну подію
    document.dispatchEvent(event);
  }

  /**
   * Перевірка валідності токена
   */
  async validateToken(): Promise<boolean> {
    try {
      const isValid = await apiService.validateToken();
      if (!isValid && this.isAuthenticated) {
        this.currentUser = null;
        this.isAuthenticated = false;
        this.notifyListeners();
      }
      return isValid;
    } catch (error) {
      console.error('Помилка валідації токена:', error);
      return false;
    }
  }

  /**
   * Оновлення токена (якщо потрібно)
   */
  async refreshToken(): Promise<void> {
    // TODO: Реалізувати логіку оновлення токена, якщо бекенд підтримує
    console.log('Refresh token не реалізовано');
  }
}

// Створюємо глобальний екземпляр сервісу
export const authService = new AuthService();
