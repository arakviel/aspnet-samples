import { BaseComponent } from './BaseComponent';
import { authService } from '../services/AuthService';
import type { User } from '../types';

/**
 * Головний компонент додатку
 */
export class BlogApp extends BaseComponent {
  private currentUser: User | null = null;
  private isAuthenticated = false;

  constructor() {
    super();
  }

  connectedCallback(): void {
    super.connectedCallback();
    this.initAuth();
  }

  disconnectedCallback(): void {
    super.disconnectedCallback();
    // Видаляємо слухач стану аутентифікації
    authService.removeAuthStateListener(this.handleAuthStateChange.bind(this));
  }

  protected render(): void {
    console.log('BlogApp render викликано');
    this.shadow.innerHTML = '';

    // Додаємо стилі
    const styles = this.createStyles(`
      :host {
        display: block;
        min-height: 100vh;
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
        line-height: 1.6;
        color: #333;
        background-color: #f5f5f5;
      }

      .app-container {
        padding: 2rem;
        max-width: 1200px;
        margin: 0 auto;
      }

      .header {
        background: white;
        padding: 1rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        margin-bottom: 2rem;
      }

      .content {
        background: white;
        padding: 2rem;
        border-radius: 8px;
        box-shadow: 0 2px 4px rgba(0,0,0,0.1);
      }

      h1 {
        color: #333;
        margin: 0 0 1rem;
      }

      .nav {
        margin-top: 1rem;
      }

      .nav a {
        display: inline-block;
        margin-right: 1rem;
        padding: 0.5rem 1rem;
        background: #007bff;
        color: white;
        text-decoration: none;
        border-radius: 4px;
        transition: background-color 0.2s;
      }

      .nav a:hover {
        background: #0056b3;
      }
    `);

    // Створюємо простішу структуру
    const container = this.createElement('div', '', 'app-container');

    const header = this.createElement('div', '', 'header');

    let navLinks = '';
    if (this.isAuthenticated && this.currentUser) {
      // Навігація для аутентифікованих користувачів
      navLinks = `
        <a href="/">Головна</a>
        <a href="/create-post">Створити пост</a>
        <a href="/profile">Профіль</a>
        <span style="color: #666; margin: 0 1rem;">Привіт, ${this.escapeHtml(this.currentUser.name)}!</span>
        <button id="logout-btn" style="background: #dc3545; color: white; border: none; padding: 0.5rem 1rem; border-radius: 4px; cursor: pointer;">Вийти</button>
      `;
    } else {
      // Навігація для неаутентифікованих користувачів
      navLinks = `
        <a href="/">Головна</a>
        <a href="/login">Вхід</a>
        <a href="/register">Реєстрація</a>
      `;
    }

    header.innerHTML = `
      <h1>📝 Блог</h1>
      <div class="nav">
        ${navLinks}
      </div>
    `;

    const content = this.createElement('div', '', 'content');
    content.id = 'page-content';

    container.appendChild(header);
    container.appendChild(content);

    this.shadow.appendChild(styles);
    this.shadow.appendChild(container);

    // Показуємо відповідну сторінку після того, як DOM готовий
    setTimeout(() => {
      this.showCurrentPage();
    }, 0);

    console.log('BlogApp render завершено');
  }

  protected setupEventListeners(): void {
    // Додаємо обробку кліків по посиланнях та кнопках
    this.shadow.addEventListener('click', (event) => {
      const target = event.target as HTMLElement;

      if (target.tagName === 'A') {
        event.preventDefault();
        const href = target.getAttribute('href');
        if (href) {
          this.navigate(href);
        }
      } else if (target.id === 'logout-btn') {
        this.handleLogout();
      }
    });

    // Обробляємо зміни в історії браузера (кнопки назад/вперед)
    window.addEventListener('popstate', () => {
      this.updateContent(window.location.pathname);
    });
  }

  /**
   * Проста навігація
   */
  private navigate(path: string): void {
    window.history.pushState(null, '', path);
    this.updateContent(path);
  }

  /**
   * Показати поточну сторінку
   */
  private showCurrentPage(): void {
    const path = window.location.pathname;
    console.log('showCurrentPage викликано для шляху:', path);
    this.updateContent(path);
  }

  /**
   * Оновлення контенту залежно від шляху
   */
  private updateContent(path: string): void {
    const content = this.shadowQuery('#page-content');
    if (!content) return;

    // Очищуємо контент
    content.innerHTML = '';

    // Створюємо відповідний компонент
    let pageElement: HTMLElement;

    switch (path) {
      case '/':
        pageElement = document.createElement('home-page');
        break;
      case '/login':
        pageElement = document.createElement('login-page');
        break;
      case '/register':
        pageElement = document.createElement('register-page');
        break;
      case '/confirm-email':
        pageElement = document.createElement('confirm-email-page');
        break;
      case '/forgot-password':
        pageElement = document.createElement('forgot-password-page');
        break;
      case '/profile':
        pageElement = document.createElement('profile-page');
        break;
      case '/create-post':
        pageElement = document.createElement('create-post-page');
        break;
      case '/reset-password':
        pageElement = document.createElement('reset-password-page');
        break;
      default:
        // Перевіряємо, чи це сторінка поста або редагування
        if (path.startsWith('/posts/')) {
          pageElement = document.createElement('post-page');
        } else if (path.startsWith('/edit-post/')) {
          pageElement = document.createElement('edit-post-page');
        } else {
          pageElement = document.createElement('div');
          pageElement.innerHTML = `
            <h2>Сторінка не знайдена</h2>
            <p>Шлях: ${path}</p>
            <p><a href="/">Повернутися на головну</a></p>
          `;
        }
    }

    content.appendChild(pageElement);
  }

  /**
   * Ініціалізація аутентифікації
   */
  private initAuth(): void {
    // Отримуємо поточний стан
    this.currentUser = authService.getCurrentUser();
    this.isAuthenticated = authService.getIsAuthenticated();

    // Додаємо слухач змін стану
    authService.addAuthStateListener(this.handleAuthStateChange.bind(this));
  }

  /**
   * Обробка зміни стану аутентифікації
   */
  private handleAuthStateChange(event: any): void {
    this.currentUser = event.detail.user;
    this.isAuthenticated = event.detail.isAuthenticated;
    this.render();
  }

  /**
   * Обробка виходу з системи
   */
  private async handleLogout(): Promise<void> {
    try {
      await authService.logout();
      // Перенаправляємо на головну сторінку
      this.navigate('/');
    } catch (error) {
      console.error('Помилка виходу:', error);
      alert('Помилка виходу з системи');
    }
  }

  /**
   * Екранування HTML
   */
  private escapeHtml(text: string): string {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
  }
}

// Реєструємо компонент
customElements.define('blog-app', BlogApp);
